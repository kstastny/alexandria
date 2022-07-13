module Alexandria.Client.Pages.BookList

open System

open Alexandria.Shared.BooksApi
open Alexandria.Shared.Domain
open Fable.FontAwesome

open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Feliz.UseElmish


open Alexandria.Client

open Components.Common
open Components.Dialogs
open Components.Form



[<ReactComponent>]
let BookEditView (editedBook: Book option) onSaved onClose =

    let title, setTitle = React.useState (editedBook |> Option.map (fun x -> x.Title) |> Option.defaultValue "")
    //TODO multiple authors
    let author, setAuthor =
        React.useState (editedBook |> Option.bind
                                          (fun x -> x.Authors |> List.tryHead)
                        |> Option.map (fun x -> x.Name)
                        |> Option.defaultValue "")

    let error, setError = React.useState None

    let addOrEditBook  =
        //TODO validation
        React.useDeferredCallback(
            (fun _ ->
                match editedBook with
                | None ->
                    let arg = {
                        Title = title
                        //TODO multiple
                        Authors = [ author ]
                        Year = None
                        InventoryLocation = ""
                        Note = ""
                    }
                    Server.bookService.AddBook(arg)
                | Some x ->
                    let arg : EditBook = {
                        BookId = x.Id
                        Title = title
                        Authors = [ author ]
                        Year = x.Year
                        InventoryLocation = x.InventoryLocation
                        Note = ""
                    }
                    Server.bookService.EditBook(arg)
            ),
            (fun x ->
                match x with
                | Deferred.HasNotStartedYet -> printfn "has not started"
                | Deferred.InProgress -> printfn "in progress"
                | Deferred.Resolved x ->
                    onSaved x
                | Deferred.Failed err ->
                    printfn "err: %A" (string err)
                    setError (Some err.Message)
            ))



    let editFormElements =
        [
            Html.form [
                formField "Title"
                    (Bulma.input.text [
                        prop.valueOrDefault title
                        prop.onTextChange setTitle ])
                formField "Author"
                    (Bulma.input.text [
                        prop.valueOrDefault author
                        prop.onTextChange setAuthor ])
            ]
        ]


    //TODO propagate to error report upwards, better handling
    match error with
    | None ->
        editDialog
            "Book Edit"
            editFormElements
            true
            (fun _ -> addOrEditBook()) //TODO do immediately or return up? should be fine here in this style...
            (fun _ -> onClose ())
    | Some x ->
        Dialog.ErrorDialog("Err", sprintf "%A" x, true, (fun _ -> setError None))


let private sort (books: Book list) =
    books |> List.sortBy (fun x -> (x.Authors |> List.tryHead, x.Title))

[<ReactComponent>]
let BookListView () =

    let isEditing, setIsEditing = React.useState false
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
                                        )
                                                     )
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
                                    prop.children [
                                        Html.td book.Title
                                        Html.td (book.Authors |> List.map (fun y -> y.Name) |> listString)
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
                                    books @ [ b ]
                                    |> List.distinctBy (fun x -> x.Id)
                                    |> setBooks
                                    setIsEditing false)
                               (fun _ -> setIsEditing false)
                               ]
        ]

    mainContent content