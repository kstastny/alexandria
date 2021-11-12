module Alexandria.Client.Pages.BookList

open System

open Alexandria.Shared.BooksApi
open Fable.FontAwesome

open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Feliz.UseElmish


open Alexandria.Client

open Components

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
let BookListView () =

    let callReq,setCallReq = React.useState(Deferred.HasNotStartedYet)
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
                        buttonAdd (fun _ -> showAlert "Clicked add") true
                        buttonEdit (fun _ -> printfn "Clicked Edit") true
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
            ]
        | Deferred.Failed err -> Html.p [ prop.text err.Message ]

    mainContent content