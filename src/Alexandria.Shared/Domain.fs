module Alexandria.Shared.Domain

open System

type Author = {
    Id: Guid
    Name: string
    // name for sorting, similar reasoniong to https://help.goodreads.com/s/article/Librarian-Manual-Author-names-and-profiles
    SortByName: string
}

type Book = {
    Id: Guid
    Title: string
    Note: string
    Authors: Author list
    Year: uint16 option
    InventoryLocation: string
}


type SortDirection =
    | Ascending
    | Descending


type BookSort =
    | Name of SortDirection
    | Author of SortDirection