namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fable.Core
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module DockButton =

    [<ReactComponent>]
    let DockButton
        (input: {| Name: string
                   Icon: obj
                   Atom: Store.Atom<TempUI.DockType option>
                   OnClick: TempUI.DockType option -> JS.Promise<unit>
                   DockType: TempUI.DockType
                   Props: UI.IChakraProps -> unit |})
        =
        let atom, setAtom = Store.useState input.Atom

        UI.button
            (fun x ->
                x.height <- "100%"
                x.borderRadius <- "0"
                x.backgroundColor <- if atom = Some input.DockType then "gray.10" else "transparent"
                x.fontWeight <- "inherit"
                x.fontSize <- "14px"
                x.fontFamily <- "inherit"

                x.onClick <-
                    fun _ ->
                        promise {
                            let newAtomValue = if atom = Some input.DockType then None else Some input.DockType
                            setAtom newAtomValue
                            do! input.OnClick newAtomValue
                        }

                input.Props x)
            [
                UI.icon
                    (fun x ->
                        x.``as`` <- input.Icon
                        x.marginRight <- "6px")
                    []
                str input.Name
            ]
