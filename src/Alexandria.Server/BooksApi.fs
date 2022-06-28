module Alexandria.Server.BooksApi

open System
open System.Data

open Alexandria.Data.Query
open Alexandria.Server.Configuration
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open FSharp.Control.Tasks
open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Http
open MySql.Data.MySqlClient

open Dapper.FSharp
open Dapper.FSharp.MySQL


open Alexandria.Shared.Domain
open Alexandria.Shared.BooksApi
open Alexandria.Data.DataObjects

let private booksApi (config: ServerConfiguration)  =
    {
        GetBooks = fun _ ->
            task {
                use conn = new MySqlConnection(config.Database.ConnectionString) :> IDbConnection

                return! Books.getBooks conn
            } |> Async.AwaitTask

        AddBook = fun b ->
            task {
                use conn = new MySqlConnection(config.Database.ConnectionString) :> IDbConnection

                //TODO validation
                let bookDO = {
                    BookId = Guid.NewGuid().ToByteArray()
                    Title = b.Title
                    Year = b.Year
                    Note = Some b.Note
                }

                let! _ =
                    insert {
                        into bookTable
                        value bookDO
                    } |> conn.InsertAsync

                //TODO locate author if it exists (normalized string, no diacritics, lowercase)
                for a in b.Authors do
                    let authorDO = {
                        AuthorId = Guid.NewGuid().ToByteArray()
                        Name = a
                    }

                    let! _ = insert { into authorTable; value authorDO } |> conn.InsertAsync

                    let bookAuthorDO = { BookId = bookDO.BookId; AuthorId = authorDO.AuthorId }
                    let! _ = insert { into bookAuthorTable; value bookAuthorDO } |> conn.InsertAsync
                    ()

                //TODO load from DB
                return {
                    Id = bookDO.BookId |> Guid
                    Title = bookDO.Title
                    Year = bookDO.Year
                    //InventoryLocation = book.InventoryLocation
                    InventoryLocation = "TBD"
                    //TODO Authors = b.Authors// :)
                    Authors = []
                }

             } |> Async.AwaitTask

        EditBook = fun editBook ->
            task {
                use conn = new MySqlConnection(config.Database.ConnectionString) :> IDbConnection

                let bookId = editBook.BookId.ToByteArray()
                let! bookDO =
                    select {
                        for book in bookTable do
                        where (book.BookId = bookId)
                    }
                    |> conn.SelectAsync<BookDO>
                    |> Task.map Seq.tryHead
                match bookDO with
                | None -> return failwith "TODO"
                | Some book ->
                    return! task {

//                    let authorsByBookId =
//                        authors
//                        |> List.groupBy (fun (ba, _) -> ba.BookId)
//                        |> List.map (fun (bookId, x) -> bookId, (x |> List.map snd))
//                        |> Map.ofList

                    let updatedBook = {
                        book with Title = editBook.Title
                    }

                    let! _ =
                        update {
                            for b in bookTable do
                            set updatedBook
                            where (b.BookId = updatedBook.BookId)
                        } |> conn.UpdateAsync

                    //TODO locate author if it exists (normalized string, no diacritics, lowercase)
                    //TODO now we are duplicating authors with each edit, WIP :)
                    for a in editBook.Authors do
                        let authorDO = {
                            AuthorId = Guid.NewGuid().ToByteArray()
                            Name = a
                        }

                        let! _ = delete { for ba in bookAuthorTable do where (ba.BookId = book.BookId) } |> conn.DeleteAsync
                        let! _ = insert { into authorTable; value authorDO } |> conn.InsertAsync

                        let bookAuthorDO = { BookId = book.BookId; AuthorId = authorDO.AuthorId }
                        let! _ = insert { into bookAuthorTable; value bookAuthorDO } |> conn.InsertAsync
                        ()

                    //TODO load from DB
                    return {
                        Id = book.BookId |> Guid
                        Title = editBook.Title
                        Year = book.Year
                        //InventoryLocation = book.InventoryLocation
                        InventoryLocation = "TBD"
                        //TODO Authors = editBook.Authors// :)
                        Authors = []
                    }
                }



             } |> Async.AwaitTask
    }

type Error = { ErrorMsg: string }

let booksApiRemoting config =
    Remoting.createApi()
    |> Remoting.withRouteBuilder BookService.RouteBuilder
    |> Remoting.withErrorHandler(fun ex (routeInfo: RouteInfo<HttpContext>) ->
        printfn "%A" ex
        Propagate  { ErrorMsg = sprintf "Error at %s on method %s: %A" routeInfo.path routeInfo.methodName ex.Message})
    |> Remoting.fromValue (booksApi config)
    |> Remoting.buildHttpHandler