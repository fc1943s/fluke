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
open Fable.Core.JsInterop


module Recoil =
    open Model

    module Profiling =
        let private initialTicks = DateTime.Now.Ticks
        let private ticksDiff ticks =
            int64 (TimeSpan(ticks - initialTicks).TotalMilliseconds)

        let state =
            {| CallCount = Dictionary()
               Timestamps = List<string * int64>() |}
        Browser.Dom.window?profilingState <- state
        let internal addCount id =
            match state.CallCount.ContainsKey id with
            | false -> state.CallCount.[id] <- 1
            | true -> state.CallCount.[id] <- state.CallCount.[id] + 1

        let addTimestamp id =
            state.Timestamps.Add (id, ticksDiff DateTime.Now.Ticks)

        addTimestamp "Init"


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
        ()

    module FakeBackend =
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

        type FakeInformation =
            { Comments: UserComment list
              Information: Information }

        type FakeTree =
            { InformationList: FakeInformation list
              Owner: User
              SharedWith: TreeAccess list
              TaskList: Task list }

        let getLivePosition () =
            RootPrivateData.getLivePosition ()

        let getCurrentUser () =
            RootPrivateData.currentUser

        let getDayStart () =
            RootPrivateData.dayStart

        let hasAccess tree user =
            match tree with
            | tree when tree.Owner = user -> true
            | tree ->
                tree.SharedWith
                |> List.exists (function
                    | TreeAccess.Admin dbUser -> dbUser = user
                    | TreeAccess.ReadOnly dbUser -> dbUser = user)

        let rec filterTaskList view dateRange (taskList: Task list) =
            match view with
            | View.Calendar
            | View.Week ->
                taskList
                |> List.filter (function
                    | { Scheduling = Manual WithoutSuggestion
                        StatusEntries = statusEntries
                        Sessions = sessions }
                        when
                            statusEntries
                            |> List.exists (fun (TaskStatusEntry (user, date, _)) -> date.DateTime >==< dateRange)
                            |> not
                        &&
                            sessions
                            |> List.exists (fun (TaskSession start) -> start.Date.DateTime >==< dateRange)
                            |> not
                        -> false
                    | _ -> true
                )
            | View.Groups ->
                taskList
                |> List.filter (function
                    | { Scheduling = Manual WithoutSuggestion
//                        StatusEntries = []
//                        Sessions = []
                        } -> true
                    | _ -> false
                )
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
                taskList
                |> List.filter (function
                    | { Information = Archive _ } -> false
                    | { Sessions = []
                        Priority = priority } when ofTaskPriorityValue priority < 5 -> false
                    | { Scheduling = Manual _ } -> true
                    | _ -> false)

        let sortLanes (input: {| View: View
                                 DayStart: FlukeTime
                                 Position: FlukeDateTime
                                 TaskOrderList: TaskOrderEntry list
                                 InformationList: Information list
                                 Lanes: OldLane list |}) =
            match input.View with
            | View.Calendar ->
                input.Lanes
                |> Sorting.sortLanesByFrequency
                |> Sorting.sortLanesByIncomingRecurrency input.DayStart input.Position
                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList
            | View.Groups ->
                let lanes =
                    input.Lanes
                    |> Sorting.applyManualOrder input.TaskOrderList
                    |> List.groupBy (fun (OldLane (task, _)) -> task.Information)
                    |> Map.ofList

                input.InformationList
                |> List.map (fun information ->
                    let lanes =
                        lanes
                        |> Map.tryFind information
                        |> Option.defaultValue []

                    information, lanes
                )
                |> List.collect snd
            | View.Tasks ->
                input.Lanes
                |> Sorting.applyManualOrder input.TaskOrderList
                |> List.sortByDescending (fun (OldLane (task, _)) ->
                    task.Priority
                    |> ofTaskPriorityValue
                )
            | View.Week ->
                input.Lanes

        let getTree (input: {| User: User
                               DayStart: FlukeTime
                               DateSequence: FlukeDate list
                               View: View
                               Position: FlukeDateTime |}) =

            let informationList =
                let commentsMap =
                    RootPrivateData.informationComments
                    |> List.groupBy (fun x -> x.Information)
                    |> Map.ofList
                    |> Map.mapValues (List.map (fun x -> x.Comment))

                RootPrivateData.sharedTreeData.InformationList @ RootPrivateData.treeData.InformationList
                |> List.map (fun information ->
                    { Information = information
                      Comments =
                          commentsMap
                          |> Map.tryFind information
                          |> Option.defaultValue [] }
                )

            let taskList =
                let treeData = RootPrivateData.treeData
                let sharedTreeData = RootPrivateData.sharedTreeData

                let applyEvents statusEntries taskComments (task: Task) =
                    let newStatusEntries =
                        statusEntries
                        |> createTaskStatusEntries task
                        |> List.prepend task.StatusEntries
                    let newTaskComments =
                        taskComments
                        |> List.filter (fun (TaskComment (commentTask, _)) -> commentTask = task)
                        |> List.map (ofTaskComment >> snd)
                        |> List.prepend task.Comments
                    let cellCommentsMap =
                        let externalCellComments =
                            RootPrivateData.cellComments
                            |> List.filter (fun (CellComment (task', moment, userCcomment)) -> task' = task)
                            |> List.map (fun (CellComment (user, address, userComment)) ->
                                address.Date, userComment
                            )
                        externalCellComments @ task.CellComments
                        |> List.map (Tuple2.mapItem1 DateId)
                        |> List.groupBy fst
                        |> Map.ofList
                        |> Map.mapValues (List.map snd)
                    let sessionsMap =
                        task.Sessions
                        |> List.map (fun (TaskSession start) -> dateId input.DayStart start, TaskSession start)
                        |> List.groupBy fst
                        |> Map.ofList
                        |> Map.mapValues (List.map snd)
                    let cellStateMap =
                        seq {
                            yield! sessionsMap |> Map.keys
                            yield! cellCommentsMap |> Map.keys
                        }
                        |> Seq.distinct
                        |> Seq.map (fun dateId ->
                            let sessions =
                                sessionsMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue []
                            let cellComments =
                                cellCommentsMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue []
                            let cellState =
                                { Status = Disabled
                                  Sessions = sessions
                                  Comments = cellComments }
                            dateId, cellState
                        )
                        |> Map.ofSeq
                    { task with
                        StatusEntries = newStatusEntries
                        Comments = newTaskComments
                        CellStateMap = cellStateMap }

                let privateTaskList =
                    let oldEvents, newEvents = RootPrivateData.cellStatusEntries
                    treeData.TaskList
                    |> List.map (applyEvents
                                     oldEvents
                                     RootPrivateData.taskComments)

                let sharedTaskList =
                    let oldEvents, newEvents = RootPrivateData.sharedCellStatusEntries
                    sharedTreeData.TaskList
                    |> List.map (applyEvents
                                     oldEvents
                                     RootPrivateData.sharedTaskComments)

                sharedTaskList @ privateTaskList

            let dateRange =
                // TODO: handle
                let head = input.DateSequence |> List.head |> fun x -> x.DateTime
                let last = input.DateSequence |> List.last |> fun x -> x.DateTime
                head, last

            let filteredTaskList =
                filterTaskList input.View dateRange taskList

            let filteredLanes =
                filteredTaskList
                |> List.map (fun task ->
                    Rendering.renderLane
                        input.DayStart
                        input.Position
                        input.DateSequence
                        task
                )

            let taskOrderList =
                RootPrivateData.treeData.TaskOrderList // @ RootPrivateData.taskOrderList

            let sortedTaskList =
                sortLanes
                    {| View = input.View
                       DayStart = input.DayStart
                       Position = input.Position
                       TaskOrderList = taskOrderList
                       InformationList = informationList |> List.map (fun x -> x.Information)
                       Lanes = filteredLanes |}
                |> List.map ofLane
                |> List.map (Tuple2.mapItem2 (fun cells ->
                    cells
                    |> List.map (fun (Cell (address, status)) ->
                        address.DateId, status
                    )
                    |> Map.ofList
                ))

//                    let sortedTaskList =
//                        sortedTaskList
////                        |> List.sortByDescending (fun x -> x.StatusEntries.Length)
//                        |> List.take 50

            let newTaskList =
                sortedTaskList
                |> List.map (fun (task, statusMap) ->
                    let newCellStateMap =
                        seq {
                            yield! task.CellStateMap |> Map.keys
                            yield! statusMap |> Map.keys
                        }
                        |> Seq.distinct
                        |> Seq.map (fun dateId ->
                            let status =
                                statusMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue Disabled
                            let state =
                                task.CellStateMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue
                                    { Status = Disabled
                                      Sessions = []
                                      Comments = [] }
                            dateId, { state with Status = status }
                        )
                        |> Map.ofSeq

                    { task with CellStateMap = newCellStateMap }
                )
            {
                Owner = input.User
                SharedWith = []
                InformationList = informationList
                TaskList = newTaskList
            }









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
//                            (flukeDate 2020 Month.June 13), Completed
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
//                        MissedAfter = flukeTime 09 00 |> Some
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
//            Input_Position = flukeDate 2020 Month.June 28
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
//                                (flukeDate 2020 Month.June 13), Completed
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
//                            MissedAfter = flukeTime 09 00 |> Some
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



    module Atoms =

        module RecoilInformation =
            type InformationId = InformationId of id:string
            type RecoilInformation =
                { Id: RecoilValue<InformationId, ReadWrite>
                  WrappedInformation: RecoilValue<Information, ReadWrite>
                  Comments: RecoilValue<UserComment list, ReadWrite> }
            let rec idFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof idFamily))
                def (fun (_taskId: InformationId) -> InformationId "")
            }
            let rec wrappedInformationFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof wrappedInformationFamily))
                def (fun (_taskId: InformationId) -> Area Area.Default)
            }
            let rec commentsFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof commentsFamily))
                def (fun (_informationId: InformationId) -> [])
            }
            type RecoilInformation with
                static member internal Create informationId =
                    { Id = idFamily informationId
                      WrappedInformation = wrappedInformationFamily informationId
                      Comments = commentsFamily informationId }

            let rec informationId (information: Information) : InformationId =
                match information with
                | Project x  -> sprintf "%s/%s" information.KindName x.Name
                | Area x     -> sprintf "%s/%s" information.KindName x.Name
                | Resource x -> sprintf "%s/%s" information.KindName x.Name
                | Archive x  ->
                    let (InformationId archiveId) = informationId x
                    sprintf "%s/%s" information.KindName archiveId
                |> InformationId
            let rec informationFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof informationFamily))
                def (fun (informationId: InformationId) -> RecoilInformation.Create informationId)
            }

        module RecoilTask =
            let taskId (task: Task) =
                TaskId (task.Information.Name, task.Name)
            type RecoilTask =
                { Id: RecoilValue<TaskId, ReadWrite>
                  InformationId: RecoilValue<RecoilInformation.InformationId, ReadWrite>
                  Name: RecoilValue<string, ReadWrite>
                  Scheduling: RecoilValue<TaskScheduling, ReadWrite>
                  PendingAfter: RecoilValue<FlukeTime option, ReadWrite>
                  MissedAfter: RecoilValue<FlukeTime option, ReadWrite>
                  Priority: RecoilValue<TaskPriorityValue, ReadWrite>
                  Sessions: RecoilValue<TaskSession list, ReadWrite>
                  Comments: RecoilValue<UserComment list, ReadWrite>
                  Duration: RecoilValue<int option, ReadWrite> }
            let rec idFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof idFamily))
                def (fun (_taskId: TaskId) -> taskId Task.Default)
            }
            let rec informationIdFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof informationIdFamily))
                def (fun (_taskId: TaskId) ->
                    RecoilInformation.informationId Task.Default.Information)
            }
            let rec nameFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof nameFamily))
                def (fun (_taskId: TaskId) -> Task.Default.Name)
            }
            let rec schedulingFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof schedulingFamily))
                def (fun (_taskId: TaskId) -> Task.Default.Scheduling)
            }
            let rec pendingAfterFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof pendingAfterFamily))
                def (fun (_taskId: TaskId) -> Task.Default.PendingAfter)
            }
            let rec missedAfterFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof missedAfterFamily))
                def (fun (_taskId: TaskId) -> Task.Default.MissedAfter)
            }
            let rec priorityFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof priorityFamily))
                def (fun (_taskId: TaskId) -> Task.Default.Priority)
            }
            let rec sessionsFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof sessionsFamily))
                def (fun (_taskId: TaskId) -> Task.Default.Sessions)
            }
            let rec commentsFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof commentsFamily))
                def (fun (_taskId: TaskId) -> Task.Default.Comments)
            }
            let rec durationFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof durationFamily))
                def (fun (_taskId: TaskId) -> Task.Default.Duration)
            }
            type RecoilTask with
                static member internal Create taskId =
                    { Id = idFamily taskId
                      InformationId = informationIdFamily taskId
                      Name = nameFamily taskId
                      Scheduling = schedulingFamily taskId
                      PendingAfter = pendingAfterFamily taskId
                      MissedAfter = missedAfterFamily taskId
                      Priority = priorityFamily taskId
                      Sessions = sessionsFamily taskId
                      Comments = commentsFamily taskId
                      Duration = durationFamily taskId }
            let rec taskFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof taskFamily))
                def (fun (taskId: TaskId) -> RecoilTask.Create taskId)
            }

        module RecoilCell =
            type CellId = CellId of id:string
            type RecoilCell =
                { Id: RecoilValue<CellId, ReadWrite>
                  TaskId: RecoilValue<TaskId, ReadWrite>
                  Date: RecoilValue<FlukeDate, ReadWrite>
                  Status: RecoilValue<CellStatus, ReadWrite>
                  Comments: RecoilValue<UserComment list, ReadWrite>
                  Sessions: RecoilValue<TaskSession list, ReadWrite>
                  Selected: RecoilValue<bool, ReadWrite> }

            let rec idFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof idFamily))
                def (fun (_cellId: CellId) -> CellId "")
            }
            let rec taskIdFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof taskIdFamily))
                def (fun (_cellId: CellId) -> TaskId ("", ""))
            }
            let rec dateFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof dateFamily))
                def (fun (_cellId: CellId) -> TempData.Consts.defaultDate)
            }
            let rec statusFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof statusFamily))
                def (fun (_cellId: CellId) -> Disabled)
            }
            let rec commentsFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof commentsFamily))
                def (fun (_cellId: CellId) -> [] : UserComment list)
            }
            let rec sessionsFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof sessionsFamily))
                def (fun (_cellId: CellId) -> [] : TaskSession list)
            }
            let rec selectedFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof selectedFamily))
                def (fun (_cellId: CellId) -> false)
            }
            type RecoilCell with
                static member internal Create cellId =
                    { Id = idFamily cellId
                      TaskId = taskIdFamily cellId
                      Date = dateFamily cellId
                      Status = statusFamily cellId
                      Comments = commentsFamily cellId
                      Sessions = sessionsFamily cellId
                      Selected = selectedFamily cellId }
            let cellId (TaskId (informationName, taskName)) (DateId referenceDay) =
                CellId (sprintf "%s/%s/%s" informationName taskName (referenceDay.DateTime.Format "yyyy-MM-dd"))

            let rec cellFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof cellFamily))
                def (fun (cellId: CellId) -> RecoilCell.Create cellId)
            }
        module RecoilTree =
            type TreeId = TreeId of id:string
            type RecoilTree =
                { Id: RecoilValue<TreeId, ReadWrite>
                  Owner: RecoilValue<User, ReadWrite>
                  SharedWith: RecoilValue<TreeAccess list, ReadWrite>
                  Position: RecoilValue<FlukeDateTime, ReadWrite>
                  InformationIdList: RecoilValue<RecoilInformation.InformationId list, ReadWrite>
                  TaskIdList: RecoilValue<TaskId list, ReadWrite> }
            let rec idFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTree) (nameof idFamily))
                def (fun (_treeId: TreeId) -> TreeId "")
            }
            let rec ownerFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof ownerFamily))
                def (fun (_treeId: TreeId) -> TempData.Users.testUser)
            }
            let rec sharedWithFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof sharedWithFamily))
                def (fun (_treeId: TreeId) -> [])
            }
            let rec positionFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof positionFamily))
                def (fun (_treeId: TreeId) -> { Date = TempData.Consts.defaultDate; Time = TempData.Consts.dayStart })
            }
            let rec informationIdListFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof informationIdListFamily))
                def (fun (_treeId: TreeId) -> [])
            }
            let rec taskIdListFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof taskIdListFamily))
                def (fun (_treeId: TreeId) -> [])
            }
            type RecoilTree with
                static member internal Create treeId =
                    { Id = idFamily treeId
                      Owner = ownerFamily treeId
                      SharedWith = sharedWithFamily treeId
                      Position = positionFamily treeId
                      InformationIdList = informationIdListFamily treeId
                      TaskIdList = taskIdListFamily treeId }
            let treeId owner name =
                TreeId (sprintf "%s/%s" owner.Username name)
            let rec treeFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTree) (nameof treeFamily))
                def (fun (treeId: TreeId) -> RecoilTree.Create treeId)
            }

        let rec internal debug = atom {
            key ("atom/" + nameof debug)
            def false
            local_storage
        }
        let rec internal path = atom {
            key ("atom/" + nameof path)
            def (Router.currentPath ())
        }
        let rec internal treeName = atom {
            key ("atom/" + nameof treeName)
            def "default"
            local_storage
        }
        let rec internal user = atom {
            key ("atom/" + nameof user)
            def (FakeBackend.getCurrentUser ())
        }
