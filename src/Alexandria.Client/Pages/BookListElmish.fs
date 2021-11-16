module Alexandria.Client.Pages.BookListElmish

open System

open Alexandria.Shared.BooksApi
open Fable.FontAwesome

open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Feliz.UseElmish


open Alexandria.Client

open Components.Common



module Domain =

    type Model = {
        Books: Deferred<Book list>
    }

    type Msg =
    | AddBook
    | LoadBooks
    | BooksLoaded of Book list
    | LoadError of exn

module State =

    open Domain

    open Elmish
    open Elmish.Helper

    let init () : Model * Cmd<Msg> =
        {
            Books = Deferred.HasNotStartedYet
        } |> withMsg LoadBooks

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | AddBook ->
            showAlert "Clicked add in Elmish"
            model |> withoutCommands //TODO
        | LoadBooks ->
            model
            |> withCommand
                (Cmd.OfAsync.either Server.bookService.GetBooks () BooksLoaded LoadError)
        | BooksLoaded x ->
            { model with Books = (Deferred.Resolved x)} |> withoutCommands
        | LoadError x ->
            { model with Books = (Deferred.Failed x)} |> withoutCommands





[<ReactComponent>]
let BookListViewElmish () =

    let model, dispatch = React.useElmish(State.init, State.update, [| |])

    let content =
        match model.Books with
        | Deferred.HasNotStartedYet -> Html.none
        | Deferred.InProgress -> Html.p [ prop.text "...loading" ]
        | Deferred.Resolved books ->
            Html.div [
                Html.div [
                    prop.className "toolbar"
                    prop.children [
                        buttonAdd (fun _ -> dispatch Domain.AddBook) true
                        buttonEdit (fun _ -> printfn "Clicked Edit") true
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
                                        Html.td book.Title
                                        Html.td (book.Authors |> listString)
                                    ]
                        ]
                    ]
                ]
            ]
        | Deferred.Failed err -> Html.p [ prop.text err.Message ]

    mainContent content