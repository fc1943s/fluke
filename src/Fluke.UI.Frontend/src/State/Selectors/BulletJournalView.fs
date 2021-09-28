namespace Fluke.UI.Frontend.State.Selectors

open FsJs
open System
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open FsStore

open FsStore.Bindings
open FsStore.Model


module BulletJournalView =
    let inline readSelector name defaultValue read =
        let wrapper = Atom.readSelector (StoreAtomPath.RootAtomPath (Fluke.root, AtomName name)) read

        match defaultValue with
        | None -> wrapper
        | Some _defaultValue -> wrapper
    //            |> Engine.wrapAtomWithInterval defaultValue Selectors.interval

    let rec bulletJournalWeekCellsMap =
        readSelector
            (nameof bulletJournalWeekCellsMap)
            (Some [])
            (fun getter ->
                let position = Atom.get getter Atoms.Session.position
                let sortedTaskIdAtoms = Atom.get getter Session.sortedTaskIdAtoms

                let sortedTaskIdArray =
                    sortedTaskIdAtoms
                    |> Atom.waitForAll
                    |> Atom.get getter

                let taskStateArray =
                    sortedTaskIdArray
                    |> Array.map Task.taskState
                    |> Atom.waitForAll
                    |> Atom.get getter

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
                    let dayStart = Atom.get getter Atoms.User.dayStart
                    let weekStart = Atom.get getter Atoms.User.weekStart

                    let weeks =
                        [
                            -1 .. 1
                        ]
                        |> List.map
                            (fun weekOffset ->
                                let dateSequence =
                                    let rec getWeekStart (date: DateTime) =
                                        if date.DayOfWeek = weekStart then
                                            date
                                        else
                                            date |> DateTime.addDays -1 |> getWeekStart

                                    let startDate =
                                        getReferenceDay dayStart position
                                        |> FlukeDate.DateTime
                                        |> DateTime.addDays (7 * weekOffset)
                                        |> getWeekStart

                                    [
                                        0 .. 6
                                    ]
                                    |> List.map (fun days -> startDate |> DateTime.addDays days)
                                    |> List.map FlukeDateTime.FromDateTime
                                    |> List.map (getReferenceDay dayStart)

                                let result =
                                    sortedTaskIdArray
                                    |> Array.collect
                                        (fun taskId ->
                                            dateSequence
                                            |> List.map
                                                (fun date ->
                                                    let isToday = isToday dayStart position date

                                                    let cellState =
                                                        taskStateMap
                                                        |> Map.tryFind taskId
                                                        |> Option.bind
                                                            (fun taskState ->
                                                                taskState.CellStateMap |> Map.tryFind date)
                                                        |> Option.defaultValue CellState.Default

                                                    {|
                                                        Date = date
                                                        TaskId = taskId
                                                        DateAtom = Jotai.jotai.atom date
                                                        TaskIdAtom = taskIdAtomMap.[taskId]
                                                        Status = cellState.Status
                                                        SessionList = cellState.SessionList
                                                        IsToday = isToday
                                                        AttachmentStateList = cellState.AttachmentStateList
                                                    |})
                                            |> List.toArray)
                                    |> Array.groupBy (fun x -> x.Date)
                                    |> Array.map
                                        (fun (date, cellsMetadata) ->
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
                                                            date, cellMetadata.Status
                                                        ]
                                                        |> Map.ofSeq)
                                                |> Array.toList
                                                |> Sorting.sortLanesByTimeOfDay
                                                    dayStart
                                                    (FlukeDateTime.Create (date, dayStart, Second 0))
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

                                            date, newCells)
                                    |> Map.ofSeq

                                result)

                    weeks
                | _ -> [])
