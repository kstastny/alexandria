module Alexandria.Client.Server

open Alexandria.Shared.BooksApi
open Fable.Remoting.Client

let bookService =
    Remoting.createApi()
    |> Remoting.withRouteBuilder BookService.RouteBuilder
    |> Remoting.buildProxy<BookService>