﻿module Alexandria.Client.Components.Common

open System

open Fable.FontAwesome

open Feliz
open Feliz.Bulma


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

let mainContent onEscape (content: ReactElement) =
    Bulma.container
        [
          container.isFluid
          prop.onKeyDown (fun e ->
               if (e.key = "Escape") then
                   onEscape ()
          )

          prop.children content
        ]




