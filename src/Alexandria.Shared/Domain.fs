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


type BookSort =
    | Title
    | Author


type Sort<'a when 'a: equality> =
    | Ascending of 'a
    | Descending of 'a
    with
        member x.reverse =
            match x with
            | Ascending y -> Descending y
            | Descending y -> Ascending y
        member x.udpateSortOrder sortProperty  =
            match x with
            | Ascending y when y = sortProperty -> Descending y
            | _ -> Ascending sortProperty
