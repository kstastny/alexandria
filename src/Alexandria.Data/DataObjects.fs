module Alexandria.Data.DataObjects

open System

open Dapper.FSharp

open Alexandria.Shared.Domain

type AuthorDO = {
    AuthorId: byte array
    Name: string
}
with
    static member toDomain(x: AuthorDO) : Author =
        {
            Id = x.AuthorId |> Guid
            Name = x.Name
        }



type BookDO = {
    BookId: byte array
    Title: string
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
        }

type BookAuthorDO = {
    BookId: byte array
    AuthorId: byte array
}

let bookTable = table'<BookDO> "Books"
let authorTable = table'<AuthorDO> "Authors"
let bookAuthorTable = table'<BookAuthorDO> "BookAuthors"