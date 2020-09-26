namespace Fluke.UI.Frontend

open Feliz.Router


#nowarn "40"

open System
open System.Collections.Generic
open FSharpPlus
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.DateFunctions
open Suigetsu.Core


module Recoil =
    open Model
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    module Profiling =
        let private initialTicks = DateTime.Now.Ticks

        let private ticksDiff ticks = int64 (TimeSpan(ticks - initialTicks).TotalMilliseconds)

        let internal profilingState =
            {|
                CallCount = Dictionary ()
                Timestamps = List<string * int64> ()
            |}

        Ext.setDom (nameof profilingState) profilingState

        let internal addCount id =
            match profilingState.CallCount.ContainsKey id with
            | false -> profilingState.CallCount.[id] <- 1
            | true -> profilingState.CallCount.[id] <- profilingState.CallCount.[id] + 1

        let internal addTimestamp id = profilingState.Timestamps.Add (id, ticksDiff DateTime.Now.Ticks)

        addTimestamp "Init"



    module FakeBackend =


        let rec filterTaskStateList view dateRange (taskStateList: TaskState list) =
            printfn "DR %A" dateRange
            match view with
            | View.Calendar
            | View.Week ->
                taskStateList
                |> List.filter (function
                    | { Task = { Scheduling = Manual WithoutSuggestion } } as taskState ->
                        taskState.CellStateMap
                        |> Map.toSeq
                        |> Seq.exists (fun ((DateId referenceDay), cellState) ->
                            referenceDay.DateTime
                            >==< dateRange
                            && (cellState.Attachments
                                |> List.exists (function
                                    | Attachment.Comment _ -> true
                                    | _ -> false)
                                || cellState.Status <> Disabled))
                        || taskState.Sessions
                           |> List.exists (fun (TaskSession (start, _, _)) -> start.Date.DateTime >==< dateRange)
                    | _ -> true)
            | View.Groups ->
                taskStateList
                |> List.filter (function
                    | { Task = { Scheduling = Manual WithoutSuggestion } } -> true
                    | _ -> false)
            //                |> List.filter (fun task ->
//                    task.StatusEntries
//                    |> List.filter (function
//                        | TaskStatusEntry (date, _) when date.DateTime >==< dateRange -> true
//                        | _ -> false
//                    )
//                    |> List.tryLast
//                    |> function Some (TaskStatusEntry (_, Dismissed)) -> false | _ -> true
//                )
            | View.Tasks ->
                taskStateList
                |> List.filter (function
                    | { Task = { Information = Archive _ } } -> false
                    | { Task = { Priority = Some priority }; Sessions = [] } when priority.Value < 5 -> false
                    | { Task = { Scheduling = Manual _ } } -> true
                    | _ -> false)

        let sortLanes (input: {| View: View
                                 DayStart: FlukeTime
                                 Position: FlukeDateTime
                                 InformationStateList: InformationState list // TaskOrderList: TaskOrderEntry list
                                 Lanes: (TaskState * (CellAddress * CellStatus) list) list |}) =
            match input.View with
            | View.Calendar ->
                input.Lanes
                |> Sorting.sortLanesByFrequency
                |> Sorting.sortLanesByIncomingRecurrency input.DayStart input.Position
                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position //input.TaskOrderList
            | View.Groups ->
                let lanes =
                    input.Lanes
                    //                    |> Sorting.applyManualOrder input.TaskOrderList
                    |> List.groupBy (fun (taskState, _) -> taskState.Task.Information)
                    |> Map.ofList

                input.InformationStateList
                |> List.map (fun informationState ->
                    let lanes =
                        lanes
                        |> Map.tryFind informationState.Information
                        |> Option.defaultValue []

                    informationState.Information, lanes)
                |> List.collect snd
            | View.Tasks ->
                input.Lanes
                //                |> Sorting.applyManualOrder input.TaskOrderList
                |> List.sortByDescending (fun (taskState, _) ->
                    taskState.Task.Priority
                    |> Option.map (fun x -> x.Value)
                    |> Option.defaultValue 0)
            | View.Week -> input.Lanes

        let getState (input: {| User: User
                                DateSequence: FlukeDate list
                                View: View
                                Position: FlukeDateTime
                                GetLivePosition: unit -> FlukeDateTime
                                TreeStateMap: Map<TreeId, TreeState>
                                TreeSelectionIds: Set<TreeId> |}) =
            //            let treeSelectionIds =
//                input.State.Session.TreeSelection
//                |> Set.map (fun treeState -> treeState.Id)
//
            let treeSelection =
                input.TreeSelectionIds
                |> Set.map (fun treeId -> input.TreeStateMap.[treeId])
                |> Set.toList

            let informationStateList =
                treeSelection
                |> List.collect (fun treeState ->
                    treeState.InformationStateMap
                    |> Map.values
                    |> Seq.distinctBy (fun informationState -> informationState.Information.Name)
                    |> Seq.toList)

            let taskStateList =
                treeSelection
                |> List.collect (fun treeState ->
                    treeState.TaskStateMap
                    |> Map.values
                    |> Seq.toList
                    |> List.map (fun taskState ->
                        let sessionsMap =
                            taskState.Sessions
                            |> List.map (fun (TaskSession (start, duration, breakDuration) as session) ->
                                let dateId = dateId input.User.DayStart start
                                dateId, session)
                            |> List.groupBy fst
                            |> Map.ofList
                            |> Map.mapValues (List.map snd)


                        let newCellStateMap =
                            sessionsMap
                            |> Map.keys
                            |> Seq.map (fun dateId ->
                                let cellState =
                                    taskState.CellStateMap
                                    |> Map.tryFind dateId
                                    |> Option.defaultValue
                                        {
                                            Status = Disabled
                                            Sessions = []
                                            Attachments = []
                                        }

                                let newSessions =
                                    sessionsMap
                                    |> Map.tryFind dateId
                                    |> Option.defaultValue []
                                    |> List.append cellState.Sessions

                                dateId, { cellState with Sessions = newSessions })
                            |> Map.ofSeq

                        { taskState with
                            CellStateMap = TempData.mergeCellStateMap taskState.CellStateMap newCellStateMap
                        }))

            // TODO: this might be needed
            let informationStateMap, taskStateMap =
                ((Map.empty, Map.empty), treeSelection)
                ||> List.fold (fun (informationStateMap, taskStateMap) treeState ->
                        match treeState with
                        | treeState when hasAccess treeState input.User ->
                            let newInformationStateMap =
                                TempData.mergeInformationStateMap informationStateMap treeState.InformationStateMap

                            let newTaskStateMap = TempData.mergeTaskStateMap taskStateMap treeState.TaskStateMap
                            newInformationStateMap, newTaskStateMap
                        | _ -> informationStateMap, taskStateMap)


            let dateRange =
                // TODO: handle
                let head =
                    input.DateSequence
                    |> List.head
                    |> fun x -> x.DateTime

                let last =
                    input.DateSequence
                    |> List.last
                    |> fun x -> x.DateTime

                head, last


            let filteredTaskStateList = filterTaskStateList input.View dateRange taskStateList

            printfn "getTree %A" (taskStateList.Length, filteredTaskStateList.Length)

            let filteredLanes =
                filteredTaskStateList
                |> List.map (fun taskState ->
                    Rendering.renderLane input.User.DayStart input.Position input.DateSequence taskState)

            //            let taskOrderList = RootPrivateData.treeData.TaskOrderList // @ RootPrivateData.taskOrderList
//            let taskOrderList = [] // @ RootPrivateData.taskOrderList



            let sortedTaskStateList =
                sortLanes
                    {|
                        View = input.View
                        DayStart = input.User.DayStart
                        Position = input.Position
                        InformationStateList = informationStateList
                        Lanes = filteredLanes
                    |}
                |> List.map (fun (taskState, cells) ->
                    let newCells =
                        cells
                        |> List.map (fun (address, status) -> address.DateId, status)
                        |> Map.ofList

                    taskState, newCells)

            //                    let sortedTaskList =
