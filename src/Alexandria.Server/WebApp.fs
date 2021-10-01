module Alexandria.Server.WebApp

open System.Configuration
open System.Data
open Alexandria.Server.Configuration
open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Alexandria.Shared.API
open FSharp.Control.Tasks

open MySql.Data.MySqlClient
open Dapper.FSharp
open Dapper.FSharp.MySQL

open Dapper


let service (config: ServerConfiguration) = {
    GetMessage =
        fun _ ->
            task {
                use conn = new MySqlConnection(config.Database.ConnectionString) :> IDbConnection
                let! cnt = conn.QuerySingleAsync<int>("select count(*) from Books")
                return $"Hi from Server! Number of books in DB: %i{cnt}"
            } |> Async.AwaitTask
}

let webApp config : HttpHandler =
    let remoting =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Service.RouteBuilder
        |> Remoting.fromValue (service config)
        |> Remoting.buildHttpHandler
    choose [
        routeStartsWith "/api"
            >=> choose [
                route "/api/test" >=> text "OK"
                remoting
            ]
        htmlFile "public/index.html"
    ]