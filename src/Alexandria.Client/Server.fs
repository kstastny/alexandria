module Alexandria.Client.Server

open Alexandria.Shared.BooksApi
open Fable.Remoting.Client
open Alexandria.Shared.API

let service =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Service.RouteBuilder
    |> Remoting.buildProxy<Service>


let bookService =
    Remoting.createApi()
    |> Remoting.withRouteBuilder BookService.RouteBuilder
    |> Remoting.buildProxy<BookService>