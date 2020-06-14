namespace Fluke.UI.Frontend

open Browser.Types
open FSharpPlus
open Fable.Core
open Feliz
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.React
open Fable.React.Props
open Fable.DateFunctions
open Fulma
open System
open Suigetsu.UI.Frontend.ElmishBridge
open Suigetsu.UI.Frontend.React
open Suigetsu.Core


module Recoil =
    open Model

    module Temp =
        type TempDataType =
            | TempPrivate
            | TempPublic
            | Test
        let view = View.Calendar
//        let view = View.Groups
//        let view = View.Tasks

        let tempDataType = TempPrivate
//        let tempDataType = Test
//        let tempDataType = TempPublic

        let tempState =
            let testData = TempData.tempData.RenderLaneTests
//            let testData = TempData.tempData.SortLanesTests

            let getNow =
                match tempDataType with
                | TempPrivate -> TempData.getNow
                | TempPublic  -> TempData.getNow
                | Test        -> testData.GetNow

            let dayStart =
                match tempDataType with
                | TempPrivate -> PrivateData.PrivateData.dayStart
                | TempPublic  -> TempData.dayStart
                | Test        -> TempData.testDayStart

            let informationList =
                match tempDataType with
                | TempPrivate -> PrivateData.Tasks.tempManualTasks.InformationList
                | TempPublic  -> TempData.tempData.ManualTasks.InformationList
                | Test        -> []

            let taskOrderList =
                match tempDataType with
                | TempPrivate -> PrivateData.Tasks.tempManualTasks.TaskOrderList @ PrivateData.Tasks.taskOrderList
                | TempPublic  -> TempData.tempData.ManualTasks.TaskOrderList
                | Test        -> testData.TaskOrderList

            let informationCommentsMap =
                match tempDataType with
                | TempPrivate ->
                    PrivateData.InformationComments.informationComments
                    |> List.append SharedPrivateData.Data.informationComments
                    |> List.groupBy (fun x -> x.Information)
                    |> Map.ofList
                    |> Map.mapValues (List.map (fun x -> x.Comment))
                | TempPublic  -> Map.empty
                | Test        -> Map.empty

            let taskStateList =
                match tempDataType with
                | TempPrivate ->
                    let taskData = PrivateData.Tasks.tempManualTasks
                    let sharedTaskData = SharedPrivateData.SharedTasks.tempManualTasks

                    let cellComments =
                        PrivateData.Journal.journalComments
                        |> List.append PrivateData.CellComments.cellComments
                        |> List.append SharedPrivateData.Data.cellComments

                    let applyState statusEntries comments (taskState: TaskState) =
                        { taskState with
                            StatusEntries =
                                statusEntries
                                |> createTaskStatusEntries taskState.Task
                                |> List.prepend taskState.StatusEntries
                            Comments =
                                comments
                                |> List.filter (fun (TaskComment (task, _)) -> task = taskState.Task)
                                |> List.map (ofTaskComment >> snd)
                                |> List.prepend taskState.Comments
                            CellCommentsMap =
                                cellComments
                                |> List.filter (fun (CellComment (address, _)) -> address.Task = taskState.Task)
                                |> List.map (fun (CellComment (address, comment)) -> address.Date, comment)
                                |> List.groupBy fst
                                |> Map.ofList
                                |> Map.mapValues (List.map snd)
                                |> Map.union taskState.CellCommentsMap }

                    let taskStateList =
                        taskData.TaskStateList
                        |> List.map (applyState
                                         PrivateData.CellStatusEntries.cellStatusEntries
                                         PrivateData.TaskComments.taskComments)

                    let sharedTaskStateList =
                        sharedTaskData.TaskStateList
                        |> List.map (applyState
                                         SharedPrivateData.Data.cellStatusEntries
                                         SharedPrivateData.Data.taskComments)

                    taskStateList |> List.append sharedTaskStateList
                | TempPublic  -> TempData.tempData.ManualTasks.TaskStateList
                | Test        -> testData.TaskStateList

            let taskStateList =
                taskStateList