//                        sortedTaskList
////                        |> List.sortByDescending (fun x -> x.StatusEntries.Length)
//                        |> List.take 50

            let newTaskStateList =
                sortedTaskStateList
                |> List.map (fun (taskState, statusMap) ->
                    let newCellStateMap =
                        seq {
                            yield! taskState.CellStateMap |> Map.keys
                            yield! statusMap |> Map.keys
                        }
                        |> Seq.distinct
                        |> Seq.map (fun dateId ->
                            let newStatus =
                                statusMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue Disabled

                            let cellState =
                                taskState.CellStateMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue
                                    {
                                        Status = Disabled
                                        Sessions = []
                                        Attachments = []
                                    }

                            dateId, { cellState with Status = newStatus })
                        |> Map.ofSeq

                    let newTaskState = { taskState with CellStateMap = newCellStateMap }

                    newTaskState)

            let newInformationStateMap =
                informationStateList
                |> List.map (fun informationState -> informationState.Information, informationState)
                |> Map.ofList

            let newTaskStateMap =
                newTaskStateList
                |> List.map (fun taskState -> taskState.Task, taskState)
                |> Map.ofList

            let newTaskList =
                newTaskStateList
                |> List.map (fun taskState -> taskState.Task)

            let newSession =
                {
                    User = Some input.User
                    InformationStateMap = newInformationStateMap
                    TaskStateMap = newTaskStateMap
                    TaskList = newTaskList
                    GetLivePosition = input.GetLivePosition
                    TreeStateMap = input.TreeStateMap
                    TreeSelection = treeSelection |> Set.ofList
                }

            let newState = { Session = newSession }

            newState




    module Atoms =
        module rec RecoilInformation =
            type InformationId = InformationId of id: string

            let rec wrappedInformation =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilInformation) (nameof wrappedInformation))
                    def (fun (_informationId: InformationId) ->
                            Profiling.addCount (nameof wrappedInformation)
                            Area (Area.Default, []))
                }

            let rec attachments =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilInformation) (nameof attachments))
                    def (fun (_informationId: InformationId) ->
                            Profiling.addCount (nameof attachments)
                            [])
                }

            let rec informationId (information: Information): InformationId =
                match information with
                | Project ({ Name = ProjectName name }, _) -> sprintf "%s/%s" information.KindName name
                | Area ({ Name = AreaName name }, _) -> sprintf "%s/%s" information.KindName name
                | Resource ({ Name = ResourceName name }, _) -> sprintf "%s/%s" information.KindName name
                | Archive x ->
                    let (InformationId archiveId) = informationId x
                    sprintf "%s/%s" information.KindName archiveId
                |> InformationId


        module RecoilTask =
            type TaskId = TaskId of informationName: InformationName * taskName: TaskName

            let taskId (task: Task) = TaskId (task.Information.Name, task.Name)

            type RecoilTask =
                {
                    Id: RecoilValue<TaskId, ReadWrite>
                    InformationId: RecoilValue<RecoilInformation.InformationId, ReadWrite>
                    Name: RecoilValue<TaskName, ReadWrite>
                    Scheduling: RecoilValue<Scheduling, ReadWrite>
                    PendingAfter: RecoilValue<FlukeTime option, ReadWrite>
                    MissedAfter: RecoilValue<FlukeTime option, ReadWrite>
                    Priority: RecoilValue<Priority option, ReadWrite>
                    //                    Sessions: RecoilValue<TaskSession list, ReadWrite>
                    Attachments: RecoilValue<Attachment list, ReadWrite>
                    Duration: RecoilValue<Minute option, ReadWrite>
                }

            let rec idFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof idFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof idFamily)
                            taskId Task.Default)
                }

            let rec informationIdFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof informationIdFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof informationIdFamily)
                            RecoilInformation.informationId Task.Default.Information)
                }

            let rec nameFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof nameFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof nameFamily)
                            Task.Default.Name)
                }

            let rec schedulingFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof schedulingFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof schedulingFamily)
                            Task.Default.Scheduling)
                }

            let rec pendingAfterFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof pendingAfterFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof pendingAfterFamily)
                            Task.Default.PendingAfter)
                }

            let rec missedAfterFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof missedAfterFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof missedAfterFamily)
                            Task.Default.MissedAfter)
                }

            let rec priorityFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof priorityFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof priorityFamily)
                            Task.Default.Priority)
                }

            //            let rec sessionsFamily =
//                atomFamily {
//                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof sessionsFamily))
//                    def (fun (_taskId: TaskId) ->
//                            Profiling.addCount (nameof sessionsFamily)
//                            []) // TODO: move from here?
//                }
            //            let rec commentsFamily = atomFamily {
//                key (sprintf "%s/%s" (nameof RecoilTask) (nameof commentsFamily))
//                def (fun (_taskId: TaskId) -> Profiling.addCount  (nameof commentsFamily); []) // TODO: move from here?
//            }
            let rec attachmentsFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof attachmentsFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof attachmentsFamily)
                            []) // TODO: move from here?
                }

            let rec durationFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof durationFamily))
                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (nameof durationFamily)
                            Task.Default.Duration)
                }

            type RecoilTask with

                static member inline internal Create taskId =
                    {
                        Id = idFamily taskId
                        InformationId = informationIdFamily taskId
                        Name = nameFamily taskId
                        Scheduling = schedulingFamily taskId
                        PendingAfter = pendingAfterFamily taskId
                        MissedAfter = missedAfterFamily taskId
                        Priority = priorityFamily taskId
                        //                        Sessions = sessionsFamily taskId
                        Attachments = attachmentsFamily taskId
                        Duration = durationFamily taskId
                    }

            let rec taskFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof taskFamily))
                    def (fun (taskId: TaskId) ->
                            Profiling.addCount (nameof taskFamily)
                            RecoilTask.Create taskId)
                }

        module RecoilUser =
            type RecoilUser =
                {
                    Color: RecoilValue<UserColor, ReadWrite>
                    WeekStart: RecoilValue<DayOfWeek, ReadWrite>
                    DayStart: RecoilValue<FlukeTime, ReadWrite>
                    SessionLength: RecoilValue<Minute, ReadWrite>
                    SessionBreakLength: RecoilValue<Minute, ReadWrite>
                }

            let rec colorFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilUser) (nameof colorFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof colorFamily)
                            UserColor.Black)
                }

            let rec weekStartFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilUser) (nameof weekStartFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof weekStartFamily)
                            DayOfWeek.Sunday)
                }

            let rec dayStartFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilUser) (nameof dayStartFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof dayStartFamily)
                            FlukeTime.Create 04 00)
                }

            let rec sessionLengthFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilUser) (nameof sessionLengthFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof sessionLengthFamily)
                            Minute 25.)
                }

            let rec sessionBreakLengthFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilUser) (nameof sessionBreakLengthFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof sessionBreakLengthFamily)
                            Minute 5.)
                }

            type RecoilUser with

                static member inline internal Create username =
                    {
                        Color = colorFamily username
                        WeekStart = weekStartFamily username
                        DayStart = dayStartFamily username
                        SessionLength = sessionLengthFamily username
                        SessionBreakLength = sessionBreakLengthFamily username
                    }

            let rec userFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilUser) (nameof userFamily))
                    def (fun (username: Username) ->
                            Profiling.addCount (nameof userFamily)
                            RecoilUser.Create username)
                }


        module RecoilSession =
            type RecoilSession =
                {
                    User: RecoilValue<User option, ReadWrite>
                    TreeSelectionIds: RecoilValue<Set<TreeId>, ReadWrite>
                    AvailableTreeIds: RecoilValue<TreeId list, ReadWrite>
                    TaskIdList: RecoilValue<RecoilTask.TaskId list, ReadWrite>
                }

            let rec userFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilSession) (nameof userFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof userFamily)
                            None)
                }

            let rec treeSelectionIdsFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilSession) (nameof treeSelectionIdsFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof treeSelectionIdsFamily)
                            (Set.empty: Set<TreeId>))
                }

            let rec availableTreeIdsFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilSession) (nameof availableTreeIdsFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof availableTreeIdsFamily)
                            [])
                }

            let rec taskIdListFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilSession) (nameof taskIdListFamily))
                    def (fun (_username: Username) ->
                            Profiling.addCount (nameof taskIdListFamily)
                            [])
                }

            type RecoilSession with

                static member inline internal Create username =
                    {
                        User = userFamily username
                        TreeSelectionIds = treeSelectionIdsFamily username
                        AvailableTreeIds = availableTreeIdsFamily username
                        TaskIdList = taskIdListFamily username
                    }

            let rec sessionFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilSession) (nameof sessionFamily))
                    def (fun (username: Username) ->
                            Profiling.addCount (nameof sessionFamily)
                            RecoilSession.Create username)
                }


        module RecoilCell =
            type CellId = CellId of id: string

            type RecoilCell =
                {
                    Id: RecoilValue<CellId, ReadWrite>
                    TaskId: RecoilValue<RecoilTask.TaskId, ReadWrite>
                    Date: RecoilValue<FlukeDate, ReadWrite>
                    Status: RecoilValue<CellStatus, ReadWrite>
                    Attachments: RecoilValue<Attachment list, ReadWrite>
                    Sessions: RecoilValue<TaskSession list, ReadWrite>
                    Selected: RecoilValue<bool, ReadWrite>
                }

            let rec idFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof idFamily))
                    def (fun (_cellId: CellId) ->
                            Profiling.addCount (nameof idFamily)
                            CellId "")
                }

            let rec taskIdFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof taskIdFamily))
                    def (fun (_cellId: CellId) ->
                            Profiling.addCount (nameof taskIdFamily)
                            RecoilTask.TaskId (InformationName "", TaskName ""))
                }

            let rec dateFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof dateFamily))
                    def (fun (_cellId: CellId) ->
                            Profiling.addCount (nameof dateFamily)
                            FlukeDate.MinValue)
                }

            let rec statusFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof statusFamily))
                    def (fun (_cellId: CellId) ->
                            Profiling.addCount (nameof statusFamily)
                            Disabled)
                }

            let rec attachmentsFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof attachmentsFamily))
                    def (fun (_cellId: CellId) ->
                            Profiling.addCount (nameof attachmentsFamily)
                            []: Attachment list)
                }

            let rec sessionsFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof sessionsFamily))
                    def (fun (_cellId: CellId) ->
                            Profiling.addCount (nameof sessionsFamily)
                            []: TaskSession list)
                }

            let rec selectedFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof selectedFamily))
                    def (fun (_cellId: CellId) ->
                            Profiling.addCount (nameof selectedFamily)
                            false)
                }

            type RecoilCell with

                static member inline internal Create cellId =
                    {
                        Id = idFamily cellId
                        TaskId = taskIdFamily cellId
                        Date = dateFamily cellId
                        Status = statusFamily cellId
                        Attachments = attachmentsFamily cellId
                        Sessions = sessionsFamily cellId
                        Selected = selectedFamily cellId
                    }

            let cellId (RecoilTask.TaskId (InformationName informationName, TaskName taskName)) (DateId referenceDay) =
                CellId (sprintf "%s/%s/%s" informationName taskName (referenceDay.DateTime.Format "yyyy-MM-dd"))

            let rec cellFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof cellFamily))
                    def (fun (cellId: CellId) ->
                            Profiling.addCount (nameof cellFamily)
                            RecoilCell.Create cellId)
                }

        module RecoilTree =
            //            type TreeId = TreeId of id: string

            //        type TreeState =
