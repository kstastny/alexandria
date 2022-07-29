module Alexandria.Client.Components.Sorting

open Alexandria.Shared.Domain
open Fable.React.Props
open Fable.React.Standard
open Fable.React.Helpers
open Feliz
open Fable.FontAwesome


let faSortDescending =
    Html.span
        [
            prop.children [
                Fa.span [ Fa.Solid.ChevronDown ; Fa.Size Fa.FaExtraSmall ] [  ]
            ]
        ]

let faSortAscending =
    Html.span
        [
            prop.children [
                Fa.span [ Fa.Solid.ChevronUp ; Fa.Size Fa.FaExtraSmall ] [  ]
            ]
        ]

let getSortIndicator sort currentColumn =
    [
        match sort with
        | Ascending col when col = currentColumn -> faSortAscending
        | Descending col when col = currentColumn -> faSortDescending
        | _ -> ()
    ]

let sortedColumnHeader sort onClick currentColumn (columnLabel: string) =
    Html.th [
        prop.onClick (fun _ -> onClick currentColumn)
        prop.children [
            Html.div
                [
                    prop.className "left"
                    prop.text columnLabel
                ]
            Html.div
                [
                    prop.className "right"
                    prop.children (getSortIndicator sort currentColumn)
                ]
        ]
    ]
