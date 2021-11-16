module Alexandria.Client.Components.Dialogs


open System.Runtime.InteropServices

open Fable.React


open Feliz
open Feliz.Bulma

type Buttons =
    | OkOnly
    | SaveCancel
    | YesNo

type Dialog private() =

    static member private Create
        (
            (title: string),
            (content: seq<ReactElement>),
            buttons,
            ?isActive,
            ?onOk,
            ?onClose,
            ?headerStyle
        ) =

        let lIsActive = isActive |> Option.defaultValue true
        let lOnOk = onOk |> Option.defaultValue ignore
        let lOnClose = onClose |> Option.defaultValue ignore

        Bulma.modal [
            if lIsActive then modal.isActive else ()
            prop.children [
                Bulma.modalBackground [ prop.onClick lOnClose ]
                Bulma.modalCard [
                    Bulma.modalCardHead [
                        match headerStyle with
                        | Some x -> prop.style x
                        | None -> ()
                        prop.children [
                            Bulma.modalCardTitle [
                            prop.text title ]
                            Bulma.delete [ prop.onClick lOnClose ]
                        ]
                    ]
                    Bulma.modalCardBody content
                    Bulma.modalCardFoot
                        [ match buttons with
                            | OkOnly ->
                                yield Bulma.button.button [ color.isSuccess ; prop.onClick lOnClose ; prop.text "OK"]
                            | SaveCancel ->
                                yield Bulma.button.button [ color.isSuccess; prop.onClick lOnOk ; prop.text "Save Changes"]
                                yield Bulma.button.button [ prop.onClick lOnClose ; prop.text "Cancel"]
                            | YesNo ->
                                yield Bulma.button.button [ color.isSuccess; prop.onClick lOnOk ; prop.text "Yes"]
                                yield Bulma.button.button [ prop.onClick lOnClose ; prop.text "No"]
                        ]
                ]
            ]
        ]


    static member ConfirmDialog
        (
            title,
            text,
            isActive,
            onYes,
            onNo
        ) =
        Dialog.Create
            (
                title,
                [str text],
                YesNo,
                isActive,
                onYes,
                onNo,
                headerStyle = [ Styles.ColorWarning ]
            )

    static member WarningDialog
        (
            title,
            text,
            isActive,
            onClose
        ) =
        Dialog.Create
            (
                title,
                [str text],
                OkOnly,
                isActive,
                onClose,
                onClose,
                headerStyle = [ Styles.ColorWarning ]
            )

    static member ErrorDialog
        (
            title,
            text,
            isActive,
            onClose
        ) =
        Dialog.Create
            (
                title,
                [str text],
                OkOnly,
                isActive,
                onClose,
                onClose,
                headerStyle = [ Styles.ColorError ]
            )

    static member ModalEditDialog
        (
            title,
          //TODO if needed  customClass, `prop.className customClass`
            content,
            isActive,
            onSave,
            onCancel
        ) =
            Dialog.Create(
                title,
                content,
                SaveCancel,
                isActive,
                onSave,
                onCancel)



let editDialog title content isActive onSave onCancel =
    Dialog.ModalEditDialog(title, content, isActive, onSave, onCancel)

let warningDialog title text isActive onClose =
    Dialog.WarningDialog(title, text, isActive, onClose)

let errorDialog title text isActive onClose =
    Dialog.ErrorDialog(title, text, isActive, onClose)

