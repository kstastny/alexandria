﻿module Alexandria.Client.Pages.BookListView


open Alexandria.Shared.BooksApi
open Alexandria.Shared.Domain

open Feliz
open Feliz.Bulma
open Feliz.UseDeferred


open Alexandria.Client

open Components.Common

open Alexandria.Client.Pages.BookEditView

let private sort (books: Book list) =
    //TODO author sortstring https://help.goodreads.com/s/article/Librarian-Manual-Author-names-and-profiles
    books |> List.sortBy (fun x ->
        (x.Authors |> List.tryHead |> Option.map (fun y -> y.Name.ToLowerInvariant()), x.Title.ToLowerInvariant()))

[<ReactComponent>]
let BookListView () =

    let isEditing, setIsEditing = React.useState false
    let stopEdit = (fun _ -> setIsEditing false)

    let books, setBooks = React.useState([])

    let callReq, setCallReq = React.useState Deferred.HasNotStartedYet
    let startLoadingData =
            React.useDeferredCallback((fun _ -> Server.bookService.GetBooks()),
                                      (fun x ->
                                           match x with
                                            | Deferred.HasNotStartedYet -> printfn "has not started"
                                            | Deferred.InProgress -> printfn "in progress"
                                            | Deferred.Resolved books ->
                                                setBooks books
                                                printfn "loaded"
                                            | Deferred.Failed err -> printfn "err"
                                           setCallReq x
                                        ))
    React.useEffect(startLoadingData, [| |])

    let selectedBook, setSelected = React.useState(None)

    let content =
        Html.div [
            Html.div [
                prop.className "toolbar"
                prop.children [
                    buttonAdd (fun _ ->
                        setSelected None
                        setIsEditing true) true
                    buttonEdit (fun _ -> setIsEditing true) true
                ]
            ]


            match callReq with
            | Deferred.HasNotStartedYet -> Html.none
            | Deferred.InProgress -> Html.p [ prop.text "...loading" ]
            | Deferred.Resolved books -> Html.none
            | Deferred.Failed err -> Html.p [ prop.text err.Message ]

            Bulma.table [
                yield! defaultTableOptions
                prop.children [
                    Html.thead [
                        Html.tr [
                            Html.th "Name"
                            Html.th "Author"
                            //see https://stackoverflow.com/questions/51848020/how-to-hide-a-column-under-a-break-point-tablet-with-bulma
                            // and https://bulma.io/documentation/helpers/visibility-helpers/#hide
                            Html.th [ prop.text "Note" ; prop.className "is-hidden-mobile" ]
                        ]
                    ]
                    Html.tbody [
                        for book in books |> sort do
                            yield
                                Html.tr [
                                    if Some book = selectedBook then
                                        prop.className "is-selected"
                                    else
                                        prop.onClick (fun _ -> setSelected (Some book))
                                    //TODO double tap if different (in emulator does not work)
                                    prop.onDoubleClick (fun _ -> setIsEditing true)
                                    prop.children [
                                        Html.td book.Title
                                        Html.td (book.Authors |> List.map (fun y -> y.Name) |> listString)
                                        Html.td [ prop.text book.Note ; prop.className "is-hidden-mobile" ]
                                    ]
                                ]
                    ]
                ]
            ]

            if isEditing then
                Html.div [ BookEditView
                               selectedBook
                               (fun b ->
                                    setSelected (Some b)
                                    b::books
                                    |> List.distinctBy (fun x -> x.Id)
                                    |> setBooks
                                    setIsEditing false)
                               stopEdit
                               ]
        ]

    mainContent stopEdit content