namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks


module ShiftListener =

    [<ReactComponent>]
    let ShiftListener () =
        Listener.useKeyPress
            (fun setter e ->
                async {
                    let! shiftPressed = setter.snapshot.getAsync Atoms.shiftPressed

                    if e.shiftKey <> shiftPressed then
                        setter.set (Atoms.shiftPressed, (fun _ -> e.shiftKey))
                })

        nothing
