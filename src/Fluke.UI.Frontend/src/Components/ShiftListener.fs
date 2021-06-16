namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fable.Core
open Feliz.Recoil
open Browser.Types
open Feliz.UseListener
open Fluke.Shared
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module ShiftListener =

    [<ReactComponent>]
    let ShiftListener () =
        let username = Store.useValue Atoms.username
        let setView = Recoil.useSetStateKeyDefault Atoms.User.view username TempUI.defaultView

        let onKeyDown =
            Recoil.useCallbackRef
                (fun _ (e: KeyboardEvent) ->
                    match e.ctrlKey, e.shiftKey, e.key with
                    | false, true, "I" ->
                        JS.log (fun () -> "RouterObserver.onKeyDown() View.Information")
                        setView View.View.Information
                    | false, true, "H" -> setView View.View.HabitTracker
                    | false, true, "P" -> setView View.View.Priority
                    | false, true, "B" -> setView View.View.BulletJournal
                    | _ -> ())

        React.useListener.onKeyDown onKeyDown

        Listener.useKeyPress
            (fun setter e ->
                async {
                    let! shiftPressed = setter.snapshot.getAsync Atoms.shiftPressed

                    if e.shiftKey <> shiftPressed then
                        setter.set (Atoms.shiftPressed, (fun _ -> e.shiftKey))
                })

        nothing
