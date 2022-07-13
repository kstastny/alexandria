module Alexandria.Shared.Domain

open System

type Author = {
    Id: Guid
    Name: string
}

type Book = {
    Id: Guid
    Title: string
    Authors: Author list
    Year: uint16 option
    InventoryLocation: string
}