﻿module Alexandria.Server.Startup

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

open Giraffe

open Alexandria.Server.Configuration

type Startup(cfg: IConfiguration, env: IWebHostEnvironment) =
    member _.ConfigureServices (services: IServiceCollection) =

        Dapper.FSharp.OptionTypes.register()

        let config =
            match Configuration.getConfig cfg with
            | Ok x -> x
            | Error e -> failwithf $"Configuration error: %A{e}"

        services.AddSingleton<ServerConfiguration>(config) |> ignore

        services
            .AddApplicationInsightsTelemetry(cfg.["APPINSIGHTS_INSTRUMENTATIONKEY"])
            .AddGiraffe() |> ignore

    member _.Configure(app:IApplicationBuilder) =

        let config = app.ApplicationServices.GetRequiredService<ServerConfiguration>()

        app
            .UseStaticFiles()
            .UseGiraffe (WebApp.webApp config)