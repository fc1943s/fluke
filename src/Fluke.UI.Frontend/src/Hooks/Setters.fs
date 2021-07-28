namespace Fluke.UI.Frontend.Hooks

open Fluke.Shared
open Fluke.UI.Frontend
open Fluke.Shared.Domain
open FsStore


module Setters =
    open UserInteraction
    open State

    let useSetTaskSelectedDateIdMap () =
        Store.useCallback (
            (fun getter setter newSelection ->
                promise {
                    let sortedTaskIdArray = Store.value getter Selectors.Session.sortedTaskIdArray
                    let visibleTaskSelectedDateIdMap = Store.value getter Selectors.Session.visibleTaskSelectedDateIdMap

                    let operations =
                        sortedTaskIdArray
                        |> Array.collect
                            (fun taskId ->
                                let dates =
                                    visibleTaskSelectedDateIdMap
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let newDates =
                                    newSelection
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let deselect =
                                    newDates
                                    |> Set.difference dates
                                    |> Set.toArray
                                    |> Array.map (fun date -> taskId, date, false)

                                let select =
                                    dates
                                    |> Set.difference newDates
                                    |> Set.toArray
                                    |> Array.map (fun date -> taskId, date, true)

                                select |> Array.append deselect)

                    operations
                    |> Array.iter
                        (fun (taskId, dateId, selected) ->
                            Store.set setter (Selectors.Cell.selected (taskId, dateId)) selected)
                }),
            [||]
        )

    let useSetSelected () =
        let setTaskSelectedDateIdMap = useSetTaskSelectedDateIdMap ()

        Store.useCallback (
            (fun getter _ (taskId, dateId, newValue) ->
                promise {
                    let ctrlPressed = Store.value getter Atoms.Session.ctrlPressed
                    let shiftPressed = Store.value getter Atoms.Session.shiftPressed

                    let! newCellSelectionMap =
                        match shiftPressed, ctrlPressed with
                        | false, false ->
                            let newTaskSelection = if newValue then Set.singleton dateId else Set.empty

                            [
                                taskId, newTaskSelection
                            ]
                            |> Map.ofSeq
                            |> Promise.lift
                        | false, true ->
                            promise {
                                let swapSelection oldSelection taskId dateId =
                                    let oldSet =
                                        oldSelection
                                        |> Map.tryFind taskId
                                        |> Option.defaultValue Set.empty

                                    let newSet =
                                        let fn = if newValue then Set.add else Set.remove

                                        fn dateId oldSet

                                    oldSelection |> Map.add taskId newSet

                                let oldSelection = Store.value getter Selectors.Session.visibleTaskSelectedDateIdMap

                                return swapSelection oldSelection taskId dateId
                            }
                        | true, _ ->
                            promise {
                                let sortedTaskIdArray = Store.value getter Selectors.Session.sortedTaskIdArray

                                let visibleTaskSelectedDateIdMap =
                                    Store.value getter Selectors.Session.visibleTaskSelectedDateIdMap

                                let initialTaskIdSet =
                                    visibleTaskSelectedDateIdMap
                                    |> Map.toSeq
                                    |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                                    |> Seq.map fst
                                    |> Set.ofSeq
                                    |> Set.add taskId

                                let newTaskIdArray =
                                    sortedTaskIdArray
                                    |> Array.skipWhile (initialTaskIdSet.Contains >> not)
                                    |> Array.rev
                                    |> Array.skipWhile (initialTaskIdSet.Contains >> not)
                                    |> Array.rev

                                let initialDateList =
                                    visibleTaskSelectedDateIdMap
                                    |> Map.values
                                    |> Set.unionMany
                                    |> Set.add dateId
                                    |> Set.toList
                                    |> List.choose DateId.Value
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
                                        |> List.map DateId
                                    |> Set.ofSeq

                                let newMap =
                                    newTaskIdArray
                                    |> Array.map (fun taskId -> taskId, dateSet)
                                    |> Map.ofSeq

                                return newMap
                            }

                    do! setTaskSelectedDateIdMap newCellSelectionMap
                }),
            [|
                box setTaskSelectedDateIdMap
            |]
        )