//                |> List.sortByDescending (fun x -> x.StatusEntries.Length)
//                |> List.take 30

            let taskStateMap =
                taskStateList
                |> List.map (fun taskState -> taskState.Task, taskState)
                |> Map.ofList

            let lastSessions =
                taskStateList
                |> Seq.filter (fun taskState -> not taskState.Sessions.IsEmpty)
                |> Seq.map (fun taskState -> taskState.Task, taskState.Sessions)
                |> Seq.map (Tuple2.mapSnd (fun sessions ->
                    sessions
                    |> Seq.sortByDescending (fun (TaskSession start) -> start.DateTime)
                    |> Seq.head
                ))
                |> Seq.toList

            {| GetNow = getNow
               DayStart = dayStart
               InformationCommentsMap = informationCommentsMap
               InformationList = informationList
               TaskOrderList = taskOrderList
               TaskStateList = taskStateList
               LastSessions = lastSessions
               TaskStateMap = taskStateMap |}


    module Atoms =
        let rec getNow = atom {
            key (nameof getNow)
            def Temp.tempState.GetNow
        }
        let rec dayStart = atom {
            key (nameof dayStart)
            def Temp.tempState.DayStart
        }
        let rec now = atom {
            key (nameof now)
            def (flukeDateTime 0000 Month.January 01 00 00)
        }
        let rec view = atom {
            key (nameof view)
            def Temp.view
        }
        let rec ctrlPressed = atom {
            key (nameof ctrlPressed)
            def false
        }
        let rec hovered = atom {
            key (nameof hovered)
            def Hover.None
        }
        let rec activeSessions = atom {
            key (nameof activeSessions)
            def ([] : ActiveSession list)
        }
        let rec selection = atom {
            key (nameof selection)
            def (Map.empty : Map<Task, Set<FlukeDate>>)
        }
//        let rec cells = atom {
//            key (nameof cells)
//            def (Map.empty : Map<Task, Map<FlukeDate, {| Status: CellStatus
//                                                         Selected: bool |}>>)
//        }
        let rec taskStateList = selector {
            key (nameof taskStateList)
            get (fun _getter -> async {
                return Temp.tempState.TaskStateList
            })
        }
        let rec informationList = selector {
            key (nameof informationList)
            get (fun _getter -> async {
                return Temp.tempState.InformationList
            })
        }
        let rec taskOrderList = selector {
            key (nameof taskOrderList)
            get (fun _getter -> async {
                return Temp.tempState.TaskOrderList
            })
        }
        let rec lastSessions = selector {
            key (nameof lastSessions)
            get (fun _getter -> async {
                return Temp.tempState.LastSessions
            })
        }
//        let rec dingsFamily = atomFamily {
//            key (nameof dingsFamily)
//            def (fun (_date: FlukeDateTime) -> false)
//        }
        let rec dateSequence = selector {
            key (nameof dateSequence)
            get (fun getter ->
                let now = getter.get now

                [ now.Date ]
                |> Rendering.getDateSequence (45, 20)
            )
        }
        let rec informationCommentsFamily = selectorFamily {
            key (nameof informationCommentsFamily)
            get (fun (information: Information) _getter -> async {
                return Temp.tempState.InformationCommentsMap
                |> Map.tryFind information
                |> Option.defaultValue []
            })
        }
        let rec taskStateFamily = selectorFamily {
            key (nameof taskStateFamily)
            get (fun (task: Task) _getter -> async {
                return Temp.tempState.TaskStateMap.[task]
            })
        }
        let rec cellSelectedFamily = selectorFamily {
            key (nameof cellSelectedFamily)
            get (fun (cellAddress: CellAddress) getter ->
                let selection = getter.get selection
                selection
                |> Map.tryFind cellAddress.Task
                |> Option.defaultValue Set.empty
                |> Set.contains cellAddress.Date
            )
            set (fun (cellAddress: CellAddress) setter (newValue: bool) ->
                let oldSelection = setter.get selection
                let ctrlPressed = setter.get ctrlPressed

                let newSelection =
                    match ctrlPressed with
                    | false ->
                        let selection =
                            match newValue with
                            | true -> cellAddress.Date |> Set.singleton
                            | false -> Set.empty
                        Map.empty
                        |> Map.add cellAddress.Task selection
                    | true ->
                        let selection =
                            oldSelection
                            |> Map.tryFind cellAddress.Task
                            |> Option.defaultValue Set.empty
                            |> fun oldSet ->
                                match newValue with
                                | true -> oldSet |> Set.add cellAddress.Date
                                | false -> oldSet |> Set.remove cellAddress.Date
                        oldSelection
                        |> Map.add cellAddress.Task selection

                setter.set (selection, newSelection)
            )
        }
        let rec cellCommentsFamily = selectorFamily {
            key (nameof cellCommentsFamily)
            get (fun (address: CellAddress) getter ->
                let taskState = getter.get (taskStateFamily address.Task)
                taskState.CellCommentsMap
                |> Map.tryFind address.Date
                |> Option.defaultValue []
            )
        }
        let rec cellSessionsFamily = selectorFamily {
            key (nameof cellSessionsFamily)
            get (fun (address: CellAddress) getter ->
                let dayStart = getter.get dayStart
                let taskState = getter.get (taskStateFamily address.Task)

                taskState.Sessions
                |> List.filter (fun (TaskSession start) -> isToday dayStart start address.Date)
            )
        }
        let rec laneFamily = selectorFamily {
            key (nameof laneFamily)
            get (fun (task: Task) getter ->
                let dayStart = getter.get dayStart
                let now = getter.get now
                let dateSequence = getter.get dateSequence
                let taskState = getter.get (taskStateFamily task)
                Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
            )
        }
        let rec laneMapFamily = selectorFamily {
            key (nameof laneMapFamily)
            get (fun (task: Task) getter ->
                let (Lane (_, cells)) = getter.get (laneFamily task)
                cells
                |> List.map (fun (Cell (address, status)) ->
                    address.Date, status
                )
                |> Map.ofList
            )
        }
        let rec cellStatusFamily = selectorFamily {
            key (nameof cellStatusFamily)
            get (fun (address: CellAddress) getter ->
                let laneMap = getter.get (laneMapFamily address.Task)
                laneMap.[address.Date]
            )
        }
        let rec isTodayFamily = selectorFamily {
            key (nameof isTodayFamily)
            get (fun (date: FlukeDate) getter ->
                let dayStart = getter.get dayStart
                let now = getter.get now
                isToday dayStart now date
            )
        }
