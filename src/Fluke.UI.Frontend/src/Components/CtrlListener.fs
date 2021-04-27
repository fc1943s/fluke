namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks


module CtrlListener =

    [<ReactComponent>]
    let CtrlListener () =
        Listener.useKeyPress
            (fun setter e ->
                async {
                    let! ctrlPressed = setter.snapshot.getAsync Atoms.ctrlPressed

                    if e.ctrlKey <> ctrlPressed then
                        setter.set (Atoms.ctrlPressed, e.ctrlKey)
                })

        nothing
