module Alexandria.Client.Router

open Browser.Types
open Feliz.Router
open Fable.Core.JsInterop

type Page =
    | Books
    | Authors

[<RequireQualifiedAccess>]
module Page =
    let defaultPage = Page.Books

    let parseFromUrlSegments = function
        | [ "authors" ] -> Page.Authors
        | [ "books" ] -> Page.Books
        | _ -> defaultPage

    let noQueryString segments : string list * (string * string) list = segments, []

    let toUrlSegments = function
        | Page.Books -> ["books"] |> noQueryString
        | Page.Authors -> [ "authors" ] |> noQueryString

[<RequireQualifiedAccess>]
module Router =
    let goToUrl (e:MouseEvent) =
        e.preventDefault()
        let href : string = !!e.currentTarget?attributes?href?value
        Router.navigatePath href

    let navigatePage (p:Page) = p |> Page.toUrlSegments |> Router.navigatePath