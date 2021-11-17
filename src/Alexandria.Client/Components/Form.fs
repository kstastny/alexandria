module Alexandria.Client.Components.Form

open Feliz
open Feliz.Bulma

let formFieldWithValidation (label: string) control errs =

    Bulma.field.div [
        Bulma.label [ prop.text label ]
        Bulma.control.div [
            match errs with
                | [] -> ()
                | _ -> prop.style [ Styles.ColorError ]
        ]
        control
        Bulma.help [
            color.isDanger
            errs |> (fun x -> System.String.Join(" ", x)) |> prop.text ]
    ]

let formField label control = formFieldWithValidation label control [ ]