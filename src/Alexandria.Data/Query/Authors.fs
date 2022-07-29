module Alexandria.Data.Query.Authors

open System
open System.Data

open Alexandria.Shared.Domain
open FsToolkit.ErrorHandling

open Alexandria.Data
open Alexandria.Data.DataObjects
open Dapper.FSharp
open Dapper.FSharp.MySQL

open DataObjects

let tryGetById (dbConnection: IDbConnection) (authorId: Guid) =
    task {
        let authorIdBytes = authorId.ToByteArray ()
        let! authors =
            select {
                for a in authorsTable do
                where (a.AuthorId = authorIdBytes)
            }
            |> dbConnection.SelectAsync<AuthorDO>
            |> Task.map (List.ofSeq >> List.map AuthorDO.toDomain)

        return
            match authors with
            | [] -> None
            | [ x ] -> Some x
            | _ -> failwithf $"Found multiple authors with id %A{authorId}!"
    }

let getById dbConnection authorId =
    task {
        match! tryGetById dbConnection authorId with
        | None -> return failwithf $"Author with id %A{authorId} not found!"
        | Some x -> return x
    }


let addAuthor
    (dbConnection: IDbConnection)
    name =
    task {
        let authorId = Guid.NewGuid()
        let authorDO = {
            AuthorId = authorId.ToByteArray()
            Name = name
            //TODO separate field
            SortByName = name.ToLowerInvariant()
        }
        let! _ =
            insert {
                into authorsTable
                value authorDO
            } |> dbConnection.InsertAsync

        return! getById dbConnection authorId
    }


let saveMissingAuthors dbConnection (authorNames: string list) = task {
    let rec loop names (savedAuthors: Author list) = task {
        match names with
        | [] -> return savedAuthors
        | head::tail ->
            return!
                task {
                    let! newAuthor = addAuthor dbConnection head
                    return! loop tail (newAuthor::savedAuthors)
                }
        }

    return! loop authorNames []
}

//NOTE: very simple for now, should be made smarter later
//use recommendations same as https://help.goodreads.com/s/article/Librarian-Manual-Author-names-and-profiles when
//entering authors
let getOrCreateAuthorsByName (dbConnection: IDbConnection) (names: string list) =
    task {
        let trimmedNames = names |> List.map (fun x -> x.Trim())

        let! foundAuthors =
            select {
                for a in authorsTable do
                    where (isIn a.Name trimmedNames)
            }
            |> dbConnection.SelectAsync<AuthorDO>
            |> Task.map (List.ofSeq >> List.map AuthorDO.toDomain)

        let foundAuthorsNames =
            foundAuthors |> List.map (fun x -> x.Name) |> Set.ofList

        let trimmedNamesSet = trimmedNames |> Set.ofList

        let authorsToCreate =
            Set.difference trimmedNamesSet foundAuthorsNames |> Set.toList


        let! newAuthors =
            saveMissingAuthors dbConnection authorsToCreate

        return foundAuthors |> List.append newAuthors
    }