namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open FsJs
open FsStore
open FsStore.Model
open FsUi.Bindings


module MenuItemToggle =
    [<ReactComponent>]
    let MenuItemToggle key value setValue label =
        Ui.menuOptionGroup
            (fun x ->
                x.``type`` <- "checkbox"

                x.value <-
                    [|
                        if value then yield key
                    |]

                x.onChange <- fun (checks: string []) -> promise { setValue (checks |> Array.contains key) })
            [
                Ui.menuItemOption
                    (fun x ->
                        x.closeOnSelect <- true
                        x.value <- key
                        x.marginTop <- "2px"
                        x.marginBottom <- "2px"
                        x._hover <- Js.newObj (fun x -> x.backgroundColor <- "gray.10"))
                    [
                        str label
                    ]
            ]

    [<ReactComponent>]
    let MenuItemToggleAtom (atom: Atom<bool>) label =
        let value, setValue = Store.useState atom
        MenuItemToggle (atom.ToString ()) value setValue label

    [<ReactComponent>]
    let MenuItemToggleAtomOption (atom: Atom<bool option>) label =
        let value, setValue = Store.useState atom

        MenuItemToggle (atom.ToString ()) (value |> Option.defaultValue false) (Some >> setValue) label
