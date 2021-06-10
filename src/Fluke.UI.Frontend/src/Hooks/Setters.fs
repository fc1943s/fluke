namespace Fluke.UI.Frontend.Hooks

open Feliz
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module Setters =
    open UserInteraction
    open State

    let useSetCellSelectionMap () =
        Recoil.useCallbackRef
            (fun setter (username, newSelection) ->
                promise {
                    let! sortedTaskIdList = setter.snapshot.getPromise (Selectors.Session.sortedTaskIdList username)
                    let! cellSelectionMap = setter.snapshot.getPromise (Selectors.Session.cellSelectionMap username)

                    let operations =
                        sortedTaskIdList
                        |> List.collect
                            (fun taskId ->
                                let dates =
                                    cellSelectionMap
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let newDates =
                                    newSelection
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let deselect =
                                    newDates
                                    |> Set.difference dates
                                    |> Set.toList
                                    |> List.map (fun date -> taskId, date, false)

                                let select =
                                    dates
                                    |> Set.difference newDates
                                    |> Set.toList
                                    |> List.map (fun date -> taskId, date, true)

                                deselect @ select)

                    operations
                    |> List.iter
                        (fun (taskId, date, selected) ->
                            setter.set (Selectors.Cell.selected (username, taskId, DateId date), selected))
                })

    let useSetSelected () =
        let setCellSelectionMap = useSetCellSelectionMap ()

        Recoil.useCallbackRef
            (fun setter (username, taskId, dateId, newValue) ->
                promise {
                    let! ctrlPressed = setter.snapshot.getPromise Atoms.ctrlPressed
                    let! shiftPressed = setter.snapshot.getPromise Atoms.shiftPressed

                    let! newCellSelectionMap =
                        match shiftPressed, ctrlPressed with
                        | false, false ->
                            let newTaskSelection =
                                if newValue then Set.singleton (dateId |> DateId.Value) else Set.empty

                            [
                                taskId, newTaskSelection
                            ]
                            |> Map.ofList
                            |> Promise.lift
                        | false, true ->
                            promise {
                                let swapSelection oldSelection taskId date =
                                    let oldSet =
                                        oldSelection
                                        |> Map.tryFind taskId
                                        |> Option.defaultValue Set.empty

                                    let newSet =
                                        let fn = if newValue then Set.add else Set.remove

                                        fn date oldSet

                                    oldSelection |> Map.add taskId newSet

                                let! oldSelection =
                                    setter.snapshot.getPromise (Selectors.Session.cellSelectionMap username)

                                return swapSelection oldSelection taskId (dateId |> DateId.Value)
                            }
                        | true, _ ->
                            promise {
                                let! sortedTaskIdList =
                                    setter.snapshot.getPromise (Selectors.Session.sortedTaskIdList username)

                                let! oldCellSelectionMap =
                                    setter.snapshot.getPromise (Selectors.Session.cellSelectionMap username)

                                let initialTaskIdSet =
                                    oldCellSelectionMap
                                    |> Map.toSeq
                                    |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                                    |> Seq.map fst
                                    |> Set.ofSeq
                                    |> Set.add taskId

                                let newTaskIdList =
                                    sortedTaskIdList
                                    |> List.skipWhile (initialTaskIdSet.Contains >> not)
                                    |> List.rev
                                    |> List.skipWhile (initialTaskIdSet.Contains >> not)
                                    |> List.rev

                                let initialDateList =
                                    oldCellSelectionMap
                                    |> Map.values
                                    |> Set.unionMany
                                    |> Set.add (dateId |> DateId.Value)
                                    |> Set.toList
                                    |> List.sort

                                let dateSet =
                                    match initialDateList with
                                    | [] -> []
                                    | dateList ->
                                        [
                                            dateList.Head
                                            dateList |> List.last
                                        ]
                                        |> Rendering.getDateSequence (0, 0)
                                    |> Set.ofList

                                let newMap =
                                    newTaskIdList
                                    |> List.map (fun taskId -> taskId, dateSet)
                                    |> Map.ofList

                                return newMap
                            }


                    do! setCellSelectionMap (username, newCellSelectionMap)
                })