//            {
//                Id: TreeId
//                Name: TreeName
//                Owner: User
//                SharedWith: TreeAccess list
//                //                Position: FlukeDateTime option
//                InformationStateMap: Map<Information, InformationState>
//                TaskStateMap: Map<Task, TaskState>
//            }
            type RecoilTree =
                {
                    Id: RecoilValue<TreeId, ReadWrite>
                    Name: RecoilValue<TreeName, ReadWrite>
                    Owner: RecoilValue<User option, ReadWrite>
                    SharedWith: RecoilValue<TreeAccess, ReadWrite>
                    Position: RecoilValue<FlukeDateTime option, ReadWrite>
                }

            let rec idFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTree) (nameof idFamily))
                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (nameof idFamily)
                            TreeId Guid.Empty)
                }

            let rec nameFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilInformation) (nameof nameFamily))
                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (nameof nameFamily)
                            TreeName "")
                }

            let rec ownerFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilInformation) (nameof ownerFamily))
                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (nameof ownerFamily)
                            None)
                }

            let rec sharedWithFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilInformation) (nameof sharedWithFamily))
                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (nameof sharedWithFamily)
                            TreeAccess.Public)
                }

            let rec positionFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilInformation) (nameof positionFamily))
                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (nameof positionFamily)
                            None)
                }

            type RecoilTree with

                static member inline internal Create treeId =
                    {
                        Id = idFamily treeId
                        Name = nameFamily treeId
                        Owner = ownerFamily treeId
                        SharedWith = sharedWithFamily treeId
                        Position = positionFamily treeId
                    }

            //            let treeId owner name =
//                TreeId (sprintf "%s/%s" owner.Username name)
//
            let rec treeFamily =
                atomFamily {
                    key (sprintf "%s/%s" (nameof RecoilTree) (nameof treeFamily))
                    def (fun (treeId: TreeId) ->
                            Profiling.addCount (nameof treeFamily)
                            RecoilTree.Create treeId)
                }

        let rec internal debug =
            atom {
                key ("atom/" + nameof debug)
                def false
                local_storage
            }

        let rec internal path =
            atom {
                key ("atom/" + nameof path)
                def (Router.currentPath ())
            }

        let rec internal getLivePosition =
            atom {
                key ("atom/" + nameof getLivePosition)
                def
                    ({|
                         Get = fun () -> FlukeDateTime.FromDateTime DateTime.MinValue
                     |})
            }

        let rec internal username =
            atom {
                key ("atom/" + nameof username)
                def None
            }

        let rec internal cellSize =
            atom {
                key ("atom/" + nameof cellSize)
                def 17
            }

        let rec internal state =
            atom {
                key ("atom/" + nameof state)
                def (None: State option)
            }

        let rec internal selection =
            atom {
                key ("atom/" + nameof selection)
                def (Map.empty: Map<RecoilTask.TaskId, Set<FlukeDate>>)
            }

        let rec internal ctrlPressed =
            atom {
                key ("atom/" + nameof ctrlPressed)
                def false
            }

        let rec internal shiftPressed =
            atom {
                key ("atom/" + nameof shiftPressed)
                def false
            }

        let rec internal positionTrigger =
            atom {
                key ("atom/" + nameof positionTrigger)
                def 0
            }


    module Selectors =

        let rec view =
            selector {
                key ("selector/" + nameof view)
                get (fun getter ->
                        let path = getter.get Atoms.path

                        let result =
                            match path with
                            | [ "view"; "Calendar" ] -> View.Calendar
                            | [ "view"; "Groups" ] -> View.Groups
                            | [ "view"; "Tasks" ] -> View.Tasks
                            | [ "view"; "Week" ] -> View.Week
                            | _ -> View.Calendar

                        Profiling.addCount (nameof view)
                        result)
            }

        //        let rec user =
//            selector {
//                key ("selector/" + nameof user)
//                get (fun getter ->
//                        let state = getter.get Atoms.state
//                        let username = getter.get Atoms.username
//
//                        let result =
//                            match state with
//                            | Some state ->
//                                match username, state.Session.User with
//                                | Some username, Some user when user.Username = username -> Some user
//                                | _ -> None
//                            | None -> None
//
//                        Profiling.addCount (nameof user)
//                        result)
//            }

        let rec position =
            selector {
                key ("selector/" + nameof position)
                get (fun getter ->
                        let _positionTrigger = getter.get Atoms.positionTrigger
                        let getLivePosition = getter.get Atoms.getLivePosition

                        let result = Some <| getLivePosition.Get ()

                        Profiling.addCount (nameof position)
                        result)
                set (fun setter _newValue ->
                        setter.set (Atoms.positionTrigger, (fun x -> x + 1))
                        Profiling.addCount (nameof position + " (SET)"))
            }

        let rec dateSequence =
            selector {
                key ("selector/" + nameof dateSequence)
                get (fun getter ->
                        let position = getter.get position

                        let result =
                            match position with
                            | None -> []
                            | Some position ->
                                [
                                    position.Date
                                ]
                                |> Rendering.getDateSequence (45, 20)

                        Profiling.addCount (nameof dateSequence)
                        result)
            }

        //        let rec state =
