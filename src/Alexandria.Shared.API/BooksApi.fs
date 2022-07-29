module Alexandria.Shared.BooksApi

open System
open Alexandria.Shared.Domain


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
    Year: uint16 option
    InventoryLocation: string
    Note: string
}

//NOTE: must be record type for Fable.Remoting
type BookService = {
    GetBooks : BookSort  -> Async<Book list>
    AddBook : AddBook -> Async<Book>
    EditBook : EditBook -> Async<Book>
}
with
    static member RouteBuilder _ m = $"/api/bookService/%s{m}"