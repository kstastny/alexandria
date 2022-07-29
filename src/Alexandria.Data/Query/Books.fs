module Alexandria.Data.Query.Books

open System
open System.Data

open Alexandria.Shared.Domain
open FsToolkit.ErrorHandling

open Alexandria.Data
open Alexandria.Data.DataObjects
open Dapper.FSharp
open Dapper.FSharp.MySQL

open DataObjects

let private third (_, _, z) = z


let private bookForNameof = Unchecked.defaultof<BookDO>
let private authorForNameof = Unchecked.defaultof<AuthorDO>

let private getOrderBy direction = function
    | Title ->
        [ $"{booksTableName}.{nameof bookForNameof.SortByTitle}", direction ]
    | Author ->
        [
            $"{authorsTableName}.{nameof authorForNameof.SortByName}", direction
            $"{booksTableName}.{nameof bookForNameof.SortByTitle}", Asc
        ]



let private orderBy (bookSort: Sort<BookSort>) (query: SelectQuery) =
    match bookSort with
    | Ascending column -> {query with OrderBy = getOrderBy Asc column }
    | Descending column -> {query with OrderBy = getOrderBy Desc column }


//let getBooks (dbConnection: IDbConnection) (bookSort: BookSort) =
let getBooks (dbConnection: IDbConnection) (bookSort: Sort<BookSort>) =
    task {
        let! bookAuthor =
            select {
                for b in booksTable do
                    innerJoin ba in booksAuthorsTable on (b.BookId = ba.BookId)
                    innerJoin a in authorsTable on (ba.AuthorId = a.AuthorId)
                    selectAll
            }
            //TODO will the sort work for books with more authors? We should just sort by FIRST authors name (when multiple authors are implemented)
            |> orderBy bookSort
            |> dbConnection.SelectAsyncOption<BookDO, BookAuthorDO, AuthorDO>
            |> Task.map List.ofSeq


        return bookAuthor
        |> List.groupBy (fun (x, _, _) -> x)
        |> List.map (fun (book, authorList) ->
            BookDO.toDomain (authorList |> List.choose third) book)
    }

let tryGetById (dbConnection: IDbConnection) (bookId: Guid) =
    task {
        let bookIdBytes = bookId.ToByteArray ()
        let! bookAuthor =
            select {
                for b in booksTable do
                    innerJoin ba in booksAuthorsTable on (b.BookId = ba.BookId)
                    innerJoin a in authorsTable on (ba.AuthorId = a.AuthorId)
                    where (b.BookId = bookIdBytes)
            }
            |> dbConnection.SelectAsyncOption<BookDO, BookAuthorDO, AuthorDO>
            |> Task.map List.ofSeq

        let books =
            bookAuthor
            |> List.groupBy (fun (x, _, _) -> x)
            |> List.map (fun (book, authorList) -> BookDO.toDomain (authorList |> List.choose third) book)

        return
            match books with
            | [] -> None
            | [ x ] -> Some x
            | _ -> failwithf $"Found multiple books with id %A{bookId}!"
    }

let getById dbConnection bookId =
    task {
        match! tryGetById dbConnection bookId with
        | None -> return failwithf $"Book with id %A{bookId} not found!"
        | Some x -> return x
    }

let addBook
    (dbConnection: IDbConnection)
    (book: {|
      Title: string
      Authors: Author list
      Year: uint16 option
      Note: string
    |}) =
    task {
        use tran = dbConnection.BeginTransaction()

        let bookId = Guid.NewGuid()
        let bookDO = {
            BookId = bookId.ToByteArray()
            Title = book.Title
            SortByTitle = book.Title.ToLowerInvariant()
            Year = book.Year
            Note = Some book.Note
        }

        let! _ =
            insert {
                into booksTable
                value bookDO
            } |> dbConnection.InsertAsync

        for a in book.Authors do
            let bookAuthorDO = { BookId = bookDO.BookId; AuthorId = a.Id.ToByteArray() }
            let! _ = insert { into booksAuthorsTable; value bookAuthorDO } |> dbConnection.InsertAsync
            ()

        tran.Commit()

        return! getById dbConnection bookId
    }


let editBook
    (dbConnection: IDbConnection)
    (bookId: Guid)
    (book: {|
      Title: string
      Authors: Author list
      Year: uint16 option
      Note: string
    |}) =
    task {
        use tran = dbConnection.BeginTransaction()

        let bookIdBytes = bookId.ToByteArray ()
        let! bookDO =
            select {
                for b in booksTable do
                    where (b.BookId = bookIdBytes)
            }
            |> dbConnection.SelectAsync<BookDO>
            |> Task.map Seq.head

        let updatedBook = {
            bookDO with
                Title = book.Title
                Year = book.Year
                Note = Some book.Note
        }

        let! _ =
               update {
                   for b in booksTable do
                   set updatedBook
                   where (b.BookId = updatedBook.BookId)
               } |> dbConnection.UpdateAsync

        let! _ = delete { for ba in booksAuthorsTable do where (ba.BookId = bookIdBytes) }
                 |> dbConnection.DeleteAsync
        for a in book.Authors do
            let bookAuthorDO = { BookId = bookDO.BookId; AuthorId = a.Id.ToByteArray() }
            let! _ = insert { into booksAuthorsTable; value bookAuthorDO } |> dbConnection.InsertAsync
            ()

        tran.Commit()

        return! getById dbConnection bookId
    }