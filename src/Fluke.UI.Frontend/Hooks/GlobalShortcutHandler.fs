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
            let cellSelectionMap, setCellSelectionMap = Recoil.useState Recoil.Selectors.cellSelectionMap
            let ctrlPressed, setCtrlPressed = Recoil.useState Recoil.Atoms.ctrlPressed
            let shiftPressed, setShiftPressed = Recoil.useState Recoil.Atoms.shiftPressed

            let keyEvent (e: KeyboardEvent) =
                if e.ctrlKey <> ctrlPressed then
                    setCtrlPressed e.ctrlKey

                if e.shiftKey <> shiftPressed then
                    setShiftPressed e.shiftKey

                if not cellSelectionMap.IsEmpty then
                    if e.key = "Escape" && e.``type`` = "keydown" then
                        setCellSelectionMap Map.empty

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

                        setCellSelectionMap newMap

            React.useListener.onKeyDown keyEvent
            React.useListener.onKeyUp keyEvent

            nothing)
