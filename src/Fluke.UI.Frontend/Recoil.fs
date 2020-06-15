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
        Browser.Dom.window?callCountClear <- fun () -> callCount.Clear ()
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
               LastSessions = lastSessions |}


    module Atoms =

        module RecoilTask =
            type TaskId = TaskId of id:string
            type RecoilTask =
                { Id: RecoilValue<TaskId, ReadWrite>
                  Comments: RecoilValue<Comment list, ReadWrite>
                  PriorityValue: RecoilValue<TaskPriorityValue, ReadWrite> }
            let rec idFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof idFamily)
                def (fun (_taskId: TaskId) -> TaskId "")
            }
            let rec commentsFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof commentsFamily)
                def (fun (_taskId: TaskId) -> [])
            }
            let rec priorityValueFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof priorityValueFamily)
                def (fun (_taskId: TaskId) -> TaskPriorityValue 0)
            }
            type RecoilTask with
                static member internal Create taskId =
                    { Id = idFamily taskId
                      Comments = commentsFamily taskId
                      PriorityValue = priorityValueFamily taskId }
            let taskId (task: Task) =
                TaskId (task.Information.Name + "/" + task.Name)
            let rec taskFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof taskFamily)
                def (fun (taskId: TaskId) -> RecoilTask.Create taskId)
            }

        module RecoilCell =
            type CellId = CellId of id:string
            type RecoilCell =
                { Id: RecoilValue<CellId, ReadWrite>
                  Task: RecoilValue<RecoilTask.RecoilTask, ReadWrite>
                  Date: RecoilValue<FlukeDate, ReadWrite>
                  Status: RecoilValue<CellStatus, ReadWrite>
                  Comments: RecoilValue<Comment list, ReadWrite>
                  Sessions: RecoilValue<TaskSession list, ReadWrite>
                  Selected: RecoilValue<bool, ReadWrite> }

            let rec idFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof idFamily)
                def (fun (_cellId: CellId) -> CellId "")
            }
            let rec taskFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof taskFamily)
                def (fun (_cellId: CellId) -> RecoilTask.RecoilTask.Create (RecoilTask.TaskId ""))
            }
            let rec dateFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof dateFamily)
                def (fun (_cellId: CellId) -> flukeDate 0000 Month.January 01)
            }
            let rec statusFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof statusFamily)
                def (fun (_cellId: CellId) -> Disabled)
            }
            let rec commentsFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof commentsFamily)
                def (fun (_cellId: CellId) -> [] : Comment list)
            }
            let rec sessionsFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof sessionsFamily)
                def (fun (_cellId: CellId) -> [] : TaskSession list)
            }
            let rec selectedFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof selectedFamily)
                def (fun (_cellId: CellId) -> false)
            }
            type RecoilCell with
                static member internal Create cellId =
                    { Id = idFamily cellId
                      Task = taskFamily cellId
                      Date = dateFamily cellId
                      Status = statusFamily cellId
                      Comments = commentsFamily cellId
                      Sessions = sessionsFamily cellId
                      Selected = selectedFamily cellId }
            let cellId (RecoilTask.TaskId taskId) (date: FlukeDate) =
//                let task = getter.get (RecoilTask.taskFamily task)
//                let (RecoilTask.TaskId taskId) = RecoilTask.taskId task
                CellId (taskId + "/" + (date.DateTime.Format ()))
