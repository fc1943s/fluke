namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fable.Core
open Fluke.Shared
open Fluke.UI.Frontend.Bindings


module ShiftListener =

    [<ReactComponent>]
    let ShiftListener () =

        Listener.useKeyPress
            [|
                "I"
                "H"
                "P"
                "B"
                "Shift"
                "Control"
            |]
            (fun getter setter e ->
                promise {
                    let setView value = Store.set setter Atoms.view value

                    match e.ctrlKey, e.shiftKey, e.key with
                    | true, true, "I" ->
                        JS.log (fun () -> "RouterObserver.onKeyDown() View.Information")
                        setView View.View.Information
                    | true, true, "H" -> setView View.View.HabitTracker
                    | true, true, "P" -> setView View.View.Priority
                    | true, true, "B" -> setView View.View.BulletJournal
                    | _ -> ()

                    let shiftPressed = Store.value getter Atoms.shiftPressed

                    if e.shiftKey <> shiftPressed then
                        Store.set setter Atoms.shiftPressed e.shiftKey
                })

        nothing
