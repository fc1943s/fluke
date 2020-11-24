namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks


module ShiftListener =
    let render =
        React.memo (fun () ->
            Listener.useKeyPress (fun setter e ->
                async {
                    let! shiftPressed = setter.snapshot.getAsync Recoil.Atoms.shiftPressed

                    if e.shiftKey <> shiftPressed then
                        setter.set (Recoil.Atoms.shiftPressed, e.shiftKey)
                })

            nothing)
