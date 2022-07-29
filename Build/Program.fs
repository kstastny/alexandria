open System.IO
open Fake.Core
open Fake.IO
open Fake.DotNet
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators
open Build.Helpers

initializeContext ()

module Tools =
    let private findTool tool winTool =
        let tool = if Environment.isUnix then tool else winTool
        match ProcessUtils.tryFindFileOnPath tool with
        | Some t -> t
        | _ ->
            let errorMsg =
                tool + " was not found in path. " +
                "Please install it and make sure it's available from your path. "
            failwith errorMsg

    let private runTool (cmd:string) args workingDir =
        let arguments = args |> String.split ' ' |> Arguments.OfArgs
        Command.RawCommand (cmd, arguments)
        |> CreateProcess.fromCommand
        |> CreateProcess.withWorkingDirectory workingDir
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore

    let dotnet cmd workingDir =
        let result =
            DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
        if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

    let femto = runTool "femto"
    let node = runTool (findTool "node" "node.exe")
    let yarn = runTool (findTool "yarn" "yarn.cmd")

let publishPath = Path.getFullName "publish"
let srcPath = Path.getFullName "src"
let clientSrcPath = srcPath </> "Alexandria.Client"
let serverSrcPath = srcPath </> "Alexandria.Server"
let appPublishPath = publishPath </> "app"
let fableBuildPath = clientSrcPath </> ".fable-build"

// Targets
let clean proj = [ proj </> "bin"; proj </> "obj" ] |> Shell.cleanDirs

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    Tools.node "--version" clientSrcPath
    printfn "Yarn version:"
    Tools.yarn "--version" clientSrcPath
    Tools.yarn "install --frozen-lockfile" clientSrcPath
)

Target.create "CleanPublishPath" (fun _ ->
    [ appPublishPath ] |> Shell.cleanDirs
    )


Target.create "Run" (fun _ ->
    let server = async {
        Environment.setEnvironVar "ASPNETCORE_ENVIRONMENT" "Development"
        Tools.dotnet "watch run" serverSrcPath
    }
    let client = async {
        Tools.dotnet $"fable watch --outDir %s{fableBuildPath} --run webpack-dev-server" clientSrcPath
    }
    [server;client]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "Publish" (fun _ ->
    let publishArgs = $"publish -c Release -o \"%s{appPublishPath}\" --no-self-contained"
    Tools.dotnet publishArgs serverSrcPath
    [ appPublishPath </> "appsettings.Development.json" ] |> File.deleteAll
    Tools.dotnet $"fable --outDir %s{fableBuildPath} --run webpack-cli -p" clientSrcPath
)

let dependencies = [
    "InstallClient"
        ==> "CleanPublishPath"
        ==> "Publish"

    "InstallClient"
        ==> "Run"
]


[<EntryPoint>]
let main args =
  try
      match args with
      | [| target |] -> Target.runOrDefaultWithArguments target
      | _ -> Target.runOrDefaultWithArguments "Run"
      0
  with e ->
      printfn $"%A{e}"
      1