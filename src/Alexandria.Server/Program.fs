module Alexandria.Server.Program

open System.Reflection
open System.Runtime.CompilerServices

open System.Security.Authentication
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

//        use conn = new MySqlConnection("Server=localhost; Port=3306; Uid=root; Pwd=mariadbexamplepassword; SslMode=None;")
//        conn.Open()
//        let setcmd = new MySqlCommand("SET character_set_results=utf8", conn)
//        setcmd.ExecuteNonQuery() |> ignore
//        setcmd.Dispose()
//        conn.Close()
//
//        //TODO does not work https://stackoverflow.com/questions/68645324/system-notsupportedexception-character-set-utf8mb3-is-not-supported-by-net-f
//        EnsureDatabase.For
//            .MySqlDatabase(configuration.Database.ConnectionString, "utf8mb4")


        let upgrader =
            DeployChanges.To
                .MySqlDatabase(configuration.Database.ConnectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransactionPerScript()
                .LogToConsole()
                .LogScriptOutput()
                .Build()

        let result = upgrader.PerformUpgrade()
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