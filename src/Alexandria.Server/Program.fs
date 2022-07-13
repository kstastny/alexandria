module Alexandria.Server.Program

open System.Reflection
open System.Runtime.CompilerServices

open System.Security.Authentication
open Alexandria.Data
open Alexandria.Server.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Hosting

open DbUp
open MySqlConnector

[<Extension>]
type HostExtensions =
    [<Extension>]
    static member inline MigrateDb(host:IHost) =
        let log = host.Services.GetService<ILogger<HostExtensions>>()
        log.LogInformation("Starting db migration.")


        let configuration = host.Services.GetService<ServerConfiguration>()
        let result = DbMigration.migrateDb configuration.Database.ConnectionString

        log.LogInformation("Db migration ended, Success: {0}", result.Successful)
        host

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseStartup(typeof<Startup.Startup>)
                    .ConfigureLogging(fun x ->
                        x.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider> ("", LogLevel.Information) |> ignore
                    )
                    .UseUrls([|"http://0.0.0.0:5000"|])
                    .UseWebRoot("public")
                    |> ignore)
        .Build()
        .MigrateDb()
        .Run()
    0