//        let taskCellsSelector = selectorFamily {
//            key "fluke/laneStateSelector"
//            get (fun (task: Task) getter ->
//                let dayStart = getter.get dayStart
//                let now = getter.get now
//                let taskState = getter.get (taskStateFamily task)
//                ()
//            )
//        }
//        let row =
//            selectorFamily {
//                key "__datasheet__/getRow"
//                get (fun (row: int, col:int) getter ->
//                    getter.get(findCell(row,col)).row
//                    |> getter.get)
//                set (fun (row: int, col:int) setter (newValue: int) ->
//                    let row = setter.get(findCell(row,col)).row
//                    setter.set(row, newValue))
//            }

//        type RecoilTask =
//            { Name: RecoilValue<string, ReadWrite>
//              Information: RecoilValue<Information, ReadWrite>
//              Scheduling: RecoilValue<TaskScheduling, ReadWrite>
//              PendingAfter: RecoilValue<FlukeTime option, ReadWrite>
//              MissedAfter: RecoilValue<FlukeTime option, ReadWrite>
//              Duration: RecoilValue<int option, ReadWrite> }
//
//            static member NameFamily = atomFamily {
//                key "fluke/task/nameFamily"
//                def (fun (_information: Information, taskName: string) -> taskName)
//            }
//            static member InformationFamily = atomFamily {
//                key "fluke/task/informationFamily"
//                def (fun (information: Information, _taskName: string) -> information)
//            }
//            static member SchedulingFamily = atomFamily {
//                key "fluke/task/schedulingFamily"
//                def (fun (_information: Information, _taskName: string) -> Manual WithoutSuggestion)
//            }
//            static member PendingAfterFamily = atomFamily {
//                key "fluke/task/pendingAfterFamily"
//                def (fun (_information: Information, _taskName: string) -> None)
//            }
//            static member MissedAfterFamily = atomFamily {
//                key "fluke/task/missedAfterFamily"
//                def (fun (_information: Information, _taskName: string) -> None)
//            }
//            static member DurationFamily = atomFamily {
//                key "fluke/task/durationFamily"
//                def (fun (_information: Information, _taskName: string) -> None)
//            }
//            static member Create information taskName =
//                { Name = RecoilTask.NameFamily (information, taskName)
//                  Information = RecoilTask.InformationFamily (information, taskName)
//                  Scheduling = RecoilTask.SchedulingFamily (information, taskName)
//                  PendingAfter = RecoilTask.PendingAfterFamily (information, taskName)
//                  MissedAfter = RecoilTask.MissedAfterFamily (information, taskName)
//                  Duration = RecoilTask.DurationFamily (information, taskName) }
//
//        let taskFamily = atomFamily {
//            key "fluke/task"
//            def (fun (information: Information, taskName: string) -> RecoilTask.Create information taskName)
//        }

//        let date = atom {
//            key "fluke/date"
//            def (flukeDate 0000 Month.January 01)
//        }

//        let cellAddress = atom {
//            key "fluke/cellAddress"
//        }

//        type RecoilCell =
//            { Address: RecoilValue<CellAddress, ReadWrite>
//              Comments: RecoilValue<Comment list, ReadWrite>
//              Sessions: RecoilValue<TaskSession list, ReadWrite>
//              Status: RecoilValue<CellStatus, ReadWrite>
//              Selected: RecoilValue<bool, ReadWrite>
//              IsSelected: RecoilValue<bool, ReadWrite>
//              IsToday: RecoilValue<bool, ReadWrite> }
