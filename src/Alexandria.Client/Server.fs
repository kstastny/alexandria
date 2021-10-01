module Alexandria.Client.Server

open Fable.Remoting.Client
open Alexandria.Shared.API

let service =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Service.RouteBuilder
    |> Remoting.buildProxy<Service>