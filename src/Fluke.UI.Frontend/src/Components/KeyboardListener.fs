namespace Fluke.UI.Frontend.Components

open FsStore.State
open FsCore
open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.Shared
open FsStore
open FsStore.Hooks
open FsUi.Hooks


module CtrlListener =
    [<ReactComponent>]
    let CtrlListener () =
        Listener.useKeyPress
            [|
                "Control"
            |]
            (fun _getter setter e ->
                promise {
                    //                    let ctrlPressed = Atom.get getter Atoms.ctrlPressed
//
//                    if e.ctrlKey <> ctrlPressed then
                    Atom.set setter Atoms.Session.ctrlPressed e.ctrlKey
                })

        nothing


module ShiftListener =
    [<ReactComponent>]
    let ShiftListener () =
        let logger = Store.useValue Selectors.logger

        Listener.useKeyPress
            [|
                "Shift"
            |]
            (fun _getter setter e ->
                promise {
                    //                    let shiftPressed = Atom.get getter Atoms.shiftPressed
//
//                    if e.shiftKey <> shiftPressed then
                    Atom.set setter Atoms.Session.shiftPressed e.shiftKey
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
                    let setView = Atom.set setter Atoms.User.view

                    match e.ctrlKey, e.altKey, e.key with
                    | true, true, "I" ->
                        logger.Debug (fun () -> "RouterObserver.onKeyDown() View.Information") getLocals
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
                        let visibleTaskSelectedDateMap = Atom.get getter Selectors.Session.visibleTaskSelectedDateMap

                        if not visibleTaskSelectedDateMap.IsEmpty then
                            visibleTaskSelectedDateMap
                            |> Map.keys
                            |> Seq.iter (fun taskId -> Atom.set setter (Atoms.Task.selectionSet taskId) Set.empty)
                })

        nothing