//        let rec internal view = atom {
//            key ("atom/" + nameof view)
//            def View.Calendar
////            local_storage
//        }
        let rec internal dayStart = atom {
            key ("atom/" + nameof dayStart)
            def (FakeBackend.getDayStart ())
        }
        let rec internal selection = atom {
            key ("atom/" + nameof selection)
            def (Map.empty : Map<TaskId, Set<FlukeDate>>)
        }
        let rec internal ctrlPressed = atom {
            key ("atom/" + nameof ctrlPressed)
            def false
        }
        let rec internal shiftPressed = atom {
            key ("atom/" + nameof shiftPressed)
            def false
        }
        let rec internal positionTrigger = atom {
            key ("atom/" + nameof positionTrigger)
            def 0
        }
        let rec internal tree = atom {
            key ("atom/" + nameof tree)
            def (None : FakeBackend.FakeTree option)
        }

    module Selectors =
        let rec view = selector {
            key ("selector/" + nameof view)
            get (fun getter ->
                let path = getter.get Atoms.path
                let view =
                    match path with
                    | [ "view"; "Calendar" ] -> View.Calendar
                    | [ "view"; "Groups" ] -> View.Groups
                    | [ "view"; "Tasks" ] -> View.Tasks
                    | [ "view"; "Week" ] -> View.Week
                    | _ -> View.Calendar

                Profiling.addCount (nameof view)
                view
            )
        }
        let rec position = selector {
            key ("selector/" + nameof position)
            get (fun getter ->
                let _positionTrigger = getter.get Atoms.positionTrigger
                let newPosition = FakeBackend.getLivePosition ()

                Profiling.addCount (nameof position)
                newPosition
            )
            set (fun setter _newValue ->
                setter.set (Atoms.positionTrigger, fun x -> x + 1)
                Profiling.addCount (nameof position + " (SET)")
            )
        }
        let rec dateSequence = selector {
            key ("selector/" + nameof dateSequence)
            get (fun getter ->
                let position = getter.get position

                Profiling.addCount (nameof dateSequence)
                [ position.Date ] |> Rendering.getDateSequence (45, 20)
            )
        }
        let rec currentTree = selector {
            key ("selector/" + nameof currentTree)
            get (fun getter ->
                Profiling.addCount (nameof currentTree)
                getter.get Atoms.tree
            )
            set (fun setter (newValue: FakeBackend.FakeTree option) ->
                let dateSequence = setter.get dateSequence

                Profiling.addTimestamp "currentTree.set[0]"

                match newValue with
                | None -> ()
                | Some tree ->
//                    let tree =
//                        { tree with TaskList = tree.TaskList |> List.take 3 }

                    let recoilInformationList =
                        tree.InformationList
                        |> List.map (fun information ->
                            let informationId = Atoms.RecoilInformation.informationId information.Information
                            let recoilInformation = setter.get (Atoms.RecoilInformation.informationFamily informationId)
                            setter.set (recoilInformation.Id, informationId)
                            setter.set (recoilInformation.WrappedInformation, information.Information)
                            setter.set (recoilInformation.Comments, information.Comments)
                            information.Information, informationId
                        )

                    let recoilInformationMap =
                        recoilInformationList
                        |> Map.ofList

                    let taskIdList = tree.TaskList |> List.map Atoms.RecoilTask.taskId

                    Profiling.addTimestamp "currentTree.set[1]"

                    taskIdList
                    |> List.iter (fun taskId ->
                        dateSequence
                        |> List.iter (fun date ->
                            let cellId = Atoms.RecoilCell.cellId taskId (DateId date)
                            let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                            setter.set (recoilCell.Id, cellId)
                            setter.set (recoilCell.TaskId, taskId)
                            setter.set (recoilCell.Date, date)
                        )
                    )

                    tree.TaskList
                    |> List.iter (fun task ->
                        let taskId = Atoms.RecoilTask.taskId task
                        let recoilTask = setter.get (Atoms.RecoilTask.taskFamily taskId)
                        setter.set (recoilTask.Id, taskId)
                        setter.set (recoilTask.Name, task.Name)
                        setter.set (recoilTask.InformationId, recoilInformationMap.[task.Information])
                        setter.set (recoilTask.PendingAfter, task.PendingAfter)
                        setter.set (recoilTask.MissedAfter, task.MissedAfter)
                        setter.set (recoilTask.Scheduling, task.Scheduling)
                        setter.set (recoilTask.Priority, task.Priority)
                        setter.set (recoilTask.Sessions, task.Sessions)
                        setter.set (recoilTask.Comments, task.Comments)
                        setter.set (recoilTask.Duration, task.Duration)

                        task.CellStateMap
                        |> Map.filter (fun _ cellState ->
                            cellState.Status <> Disabled
                            || not cellState.Comments.IsEmpty
                            || not cellState.Sessions.IsEmpty
                        )
                        |> Map.iter (fun dateId cellState ->
                            let cellId = Atoms.RecoilCell.cellId taskId dateId
                            let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                            setter.set (recoilCell.Status, cellState.Status)
                            setter.set (recoilCell.Comments, cellState.Comments)
                            setter.set (recoilCell.Sessions, cellState.Sessions)
                            setter.set (recoilCell.Selected, false)
                        )
                    )

                    let treeName = setter.get Atoms.treeName
                    let treeId = Atoms.RecoilTree.treeId tree.Owner treeName
                    let recoilTree = setter.get (Atoms.RecoilTree.treeFamily treeId)
                    setter.set (recoilTree.Owner, tree.Owner)
                    setter.set (recoilTree.SharedWith, tree.SharedWith)
                    setter.set (recoilTree.InformationIdList, recoilInformationList |> List.map snd) // TODO: use it
                    setter.set (recoilTree.TaskIdList, taskIdList)

                setter.set (Atoms.tree, newValue)

                Profiling.addTimestamp "currentTree.set[2]"
                Profiling.addCount (nameof currentTree + " (SET)")
            )
        }
        let rec currentTaskMap = selector {
            key ("selector/" + nameof currentTaskMap)
            get (fun getter ->
                let tree = getter.get currentTree

                Profiling.addCount (nameof currentTaskMap)
                match tree with
                | None -> Map.empty
                | Some tree ->
                    tree.TaskList
                    |> List.map (fun task -> taskId task, task)
                    |> Map.ofList
            )
        }
        /// [1]
        let rec selection = selector {
            key ("selector/" + nameof selection)
            get (fun getter ->
                let selection = getter.get Atoms.selection
                Profiling.addCount (nameof selection)
                selection
            )
            set (fun setter (newSelection: Map<TaskId, Set<FlukeDate>>) ->
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
                        let taskSelection = selection |> Map.tryFind taskId |> Option.defaultValue Set.empty
                        let newTaskSelection = newSelection |> Map.tryFind taskId |> Option.defaultValue Set.empty
                        let datesToIgnore = Set.intersect taskSelection newTaskSelection
                        let datesToUnselect =
                            datesToIgnore
                            |> Set.difference taskSelection
                            |> Seq.map (fun date -> date, false)
                        let datesToSelect =
                            datesToIgnore
                            |> Set.difference newTaskSelection
                            |> Seq.map (fun date -> date, true)
                        taskId, Seq.append datesToSelect datesToUnselect
                    )

                operationsByTask
                |> Seq.iter (fun (taskId, operations) ->
                    operations
                    |> Seq.iter (fun (date, selected) ->
                        let cellId = Atoms.RecoilCell.cellId taskId (DateId date)
                        let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                        setter.set (cell.Selected, selected)
                    )
                )

                setter.set (Atoms.selection, newSelection)
                Profiling.addCount (nameof selection + "(SET)")
            )
        }
        /// [3]
        let rec selectedCells = selector {
            key ("selector/" + nameof selectedCells)
            get (fun getter ->
                let selection = Recoil.useValue selection

                let selectionCellIds =
                    selection
                    |> Seq.collect (fun (KeyValue (taskId, dates)) ->
                        dates
                        |> Seq.map DateId
                        |> Seq.map (Atoms.RecoilCell.cellId taskId)
                    )

                Profiling.addCount (nameof selectedCells)
                selectionCellIds
                |> Seq.map (Atoms.RecoilCell.cellFamily >> getter.get)
                |> Seq.toList
            )
        }
        /// [4]
        // TODO: Remove View and check performance
        let rec treeAsync = selectorFamily {
            key ("selectorFamily/" + nameof treeAsync)
            get (fun (view: View) getter -> async {
                Profiling.addTimestamp "treeAsync.get[0]"
                let dayStart = getter.get Atoms.dayStart
                let position = getter.get position
                let dateSequence = getter.get dateSequence
                let user = getter.get Atoms.user

                Profiling.addTimestamp "treeAsync.get[1]"

                let tree =
                    FakeBackend.getTree
                        {| User = user
                           DayStart = dayStart
                           DateSequence = dateSequence
                           View = view
                           Position = position |}

                Profiling.addTimestamp "treeAsync.get[2]"
                printfn "TREE COUNT: %A" tree.TaskList.Length
                Profiling.addCount (nameof treeAsync)
                return tree
            })
        }

        module rec RecoilFlukeDate =
            let rec isTodayFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof isTodayFamily))
                get (fun (date: FlukeDate) getter ->
                    let dayStart = getter.get Atoms.dayStart
                    let position = getter.get position
                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof isTodayFamily))
                    isToday dayStart position (DateId date)
                )
            }
            let rec hasSelectionFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof hasSelectionFamily))
                get (fun (date: FlukeDate) getter ->
                    let selection = getter.get selection

                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof hasSelectionFamily))
                    selection
                    |> Map.values
                    |> Seq.exists (fun dateSequence -> dateSequence |> Set.contains date)
                )
            }

        module rec RecoilInformation =
            ()

        module rec RecoilTask =
            let rec lastSessionFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof lastSessionFamily))
                get (fun (taskId: TaskId) getter ->
                    let task = getter.get (Atoms.RecoilTask.taskFamily taskId)
                    let sessions = getter.get task.Sessions

                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof lastSessionFamily))
                    sessions
                    |> List.sortByDescending (fun (TaskSession start) -> start.DateTime)
                    |> List.tryHead
                )
            }
            let rec activeSessionFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof activeSessionFamily))
                get (fun (taskId: TaskId) getter ->
                    let position = getter.get position
                    let lastSession = getter.get (lastSessionFamily taskId)

                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof activeSessionFamily))

                    lastSession
                    |> Option.bind (fun (TaskSession start) ->
                        let durationMinutes = (position.DateTime - start.DateTime).TotalMinutes
                        let active = durationMinutes < TempData.Consts.sessionLength + TempData.Consts.sessionBreakLength
                        match active with
                        | true -> Some durationMinutes
                        | false -> None
                    )
                )
            }
            let rec showUserFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof showUserFamily))
                get (fun (taskId: TaskId) getter ->
                    let taskMap = getter.get currentTaskMap

                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof showUserFamily))
                    taskMap
                    |> Map.tryFind taskId
                    |> Option.map (fun task ->
                        task.StatusEntries
                        |> Seq.map (fun (TaskStatusEntry (user, moment, manualCellStatus)) -> user)
                        |> Seq.distinct
                        |> Seq.length
                        |> fun x -> x > 1
                    )
                    |> Option.defaultValue false
                )
            }
            let rec hasSelectionFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof hasSelectionFamily))
                get (fun (taskId: TaskId) getter ->
                    let selection = getter.get selection

                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof hasSelectionFamily))
                    selection
                    |> Map.tryFind taskId
                    |> Option.defaultValue Set.empty
                    |> Set.isEmpty
                    |> not
                )
            }


        module rec RecoilTree =
            let rec taskListFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilTree) (nameof taskListFamily))
                get (fun (treeId: Atoms.RecoilTree.TreeId) getter ->
                    let taskIdList = getter.get (Atoms.RecoilTree.taskIdListFamily treeId)

                    let taskList =
                        taskIdList
                        |> List.map (fun taskId ->
                            let task = getter.get (Atoms.RecoilTask.taskFamily taskId)

                            let informationId = getter.get task.InformationId
                            let information = getter.get (Atoms.RecoilInformation.informationFamily informationId)

                            {| Id = taskId
                               Name = getter.get task.Name
                               Information = getter.get information.WrappedInformation
                               InformationComments = getter.get information.Comments
                               Scheduling = getter.get task.Scheduling
                               PendingAfter = getter.get task.PendingAfter
                               MissedAfter = getter.get task.MissedAfter
                               Priority = getter.get task.Priority
                               Sessions = getter.get task.Sessions
                               Comments = getter.get task.Comments
                               Duration = getter.get task.Duration |}
                        )

                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilTree) (nameof taskListFamily))
                    taskList
                )
            }


        let rec currentTaskList = selector {
            key ("selector/" + nameof currentTaskList)
            get (fun getter ->
                let user = getter.get Atoms.user
                let treeName = getter.get Atoms.treeName

                let treeId = Atoms.RecoilTree.treeId user treeName
                let taskList = getter.get (RecoilTree.taskListFamily treeId)

                Profiling.addCount (nameof currentTaskList)
                taskList
            )
        }
        let rec activeSessions = selector {
            key ("selector/" + nameof activeSessions)
            get (fun getter ->
                let taskList = getter.get currentTaskList
                Profiling.addCount (nameof activeSessions)
                taskList
                |> List.map (fun task ->
                    let duration = getter.get (RecoilTask.activeSessionFamily task.Id)
                    duration
                    |> Option.map (fun duration -> ActiveSession (task.Name, duration))
                )
                |> List.choose id
            )
        }

        let rec weekCellsMap = selector {
            key ("selector/" + nameof weekCellsMap)
            get (fun getter ->
                let dayStart = getter.get Atoms.dayStart
                let position = getter.get position
                let taskList = getter.get currentTaskList

                let weeks =
                    [ -1 .. 1 ]
                    |> List.map (fun weekOffset ->
                        let dateIdSequence =
                            let rec getStartDate (date:DateTime) =
                                if date.DayOfWeek = TempData.Consts.weekStart
                                then date
                                else getStartDate (date.AddDays -1)
                            let startDate =
                                dateId dayStart position
                                |> ofDateId
                                |> fun referenceDay -> referenceDay.DateTime.AddDays (7 * weekOffset)
                                |> getStartDate

                            [ 0 .. 6 ]
                            |> List.map startDate.AddDays
                            |> List.map FlukeDateTime.FromDateTime
                            |> List.map (dateId dayStart)

                        let result =
                            taskList
                            |> List.collect (fun task ->
                                dateIdSequence
                                |> List.map (fun dateId ->
                                    let cellId = Atoms.RecoilCell.cellId task.Id dateId
                                    let cell = getter.get (Atoms.RecoilCell.cellFamily cellId)
                                    let status = getter.get cell.Status
                                    let sessions = getter.get cell.Sessions
                                    let comments = getter.get cell.Comments

                                    let isToday = getter.get (RecoilFlukeDate.isTodayFamily (ofDateId dateId))

                                    match status, sessions, comments with
                                    | (Disabled | Suggested), [], [] -> None
                                    | _ ->
                                        {| DateId = dateId
                                           Task = task
                                           Status = status
                                           Sessions = sessions
                                           IsToday = isToday
                                           Comments = comments |}
                                        |> Some
                                )
                                |> List.choose id
                            )
                            |> List.groupBy (fun x -> x.DateId)
                            |> List.map (fun (dateId, cells) ->

        //                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList

                                let sortedTasksMap =
                                    cells
                                    |> List.map (fun cell ->
                                        let task =
                                            { Task.Default with
                                                Name = cell.Task.Name
                                                Information = cell.Task.Information
                                                Scheduling = cell.Task.Scheduling
                                                PendingAfter = cell.Task.PendingAfter
                                                MissedAfter = cell.Task.MissedAfter
                                                Priority = cell.Task.Priority
                                                Sessions = cell.Task.Sessions
                                                Comments = cell.Task.Comments
                                                Duration = cell.Task.Duration }
                                        OldLane (task, [ Cell ({ Task = task; DateId = dateId }, cell.Status) ])
                                    )
                                    |> Sorting.sortLanesByTimeOfDay dayStart { Date = ofDateId dateId; Time = dayStart } []
                                    |> List.indexed
                                    |> List.map (fun (i, OldLane (task, _)) ->
                                        Atoms.RecoilTask.taskId task, i
                                    )
                                    |> Map.ofList

                                let newCells =
                                    cells
                                    |> List.sortBy (fun cell -> sortedTasksMap.[cell.Task.Id])

                                dateId, newCells
                            )
                            |> Map.ofList
                        result
                    )

                Profiling.addCount (nameof weekCellsMap)
                weeks
            )
        }


        module rec RecoilCell =
            let rec selectedFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof selectedFamily))
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    let cell = getter.get (Atoms.RecoilCell.cellFamily cellId)

                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilCell) (nameof selectedFamily))
                    getter.get cell.Selected
                )
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
                                let fn = if newValue then Set.add else Set.remove
                                fn date oldSet
                            oldSelection |> Map.add taskId newSet

                        match shiftPressed, ctrlPressed with
                        | true, _ ->
                            let taskList = setter.get currentTaskList
                            let oldSelection = setter.get Atoms.selection

                            let selectionTaskList =
                                taskList
                                |> List.mapi (fun i task ->
                                    match oldSelection |> Map.tryFind task.Id with
                                    | Some oldSelectionDates ->
                                        let selectionDates =
                                            match oldSelectionDates.IsEmpty, task.Id = taskId with
                                            | _, true -> oldSelectionDates |> Set.add date
                                            | true, _ -> Set.empty
                                            | false, _ -> oldSelectionDates
                                        {| Index = i
                                           TaskId = task.Id
                                           Range = Set.minElement selectionDates, Set.maxElement selectionDates |}
                                        |> Some
                                    | _ -> None
                                )
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
                                    [ minDate; maxDate ]
                                    |> Rendering.getDateSequence (0, 0)
                                    |> Set.ofList

                                taskList
                                |> List.indexed
                                |> List.skipWhile (fun (i, task) ->
                                    i < (selectionTaskList |> List.tryHead |> Option.map (fun x -> x.Index) |> Option.defaultValue 0)
                                )
                                |> List.takeWhile (fun (i, task) ->
                                    i <= (selectionTaskList |> List.tryLast |> Option.map (fun x -> x.Index) |> Option.defaultValue 0)
                                )
                                |> List.map (fun (i, task) ->
                                    task.Id, newSet
                                )
                                |> Map.ofList
                            | _ -> Map.empty

                        | false, false ->
                            let newTaskSelection = if newValue then Set.singleton date else Set.empty
                            Map.empty |> Map.add taskId newTaskSelection
                        | false, true ->
                            let oldSelection = setter.get Atoms.selection
                            swapSelection oldSelection taskId date

                    setter.set (selection, newSelection)
                    Profiling.addCount (sprintf "%s/%s (SET)" (nameof RecoilCell) (nameof selectedFamily))
                )
            }

        /// [1]

