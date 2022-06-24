module Alexandria.Shared.BooksApi

open System

type Book = {
    Id: Guid
    Title: string
    Authors: string list
    Year: uint16 option
    InventoryLocation: string
}

type AddBook = {
    Title: string
    Authors: string list
    Year: uint16 option
    InventoryLocation: string
    Note: string
}

type EditBook = {
    BookId: Guid
    Title: string
    Authors: string list
    //TODO rest
}

//NOTE: must be record type for Fable.Remoting
type BookService = {
    GetBooks : unit -> Async<Book list>
    AddBook : AddBook -> Async<Book>
    EditBook : EditBook -> Async<Book>
}
with
    static member RouteBuilder _ m = sprintf "/api/bookService/%s" m