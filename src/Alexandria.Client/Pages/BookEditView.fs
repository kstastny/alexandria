module Alexandria.Client.Pages.BookEditView


open Alexandria.Shared.BooksApi
open Alexandria.Shared.Domain

open Feliz
open Feliz.Bulma
open Feliz.UseDeferred


open Alexandria.Client

open Components.Dialogs
open Components.Form

[<ReactComponent>]
let BookEditView (editedBook: Book option) onSaved onClose =

    let title, setTitle = React.useState (editedBook |> Option.map (fun x -> x.Title) |> Option.defaultValue "")
    let note, setNote = React.useState (editedBook |> Option.map (fun x -> x.Note) |> Option.defaultValue "")
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
                        Note = note
                    }
                    Server.bookService.AddBook(arg)
                | Some x ->
                    let arg : EditBook = {
                        BookId = x.Id
                        Title = title
                        Authors = [ author ]
                        Year = x.Year
                        InventoryLocation = x.InventoryLocation
                        Note = note
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
                        prop.onTextChange setTitle
                        prop.autoFocus true ])
                formField "Author"
                    (Bulma.input.text [
                        prop.valueOrDefault author
                        prop.onTextChange setAuthor ])
                formField "Note"
                    (Bulma.input.text [
                        prop.valueOrDefault note
                        prop.onTextChange setNote ])
            ]
        ]


    //TODO propagate to error report upwards, better handling
    match error with
    | None ->
        editDialog
            "Book Edit"
            editFormElements
            true
            (fun _ -> addOrEditBook())
            (fun _ -> onClose ())
    | Some x ->
        Dialog.ErrorDialog("Err", sprintf "%A" x, true, (fun _ -> setError None))

