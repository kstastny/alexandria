module GitVersion

open System
open Fake.DotNet


open Newtonsoft.Json
open Fake.Core

type GitVersionProperties = {
    Major : int
    Minor : int
    Patch : int
    PreReleaseTag : string
    PreReleaseTagWithDash : string
    PreReleaseLabel : string
    //PreReleaseNumber : Nullable<int>
    BuildMetaData : string
    BuildMetaDataPadded : string
    FullBuildMetaData : string
    MajorMinorPatch : string
    SemVer : string
    LegacySemVer : string
    LegacySemVerPadded : string
    AssemblySemVer : string
    FullSemVer : string
    InformationalVersion : string
    BranchName : string
    Sha : string
    NuGetVersionV2 : string
    NuGetVersion : string
    CommitsSinceVersionSource : int
    CommitsSinceVersionSourcePadded : string
    CommitDate : string
}

let generateProperties () =
    let result = DotNet.exec (fun x -> { x with RedirectOutput = true }) "dotnet-gitversion" "/config GitVersion.yml"
    if result.ExitCode <> 0 then
        failwithf "dotnet-gitversion failed with exit code %i and message %s, errors %s" result.ExitCode (String.concat "" result.Messages) (String.concat "" result.Errors)

    result.Messages |> String.concat "" |> JsonConvert.DeserializeObject<GitVersionProperties>