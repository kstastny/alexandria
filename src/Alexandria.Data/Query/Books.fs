module Alexandria.Data.Query.Books

open System.Data

open FsToolkit.ErrorHandling

open Alexandria.Data
open Alexandria.Data.DataObjects
open Dapper.FSharp
open Dapper.FSharp.MySQL

open DataObjects

let third (x, y, z) = z

let getBooks (connectionString: IDbConnection) =
    task {
        let! bookAuthor =
            select {
                for b in bookTable do
                    innerJoin ba in bookAuthorTable on (b.BookId = ba.BookId)
                    innerJoin a in authorTable on (ba.AuthorId = a.AuthorId)
                    selectAll
            }
            |> connectionString.SelectAsyncOption<BookDO, BookAuthorDO, AuthorDO>
            |> Task.map List.ofSeq


        return bookAuthor
        |> List.groupBy (fun (x, _, _) -> x)
        |> List.map (fun (book, authorList) ->
            BookDO.toDomain (authorList |> List.choose third) book)
    }