//            selector {
//                key ("selector/" + nameof state)
//                get (fun getter ->
//                        let state = getter.get Atoms.state
//                        Profiling.addCount (nameof state)
//                        state)
//                set (fun setter (newValue: State option) ->
//                        let dateSequence = setter.get dateSequence
//
//                        Profiling.addTimestamp "state.set[0]"
//
//                        printfn
//                            "dateSequence tree newValue==none=%A dateSequence.length=%A"
//                            newValue.IsNone
//                            dateSequence.Length
//
//                        match newValue with
//                        | Some state ->
//                            match state.Session with
//                            | { User = None } ->
//                                setter.set (Atoms.state, None)
//                                setter.reset Atoms.getLivePosition
//                                setter.set (Atoms.username, None)
//                            | { User = Some user } as session ->
//                                setter.set (Atoms.state, newValue)
//                                setter.set (Atoms.getLivePosition, {| Get = session.GetLivePosition |})
//                                setter.set (Atoms.username, Some user.Username)
//
//                                let recoilSession = setter.get (Atoms.RecoilSession.sessionFamily user.Username)
//
//                                let treeSelectionIds =
//                                    state.Session.TreeSelection
//                                    |> Set.map (fun treeState -> treeState.Id)
//
//                                let availableTreeIds =
//                                    state.Session.TreeStateMap
//                                    |> Map.values
//                                    |> Seq.map (fun treeState -> treeState.Id)
//                                    |> Seq.toList
//
//                                let taskIdList =
//                                    state.Session.TaskList
//                                    |> List.map (fun task -> Atoms.RecoilTask.taskId task)
//
//                                setter.set (recoilSession.User, Some user)
//                                setter.set (recoilSession.TreeSelectionIds, treeSelectionIds)
//                                setter.set (recoilSession.AvailableTreeIds, availableTreeIds)
//                                setter.set (recoilSession.TaskIdList, taskIdList)
//
//                            let recoilInformationMap =
//                                state.Session.TaskList
//                                |> Seq.map (fun task -> task.Information)
//                                |> Seq.distinct
//                                |> Seq.map (fun information -> state.Session.InformationStateMap.[information])
//                                |> Seq.map (fun informationState ->
//
//
//                                    let informationId =
//                                        Atoms.RecoilInformation.informationId informationState.Information
//
//                                    setter.set
//                                        (Atoms.RecoilInformation.wrappedInformation informationId,
//                                         informationState.Information)
//                                    setter.set
//                                        (Atoms.RecoilInformation.attachments informationId, informationState.Attachments)
//                                    informationState.Information, informationId)
//                                |> Map.ofSeq
//
//                            Profiling.addTimestamp "state.set[1]"
//
//                            state.Session.TaskList
//                            |> List.map (fun task -> state.Session.TaskStateMap.[task])
//                            |> List.iter (fun taskState ->
//                                let task = taskState.Task
//                                let taskId = Atoms.RecoilTask.taskId task
//
//                                let recoilTask = setter.get (Atoms.RecoilTask.taskFamily taskId)
//
//                                setter.set (recoilTask.Id, taskId)
//                                setter.set (recoilTask.Name, task.Name)
//                                setter.set (recoilTask.InformationId, recoilInformationMap.[task.Information])
//                                setter.set (recoilTask.PendingAfter, task.PendingAfter)
//                                setter.set (recoilTask.MissedAfter, task.MissedAfter)
//                                setter.set (recoilTask.Scheduling, task.Scheduling)
//                                setter.set (recoilTask.Priority, task.Priority)
//                                //                                setter.set (recoilTask.Sessions, taskState.Sessions) // TODO: move from here
//                                setter.set (recoilTask.Attachments, taskState.Attachments)
//                                setter.set (recoilTask.Duration, task.Duration)
//
//                                dateSequence
//                                |> List.iter (fun date ->
//                                    let cellId = Atoms.RecoilCell.cellId taskId (DateId date)
//
//                                    let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)
//
//                                    setter.set (recoilCell.Id, cellId)
//                                    setter.set (recoilCell.TaskId, taskId)
//                                    setter.set (recoilCell.Date, date))
//
//                                taskState.CellStateMap
//                                |> Map.filter (fun dateId cellState ->
//                                    (<>) cellState.Status Disabled
//                                    || not cellState.Attachments.IsEmpty
//                                    || not cellState.Sessions.IsEmpty)
//                                |> Map.iter (fun dateId cellState ->
//                                    let cellId = Atoms.RecoilCell.cellId taskId dateId
//
//                                    let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)
//
//                                    setter.set (recoilCell.Status, cellState.Status)
//                                    setter.set (recoilCell.Attachments, cellState.Attachments)
//                                    setter.set (recoilCell.Sessions, cellState.Sessions)
//                                    setter.set (recoilCell.Selected, false)))
//
//
//                            state.Session.TreeStateMap
//                            |> Map.values
//                            |> Seq.iter (fun treeState ->
//                                let recoilTree = setter.get (Atoms.RecoilTree.treeFamily treeState.Id)
//
//                                setter.set (recoilTree.Name, treeState.Name)
//                                setter.set (recoilTree.Owner, Some treeState.Owner)
//                                setter.set (recoilTree.SharedWith, treeState.SharedWith)
//                                setter.set (recoilTree.Position, treeState.Position))
//
//                        | _ -> ()
//
//                        Profiling.addTimestamp "state.set[2]"
//                        Profiling.addCount (nameof state + " (SET)"))
//            }

        /// [1]
        let rec selection =
            selector {
                key ("selector/" + nameof selection)
                get (fun getter ->
                        let selection = getter.get Atoms.selection
                        Profiling.addCount (nameof selection)
                        selection)
                set (fun setter (newSelection: Map<Atoms.RecoilTask.TaskId, Set<FlukeDate>>) ->
                        let selection = setter.get Atoms.selection

                        let operationsByTask =
                            let taskIdSet =
                                seq {
                                    yield! selection |> Map.keys
                                    yield! newSelection |> Map.keys
                                }
                                |> Set.ofSeq

                            taskIdSet
                            |> Seq.map (fun taskId ->
                                let taskSelection =
                                    selection
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let newTaskSelection =
                                    newSelection
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let datesToIgnore = Set.intersect taskSelection newTaskSelection

                                let datesToUnselect =
                                    datesToIgnore
                                    |> Set.difference taskSelection
                                    |> Seq.map (fun date -> date, false)

                                let datesToSelect =
                                    datesToIgnore
                                    |> Set.difference newTaskSelection
                                    |> Seq.map (fun date -> date, true)

                                taskId, Seq.append datesToSelect datesToUnselect)

                        operationsByTask
                        |> Seq.iter (fun (taskId, operations) ->
                            operations
                            |> Seq.iter (fun (date, selected) ->
                                let cellId = Atoms.RecoilCell.cellId taskId (DateId date)

                                let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)

                                setter.set (cell.Selected, selected)))

                        setter.set (Atoms.selection, newSelection)
                        Profiling.addCount (nameof selection + " (SET)"))
            }
        /// [3]
        let rec selectedCells =
            selector {
                key ("selector/" + nameof selectedCells)
                get (fun getter ->
                        let selection = Recoil.useValue selection

                        let selectionCellIds =
                            selection
                            |> Seq.collect (fun (KeyValue (taskId, dates)) ->
                                dates
                                |> Seq.map DateId
                                |> Seq.map (Atoms.RecoilCell.cellId taskId))

                        let result =
                            selectionCellIds
                            |> Seq.map (Atoms.RecoilCell.cellFamily >> getter.get)
                            |> Seq.toList

                        Profiling.addCount (nameof selectedCells)
                        result)
            }
        /// [4]
