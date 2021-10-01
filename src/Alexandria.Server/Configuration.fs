module Alexandria.Server.Configuration

open System.Text.RegularExpressions
open Microsoft.Extensions.Configuration

open FsToolkit.ErrorHandling

type ConfigurationError =
    | MissingConfigurationValue of key: string


module private Core =
    let valueOrDefault defaultValue value =
        match value with
        | null -> defaultValue
        | _ -> value

    let valueOrNone  value =
        match value with
        | null -> None
        | _ -> Some value

    let parseOrDefault parseFn defaultValue value =
        match parseFn value with
        | (true, v) -> v
        | (false, _) -> defaultValue

    let trimSecret (s: string) =
        match s with
        | null -> ""
        | x when x.Length > 3 -> sprintf "%s*******" (s.Substring(0, 3))
        | _ -> s

    let private passwordRegex = Regex("Pwd=.[^;]*", RegexOptions.Compiled)
    let removePasswordFromConnectionString s =
        match s with
        | null -> ""
        | _ -> passwordRegex.Replace(s, fun _ -> "Password=*******")

    let private sharedAccessKeyRegex = Regex("SharedAccessKey=.[^;]*", RegexOptions.Compiled)
    let removeSharedAccessKeyFromConnectionString s =
        match s with
        | null -> ""
        | _ -> sharedAccessKeyRegex.Replace(s, fun _ -> "SharedAccessKey=*******")


    let validateFilledKey name (conf: IConfiguration) =
        match conf.Item name with
        | null ->  MissingConfigurationValue name |> List.singleton |> Error
        | _ -> Ok conf

    /// Validates parameter against a list of validation functions. Returns parameter or validation errors
    let validate (fList: ('a -> Result<'a, 'e list>) list) (x: 'a): Result<'a, 'e list> =
        match fList with
        | [] -> Ok x
        | _ -> List.fold
                (fun xR f ->
                    match (xR, f x) with
                    | Ok x, Ok _ -> Ok x
                    | Error x, Ok _ -> Error x
                    | Ok _, Error y -> Error y
                    | Error x, Error y -> List.concat [x ; y ] |> Error
                )
                (Ok x)
                fList

    /// apply implementation that collects all errors
    let apply fR xR =
        match fR, xR with
        | Ok f, Ok x -> Ok (f x)
        | Ok _, Error e -> Error e
        | Error e, Ok _ -> Error e
        | Error x, Error y -> List.concat [x ; y ] |> Error

    let (<*>) = apply


module Database =

    open Core

    type DbConfiguration = {
       ConnectionString: string
    }

    let get (conf: IConfiguration) =
        conf
        |> validateFilledKey "db:connectionString"
        |> Result.map (fun x -> {
            ConnectionString = x.["db:connectionString"]
        })

    let toSafeString (c: DbConfiguration) =
        {
            c with ConnectionString = c.ConnectionString |> removePasswordFromConnectionString
        }


type ServerConfiguration =
    {
        Database: Database.DbConfiguration
    }

let toSafeString (c: ServerConfiguration) =
    {
        Database = c.Database |> Database.toSafeString
    }
    |> sprintf "%A"


let getConfig (conf: IConfiguration) =
    result {
        let! dbConfig = Database.get conf
        return {
            Database = dbConfig
        }
    }


