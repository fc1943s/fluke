namespace Fluke.UI.Frontend.State.Selectors

open FsJs
open System
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open FsStore

open FsStore.Bindings


module rec BulletJournalView =
    let rec bulletJournalWeekCellsMap =
        Store.readSelectorInterval
            Fluke.root
            (nameof bulletJournalWeekCellsMap)
            Selectors.interval
            []
            (fun getter ->
                let position = Store.value getter Atoms.Session.position
                let sortedTaskIdAtoms = Store.value getter Session.sortedTaskIdAtoms

                let sortedTaskIdArray =
                    sortedTaskIdAtoms
                    |> Store.waitForAll
                    |> Store.value getter

                let taskStateArray =
                    sortedTaskIdArray
                    |> Array.map Task.taskState
                    |> Store.waitForAll
                    |> Store.value getter

                let taskStateMap =
                    sortedTaskIdArray
                    |> Array.mapi (fun i taskId -> taskId, taskStateArray.[i])
                    |> Map.ofSeq

                let taskIdAtomMap =
                    sortedTaskIdArray
                    |> Array.mapi (fun i taskId -> taskId, sortedTaskIdAtoms.[i])
                    |> Map.ofSeq

                match position with
                | Some position ->
                    let dayStart = Store.value getter Atoms.User.dayStart
                    let weekStart = Store.value getter Atoms.User.weekStart

                    let weeks =
                        [
                            -1 .. 1
                        ]
                        |> List.map
                            (fun weekOffset ->
                                let dateIdSequence =
                                    let rec getWeekStart (date: DateTime) =
                                        if date.DayOfWeek = weekStart then
                                            date
                                        else
                                            date |> DateTime.addDays -1 |> getWeekStart

                                    let startDate =
                                        dateId dayStart position
                                        |> fun (DateId referenceDay) ->
                                            referenceDay
                                            |> FlukeDate.DateTime
                                            |> DateTime.addDays (7 * weekOffset)
                                        |> getWeekStart

                                    [
                                        0 .. 6
                                    ]
                                    |> List.map (fun days -> startDate |> DateTime.addDays days)
                                    |> List.map FlukeDateTime.FromDateTime
                                    |> List.map (dateId dayStart)

                                let result =
                                    sortedTaskIdArray
                                    |> Array.collect
                                        (fun taskId ->
                                            dateIdSequence
                                            |> List.map
                                                (fun dateId ->
                                                    let isToday = isToday dayStart position dateId

                                                    let cellState =
                                                        taskStateMap
                                                        |> Map.tryFind taskId
                                                        |> Option.bind
                                                            (fun taskState ->
                                                                taskState.CellStateMap |> Map.tryFind dateId)
                                                        |> Option.defaultValue CellState.Default

                                                    {|
                                                        DateId = dateId
                                                        TaskId = taskId
                                                        DateIdAtom = Jotai.jotai.atom dateId
                                                        TaskIdAtom = taskIdAtomMap.[taskId]
                                                        Status = cellState.Status
                                                        SessionList = cellState.SessionList
                                                        IsToday = isToday
                                                        AttachmentStateList = cellState.AttachmentStateList
                                                    |})
                                            |> List.toArray)
                                    |> Array.groupBy (fun x -> x.DateId)
                                    |> Array.map
                                        (fun (dateId, cellsMetadata) ->
                                            match dateId with
                                            | DateId referenceDay as dateId ->
                                                //                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList
                                                let taskSessionList =
                                                    cellsMetadata
                                                    |> Array.toList
                                                    |> List.collect (fun x -> x.SessionList)

                                                let sortedTasksMap =
                                                    cellsMetadata
                                                    |> Array.map
                                                        (fun cellMetadata ->
                                                            let taskState =
                                                                { taskStateMap.[cellMetadata.TaskId] with
                                                                    SessionList = taskSessionList
                                                                }

                                                            taskState,
                                                            [
                                                                dateId, cellMetadata.Status
                                                            ]
                                                            |> Map.ofSeq)
                                                    |> Array.toList
                                                    |> Sorting.sortLanesByTimeOfDay
                                                        dayStart
                                                        (FlukeDateTime.Create (referenceDay, dayStart, Second 0))
                                                    |> List.indexed
                                                    |> List.map (fun (i, (taskState, _)) -> taskState.Task.Id, i)
                                                    |> Map.ofSeq

                                                let newCells =
                                                    cellsMetadata
                                                    |> Array.sortBy
                                                        (fun cell ->
                                                            sortedTasksMap
                                                            |> Map.tryFind cell.TaskId
                                                            |> Option.defaultValue -1)

                                                dateId, newCells)
                                    |> Map.ofSeq

                                result)

                    weeks
                | _ -> [])