//                RecoilCell.Create cellId

            let rec cellFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof cellFamily)
                def (fun (cellId: CellId) -> RecoilCell.Create cellId)
            }

        let rec getNow = atom {
            key (nameof getNow)
            def (fun () -> flukeDateTime 0000 Month.January 01 00 00)
        }
        let rec dayStart = atom {
            key (nameof dayStart)
            def (flukeTime 00 00)
        }
        let rec now = atom {
            key (nameof now)
            def (flukeDateTime 0000 Month.January 01 00 00)
        }
        let rec view = atom {
            key (nameof view)
            def View.Calendar
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
            def (Map.empty : Map<RecoilTask.TaskId, Set<FlukeDate>>)
        }
        let rec taskOrderList = atom {
            key (nameof taskOrderList)
            def ([] : TaskOrderEntry list)
        }
        let rec informationList = atom {
            key (nameof informationList)
            def ([] : Information list)
        }
        let rec lastSessions = atom {
            key (nameof lastSessions)
            def ([] : (Task * TaskSession) list)
        }
        let rec taskStateList = atom {
            key (nameof taskStateList)
            def ([] : TaskState list)
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
        let rec dateSequence = selector {
            key (nameof dateSequence)
            get (fun getter ->
                let now = getter.get Atoms.now

                printfn "DATESEQUENCE. NOW: %A" now


                Temp.addCount "dateSequence."
                [ now.Date ]
                |> Rendering.getDateSequence (45, 20)
            )
        }
        let rec dateRange = selector {
            key (nameof dateRange)
            get (fun getter ->
                let dateSequence = getter.get dateSequence

                Temp.addCount "dateRange"
                let head = dateSequence |> List.head |> fun x -> x.DateTime
                let last = dateSequence |> List.last |> fun x -> x.DateTime
                head, last
            )
        }
        let rec filteredTaskStateList = selector {
            key (nameof filteredTaskStateList)
            get (fun getter ->
                printfn "filteredTaskStateList"

                let view = getter.get Atoms.view
                let taskStateList = getter.get Atoms.taskStateList

                printfn "- before len: %A" taskStateList.Length

                let result =
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

                printfn "- RESULT: %A" result.Length
                Temp.addCount "filteredTaskStateList"
                result
            )
        }
        let rec filteredTaskStateMap = selector {
            key (nameof filteredTaskStateMap)
            get (fun getter ->
                printfn "filteredTaskStateMap"
                let filteredTaskStateList = getter.get filteredTaskStateList

                Temp.addCount "filteredTaskStateMap"
                filteredTaskStateList
                |> List.map (fun taskState ->
//                    let taskId = getter.get(Atoms.RecoilTask.taskFamily taskState.Task).Id |> getter.get
                    let taskId = Atoms.RecoilTask.taskId taskState.Task
                    taskId, taskState
                )
                |> Map.ofList
            )
        }
//                return Temp.tempState.TaskStateMap.[task]
//            let taskStateMap =
//                taskStateList
//                |> List.map (fun taskState -> taskState.Task, taskState)
//                |> Map.ofList
//        let rec taskStateMap = selector {
//            key (nameof taskStateMap)
//            get (fun (taskId: Atoms.RecoilTask.TaskId) getter -> async {
//                let filteredTaskStateList = getter.get filteredTaskStateList
//                Temp.addCount "taskStateMap"
//                printfn "taskStateFamily. task: %A" task.Name
//                return Temp.tempState.TaskStateMap.[task]
//            })
//        }
//            let taskStateMap =
//                taskStateList
//                |> List.map (fun taskState -> taskState.Task, taskState)
//                |> Map.ofList
//
        let rec taskStateFamily = selectorFamily {
            key (nameof taskStateFamily)
            get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                printfn "taskStateFamily"
                let filteredTaskStateMap = getter.get filteredTaskStateMap
                Temp.addCount "taskStateFamily"
//                taskStateMap
//                |> Map.find task
                filteredTaskStateMap.[taskId]
            )
        }
        let rec laneFamily = selectorFamily {
            key (nameof laneFamily)
            get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                let now = getter.get Atoms.now
                let dayStart = getter.get Atoms.dayStart
                let dateSequence = getter.get dateSequence
                let taskState = getter.get (taskStateFamily taskId)
                Temp.addCount "laneFamily"
                Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
            )
        }
        let rec filteredLaneList = selector {
            key (nameof filteredLaneList)
            get (fun getter ->
                printfn "filteredLaneList"

                let filteredTaskStateList = getter.get filteredTaskStateList

                let result =
                    filteredTaskStateList
                    |> List.map (fun taskState ->
    //                    let taskId = getter.get(Atoms.RecoilTask.taskFamily taskState.Task).Id |> getter.get
                        let taskId = Atoms.RecoilTask.taskId taskState.Task
                        getter.get (laneFamily taskId)
                    )

                Temp.addCount "filteredLaneList"
                result
            )
        }
        let rec laneMapFamily = selectorFamily {
            key (nameof laneMapFamily)
            get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                let (Lane (_, cells)) = getter.get (laneFamily taskId)

                Temp.addCount "laneMapFamily"
                cells
                |> List.map (fun (Cell (address, status)) ->
                    address.Date, status
                )
                |> Map.ofList
            )
        }
        let rec isTodayFamily = selectorFamily {
            key (nameof isTodayFamily)
            get (fun (date: FlukeDate) getter ->
                let dayStart = getter.get Atoms.dayStart
                let now = getter.get Atoms.now
                Temp.addCount "isTodayFamily"
                isToday dayStart now date
            )
        }
        let rec findCell = selectorFamily {
            key (nameof findCell)
            get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                let cell = getter.get (Atoms.RecoilCell.cellFamily cellId)
                Temp.addCount "findCell"
                cell
            )
