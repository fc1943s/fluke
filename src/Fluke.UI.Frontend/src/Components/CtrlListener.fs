namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module CtrlListener =

    [<ReactComponent>]
    let CtrlListener () =
        Listener.useKeyPress
            [|
                "Control"
            |]
            (fun getter setter e ->
                promise {
                    let ctrlPressed = Store.value getter Atoms.ctrlPressed

                    if e.ctrlKey <> ctrlPressed then
                        Store.set setter Atoms.ctrlPressed e.ctrlKey
                })

        nothing
