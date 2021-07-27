namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fable.Core
open Fluke.Shared
open Fluke.UI.Frontend.Bindings


module CtrlListener =
    [<ReactComponent>]
    let CtrlListener () =
        Listener.useKeyPress
            [|
                "Control"
            |]
            (fun _getter setter e ->
                promise {
                    //                    let ctrlPressed = Store.value getter Atoms.ctrlPressed
//
//                    if e.ctrlKey <> ctrlPressed then
                    Store.set setter Atoms.Session.ctrlPressed e.ctrlKey
                })

        nothing


module ShiftListener =
    [<ReactComponent>]
    let ShiftListener () =
        Listener.useKeyPress
            [|
                "Shift"
            |]
            (fun _getter setter e ->
                promise {
                    //                    let shiftPressed = Store.value getter Atoms.shiftPressed
//
//                    if e.shiftKey <> shiftPressed then
                    Store.set setter Atoms.Session.shiftPressed e.shiftKey
                })

        Listener.useKeyPress
            [|
                "I"
                "H"
                "P"
                "B"
            |]
            (fun _ setter e ->
                promise {
                    let setView = Store.set setter Atoms.User.view

                    match e.ctrlKey, e.altKey, e.key with
                    | true, true, "I" ->
                        JS.log (fun () -> "RouterObserver.onKeyDown() View.Information")
                        setView View.View.Information
                    | true, true, "H" -> setView View.View.HabitTracker
                    | true, true, "P" -> setView View.View.Priority
                    | true, true, "B" -> setView View.View.BulletJournal
                    | _ -> ()
                })


        nothing


module SelectionListener =
    [<ReactComponent>]
    let SelectionListener () =
        Listener.useKeyPress
            [|
                "Escape"
            |]
            (fun getter setter e ->
                promise {
                    if e.key = "Escape" && e.``type`` = "keydown" then
                        let visibleTaskSelectedDateIdMap =
                            Store.value getter Selectors.Session.visibleTaskSelectedDateIdMap

                        if not visibleTaskSelectedDateIdMap.IsEmpty then
                            visibleTaskSelectedDateIdMap
                            |> Map.keys
                            |> Seq.iter (fun taskId -> Store.set setter (Atoms.Task.selectionSet taskId) Set.empty)
                })

        nothing
