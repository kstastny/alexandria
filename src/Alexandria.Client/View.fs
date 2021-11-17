module Alexandria.Client.View

open Feliz
open Feliz.Bulma

open Fable.FontAwesome

open Router
open SharedView

[<ReactComponent>]
let navigation setPage =

    let burgerMenuActive, setBurgerMenuActive = React.useState(false)
    let toggleBurgerMenu _ = burgerMenuActive |> not |> setBurgerMenuActive

    Bulma.navbar [
        Bulma.color.isDanger
        prop.children [
            Bulma.navbarBrand.div [
                Bulma.navbarItem.a [
                    prop.onClick (fun _ -> setPage Page.Books)
                    prop.children [
                        Fa.span [ Fa.Solid.Book ] [ ]
                    ]
                ]

                Bulma.navbarBurger [
                    prop.custom ("data-target", "topMenu")
                    prop.ariaLabel "menu"
                    prop.ariaExpanded false
                    prop.role "button"
                    if burgerMenuActive then
                        Bulma.navbarBurger.isActive
                    prop.onClick toggleBurgerMenu
                    prop.children [
                        Html.span [ prop.ariaHidden true ]
                        Html.span [ prop.ariaHidden true]
                    ]
                ]
            ]

            Bulma.navbarMenu [
                prop.id "topMenu"
                if burgerMenuActive then
                    Bulma.navbarMenu.isActive
                prop.onClick toggleBurgerMenu
                prop.children [
                    Bulma.navbarStart.div [
                        Bulma.navbarItem.a [ prop.text "Books"; prop.onClick (fun _ -> setPage Page.Books) ]
                        Bulma.navbarItem.a [ prop.text "Authors"; prop.onClick (fun _ -> setPage Page.Authors) ]
                    ]
                ]
            ]
        ]
    ]



[<ReactComponent>]
let AppView () =
    let page, setPage = React.useState(Router.currentPath() |> Page.parseFromUrlSegments)

    // routing for full refreshed page (to fix wrong urls)
    React.useEffectOnce (fun _ -> Router.navigatePage page)

    let render =
        match page with
        | Page.Books -> Pages.BookList.BookListView ()
        | Page.Authors -> Html.text "TBD"

    React.router [
        router.pathMode
        router.onUrlChanged (Page.parseFromUrlSegments >> setPage)
        router.children [ navigation setPage; render ]
    ]