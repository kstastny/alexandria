module Alexandria.Server.BooksApi

open System
open System.Configuration
open System.Data

open System.Threading.Tasks
open Alexandria.Server.Configuration
open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Alexandria.Shared.API
open FSharp.Control.Tasks

open Microsoft.AspNetCore.Http
open MySql.Data.MySqlClient
open Dapper.FSharp
open Dapper.FSharp.MySQL

open Dapper

//TODO better structure, connection pooling etc. this is just quick and dirty prototype

type Book = {
    Id: Guid
    Title: string
    Authors: string list
}

type AddBook = {
    Title: string
    Authors: string list
}


//NOTE: must be record type
type IBooksApi = {
    GetBooks : unit -> Async<Book list>
    AddBook : AddBook -> Async<unit>
}


type BookDO = {
    BookId: byte array
    Title: string
    AuthorName: string
}



let private booksApi (config: ServerConfiguration)  =
    {
        GetBooks = fun _ ->
            task {
                use conn = new MySqlConnection(config.Database.ConnectionString) :> IDbConnection

                //TODO two queries, otherwise paging won't be possible. and use Dapper.FSharp
                let! books =
                    //conn.QueryAsync<byte array * string * string>(
                    conn.QueryAsync<BookDO>(
                        """select b.BookId, b.Title, a.Name as AuthorName from Books b, Authors a, BookAuthors ba where ba.BookId = b.BookId and a.AuthorId = ba.AuthorId""")

                return
                    books
                    |> Seq.toList
                    |> List.groupBy (fun x -> x.BookId, x.Title)
                    |> List.map (fun ((bookId, title), li ) ->
                            printfn "%A" (bookId, title)
                            let authors = li |> List.map (fun x -> x.AuthorName)
                            {
                                Id = Guid(bookId)
                                Title = title
                                Authors = authors
                            }
                        )
            } |> Async.AwaitTask

        AddBook = fun _ -> failwith "tbd"
    }

let routeBuilder _ m = $"/api/books/%s{m}"

type Error = { ErrorMsg: string }

let booksApiRemoting config =
    Remoting.createApi()
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.withErrorHandler(fun ex (routeInfo: RouteInfo<HttpContext>) ->
        printfn "%A" ex
        Propagate  { ErrorMsg = sprintf "Error at %s on method %s: %A" routeInfo.path routeInfo.methodName ex.Message})
    |> Remoting.fromValue (booksApi config)
    |> Remoting.buildHttpHandler