//            set (fun (task: Task, date: FlukeDate) setter (newCell: RecoilCell) ->
//                setter.set(cellFamily (task, date), newCell))
        }
        let rec findTask = selectorFamily {
            key (nameof findTask)
            get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
//                    let taskId = getter.get(Atoms.RecoilTask.taskFamily taskState.Task).Id |> getter.get
//                let taskId = Atoms.RecoilTask.taskId taskState.Task
                let task = getter.get (Atoms.RecoilTask.taskFamily taskId)
                Temp.addCount "findTask"
                task
            )
        }
        let rec selectionTracker = selector {
            key (nameof selectionTracker)
            get (fun getter ->
                let selection = getter.get Atoms.selection
                Temp.addCount "selectionTracker"
                selection
            )
            set (fun setter (newValue: Map<Atoms.RecoilTask.TaskId, Set<FlukeDate>>) ->
                // TODO: refactor
                setter.get Atoms.selection
                |> Seq.iter (fun (KeyValue (taskId, dates)) ->
                    dates
                    |> Seq.iter (fun date ->
                        let cellId = Atoms.RecoilCell.cellId taskId date
                        let selected = setter.get(findCell cellId).Selected
                        setter.set (selected, false)
                    )
                )

                newValue
                |> Seq.iter (fun (KeyValue (taskId, dates)) ->
                    dates
                    |> Seq.iter (fun date ->
                        let cellId = Atoms.RecoilCell.cellId taskId date
                        let selected = setter.get(findCell cellId).Selected
                        setter.set (selected, true)
                    )
                )

                setter.set (Atoms.selection, newValue)
                Temp.addCount "selectionTracker (SET)"
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

        module rec RecoilTask =
            let rec comments = selectorFamily {
                key (nameof RecoilTask + "/" + nameof comments)
                get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                    Temp.addCount (nameof RecoilTask + "/" + nameof comments)
                    getter.get(findTask taskId).Comments |> getter.get
                )
            }
            let rec priorityValue = selectorFamily {
                key (nameof RecoilTask + "/" + nameof priorityValue)
                get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                    Temp.addCount (nameof RecoilTask + "/" + nameof priorityValue)
                    getter.get(findTask taskId).PriorityValue |> getter.get
                )
            }

        module rec RecoilCell =
            let rec status = selectorFamily {
                key (nameof RecoilCell + "/" + nameof status)
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof status)
                    getter.get(findCell cellId).Status |> getter.get
                )
            }
            let rec comments = selectorFamily {
                key (nameof RecoilCell + "/" + nameof comments)
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof comments)
                    getter.get(findCell cellId).Comments |> getter.get
                )
            }
            let rec sessions = selectorFamily {
                key (nameof RecoilCell + "/" + nameof sessions)
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof sessions)
                    getter.get(findCell cellId).Sessions |> getter.get
                )
            }
//            let rec testFamily = selectorFamily {
//                key (nameof RecoilCell + "/" + nameof testFamily)
//                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
//                    None : (Task * FlukeDate) option
//                )
//            }
            let rec selected = selectorFamily {
                key (nameof RecoilCell + "/" + nameof selected)
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof selected)
                    getter.get(findCell cellId).Selected |> getter.get
                )
                set (fun (cellId: Atoms.RecoilCell.CellId) setter (newValue: bool) ->
                    Temp.addCount (nameof RecoilCell + "/" + nameof selected + " (SET)")
                    let ctrlPressed = setter.get Atoms.ctrlPressed

                    let cell = setter.get (findCell cellId)
                    let date = setter.get cell.Date
                    let taskId =
                        let task = setter.get cell.Task
                        setter.get task.Id

                    let newSelection =
                        match ctrlPressed with
                        | false ->
                            let newTaskSelection =
                                match newValue with
                                | true -> date |> Set.singleton
                                | false -> Set.empty
                            Map.empty |> Map.add taskId newTaskSelection
                        | true ->
                            let oldSelection = setter.get Atoms.selection
                            let newTaskSelection =
                                oldSelection
                                |> Map.tryFind taskId
                                |> Option.defaultValue Set.empty
                                |> fun oldSet ->
                                    match newValue with
                                    | true -> oldSet |> Set.add date
                                    | false -> oldSet |> Set.remove date
                            oldSelection |> Map.add taskId newTaskSelection

                    setter.set (selectionTracker, newSelection)
                )
            }

        let rec sortedLaneList = selector {
            key (nameof sortedLaneList)
            get (fun getter ->
                printfn "sortedLaneList"
                let view = getter.get Atoms.view
                let dayStart = getter.get Atoms.dayStart
                let now = getter.get Atoms.now
                let taskOrderList = getter.get Atoms.taskOrderList
                let filteredLaneList = getter.get filteredLaneList

                let result =
                    match view with
                    | View.Calendar ->
                        filteredLaneList
                        |> Sorting.sortLanesByFrequency
                        |> Sorting.sortLanesByIncomingRecurrency dayStart now
                        |> Sorting.sortLanesByTimeOfDay dayStart now taskOrderList
                    | View.Groups ->
                        let lanes =
                            filteredLaneList
                            |> Sorting.applyManualOrder taskOrderList

                        getter.get Atoms.informationList
                        |> List.map (fun information ->
                            let lanes =
                                lanes
                                |> List.filter (fun (Lane (task, _)) -> task.Information = information)

                            information, lanes
                        )
                        |> List.collect snd
                    | View.Tasks ->
                        filteredLaneList
                        |> Sorting.applyManualOrder taskOrderList
                        |> List.sortByDescending (fun (Lane (task, _)) ->
                            let taskId = Atoms.RecoilTask.taskId task
                            let priorityValue = Recoil.useValue (RecoilTask.priorityValue taskId)

                            priorityValue |> ofTaskPriorityValue
                        )
                    | View.Week ->
                        []

                Temp.addCount "sortedLaneList"
                result
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
