namespace Fluke.UI.Frontend

open System.Collections.Generic
open Browser.Types
open FSharpPlus
open Fable.Core
open Fable.Core.JsInterop
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

        let callCount = Dictionary<string, int>()
        Browser.Dom.window?callCount <- callCount
        let addCount id =
            if not (callCount.ContainsKey id) then
                callCount.[id] <- 0
            callCount.[id] <- callCount.[id] + 1
            mountById "diag" (str (Fable.SimpleJson.SimpleJson.stringify Browser.Dom.window?callCount))

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
                |> List.sortByDescending (fun x -> x.StatusEntries.Length)
                |> List.take 10

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

            printfn "RETURNING TEMPSTATE."
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
            def (Temp.tempState.GetNow ())
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

        module RecoilCell =
            type RecoilCell =
                { Status: RecoilValue<CellStatus, ReadWrite>
                  Comments: RecoilValue<Comment list, ReadWrite>
                  Sessions: RecoilValue<TaskSession list, ReadWrite>
                  Selected: RecoilValue<bool, ReadWrite> }
            let rec statusFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof statusFamily)
                def (fun (_task: Task, _date: FlukeDate) -> Disabled)
            }

            let rec commentsFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof commentsFamily)
                def (fun (_task: Task, _date: FlukeDate) -> [] : Comment list)
            }

            let rec sessionsFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof sessionsFamily)
                def (fun (_task: Task, _date: FlukeDate) -> [] : TaskSession list)
            }
            let rec selectedFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof selectedFamily)
                def (fun (_task: Task, _date: FlukeDate) -> false)
            }
            type RecoilCell with
                static member internal Create task date =
                    { Status = statusFamily (task, date)
                      Comments = commentsFamily (task, date)
                      Sessions = sessionsFamily (task, date)
                      Selected = selectedFamily (task, date) }
            let rec cellFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof cellFamily)
                def (fun (task: Task, date: FlukeDate) -> RecoilCell.Create task date)
            }

