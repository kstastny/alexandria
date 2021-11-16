module Alexandria.Server.BooksApi

open System
open System.Configuration
open System.Data

open System.Threading.Tasks
open Alexandria.Server
open Alexandria.Server.Configuration
open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Alexandria.Shared.API
open FSharp.Control.Tasks
open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Http
open MySql.Data.MySqlClient

open Dapper.FSharp
open Dapper.FSharp.MySQL

open Dapper

open Alexandria.Shared.BooksApi


//TODO better structure, connection pooling etc. this is just quick and dirty prototype


type AuthorDO = {
    AuthorId: byte array
    Name: string
}


type BookDO = {
    BookId: byte array
    Title: string
    Year: uint16 option
    //TODO is just FK, use join InventoryLocation: string option
    Note: string option
}

type BookAuthorDO = {
    BookId: byte array
    AuthorId: byte array
}

let bookTable = table'<BookDO> "Books"
let authorTable = table'<AuthorDO> "Authors"
let bookAuthorTable = table'<BookAuthorDO> "BookAuthors"

let private booksApi (config: ServerConfiguration)  =
    {
        GetBooks = fun _ ->
            task {
                use conn = new MySqlConnection(config.Database.ConnectionString) :> IDbConnection

                let! bookDOList =
                    select {
                        for b in bookTable do
                            selectAll
                    }
                    |> conn.SelectAsync<BookDO>
                    |> Task.map Seq.toList

                let! authors =
                    task {
                        let bookIds = bookDOList |> List.map (fun x -> x.BookId)
                        return!
                            select {
                                for ba in bookAuthorTable do
                                    innerJoin a in authorTable on (ba.AuthorId = a.AuthorId)
                                    where (isIn ba.BookId bookIds)
                            }
                            |> conn.SelectAsync<BookAuthorDO, AuthorDO>
                            |> Task.map Seq.toList
                    }

                let authorsByBookId =
                    authors
                    |> List.groupBy (fun (ba, _) ->ba.BookId)
                    |> List.map (fun (bookId, x) -> bookId, (x |> List.map snd))
                    |> Map.ofList

                return
                    bookDOList
                    |> List.map (fun book ->
                            {
                                Id = book.BookId |> Guid
                                Title = book.Title
                                Authors =
                                    authorsByBookId
                                    |> Map.tryFind book.BookId
                                    |> Option.defaultValue []
                                    |> List.map (fun x -> x.Name)
                                Year = book.Year
                                //InventoryLocation = book.InventoryLocation
                                InventoryLocation = "TBD"
                                //TODO Note
                            }
                        )
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

                //TODO locate author if it exists
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
                    Authors = b.Authors// :)
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