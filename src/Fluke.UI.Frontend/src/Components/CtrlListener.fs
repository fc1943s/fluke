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
            (fun get set e ->
                promise {
                    let ctrlPressed = Atoms.getAtomValue get Atoms.ctrlPressed

                    if e.ctrlKey <> ctrlPressed then
                        Atoms.setAtomValue set Atoms.ctrlPressed (fun _ -> e.ctrlKey)
                })

        nothing