//        // TODO: Remove View and check performance
//        let rec stateAsync =
//            selectorFamily {
//                key ("selectorFamily/" + nameof stateAsync)
//                get (fun (view: View) getter ->
//                        async {
//                            Profiling.addTimestamp "stateAsync.get[0]"
//                            let state = getter.get Atoms.state
//                            let position = getter.get position
//
//                            let result =
//                                match state, position with
//                                | Some state, Some position ->
//                                    match state.Session.User with
//                                    | Some user ->
//                                        let dateSequence = getter.get dateSequence
//
//                                        let treeSelectionIds =
//                                            getter.get (Atoms.RecoilSession.treeSelectionIdsFamily user.Username)
//
//                                        let newTreeSelectionIds =
//                                            if treeSelectionIds.IsEmpty then
//                                                state.Session.TreeSelection
//                                                |> Set.map (fun treeState -> treeState.Id)
//                                            else
//                                                treeSelectionIds
//
//                                        Profiling.addTimestamp "stateAsync.get[1]"
//
//                                        let newState =
//                                            FakeBackend.getState
//                                                {|
//                                                    User = user
//                                                    DateSequence = dateSequence
//                                                    View = view
//                                                    Position = position
//                                                    TreeSelectionIds = newTreeSelectionIds
//                                                    TreeStateMap = state.Session.TreeStateMap
//                                                    GetLivePosition = state.Session.GetLivePosition
//                                                |}
//
//                                        Profiling.addTimestamp "stateAsync.get[2]"
//                                        Profiling.addCount (nameof stateAsync)
//
//                                        printfn
//                                            "B %A A %A"
//                                            state.Session.InformationStateMap.Count
//                                            newState.Session.InformationStateMap.Count
//
//                                        Some newState
//                                    | _ -> None
//                                | _ -> None
//
//                            return result
//                        })
//            }

        module rec RecoilFlukeDate =
            let rec isTodayFamily =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof isTodayFamily))
                    get (fun (date: FlukeDate) getter ->
                            let username = getter.get Atoms.username
                            let position = getter.get position

                            let result =
                                match username, position with
                                | Some username, Some position ->
                                    let dayStart = getter.get (Atoms.RecoilUser.dayStartFamily username)

                                    isToday dayStart position (DateId date)
                                | _ -> false

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof isTodayFamily))
                            result)
                }

            let rec hasSelectionFamily =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof hasSelectionFamily))
                    get (fun (date: FlukeDate) getter ->
                            let selection = getter.get selection

                            let result =
                                selection
                                |> Map.values
                                |> Seq.exists (fun dateSequence -> dateSequence |> Set.contains date)

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof hasSelectionFamily))
                            result

                        )
                }

        module rec RecoilSession =
            let rec state =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilSession) (nameof state))
                    get (fun (username: Username) getter ->
                            let state = getter.get Atoms.state
                            let user = getter.get (Atoms.RecoilSession.userFamily username)
                            let dateSequence = getter.get dateSequence
                            let view = getter.get view
                            let position = getter.get position
                            let getLivePosition = (getter.get Atoms.getLivePosition).Get
                            let treeSelectionIds = getter.get (Atoms.RecoilSession.treeSelectionIdsFamily username)

                            let result =
                                match user, position, state with
                                | Some user, Some position, Some state ->
                                    let newTreeSelectionIds =
                                        if treeSelectionIds.IsEmpty then
                                            state.Session.TreeSelection
                                            |> Set.map (fun treeState -> treeState.Id)
                                        else
                                            treeSelectionIds

                                    let newState =
                                        FakeBackend.getState
                                            {|
                                                User = user
                                                DateSequence = dateSequence
                                                View = view
                                                Position = position
                                                TreeSelectionIds = newTreeSelectionIds
                                                TreeStateMap = state.Session.TreeStateMap
                                                GetLivePosition = getLivePosition
                                            |}

                                    Some newState
                                | _ -> None

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilSession) (nameof state))
                            result)
                    set (fun (username: Username) setter (newValue: State option) ->
                            let dateSequence = setter.get dateSequence

                            Profiling.addTimestamp "state.set[0]"

                            printfn
                                "dateSequence tree newValue==none=%A dateSequence.length=%A"
                                newValue.IsNone
                                dateSequence.Length

                            match newValue with
                            | Some state ->
                                match state.Session with
                                | { User = Some user } as session when user.Username = username ->
//                                    setter.set (Atoms.state, newValue)
//                                    setter.set (Atoms.getLivePosition, {| Get = session.GetLivePosition |})
//                                    setter.set (Atoms.username, Some user.Username)

                                    let recoilSession = setter.get (Atoms.RecoilSession.sessionFamily user.Username)

                                    let treeSelectionIds =
                                        state.Session.TreeSelection
                                        |> Set.map (fun treeState -> treeState.Id)

                                    let availableTreeIds =
                                        state.Session.TreeStateMap
                                        |> Map.values
                                        |> Seq.map (fun treeState -> treeState.Id)
                                        |> Seq.toList

                                    let taskIdList =
                                        state.Session.TaskList
                                        |> List.map (fun task -> Atoms.RecoilTask.taskId task)

//                                    setter.set (recoilSession.User, Some user)
                                    setter.set (recoilSession.TreeSelectionIds, treeSelectionIds)
                                    setter.set (recoilSession.AvailableTreeIds, availableTreeIds)
                                    setter.set (recoilSession.TaskIdList, taskIdList)

                                    let recoilInformationMap =
                                        state.Session.TaskList
                                        |> Seq.map (fun task -> task.Information)
                                        |> Seq.distinct
                                        |> Seq.map (fun information -> state.Session.InformationStateMap.[information])
                                        |> Seq.map (fun informationState ->


                                            let informationId =
                                                Atoms.RecoilInformation.informationId informationState.Information

                                            setter.set
                                                (Atoms.RecoilInformation.wrappedInformation informationId,
                                                 informationState.Information)
                                            setter.set
                                                (Atoms.RecoilInformation.attachments informationId,
                                                 informationState.Attachments)
                                            informationState.Information, informationId)
                                        |> Map.ofSeq

                                    Profiling.addTimestamp "state.set[1]"

                                    state.Session.TaskList
                                    |> List.map (fun task -> state.Session.TaskStateMap.[task])
                                    |> List.iter (fun taskState ->
                                        let task = taskState.Task
                                        let taskId = Atoms.RecoilTask.taskId task

                                        let recoilTask = setter.get (Atoms.RecoilTask.taskFamily taskId)

                                        setter.set (recoilTask.Id, taskId)
                                        setter.set (recoilTask.Name, task.Name)
                                        setter.set (recoilTask.InformationId, recoilInformationMap.[task.Information])
                                        setter.set (recoilTask.PendingAfter, task.PendingAfter)
                                        setter.set (recoilTask.MissedAfter, task.MissedAfter)
                                        setter.set (recoilTask.Scheduling, task.Scheduling)
                                        setter.set (recoilTask.Priority, task.Priority)
                                        //                                setter.set (recoilTask.Sessions, taskState.Sessions) // TODO: move from here
                                        setter.set (recoilTask.Attachments, taskState.Attachments)
                                        setter.set (recoilTask.Duration, task.Duration)

                                        dateSequence
                                        |> List.iter (fun date ->
                                            let cellId = Atoms.RecoilCell.cellId taskId (DateId date)

                                            let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)

                                            setter.set (recoilCell.Id, cellId)
                                            setter.set (recoilCell.TaskId, taskId)
                                            setter.set (recoilCell.Date, date))

                                        taskState.CellStateMap
                                        |> Map.filter (fun dateId cellState ->
                                            (<>) cellState.Status Disabled
                                            || not cellState.Attachments.IsEmpty
                                            || not cellState.Sessions.IsEmpty)
                                        |> Map.iter (fun dateId cellState ->
                                            let cellId = Atoms.RecoilCell.cellId taskId dateId

                                            let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)

                                            setter.set (recoilCell.Status, cellState.Status)
                                            setter.set (recoilCell.Attachments, cellState.Attachments)
                                            setter.set (recoilCell.Sessions, cellState.Sessions)
                                            setter.set (recoilCell.Selected, false)))


                                    state.Session.TreeStateMap
                                    |> Map.values
                                    |> Seq.iter (fun treeState ->
                                        let recoilTree = setter.get (Atoms.RecoilTree.treeFamily treeState.Id)

                                        setter.set (recoilTree.Name, treeState.Name)
                                        setter.set (recoilTree.Owner, Some treeState.Owner)
                                        setter.set (recoilTree.SharedWith, treeState.SharedWith)
                                        setter.set (recoilTree.Position, treeState.Position))

                                | _ ->
                                    setter.set (Atoms.state, None)
                                    setter.reset Atoms.getLivePosition
                                    setter.set (Atoms.username, None)
                            | _ -> ()

                            Profiling.addTimestamp "state.set[2]"
                            Profiling.addCount (nameof state + " (SET)"))
                }

            let rec currentSession =
                selectorFamily {
                    key ("selector/" + nameof currentSession)
                    get (fun (username: Username) getter ->
                            let state = getter.get (state username)

                            let result =
                                match state with
                                | Some state -> Some state.Session
                                | None -> None

                            Profiling.addCount (nameof currentSession)
                            result)
                }


        module rec RecoilInformation =
            ()

        module rec RecoilTask =
            let rec lastSessionFamily =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof lastSessionFamily))
                    get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                            let username = getter.get Atoms.username
                            match username with
                            | Some username ->
                                let state = getter.get (RecoilSession.state username)

                                let sessions =
                                    match state with
                                    | Some state ->
                                        state.Session.TaskStateMap
                                        |> Map.tryPick (fun task taskState ->
                                            let taskId' = Atoms.RecoilTask.taskId task
                                            if taskId = taskId' then
                                                Some taskState
                                            else
                                                None)
                                        |> Option.map (fun taskState -> taskState.Sessions)
                                        |> Option.defaultValue []
                                    | None -> []

                                let result =
                                    sessions
                                    |> List.sortByDescending (fun (TaskSession (start, _, _)) -> start.DateTime)
                                    |> List.tryHead

                                Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof lastSessionFamily))
                                result
                            | None -> None)
                }

            let rec activeSessionFamily =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof activeSessionFamily))
                    get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                            let position = getter.get position
                            let lastSession = getter.get (lastSessionFamily taskId)


                            let result =
                                match position, lastSession with
                                | Some position, Some lastSession ->
                                    let (TaskSession (start, (Minute duration), (Minute breakDuration))) = lastSession

                                    let currentDuration = (position.DateTime - start.DateTime).TotalMinutes

                                    let active = currentDuration < duration + breakDuration

                                    match active with
                                    | true -> Some currentDuration
                                    | false -> None

                                | _ -> None

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof activeSessionFamily))

                            result)
                }

            let rec showUserFamily =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof showUserFamily))
                    get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                            let username = getter.get Atoms.username
                            match username with
                            | Some username ->
                                let currentSession = getter.get (RecoilSession.currentSession username)

                                let result =
                                    currentSession
                                    |> Option.map (fun session -> session.TaskStateMap)
                                    |> Option.defaultValue Map.empty
                                    |> Map.tryPick (fun task taskState ->
                                        if taskId = Atoms.RecoilTask.taskId task then
                                            Some taskState
                                        else
                                            None)
                                    |> Option.map (fun taskState ->
                                        let usersCount =
                                            taskState.CellStateMap
                                            |> Map.values
                                            |> Seq.map (fun cellState -> cellState.Status)
                                            |> Seq.choose (function
                                                | UserStatus (user, _) -> Some user
                                                | _ -> None)
                                            |> Seq.distinct
                                            |> Seq.length

                                        usersCount > 1)
                                    |> Option.defaultValue false

                                Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof showUserFamily))
                                result
                            | None -> false)
                }

            let rec hasSelectionFamily =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof hasSelectionFamily))
                    get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                            let selection = getter.get selection

                            let result =
                                selection
                                |> Map.tryFind taskId
                                |> Option.defaultValue Set.empty
                                |> Set.isEmpty
                                |> not

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof hasSelectionFamily))
                            result)
                }


        module rec RecoilTree =
            //            let rec taskListFamily =
