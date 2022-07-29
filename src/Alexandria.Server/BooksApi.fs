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


let private createAndOpenConnection (config: ServerConfiguration) =
    let conn = new MySqlConnection(config.Database.ConnectionString) :> IDbConnection
    conn.Open()
    conn

let private booksApi (config: ServerConfiguration)  =
    {
        GetBooks = fun bookSort ->
            task {
                use conn = createAndOpenConnection config

                return! Books.getBooks conn bookSort
            } |> Async.AwaitTask

        AddBook = fun b ->
            task {
                use conn = createAndOpenConnection config

                let! authors =
                    Authors.getOrCreateAuthorsByName conn b.Authors

                return! Books.addBook
                            conn
                            {| Title = b.Title
                               //TODO preserve order of authors
                               Authors = authors
                               Year = b.Year
                               Note = b.Note |}

             } |> Async.AwaitTask

        EditBook = fun editBook ->
            task {
                use conn = createAndOpenConnection config

                let! authors =
                    Authors.getOrCreateAuthorsByName conn editBook.Authors

                return! Books.editBook
                            conn
                            editBook.BookId
                            {| Title = editBook.Title
                               Authors = authors
                               Note = editBook.Note
                               Year = editBook.Year
                              |}

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