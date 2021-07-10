namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module MenuItemToggle =
    [<ReactComponent>]
    let MenuItemToggle key value setValue label =
        UI.menuOptionGroup
            (fun x ->
                x.``type`` <- "checkbox"

                x.value <-
                    [|
                        if value then yield key
                    |]

                x.onChange <- fun (checks: string []) -> promise { setValue (checks |> Array.contains key) })
            [
                UI.menuItemOption
                    (fun x ->
                        x.closeOnSelect <- true
                        x.value <- key
                        x.marginTop <- "2px"
                        x.marginBottom <- "2px")
                    [
                        str label
                    ]
            ]

    [<ReactComponent>]
    let MenuItemToggleAtom (atom: Store.Atom<bool>) label =
        let value, setValue = Store.useState atom
        MenuItemToggle (atom.ToString ()) value setValue label

    [<ReactComponent>]
    let MenuItemToggleFlagAtom (atom: Store.Atom<Flag option>) label =
        let value, setValue = Store.useState atom

        MenuItemToggle
            (atom.ToString ())
            (value
             |> Option.map Flag.Value
             |> Option.defaultValue false)
            (Flag >> Some >> setValue)
            label
