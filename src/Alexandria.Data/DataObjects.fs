module Alexandria.Data.DataObjects

open System

open Dapper.FSharp

open Alexandria.Shared.Domain

type AuthorDO = {
    AuthorId: byte array
    Name: string
    // name for sorting, similar reasoniong to https://help.goodreads.com/s/article/Librarian-Manual-Author-names-and-profiles
    SortByName: string
}
with
    static member toDomain(x: AuthorDO) : Author =
        {
            Id = x.AuthorId |> Guid
            Name = x.Name
            SortByName = x.SortByName
        }



type BookDO = {
    BookId: byte array
    Title: string
    // title for sorting https://help.goodreads.com/s/article/Librarian-Manual-Book-edit-page-how-to-use-the-sort-by-title-field
    SortByTitle: string
    Year: uint16 option
    Note: string option
    //TODO add location just FK, use join InventoryLocation: string option
}
with
    static member toDomain (authors: AuthorDO list) (x: BookDO) =
        {
            Id = x.BookId |> Guid
            Title = x.Title
            Year = x.Year
            Authors = authors |> List.map AuthorDO.toDomain
            InventoryLocation = ""
            Note = x.Note |> Option.defaultValue ""
        }

type BookAuthorDO = {
    BookId: byte array
    AuthorId: byte array
}


[<Literal>]
let booksTableName = "Books"

let booksTable = table'<BookDO> booksTableName

[<Literal>]
let authorsTableName = "Authors"

let authorsTable = table'<AuthorDO> authorsTableName

let booksAuthorsTable = table'<BookAuthorDO> "BookAuthors"