//                selectorFamily {
//                    key (sprintf "%s/%s" (nameof RecoilTree) (nameof taskListFamily))
//                    get (fun (treeId: TreeId) getter ->
//                            let taskIdList =
//                                getter.get (Atoms.RecoilTree.taskIdListFamily treeId)
//
//                            let taskList =
//                                match state with
//                                | Some state ->
//                                    taskIdList
//                                    |> List.map (fun taskId ->
//                                        let task =
//                                            getter.get (Atoms.RecoilTask.taskFamily taskId)
//
//                                        let informationId = getter.get task.InformationId
//
//                                        let information =
//                                            getter.get (Atoms.RecoilInformation.informationFamily informationId)
//
//                                        {|
//                                            Id = taskId
//                                            Name = getter.get task.Name
//                                            Information = getter.get information.WrappedInformation
//                                            InformationAttachments = getter.get information.Attachments
//                                            Scheduling = getter.get task.Scheduling
//                                            PendingAfter = getter.get task.PendingAfter
//                                            MissedAfter = getter.get task.MissedAfter
//                                            Priority = getter.get task.Priority
//    //                                        Sessions = getter.get task.Sessions
//                                            Attachments = getter.get task.Attachments
//                                            Duration = getter.get task.Duration
//                                        |})
//                                | None -> []
//
//                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilTree) (nameof taskListFamily))
//                            taskList)
//                }
            ()
        //

        let rec currentTaskList =
            selector {
                key ("selector/" + nameof currentTaskList)
                get (fun getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = getter.get (Atoms.RecoilSession.taskIdListFamily username)

                            let result =
                                taskIdList
                                |> List.map (fun taskId ->
                                    //                                    state.TaskStateMap.[task]
                                    let task = getter.get (Atoms.RecoilTask.taskFamily taskId)

                                    let informationId = getter.get task.InformationId
                                    let information = Atoms.RecoilInformation.wrappedInformation informationId
                                    let attachments = Atoms.RecoilInformation.attachments informationId

                                    {|
                                        Id = taskId
                                        Name = getter.get task.Name
                                        Information = getter.get information
                                        InformationAttachments = getter.get attachments
                                        Scheduling = getter.get task.Scheduling
                                        PendingAfter = getter.get task.PendingAfter
                                        MissedAfter = getter.get task.MissedAfter
                                        Priority = getter.get task.Priority
                                        Attachments = getter.get task.Attachments
                                        Duration = getter.get task.Duration
                                    |})

                            //
                            //                            |> List.map (fun task ->
                            //                                getter.get (Atoms.RecoilTask.taskFamily (Atoms.RecoilTask.taskId task)))

                            Profiling.addCount (nameof currentTaskList)
                            result
                        | None -> [])
            }

        let rec activeSessions =
            selector {
                key ("selector/" + nameof activeSessions)
                get (fun getter ->
                        let taskList = getter.get currentTaskList
                        let username = getter.get Atoms.username

                        let result =
                            match username with
                            | Some username ->
                                let user = getter.get (Atoms.RecoilUser.userFamily username)
                                let sessionLength = getter.get user.SessionLength
                                let sessionBreakLength = getter.get user.SessionBreakLength
                                taskList
                                |> List.map (fun task ->
                                    let (TaskName taskName) = task.Name

                                    let duration = getter.get (RecoilTask.activeSessionFamily task.Id)

                                    duration
                                    |> Option.map (fun duration ->
                                        ActiveSession (taskName, Minute duration, sessionLength, sessionBreakLength)))
                                |> List.choose id
                            | _ -> []

                        Profiling.addCount (nameof activeSessions)
                        result)
            }

        let rec weekCellsMap =
            selector {
                key ("selector/" + nameof weekCellsMap)
                get (fun getter ->
                        let username = getter.get Atoms.username
                        let position = getter.get position
                        let taskList = getter.get currentTaskList

                        let result =
                            match username, position with
                            | Some username, Some position ->
                                let user = getter.get (Atoms.RecoilUser.userFamily username)
                                let dayStart = getter.get user.DayStart
                                let weekStart = getter.get user.WeekStart

                                let weeks =
                                    [
                                        -1 .. 1
                                    ]
                                    |> List.map (fun weekOffset ->
                                        let dateIdSequence =
                                            let rec getStartDate (date: DateTime) =
                                                if date.DayOfWeek = weekStart then
                                                    date
                                                else
                                                    getStartDate (date.AddDays -1)

                                            let startDate =
                                                dateId dayStart position
                                                |> fun (DateId referenceDay) ->
                                                    referenceDay.DateTime.AddDays (7 * weekOffset)
                                                |> getStartDate

                                            [
                                                0 .. 6
                                            ]
                                            |> List.map startDate.AddDays
                                            |> List.map FlukeDateTime.FromDateTime
                                            |> List.map (dateId dayStart)

                                        let result =
                                            taskList
                                            |> List.collect (fun task ->
                                                dateIdSequence
                                                |> List.map (fun ((DateId referenceDay) as dateId) ->
                                                    //                                                    let taskId = getter.get task.Id
                                                    let cellId = Atoms.RecoilCell.cellId task.Id dateId

                                                    let cell = getter.get (Atoms.RecoilCell.cellFamily cellId)

                                                    let status = getter.get cell.Status
                                                    let sessions = getter.get cell.Sessions
                                                    let attachments = getter.get cell.Attachments

                                                    let isToday =
                                                        getter.get (RecoilFlukeDate.isTodayFamily referenceDay)

                                                    match status, sessions, attachments with
                                                    | (Disabled
                                                      | Suggested),
                                                      [],
                                                      [] -> None
                                                    | _ ->
                                                        {|
                                                            DateId = dateId
                                                            Task = task
                                                            Status = status
                                                            Sessions = sessions
                                                            IsToday = isToday
                                                            Attachments = attachments
                                                        |}
                                                        |> Some)
                                                |> List.choose id)
                                            |> List.groupBy (fun x -> x.DateId)
                                            |> List.map (fun (((DateId referenceDay) as dateId), cells) ->

                                                //                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList
                                                let taskSessions = cells |> List.collect (fun x -> x.Sessions)

                                                let sortedTasksMap =
                                                    cells
                                                    |> List.map (fun cell ->
                                                        let taskState =
                                                            //                                                            let informationId = getter.get cell.Task.InformationId
//
//                                                            let information =
//                                                                getter.get
//                                                                    (Atoms.RecoilInformation.informationFamily
//                                                                        informationId)

                                                            let task =
                                                                { Task.Default with
                                                                    Name = cell.Task.Name
                                                                    Information = cell.Task.Information
                                                                    Scheduling = cell.Task.Scheduling
                                                                    PendingAfter = cell.Task.PendingAfter
                                                                    MissedAfter = cell.Task.MissedAfter
                                                                    Priority = cell.Task.Priority
                                                                    Duration = cell.Task.Duration
                                                                }

                                                            {
                                                                Task = task
                                                                Sessions = taskSessions
                                                                Attachments = []
                                                                SortList = []
                                                                //                                                        UserInteractions = cell.Task.UserInteractions
                                                                InformationMap = Map.empty
                                                                CellStateMap = Map.empty
                                                            }

                                                        taskState,
                                                        [
                                                            { Task = taskState.Task; DateId = dateId }, cell.Status
                                                        ])
                                                    |> Sorting.sortLanesByTimeOfDay
                                                        dayStart
                                                           { Date = referenceDay; Time = dayStart }
                                                    |> List.indexed
                                                    |> List.map (fun (i, (taskState, _)) ->
                                                        Atoms.RecoilTask.taskId taskState.Task, i)
                                                    |> Map.ofList

                                                let newCells =
                                                    cells
                                                    |> List.sortBy (fun cell -> sortedTasksMap.[cell.Task.Id])

                                                dateId, newCells)
                                            |> Map.ofList

                                        result)

                                weeks
                            | _ -> []

                        Profiling.addCount (nameof weekCellsMap)
                        result)
            }


        module rec RecoilCell =
            let rec selectedFamily =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof selectedFamily))
                    get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                            let cell = getter.get (Atoms.RecoilCell.cellFamily cellId)

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilCell) (nameof selectedFamily))
                            getter.get cell.Selected)
                    set (fun (cellId: Atoms.RecoilCell.CellId) setter (newValue: bool) ->
                            let ctrlPressed = setter.get Atoms.ctrlPressed
                            let shiftPressed = setter.get Atoms.shiftPressed

                            let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)

                            let date = setter.get cell.Date
                            let taskId = setter.get cell.TaskId

                            let newSelection =
                                let swapSelection oldSelection taskId date =
                                    let oldSet =
                                        oldSelection
                                        |> Map.tryFind taskId
                                        |> Option.defaultValue Set.empty

                                    let newSet =
                                        let fn =
                                            if newValue then
                                                Set.add
                                            else
                                                Set.remove

                                        fn date oldSet

                                    oldSelection |> Map.add taskId newSet

                                let taskList = setter.get currentTaskList
                                match shiftPressed, ctrlPressed with
                                | true, _ ->
                                    let oldSelection = setter.get Atoms.selection

                                    let selectionTaskList =
                                        taskList
                                        |> List.mapi (fun i task ->
                                            match oldSelection |> Map.tryFind taskId with
                                            | Some oldSelectionDates ->
                                                let selectionDates =
                                                    match oldSelectionDates.IsEmpty, task.Id = taskId with
                                                    | _, true -> oldSelectionDates |> Set.add date
                                                    | true, _ -> Set.empty
                                                    | false, _ -> oldSelectionDates

                                                {|
                                                    Index = i
                                                    TaskId = task.Id
                                                    Range = Set.minElement selectionDates, Set.maxElement selectionDates
                                                |}
                                                |> Some
                                            | _ -> None)
                                        |> List.choose id

                                    let minDates, maxDates =
                                        selectionTaskList
                                        |> List.map (fun selectionTask -> selectionTask.Range)
                                        |> List.unzip

                                    let minDate =
                                        match minDates with
                                        | [] -> None
                                        | x -> Some (List.min x)

                                    let maxDate =
                                        match maxDates with
                                        | [] -> None
                                        | x -> Some (List.max x)

                                    match minDate, maxDate with
                                    | Some minDate, Some maxDate ->
                                        let newSet =
                                            [
                                                minDate
                                                maxDate
                                            ]
                                            |> Rendering.getDateSequence (0, 0)
                                            |> Set.ofList

                                        taskList
                                        |> List.indexed
                                        |> List.skipWhile (fun (i, task) ->
                                            i < (selectionTaskList
                                                 |> List.tryHead
                                                 |> Option.map (fun x -> x.Index)
                                                 |> Option.defaultValue 0))
                                        |> List.takeWhile (fun (i, task) ->
                                            i
                                            <= (selectionTaskList
                                                |> List.tryLast
                                                |> Option.map (fun x -> x.Index)
                                                |> Option.defaultValue 0))
                                        |> List.map (fun (i, task) -> task.Id, newSet)
                                        |> Map.ofList
                                    | _ -> Map.empty

                                | false, false ->
                                    let newTaskSelection =
                                        if newValue then
                                            Set.singleton date
                                        else
                                            Set.empty

                                    [
                                        taskId, newTaskSelection
                                    ]
                                    |> Map.ofList
                                | false, true ->
                                    let oldSelection = setter.get Atoms.selection
                                    swapSelection oldSelection taskId date

                            setter.set (selection, newSelection)
                            Profiling.addCount (sprintf "%s/%s (SET)" (nameof RecoilCell) (nameof selectedFamily)))
                }

    /// [1]

    let initState (initializer: MutableSnapshot) =
        let baseState = RootPrivateData.State.getBaseState ()

        //        let state2 = {| User =state.User; TreeStateMap = state.TreeStateMap |}
