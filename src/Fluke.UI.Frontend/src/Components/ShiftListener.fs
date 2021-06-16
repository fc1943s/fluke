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
            (fun setter e ->
                async {
                    let setView value =
                        async {
                            let! username = setter.snapshot.getAsync Atoms.username

                            match username with
                            | Some username -> setter.set (Atoms.User.view username, (fun _ -> value))
                            | None -> ()
                        }

                    match e.ctrlKey, e.shiftKey, e.key with
                    | false, true, "I" ->
                        JS.log (fun () -> "RouterObserver.onKeyDown() View.Information")
                        do! setView View.View.Information
                    | false, true, "H" -> do! setView View.View.HabitTracker
                    | false, true, "P" -> do! setView View.View.Priority
                    | false, true, "B" -> do! setView View.View.BulletJournal
                    | _ -> ()

                    let! shiftPressed = setter.snapshot.getAsync Atoms.shiftPressed

                    if e.shiftKey <> shiftPressed then
                        setter.set (Atoms.shiftPressed, (fun _ -> e.shiftKey))
                })

        nothing