//        let rec dingsFamily = atomFamily {
//            key (nameof dingsFamily)
//            def (fun (_date: FlukeDateTime) -> false)
//        }
//        let rec selection = atom {
//            key (nameof selection)
//            def (Map.empty : Map<Task, Set<FlukeDate>>)
//        }
//        let rec taskSelectionFamily = selectorFamily {
//            key (nameof taskSelectionFamily)
//            get (fun (task: Task) getter ->
//                Temp.addCount "taskSelectionFamily"
//                let selection = getter.get selection
//
//                selection
//                |> Map.tryFind task
//                |> Option.defaultValue Set.empty
//            )
//        }
//        let rec cellSelectedFamily = selectorFamily {
//            key (nameof cellSelectedFamily)
//            get (fun (task: Task, date: FlukeDate) getter ->
//                Temp.addCount "cellSelectedFamily"
//                let taskSelection = getter.get (taskSelectionFamily task)
//
//                taskSelection |> Set.contains date
//            )
//            set (fun (task: Task, date: FlukeDate) setter (newValue:bool) ->
//                let ctrlPressed = setter.get ctrlPressed
//
//                let newSelection =
//                    match ctrlPressed with
//                    | false ->
//                        let selection =
//                            match newValue with
//                            | true -> date |> Set.singleton
//                            | false -> Set.empty
//                        Map.empty |> Map.add task selection
//                    | true ->
//                        let oldSelection = setter.get selection
//                        let selection =
//                            oldSelection
//                            |> Map.tryFind task
//                            |> Option.defaultValue Set.empty
//                            |> fun oldSet ->
//                                match newValue with
//                                | true -> oldSet |> Set.add date
//                                | false -> oldSet |> Set.remove date
//                        oldSelection |> Map.add task selection
//
//                setter.set (selection, newSelection)
//            )
//        }

    module Selectors =
        let rec dateSequence = selectorFamily {
            key (nameof dateSequence)
            get (fun (now: FlukeDateTime) getter ->
                Temp.addCount "dateSequence"

                printfn "DATESEQUENCE. NOW: %A" now


                [ now.Date ]
                |> Rendering.getDateSequence (45, 20)
            )
        }
        let rec dateRange = selector {
            key (nameof dateRange)
            get (fun getter ->
                Temp.addCount "dateRange"
                let now = getter.get Atoms.now
                let dateSequence = getter.get (dateSequence now)

                let head = dateSequence |> List.head |> fun x -> x.DateTime
                let last = dateSequence |> List.last |> fun x -> x.DateTime
                head, last
            )
        }
        let rec taskOrderList = selector {
            key (nameof taskOrderList)
            get (fun _getter -> async {
                Temp.addCount "taskOrderList"
                return Temp.tempState.TaskOrderList
            })
        }
        let rec informationList = selector {
            key (nameof informationList)
            get (fun _getter -> async {
                Temp.addCount "informationList"
                return Temp.tempState.InformationList
            })
        }
        let rec lastSessions = selector {
            key (nameof lastSessions)
            get (fun _getter -> async {
                Temp.addCount "lastSessions"
                return Temp.tempState.LastSessions
            })
        }
        let rec taskStateList = selector {
            key (nameof taskStateList)
            get (fun _getter -> async {
                Temp.addCount "taskStateList"
                return Temp.tempState.TaskStateList
            })
        }
        let rec filteredTaskStateList = selectorFamily {
            key (nameof filteredTaskStateList)
            get (fun (view: View) getter ->
                Temp.addCount "filteredTaskStateList"

                let taskStateList = getter.get taskStateList

                match view with
                | View.Calendar ->
                    let dateRange = getter.get dateRange
                    taskStateList
                    |> List.filter (function
                        | { Task = { Task.Scheduling = Manual WithoutSuggestion }
                            StatusEntries = statusEntries
                            Sessions = sessions }
                            when
                                statusEntries
                                |> List.exists (fun (TaskStatusEntry (date, _)) -> date.DateTime >==< dateRange)
                                |> not
                            &&
                                sessions
                                |> List.exists (fun (TaskSession start) -> start.Date.DateTime >==< dateRange)
                                |> not
                            -> false
                        | _ -> true
                    )
                | View.Groups ->
                    taskStateList
                    |> List.filter (function
                        | { Task = { Task.Scheduling = Manual WithoutSuggestion }
                            StatusEntries = []
                            Sessions = [] } -> true
                        | _ -> false
                    )
//                    |> List.filter (fun (_, statusEntries) ->
//                        statusEntries
//                        |> List.filter (function
//                            | { Cell = { Date = date } } when date.DateTime <= now.Date.DateTime -> true
//                            | _ -> false
//                        )
//                        |> List.tryLast
//                        |> function Some { Status = Dismissed } -> false | _ -> true
//                    )
                | View.Tasks ->
                    taskStateList
                    |> List.filter (function { Task = { Task.Scheduling = Manual _ }} -> true | _ -> false)
                | View.Week ->
                    taskStateList
            )
        }
        let rec taskStateFamily = selectorFamily {
            key (nameof taskStateFamily)
            get (fun (task: Task) _getter -> async {
                Temp.addCount "taskStateFamily"
                return Temp.tempState.TaskStateMap.[task]
            })
        }
        let rec laneFamily = selectorFamily {
            key (nameof laneFamily)
            get (fun (task: Task) getter ->
                Temp.addCount "laneFamily"
                let now = getter.get Atoms.now
                let dayStart = getter.get Atoms.dayStart
                let dateSequence = getter.get (dateSequence now)
                let taskState = getter.get (taskStateFamily task)
                Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
            )
        }
        let rec laneMapFamily = selectorFamily {
            key (nameof laneMapFamily)
            get (fun (task: Task) getter ->
                Temp.addCount "laneMapFamily"
                let (Lane (_, cells)) = getter.get (laneFamily task)
                cells
                |> List.map (fun (Cell (address, status)) ->
                    address.Date, status
                )
                |> Map.ofList
            )
        }
        let rec sortedLaneList = selectorFamily {
            key (nameof sortedLaneList)
            get (fun (view: View) getter ->
                Temp.addCount "sortedLaneList"
                let dayStart = getter.get Atoms.dayStart
                let now = getter.get Atoms.now
                let taskOrderList = getter.get taskOrderList
                let filteredTaskStateList = getter.get (filteredTaskStateList view)

                match view with
                | View.Calendar ->
                    filteredTaskStateList
                    |> List.map (fun taskState ->
                        getter.get (laneFamily taskState.Task)
                    )
                    |> Sorting.sortLanesByFrequency
                    |> Sorting.sortLanesByIncomingRecurrency dayStart now
                    |> Sorting.sortLanesByTimeOfDay dayStart now taskOrderList
                | View.Groups ->
                    let lanes =
                        filteredTaskStateList
                        |> List.map (fun taskState ->
                            getter.get (laneFamily taskState.Task)
                        )
                        |> Sorting.applyManualOrder taskOrderList

                    getter.get informationList
                    |> List.map (fun information ->
                        let lanes =
                            lanes
                            |> List.filter (fun (Lane (task, _)) -> task.Information = information)

                        information, lanes
                    )
                    |> List.collect snd
                | View.Tasks ->
                    filteredTaskStateList
                    |> List.map (fun taskState ->
                        getter.get (laneFamily taskState.Task)
                    )
                    |> Sorting.applyManualOrder taskOrderList
                    |> List.sortByDescending (fun (Lane (task, _)) ->
                        let taskState = Recoil.useValue (taskStateFamily task)

                        taskState.PriorityValue
                        |> Option.map ofTaskPriorityValue
                        |> Option.defaultValue 0
                    )
                | View.Week ->
                    []
            )
        }
        type CellId = CellId of ticks:int64 * informationName:string * taskName:string
        let rec cellIdFamily = selectorFamily {
            key (nameof cellIdFamily)
            get (fun (address: CellAddress) _getter ->
                Temp.addCount "cellIdFamily"
                CellId (address.Date.DateTime.Ticks, address.Task.Information.Name, address.Task.Name)
            )
        }
        let rec isTodayFamily = selectorFamily {
            key (nameof isTodayFamily)
            get (fun (date: FlukeDate) getter ->
                Temp.addCount "isTodayFamily"
                let dayStart = getter.get Atoms.dayStart
                let now = getter.get Atoms.now
                isToday dayStart now date
            )
        }
        let rec findCell = selectorFamily {
            key (nameof findCell)
            get (fun (task: Task, date: FlukeDate) getter ->
                Temp.addCount "findCell"
                getter.get (Atoms.RecoilCell.cellFamily (task, date))
            )
//            set (fun (task: Task, date: FlukeDate) setter (newCell: RecoilCell) ->
//                setter.set(cellFamily (task, date), newCell))
        }
        let rec selectionTracker = selector {
            key (nameof selectionTracker)
            get (fun getter ->
                Temp.addCount "selectionTracker"
                getter.get Atoms.selection
            )
            set (fun setter (newValue: Map<Task, Set<FlukeDate>>) ->
                Temp.addCount "selectionTracker (SET)"
                setter.get Atoms.selection
                |> Seq.iter (fun (KeyValue (task, dates)) ->
                    dates
                    |> Seq.iter (fun date ->
                        let selected = setter.get(findCell (task, date)).Selected
                        setter.set (selected, false)
                    )
                )

                newValue
                |> Seq.iter (fun (KeyValue (task, dates)) ->
                    dates
                    |> Seq.iter (fun date ->
                        let selected = setter.get(findCell (task, date)).Selected
                        setter.set (selected, true)
                    )
                )

                setter.set (Atoms.selection, newValue)
            )
        }

        module rec RecoilInformation =
            let rec comments = selectorFamily {
                key (nameof RecoilInformation + "/" + nameof comments)
                get (fun (information: Information) _getter -> async {
                    Temp.addCount (nameof RecoilInformation + "/" + nameof comments)
                    return Temp.tempState.InformationCommentsMap
                    |> Map.tryFind information
                    |> Option.defaultValue []
                })
            }

        module rec RecoilCell =
            let rec status = selectorFamily {
                key (nameof RecoilCell + "/" + nameof status)
                get (fun (task: Task, date: FlukeDate) getter ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof status)
                    getter.get(findCell (task, date)).Status |> getter.get
                )
            }
            let rec comments = selectorFamily {
                key (nameof RecoilCell + "/" + nameof comments)
                get (fun (task: Task, date: FlukeDate) getter ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof comments)
                    getter.get(findCell (task, date)).Comments |> getter.get
                )
            }
            let rec sessions = selectorFamily {
                key (nameof RecoilCell + "/" + nameof sessions)
                get (fun (task: Task, date: FlukeDate) getter ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof sessions)
                    getter.get(findCell (task, date)).Sessions |> getter.get
                )
            }
            let rec selected = selectorFamily {
                key (nameof RecoilCell + "/" + nameof selected)
                get (fun (task: Task, date: FlukeDate) getter ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof selected)
                    getter.get(findCell (task, date)).Selected |> getter.get
                )
                set (fun (task: Task, date: FlukeDate) setter (newValue: bool) ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof selected + " (SET)")
                    let ctrlPressed = setter.get Atoms.ctrlPressed

                    let newSelection =
                        match ctrlPressed with
                        | false ->
                            let newTaskSelection =
                                match newValue with
                                | true -> date |> Set.singleton
                                | false -> Set.empty
                            Map.empty |> Map.add task newTaskSelection
                        | true ->
                            let oldSelection = setter.get Atoms.selection
                            let newTaskSelection =
                                oldSelection
                                |> Map.tryFind task
                                |> Option.defaultValue Set.empty
                                |> fun oldSet ->
                                    match newValue with
                                    | true -> oldSet |> Set.add date
                                    | false -> oldSet |> Set.remove date
                            oldSelection |> Map.add task newTaskSelection

                    setter.set (selectionTracker, newSelection)
                )
            }
