module Alexandria.Client.Pages.BookList

open System

open Fable.FontAwesome

open Feliz
open Feliz.Bulma
open Feliz.UseDeferred
open Alexandria.Client

let faButton icon title onClick isEnabled =
    Bulma.button.button
        [
            prop.className "button"
            prop.title title
            prop.disabled (not isEnabled)
            prop.onClick onClick
            prop.children [ Fa.span [ icon ] [  ] ]
        ]

let buttonAdd = faButton Fa.Solid.PlusSquare "Add"
let buttonEdit = faButton Fa.Solid.Edit "Edit"
let buttonDelete = faButton Fa.Solid.Trash "Delete"

let defaultTableOptions = [ table.isHoverable; table.isStriped; table.isBordered; table.isFullWidth; ]

let showAlert (x: string) =
     Fable.Core.JS.eval $"alert('%s{x}')" |> ignore

let listString (x: string list) = String.Join(",", x)



[<ReactComponent>]
let BookListView () =

    let callReq,setCallReq = React.useState(Deferred.HasNotStartedYet)
    let startLoadingData = React.useDeferredCallback((fun _ -> Server.bookService.GetBooks()), setCallReq)
    React.useEffect(startLoadingData, [| |])

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

    Bulma.container
        [
          container.isFluid
//          prop.onKeyDown (fun e ->
//               if (e.key = "Escape") then
//                   onEscape ()
//          )

          prop.children content
        ]