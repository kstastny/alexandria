module Alexandria.Server.WebApp

open Alexandria.Server.Configuration
open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe


let webApp config : HttpHandler =

    choose [
        routeStartsWith "/api"
            >=> choose [
                route "/api/test" >=> text "OK"
                BooksApi.booksApiRemoting config
            ]
        htmlFile "public/index.html"
    ]