//            set (fun (task: Task, date: FlukeDate) setter (newValue:bool) ->
//                let ctrlPressed = setter.get ctrlPressed
//
//                let newSelection =
//                    match ctrlPressed with
//                    | false ->
//                        let selection =
//                            match newValue with
//                            | true -> date |> Set.singleton
//                            | false -> Set.empty
//                        Map.empty |> Map.add task selection
//                    | true ->
//                        let oldSelection = setter.get selection
//                        let selection =
//                            oldSelection
//                            |> Map.tryFind task
//                            |> Option.defaultValue Set.empty
//                            |> fun oldSet ->
//                                match newValue with
//                                | true -> oldSet |> Set.add date
//                                | false -> oldSet |> Set.remove date
//                        oldSelection |> Map.add task selection
//
//                setter.set (selection, newSelection)
//            )


//        let rec cellFamily = selectorFamily {
//            key (nameof cellFamily)
//            get (fun (task: Task, date: FlukeDate) getter ->
//                Temp.addCount "cellFamily"
//                let taskState = getter.get (taskStateFamily task)
//
//                let comments =
//                    taskState.CellCommentsMap
//                    |> Map.tryFind date
//                    |> Option.defaultValue []
//
//                let dayStart = getter.get dayStart
//                let sessions =
//                    taskState.Sessions
//                    |> List.filter (fun (TaskSession start) -> isToday dayStart start date)
//
//                let laneMap = getter.get (laneMapFamily task)
//                let status = laneMap.[date]
//
//                { Status = status
//                  Comments = comments
//                  Sessions = sessions }
//            )
//        }
//        let rec cells = atom {
//            key (nameof cells)
//            def (Map.empty : Map<Task, Map<FlukeDate, {| Status: CellStatus
//                                                         Selected: bool |}>>)
//        }
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