//
//        let simpleJson = Fable.SimpleJson.SimpleJson.stringify state2
//        let thothJson = Thoth.Json.Encode.Auto.toString(4, state2)
//
        Ext.setDom (nameof baseState) baseState
        //        Browser.Dom.window?flukeStateSimple <- simpleJson
//        Browser.Dom.window?flukeStateThoth <- thothJson

        match baseState.Session.User with
        | Some user ->
            initializer.set (Atoms.state, Some baseState)
            initializer.set (Atoms.getLivePosition, {| Get = baseState.Session.GetLivePosition |})
            initializer.set (Atoms.username, Some user.Username)
            initializer.set (Atoms.RecoilSession.userFamily user.Username, Some user)
        | None -> ()

    (************************* END *************************)





    //                    {|
//                        Name = "task1"
//                        Information = Area { Name = "area1" }
//                        Scheduling = Recurrency (Offset (Days 1))
//                        PendingAfter = None
//                        MissedAfter = None
//                        Priority = Critical10
//                        Duration = Some 30
//                        Sessions = [
//                            flukeDateTime 2020 Month.May 20 02 05
//                            flukeDateTime 2020 Month.May 31 00 09
//                            flukeDateTime 2020 Month.May 31 23 21
//                            flukeDateTime 2020 Month.June 04 01 16
//                            flukeDateTime 2020 Month.June 20 15 53
//                        ]
//                        Lane = [
//                            (FlukeDate.Create 2020 Month.June 13), Completed
//                        ]
//                        Comments = [
//                            Comment (TempData.Users.fc1943s, "fc1943s: task1 comment")
//                            Comment (TempData.Users.liryanne, "liryanne: task1 comment")
//                        ]
//                    |}
//
//                    {|
//                        Name = "task2"
//                        Information = Area { Name = "area2" }
//                        Scheduling = Recurrency (Offset (Days 1))
//                        PendingAfter = None
//                        MissedAfter = FlukeTime.Create 09 00 |> Some
//                        Priority = Critical10
//                        Duration = Some 5
//                        Sessions = []
//                        Lane = []
//                        Comments = []
//                    |}
//
//                {|
//                    Id = TreeId "liryanne/shared"
//                    Access = [ TreeAccess.Owner TempData.Users.liryanne
//                               TreeAccess.Admin TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//
//                {|
//                    Id = TreeId "fluke/samples/laneSorting/frequency"
//                    Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
//                               TreeAccess.ReadOnly TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//
//                {|
//                    Id = TreeId "fluke/samples/laneSorting/timeOfDay"
//                    Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
//                               TreeAccess.ReadOnly TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//            treeList
//            |> List.tryFind (fun tree -> tree.Id = treeId && hasAccess tree user)


    //        let state = {|
