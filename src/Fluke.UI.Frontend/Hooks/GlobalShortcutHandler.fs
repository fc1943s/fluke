namespace Fluke.UI.Frontend.Hooks

open System
open Browser.Types
open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module GlobalShortcutHandler =
    let hook =
        React.memo (fun () ->
            Profiling.addTimestamp "GlobalShortcutHandler.render"
            let keyEvent =
                Recoil.useCallbackRef (fun setter (e: KeyboardEvent) ->
                    async {
                        let! cellSelectionMap = setter.snapshot.getAsync Recoil.Selectors.cellSelectionMap
                        let! ctrlPressed = setter.snapshot.getAsync Recoil.Atoms.ctrlPressed
                        let! shiftPressed = setter.snapshot.getAsync Recoil.Atoms.shiftPressed

                        if e.ctrlKey <> ctrlPressed then
                            setter.set (Recoil.Atoms.ctrlPressed, e.ctrlKey)

                        if e.shiftKey <> shiftPressed then
                            setter.set (Recoil.Atoms.shiftPressed, e.shiftKey)

                        if not cellSelectionMap.IsEmpty then
                            if e.key = "Escape" && e.``type`` = "keydown" then
                                setter.set (Recoil.Selectors.cellSelectionMap, Map.empty)

                            if e.key = "R" && e.``type`` = "keydown" then
                                let newMap =
                                    if cellSelectionMap.Count = 1 then
                                        cellSelectionMap
                                        |> Map.toList
                                        |> List.map (fun (taskId, dates) ->
                                            let date =
                                                dates
                                                |> Seq.item (Random().Next(0, dates.Count - 1))

                                            taskId, Set.singleton date)
                                        |> Map.ofList
                                    else
                                        let key =
                                            cellSelectionMap
                                            |> Map.keys
                                            |> Seq.item (Random().Next(0, cellSelectionMap.Count - 1))

                                        Map.singleton key cellSelectionMap.[key]

                                setter.set (Recoil.Selectors.cellSelectionMap, newMap)
                    }
                    |> Async.StartImmediate)

            React.useListener.onKeyDown keyEvent
            React.useListener.onKeyUp keyEvent

            nothing)
