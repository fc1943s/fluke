namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks


module CtrlListener =
    let render =
        React.memo (fun () ->
            Listener.useKeyPress (fun setter e ->
                async {
                    let! ctrlPressed = setter.snapshot.getAsync Recoil.Atoms.ctrlPressed

                    if e.ctrlKey <> ctrlPressed then
                        setter.set (Recoil.Atoms.ctrlPressed, e.ctrlKey)
                })

            nothing)
