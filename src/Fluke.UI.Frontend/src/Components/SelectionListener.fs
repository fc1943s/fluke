namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.Shared
open Fluke.UI.Frontend.Bindings


module SelectionListener =

    [<ReactComponent>]
    let SelectionListener () =
        Listener.useKeyPress
            [|
                "Escape"
                "R"
            |]
            (fun getter setter e ->
                promise {
                    let cellSelectionMap = Store.value getter Selectors.Session.cellSelectionMap

                    if e.key = "Escape" && e.``type`` = "keydown" then
                        if not cellSelectionMap.IsEmpty then
                            cellSelectionMap
                            |> Map.keys
                            |> Seq.iter
                                (fun taskId -> Store.set setter (Atoms.Task.selectionSet taskId) Set.empty)

                    if e.key = "R" && e.``type`` = "keydown" then
                        if not cellSelectionMap.IsEmpty then
                            let newMap =
                                if cellSelectionMap.Count = 1 then
                                    cellSelectionMap
                                    |> Map.toList
                                    |> List.map
                                        (fun (taskId, dates) ->
                                            let date =
                                                dates
                                                |> Seq.item (Random().Next (0, dates.Count - 1))

                                            taskId, Set.singleton date)
                                    |> Map.ofSeq
                                else
                                    let key =
                                        cellSelectionMap
                                        |> Map.keys
                                        |> Seq.item (Random().Next (0, cellSelectionMap.Count - 1))

                                    Map.singleton key cellSelectionMap.[key]

                            newMap
                            |> Map.iter
                                (fun taskId dates ->
                                    Store.set setter (Atoms.Task.selectionSet taskId) (dates |> Set.map DateId))
                })

        nothing