//            Input_User = TempData.Users.fc1943s
//            Input_View = View.Calendar
//            Input_Position = FlukeDate.Create 2020 Month.June 28
//            TreeList = [
//                {|
//                    Id = TreeId "fc1943s/tree/default"
//                    Access = [ TreeAccess.Owner TempData.Users.fc1943s ]
//                    InformationList = [
//                        {|
//                            Information = Area { Name = "area1" }
//                            Comments = [
//                                Comment (TempData.Users.fc1943s, "fc1943s: area1 area/information comment")
//                                Comment (TempData.Users.liryanne, "liryanne: area1 area/information comment")
//                            ]
//                        |}
//
//                        {|
//                            Information = Area { Name = "area2" }
//                            Comments = []
//                        |}
//                    ]
//                    Tasks = [
//                        {|
//                            Name = "task1"
//                            Information = Area { Name = "area1" }
//                            Scheduling = Recurrency (Offset (Days 1))
//                            PendingAfter = None
//                            MissedAfter = None
//                            Priority = Critical10
//                            Duration = Some 30
//                            Sessions = [
//                                flukeDateTime 2020 Month.May 20 02 05
//                                flukeDateTime 2020 Month.May 31 00 09
//                                flukeDateTime 2020 Month.May 31 23 21
//                                flukeDateTime 2020 Month.June 04 01 16
//                                flukeDateTime 2020 Month.June 20 15 53
//                            ]
//                            Lane = [
//                                (FlukeDate.Create 2020 Month.June 13), Completed
//                            ]
//                            Comments = [
//                                Comment (TempData.Users.fc1943s, "fc1943s: task1 comment")
//                                Comment (TempData.Users.liryanne, "liryanne: task1 comment")
//                            ]
//                        |}
//
//                        {|
//                            Name = "task2"
//                            Information = Area { Name = "area2" }
//                            Scheduling = Recurrency (Offset (Days 1))
//                            PendingAfter = None
//                            MissedAfter = FlukeTime.Create 09 00 |> Some
//                            Priority = Critical10
//                            Duration = Some 5
//                            Sessions = []
//                            Lane = []
//                            Comments = []
//                        |}
//                    ]
//                |}
//
//                {|
//                    Id = TreeId "liryanne/shared"
//                    Access = [ TreeAccess.Owner TempData.Users.liryanne
//                               TreeAccess.Admin TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//
//                {|
//                    Id = TreeId "fluke/samples/laneSorting/frequency"
//                    Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
//                               TreeAccess.ReadOnly TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//
//                {|
//                    Id = TreeId "fluke/samples/laneSorting/timeOfDay"
//                    Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
//                               TreeAccess.ReadOnly TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//            ]
//        |}

    module private OldData =
        //        type TempDataType =
//            | TempPrivate
//            | TempPublic
//            | Test
//        let tempDataType = TempPrivate
//        let tempDataType = Test
//        let tempDataType = TempPublic



        //        let tempState =
//            let testData = TempData.tempData.RenderLaneTests
//            let testData = TempData.tempData.SortLanesTests

        //            let getNow =
//                match tempDataType with
//                | TempPrivate -> TempData.getNow
//                | TempPublic  -> TempData.getNow
//                | Test        -> testData.GetNow

        //            let dayStart =
//                match tempDataType with
//                | TempPrivate -> TempData.dayStart
//                | TempPublic  -> TempData.dayStart
//                | Test        -> TempData.testDayStart

        //            let informationList =
//                match tempDataType with
//                | TempPrivate -> RootPrivateData.manualTasks.InformationList
//                | TempPublic  -> TempData.tempData.ManualTasks.InformationList
//                | Test        -> []

        //            let taskOrderList =
//                match tempDataType with
//                | TempPrivate -> RootPrivateData.manualTasks.TaskOrderList @ RootPrivateData.taskOrderList
//                | TempPublic  -> TempData.tempData.ManualTasks.TaskOrderList
//                | Test        -> testData.TaskOrderList

        //            let informationCommentsMap =
//                match tempDataType with
//                | TempPrivate ->
//                    RootPrivateData.informationComments
//                    |> List.append RootPrivateData.Shared.informationComments
//                    |> List.groupBy (fun x -> x.Information)
//                    |> Map.ofList
//                    |> Map.mapValues (List.map (fun x -> x.Comment))
//                | TempPublic  -> Map.empty
//                | Test        -> Map.empty

        //            let taskStateList =
//                match tempDataType with
//                | TempPrivate ->
//                    let taskData = RootPrivateData.manualTasks
//                    let sharedTaskData = RootPrivateData.Shared.manualTasks
//
//                    let cellComments =
//                        RootPrivateData.cellComments
//                        |> List.append RootPrivateData.Shared.cellComments
//
//                    let applyState statusEntries comments (taskState: TaskState) =
//                        { taskState with
//                            StatusEntries =
//                                statusEntries
//                                |> createTaskStatusEntries taskState.Task
//                                |> List.prepend taskState.StatusEntries
//                            Comments =
//                                comments
//                                |> List.filter (fun (TaskComment (task, _)) -> task = taskState.Task)
//                                |> List.map (ofTaskComment >> snd)
//                                |> List.prepend taskState.Comments
//                            CellCommentsMap =
//                                cellComments
//                                |> List.filter (fun (CellComment (address, _)) -> address.Task = taskState.Task)
//                                |> List.map (fun (CellComment (address, comment)) -> address.Date, comment)
//                                |> List.groupBy fst
//                                |> Map.ofList
//                                |> Map.mapValues (List.map snd)
//                                |> Map.union taskState.CellCommentsMap }
//
//                    let taskStateList =
//                        taskData.TaskStateList
//                        |> List.map (applyState
//                                         RootPrivateData.cellStatusEntries
//                                         RootPrivateData.taskComments)
//
//                    let sharedTaskStateList =
//                        sharedTaskData.TaskStateList
//                        |> List.map (applyState
//                                         RootPrivateData.Shared.cellStatusEntries
//                                         RootPrivateData.Shared.taskComments)
//
//                    taskStateList |> List.append sharedTaskStateList
//                | TempPublic  -> TempData.tempData.ManualTasks.TaskStateList
//                | Test        -> testData.TaskStateList

        //            let taskStateList =
//                taskStateList
//                |> List.sortByDescending (fun x -> x.StatusEntries.Length)
//                |> List.take 10

        //            let lastSessions =
//                taskStateList
//                |> Seq.filter (fun taskState -> not taskState.Sessions.IsEmpty)
//                |> Seq.map (fun taskState -> taskState.Task, taskState.Sessions)
//                |> Seq.map (Tuple2.mapItem2 (fun sessions ->
//                    sessions
//                    |> Seq.sortByDescending (fun (TaskSession start) -> start.DateTime)
//                    |> Seq.head
//                ))
//                |> Seq.toList

        //            printfn "RETURNING TEMPSTATE."
//            {| GetNow = getNow
//               DayStart = dayStart
//               InformationCommentsMap = informationCommentsMap
//               InformationList = informationList
//               TaskOrderList = taskOrderList
//               TaskStateList = taskStateList
//               LastSessions = lastSessions |}




        //        type TestTemplate =
//            | LaneSorting of string
//            | LaneRendering of string
//
//        [<RequireQualifiedAccess>]
//        type Database =
//            | Test of TestTemplate
//            | Public
//            | Private

        //        let a = [
//            "test", [
//                "laneSorting", [
//
//                ]
//                "laneRendering", [
//
//                ]
//            ]
//            "public", [
//                "default", []
//            ]
//            "private", [
//                "default", []
//            ]
//        ]


        //        let getLivePosition () = RootPrivateData.getLivePosition ()
//
//        let getCurrentUser () = RootPrivateData.currentUser
//
//        let getDayStart () = RootPrivateData.dayStart
//
//        let getWeekStart () = RootPrivateData.weekStart
        ()
