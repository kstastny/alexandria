module Alexandria.Client.Pages.BookList

open System

open Alexandria.Shared.BooksApi
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
let BasicDeferred() =
    let loadData = async {
        do! Async.Sleep 1000
        return "Hello!"
    }

    let data = React.useDeferred(loadData, [| |])

    match data with
    | Deferred.HasNotStartedYet -> Html.none
    | Deferred.InProgress -> Html.i [ prop.className [ "fa"; "fa-refresh"; "fa-spin"; "fa-2x" ] ]
    | Deferred.Failed error -> Html.div error.Message
    | Deferred.Resolved content -> Html.h1 content






[<ReactComponent>]
let BookEditView onClose =

  //  let saveRequest, setSaveRequest = React.useState Deferred.HasNotStartedYet

    let title, setTitle = React.useState ""
    let author, setAuthor = React.useState ""

    let error, setError = React.useState None

    let addBook  =
        //TODO validation
        React.useDeferredCallback(
            (fun _ ->
                let arg = {
                    Title = title
                    //TODO multiple
                    Authors = [ author ]
                    Year = None
                    InventoryLocation = ""
                    Note = ""
                }
                Server.bookService.AddBook(arg)
            ),
            (fun x ->
                match x with
                | Deferred.HasNotStartedYet -> printfn "has not started"
                | Deferred.InProgress -> printfn "in progress"
                | Deferred.Resolved _ ->
                    printfn "saved"; onClose()
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
            (fun _ -> addBook()) //TODO do immediately or return up? should be fine here in this style...
            (fun _ -> onClose ())
    | Some x ->
        Dialog.ErrorDialog("Err", sprintf "%A" x, true, (fun _ -> setError None))


[<ReactComponent>]
let BookListView () =

    let isEditing, setIsEditing = React.useState false

    let callReq, setCallReq = React.useState Deferred.HasNotStartedYet
    let startLoadingData =
            React.useDeferredCallback((fun _ -> Server.bookService.GetBooks()),
                                      (fun x ->
                                           match x with
                                            | Deferred.HasNotStartedYet -> printfn "has not started"
                                            | Deferred.InProgress -> printfn "in progress"
                                            | Deferred.Resolved books -> printfn "loaded"
                                            | Deferred.Failed err -> printfn "err"
                                           setCallReq x
                                        )
                                                     )
    React.useEffect(startLoadingData, [| |])

    let selectedBook, setSelected = React.useState(None)

    let content =
        match callReq with
        | Deferred.HasNotStartedYet -> Html.none
        | Deferred.InProgress -> Html.p [ prop.text "...loading" ]
        | Deferred.Resolved books ->
            Html.div [
                Html.div [
                    prop.className "toolbar"
                    prop.children [
                        buttonAdd (fun _ -> setIsEditing true) true
                        buttonEdit (fun _ -> showAlert "Clicked Edit") true
                        BasicDeferred ()
                    ]
                ]

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
                            for book in books do
                                yield
                                    Html.tr [
                                        if Some book = selectedBook then
                                            prop.className "is-selected"
                                        else
                                            prop.onClick (fun _ -> setSelected (Some book))
                                        prop.children [
                                            Html.td book.Title
                                            Html.td (book.Authors |> listString)
                                        ]
                                    ]
                        ]
                    ]
                ]

                if isEditing then
                    Html.div [ BookEditView (fun _ -> setIsEditing false) ]
            ]
        | Deferred.Failed err -> Html.p [ prop.text err.Message ]

    mainContent content