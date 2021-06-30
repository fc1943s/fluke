namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module MenuItemToggle =
    [<ReactComponent>]
    let MenuItemToggle (atom: Store.Atom<_>) label =
        let value, setValue = Store.useState atom

        let key = atom.toString ()

        Chakra.menuOptionGroup
            (fun x ->
                x.``type`` <- "checkbox"

                x.value <-
                    [|
                        if value then yield key
                    |]

                x.onChange <- fun (checks: string []) -> promise { setValue (checks |> Array.contains key) })
            [
                Chakra.menuItemOption
                    (fun x ->
                        x.closeOnSelect <- true
                        x.value <- key
                        x.marginTop <- "2px"
                        x.marginBottom <- "2px")
                    [
                        str label
                    ]
            ]
