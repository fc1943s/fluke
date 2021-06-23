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
            (fun get set e ->
                promise {
                    let setView value =
                        let username = Atoms.getAtomValue get Atoms.username

                        match username with
                        | Some username -> Atoms.setAtomValue set (Atoms.User.view username) value
                        | None -> ()

                    match e.ctrlKey, e.shiftKey, e.key with
                    | false, true, "I" ->
                        JS.log (fun () -> "RouterObserver.onKeyDown() View.Information")
                        setView View.View.Information
                    | false, true, "H" -> setView View.View.HabitTracker
                    | false, true, "P" -> setView View.View.Priority
                    | false, true, "B" -> setView View.View.BulletJournal
                    | _ -> ()

                    let shiftPressed = Atoms.getAtomValue get Atoms.shiftPressed

                    if e.shiftKey <> shiftPressed then
                        Atoms.setAtomValue set Atoms.shiftPressed e.shiftKey
                })

        nothing
