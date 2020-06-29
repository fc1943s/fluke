namespace Fluke.UI.Frontend

open System
open System.Collections.Generic
open FSharpPlus
open Fable.Core.JsInterop
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.React
open Fable.DateFunctions
open Suigetsu.Core


module Recoil =
    open Model

    module Profiling =
        let private callCount = Dictionary<string, int>()
        Browser.Dom.window?callCount <- callCount
        Browser.Dom.window?callCountClear <- fun () -> callCount.Clear ()
        let addCount id =
            if not (callCount.ContainsKey id) then
                callCount.[id] <- 0
            callCount.[id] <- callCount.[id] + 1
            mountById "diag" (str (Fable.SimpleJson.SimpleJson.stringify Browser.Dom.window?callCount))

    module OldData =
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
//                |> Seq.map (Tuple2.mapSnd (fun sessions ->
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

        [<RequireQualifiedAccess>]
        type TreeAccess =
            | Owner of user:User
            | Admin of user:User
            | ReadOnly of user:User

        type TreeId = TreeId of id:string

        type Tree =
            { Id: TreeId
              Access: TreeAccess list
              Position: FlukeDateTime
              InformationList: Information list
              TaskList: Task list }

        let getLivePosition () =
            FlukeDateTime.FromDateTime DateTime.Now

        let getCurrentUser () =
            RootPrivateData.currentUser

        let getDayStart () =
            TempData.dayStart

        let hasAccess tree user =
            tree.Access
            |> List.exists (function
                | TreeAccess.Owner dbUser -> dbUser = user
                | TreeAccess.Admin dbUser -> dbUser = user
                | TreeAccess.ReadOnly dbUser -> dbUser = user)

        let rec filterTaskList view dateRange (taskList: Task list) =
            match view with
            | View.Calendar ->
                taskList
                |> List.filter (function
                    | { Scheduling = Manual WithoutSuggestion
                        StatusEntries = statusEntries
                        Sessions = sessions
                        LaneMap = laneMap }
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
                taskList
                |> List.filter (function
                    | { Scheduling = Manual WithoutSuggestion
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
                taskList
                |> List.filter (function { Scheduling = Manual _ } -> true | _ -> false)
            | View.Week ->
                taskList

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

                input.InformationList
                |> List.map (fun information ->
                    let lanes =
                        lanes
                        |> List.filter (fun (OldLane (task, _)) -> task.Information = information)

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
                []

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

                RootPrivateData.treeData.InformationList
                |> List.append RootPrivateData.sharedTreeData.InformationList
                |> List.map (fun information ->
                    {|
                        Information = information
                        Comments =
                            commentsMap
                            |> Map.tryFind information
                            |> Option.defaultValue []
                    |}
                )

            {|
                Id = TreeId "fc1943s/tree/default"
                Access = [ TreeAccess.Owner TempData.Users.fc1943s ]
                InformationList = informationList
                Tasks =
                    let taskList =
                        let treeData = RootPrivateData.treeData
                        let sharedTreeData = RootPrivateData.treeData

                        let applyEvents statusEntries comments (task: Task) =
                            let newLaneCommentMap =
                                RootPrivateData.cellComments
                                |> List.filter (fun (CellComment (address, _)) -> address.Task = task)
                                |> List.map (fun (CellComment (address, comment)) -> address.Date, comment)
                                |> List.groupBy fst
                                |> Map.ofList
                                |> Map.mapValues (List.map snd)
                            { task with
                                StatusEntries =
                                    statusEntries
                                    |> createTaskStatusEntries task
                                    |> List.prepend task.StatusEntries
                                Comments =
                                    comments
                                    |> List.filter (fun (TaskComment (commentTask, _)) -> commentTask = task)
                                    |> List.map (ofTaskComment >> snd)
                                    |> List.prepend task.Comments
                                LaneMap =
                                    task.LaneMap
                                    |> Map.map (fun date lane ->
                                        let newComments =
                                            newLaneCommentMap
                                            |> Map.tryFind date
                                            |> Option.defaultValue []
                                        { lane with
                                            Comments =
                                                lane.Comments
                                                |> List.append newComments }
                                    ) }

                        let taskList =
                            treeData.TaskList
                            |> List.map (applyEvents
                                             RootPrivateData.cellStatusEntries
                                             RootPrivateData.taskComments)

                        let sharedTaskList =
                            sharedTreeData.TaskList
                            |> List.map (applyEvents
                                             RootPrivateData.sharedCellStatusEntries
                                             RootPrivateData.sharedTaskComments)

                        taskList |> List.append sharedTaskList

                    let taskList =
                        taskList
                        |> List.sortByDescending (fun x -> x.StatusEntries.Length)
                        |> List.take 10

                    let dateRange =
                        let dateSequence = input.DateSequence
                        let head = dateSequence |> List.head |> fun x -> x.DateTime
                        let last = dateSequence |> List.last |> fun x -> x.DateTime
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
                        RootPrivateData.treeData.TaskOrderList// @ RootPrivateData.taskOrderList

                    let sortedLaneStatusMap =
                        sortLanes
                            {| View = input.View
                               DayStart = input.DayStart
                               Position = input.Position
                               TaskOrderList = taskOrderList
                               InformationList = informationList |> List.map (fun x -> x.Information)
                               Lanes = filteredLanes |}
                        |> List.map ofLane
                        |> Map.ofList
                        |> Map.map (fun _task cells ->
                            cells
                            |> List.map (fun (Cell (address, status)) ->
                                address.Date, status
                            )
                            |> Map.ofList
                        )

                    taskList
                    |> List.map (fun task ->
                        let mergedLaneMap =
                            sortedLaneStatusMap
                            |> Map.tryFind task
                            |> Option.defaultValue Map.empty
                            |> Map.map (fun date status ->
                                let lane =
                                    task.LaneMap
                                    |> Map.tryFind date
                                    |> Option.defaultValue
                                        { Comments = []
                                          Status = Disabled }
                                { lane with Status = status }
                            )
                        let newLaneMap =
                            task.LaneMap
                            |> Map.union mergedLaneMap

                        { task with LaneMap = newLaneMap }
//                        let newStatus =
//                            sortedLaneStatusMap
//                            |> Map.tryFind task
//                            |> Option.defaultValue Map.empty
//                        { task with
//                            LaneMap =
//
//                                |> Option.defaultValue
//                                    { Comments = []
//                                      Sessions = []
//                                      Status = Disabled } }
                    )
            |}
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
                  Comments: RecoilValue<Comment list, ReadWrite> }
            let rec idFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof idFamily)
                def (fun (_taskId: InformationId) -> InformationId "")
            }
            let rec wrappedInformationFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof wrappedInformationFamily)
                def (fun (_taskId: InformationId) -> Area Area.Default)
            }
            let rec commentsFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof commentsFamily)
                def (fun (_informationId: InformationId) -> [])
            }
            type RecoilInformation with
                static member internal Create informationId =
                    { Id = idFamily informationId
                      WrappedInformation = wrappedInformationFamily informationId
                      Comments = commentsFamily informationId }

            let rec informationId (information: Information) : InformationId =
                match information with
                | Project x  -> information.KindName + "/" + x.Name
                | Area x     -> information.KindName + "/" + x.Name
                | Resource x -> information.KindName + "/" + x.Name
                | Archive x  ->
                    let (InformationId archiveId) = informationId x
                    information.KindName + "/" + archiveId
                |> InformationId
            let rec informationFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof informationFamily)
                def (fun (informationId: InformationId) -> RecoilInformation.Create informationId)
            }

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
//            let rec laneFamily = atomFamily {
//                key (nameof laneFamily)
//                get (fun (taskId: Atoms.RecoilTask.TaskId) ->
//                    let now = getter.get Atoms.now
//                    let dayStart = getter.get Atoms.dayStart
//                    let dateSequence = getter.get dateSequence
//                    let taskState = getter.get (taskStateFamily taskId)
//                    Profiling.addCount (nameof laneFamily)
//                    Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
//                )
//            }
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
                CellId (taskId + "/" + (date.DateTime.Format "yyyy-MM-dd"))
//                RecoilCell.Create cellId

            let rec cellFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof cellFamily)
                def (fun (cellId: CellId) -> RecoilCell.Create cellId)
            }
        module RecoilTree =
            type RecoilTree =
                { Id: RecoilValue<FakeBackend.TreeId, ReadWrite>
                  Access: RecoilValue<FakeBackend.TreeAccess list, ReadWrite>
                  Position: RecoilValue<FlukeDateTime, ReadWrite>
                  InformationList: RecoilValue<RecoilInformation.InformationId list, ReadWrite>
                  TaskList: RecoilValue<RecoilTask.TaskId list, ReadWrite> }
            let rec idFamily = atomFamily {
                key (nameof RecoilTree + "/" + nameof idFamily)
                def (fun (_treeId: FakeBackend.TreeId) -> FakeBackend.TreeId "")
            }
            let rec accessFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof accessFamily)
                def (fun (_treeId: FakeBackend.TreeId) -> [])
            }
            let rec positionFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof positionFamily)
                def (fun (_treeId: FakeBackend.TreeId) -> flukeDateTime 0000 Month.January 01 00 00)
            }
            let rec informationListFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof informationListFamily)
                def (fun (_treeId: FakeBackend.TreeId) -> [])
            }
            let rec taskListFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof taskListFamily)
                def (fun (_treeId: FakeBackend.TreeId) -> [])
            }
            type RecoilTree with
                static member internal Create treeId =
                    { Id = idFamily treeId
                      Access = accessFamily treeId
                      Position = positionFamily treeId
                      InformationList = informationListFamily treeId
                      TaskList = taskListFamily treeId }

            let rec treeFamily = atomFamily {
                key (nameof RecoilTree + "/" + nameof treeFamily)
                def (fun (treeId: FakeBackend.TreeId) -> RecoilTree.Create treeId)
            }

        let rec user = atom {
            key (nameof user)
            def (async {
                Profiling.addCount (nameof user)
                return FakeBackend.getCurrentUser ()
            })
        }
        let rec view = atom {
            key (nameof view)
            def View.Calendar
            local_storage
        }
        let rec dayStart = atom {
            key (nameof dayStart)
            def (async {
                Profiling.addCount (nameof dayStart)
                return FakeBackend.getDayStart ()
            })
        }
        let rec selection = atom {
            key (nameof selection)
            def (Map.empty : Map<RecoilTask.TaskId, Set<FlukeDate>>)
        }
        let rec ctrlPressed = atom {
            key (nameof ctrlPressed)
            def false
        }
        let rec activeSessions = atom {
            key (nameof activeSessions)
            def ([] : ActiveSession list)
        }
//        let rec dayStart = atom {
//            key (nameof dayStart)
//            def (flukeTime 00 00)
//        }
//        let rec now = atom {
//            key (nameof now)
//            def (flukeDateTime 0000 Month.January 01 00 00)
//        }
//        let rec hovered = atom {
//            key (nameof hovered)
//            def Hover.None
//        }
//        let rec taskOrderList = atom {
//            key (nameof taskOrderList)
//            def ([] : TaskOrderEntry list)
//        }
//        let rec informationList = atom {
//            key (nameof informationList)
//            def ([] : Information list)
//        }
//        let rec lastSessions = atom {
//            key (nameof lastSessions)
//            def ([] : (Task * TaskSession) list)
//        }
//        let rec taskStateList = atom {
//            key (nameof taskStateList)
//            def ([] : TaskState list)
//        }

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
//                Profiling.addCount (nameof taskSelectionFamily)
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
//                Profiling.addCount (nameof cellSelectedFamily)
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
        let rec positionTrigger = atom {
            key (nameof positionTrigger)
            def 0
        }

    module Selectors =

//        let rec treeMap = recoil {
//            let! user = Atoms.user
//            let! view = Atoms.view
//            let! position = Atoms.position
//
//            let treeMap =
//                FakeBackend.getTreeMap
//                    {| User = user
//                       View = view
//                       Position = position |}
//
//            Profiling.addCount (nameof treeMap)
//            return treeMap
//        }


        let rec position = selector {
            key (nameof position)
            get (fun getter -> async {
                let positionTrigger = getter.get Atoms.positionTrigger
                Profiling.addCount (nameof position)
                let newPosition = FakeBackend.getLivePosition ()
                printfn "NEWPOSITION: %A. TRIGGER: %A" newPosition positionTrigger
                return newPosition
            })
            set (fun setter _newValue ->
                Profiling.addCount (nameof position + " (SET)")
                setter.set (Atoms.positionTrigger, fun x -> x + 1)
            )
        }
        let rec dateSequence = recoil {
            let! position = position

            printfn "DATESEQUENCE. position: %A" position

            Profiling.addCount (nameof dateSequence)
            return [ position.Date ] |> Rendering.getDateSequence (45, 20)
        }
        let rec dateRange = recoil {
            let! dateSequence = dateSequence

            Profiling.addCount (nameof dateRange)
            let head = dateSequence |> List.head |> fun x -> x.DateTime
            let last = dateSequence |> List.last |> fun x -> x.DateTime
            return head, last
        }
        let rec treeFamily = selectorFamily {
            key (nameof treeFamily)
            get (fun (position: FlukeDateTime) getter ->
                let user = getter.get Atoms.user
                let dayStart = getter.get Atoms.dayStart
                let dateSequence = getter.get dateSequence
                let view = getter.get Atoms.view

                printfn "TREE: %A" (user.Username, dayStart, dateSequence.Length, view, position)

                Profiling.addCount (nameof treeFamily)
                let tree =
                    FakeBackend.getTree
                        {| User = user
                           DayStart = dayStart
                           DateSequence = dateSequence
                           View = view
                           Position = position |}
                tree
            )
        }
//        let rec tree position = recoil {
//            let! user = Atoms.user
//            let! dayStart = Atoms.dayStart
//            let! dateSequence = dateSequence
//            let! view = Atoms.view
//
//            printfn "TREE: %A" (user.Username, dayStart, dateSequence.Length, view, position)
//
//            Profiling.addCount (nameof tree)
//            let tree =
//                FakeBackend.getTree
//                    {| User = user
//                       DayStart = dayStart
//                       DateSequence = dateSequence
//                       View = view
//                       Position = position |}
//            return tree
//        }
        let rec informationMapFamily = selectorFamily {
            key (nameof informationMapFamily)
            get (fun (position: FlukeDateTime) getter ->
                let tree = getter.get (treeFamily position)
                Profiling.addCount (nameof informationMapFamily)
                tree.InformationList
                |> List.map (fun information ->
                    let informationId = Atoms.RecoilInformation.informationId information.Information
                    informationId, information
                )
                |> Map.ofList
            )
        }
//        let rec informationMap = recoil {
//            let! position = position
//            let! tree = treeFamily position
//            Profiling.addCount (nameof informationMap)
//            return tree.InformationList
//            |> List.map (fun information ->
//                let informationId = Atoms.RecoilInformation.informationId information.Information
//                informationId, information
//            )
//            |> Map.ofList
//        }
        let rec taskIdListFamily = selectorFamily {
            key (nameof taskIdListFamily)
            get (fun (position: FlukeDateTime) getter ->
                let tree = getter.get (treeFamily position)
                Profiling.addCount (nameof taskIdListFamily)
                tree.Tasks |> List.map Atoms.RecoilTask.taskId
            )
        }
//        let rec taskIdList = recoil {
//            let! position = position
//            let! tree = treeFamily position
//            Profiling.addCount (nameof taskIdList)
//            return tree.Tasks |> List.map Atoms.RecoilTask.taskId
//        }
        let rec taskMapFamily = selectorFamily {
            key (nameof taskMapFamily)
            get (fun (position: FlukeDateTime) getter ->
                let tree = getter.get (treeFamily position)
                Profiling.addCount (nameof taskMapFamily)
                tree.Tasks
                |> List.map (fun task ->
                    let taskId = Atoms.RecoilTask.taskId task
                    taskId, task
                )
                |> Map.ofList
            )
        }
//        let rec taskMap = recoil {
//            let! position = position
//            let! tree = treeFamily position
//            Profiling.addCount (nameof taskMap)
//            return tree.Tasks
//            |> List.map (fun task ->
//                let taskId = Atoms.RecoilTask.taskId task
//                taskId, task
//            )
//            |> Map.ofList
//        }
        let rec taskFamily = selectorFamily {
            key (nameof taskFamily)
            get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                let position = getter.get position
                let taskMap = getter.get (taskMapFamily position)
                Profiling.addCount (nameof taskFamily)
                taskMap.[taskId]
            )
        }
        let rec laneFamily = selectorFamily {
            key (nameof laneFamily)
            get (fun (taskId: Atoms.RecoilTask.TaskId, date: FlukeDate) getter ->
                let task = getter.get (taskFamily taskId)
                Profiling.addCount (nameof laneFamily)
                task.LaneMap
                |> Map.tryFind date
                |> Option.defaultValue
                    { Comments = []
                      Status = Disabled }
            )
        }


//        let rec filteredTaskStateList = recoil {
//            printfn "filteredTaskStateList"
//
//            let! view = Atoms.view
//            let! taskStateList = Atoms.taskStateList
//            let! dateRange = dateRange
//
//            printfn "- before len: %A" taskStateList.Length
//
//            let result =
//                FakeBackend.filterTaskStateList view dateRange taskStateList
//
//            printfn "- RESULT: %A" result.Length
//            Profiling.addCount (nameof filteredTaskStateList)
//            return result
//        }

//        let rec filteredTaskStateMap = recoil {
//            printfn "filteredTaskStateMap"
//            let! filteredTaskStateList = filteredTaskStateList
//
//            Profiling.addCount (nameof filteredTaskStateMap)
//            return
//                filteredTaskStateList
//                |> List.map (fun taskState ->
//    //                    let taskId = getter.get(Atoms.RecoilTask.taskFamily taskState.Task).Id |> getter.get
//                    let taskId = Atoms.RecoilTask.taskId taskState.Task
//                    taskId, taskState
//                )
//                |> Map.ofList
//        }

//                return Temp.tempState.TaskStateMap.[task]
//            let taskStateMap =
//                taskStateList
//                |> List.map (fun taskState -> taskState.Task, taskState)
//                |> Map.ofList
//        let rec taskStateMap = selector {
//            key (nameof taskStateMap)
//            get (fun (taskId: Atoms.RecoilTask.TaskId) getter -> async {
//                let filteredTaskStateList = getter.get filteredTaskStateList
//                Profiling.addCount (nameof taskStateMap)
//                printfn "taskStateFamily. task: %A" task.Name
//                return Temp.tempState.TaskStateMap.[task]
//            })
//        }
//            let taskStateMap =
//                taskStateList
//                |> List.map (fun taskState -> taskState.Task, taskState)
//                |> Map.ofList
//
//        let rec taskStateFamily = selectorFamily {
//            key (nameof taskStateFamily)
//            get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
//                printfn "taskStateFamily"
//                let filteredTaskStateMap = getter.get filteredTaskStateMap
//                Profiling.addCount (nameof taskStateFamily)
////                taskStateMap
////                |> Map.find task
//                filteredTaskStateMap.[taskId]
//            )
//        }
//        let rec filteredLaneList = selector {
//            key (nameof filteredLaneList)
//            get (fun getter ->
//                printfn "filteredLaneList"
//
//                let filteredTaskStateList = getter.get filteredTaskStateList
//
//                let result =
//                    filteredTaskStateList
//                    |> List.map (fun taskState ->
//    //                    let taskId = getter.get(Atoms.RecoilTask.taskFamily taskState.Task).Id |> getter.get
//                        let taskId = Atoms.RecoilTask.taskId taskState.Task
//                        getter.get (laneFamily taskId)
//                    )
//
//                Profiling.addCount (nameof filteredLaneList)
//                result
//            )
//        }
//        let rec laneMapFamily = selectorFamily {
//            key (nameof laneMapFamily)
//            get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
//                let (Lane (_, cells)) = getter.get (laneFamily taskId)
//
//                Profiling.addCount (nameof laneMapFamily)
//                cells
//                |> List.map (fun (Cell (address, status)) ->
//                    address.Date, status
//                )
//                |> Map.ofList
//            )
//        }
        let rec isTodayFamily = selectorFamily {
            key (nameof isTodayFamily)
            get (fun (date: FlukeDate) getter ->
                let dayStart = getter.get Atoms.dayStart
                let position = getter.get position
                Profiling.addCount (nameof isTodayFamily)
                isToday dayStart position date
            )
        }
        let rec findInformation = selectorFamily {
            key (nameof findInformation)
            get (fun (informationId: Atoms.RecoilInformation.InformationId) getter ->
//                    let taskId = getter.get(Atoms.RecoilTask.taskFamily taskState.Task).Id |> getter.get
//                let taskId = Atoms.RecoilTask.taskId taskState.Task
                let information = getter.get (Atoms.RecoilInformation.informationFamily informationId)
                Profiling.addCount (nameof findInformation)
                information
            )
        }
        let rec findTask = selectorFamily {
            key (nameof findTask)
            get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
//                    let taskId = getter.get(Atoms.RecoilTask.taskFamily taskState.Task).Id |> getter.get
//                let taskId = Atoms.RecoilTask.taskId taskState.Task
                let task = getter.get (Atoms.RecoilTask.taskFamily taskId)
                Profiling.addCount (nameof findTask)
                task
            )
        }
        let rec findCell = selectorFamily {
            key (nameof findCell)
            get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                let cell = getter.get (Atoms.RecoilCell.cellFamily cellId)
                Profiling.addCount (nameof findCell)
                cell
            )
//            set (fun (task: Task, date: FlukeDate) setter (newCell: RecoilCell) ->
//                setter.set(cellFamily (task, date), newCell))
        }
        let rec selectionTracker = selector {
            key (nameof selectionTracker)
            get (fun getter ->
                let selection = getter.get Atoms.selection
                Profiling.addCount (nameof selectionTracker)
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
                Profiling.addCount "selectionTracker (SET)"
            )
        }

        module rec RecoilInformation =
            let rec comments = selectorFamily {
                key (nameof RecoilInformation + "/" + nameof comments)
                get (fun (informationId: Atoms.RecoilInformation.InformationId) getter ->
                    Profiling.addCount (nameof RecoilInformation + "/" + nameof comments)
                    let position = getter.get position
                    let informationMap = getter.get (informationMapFamily position)

                    informationMap
                    |> Map.tryFind informationId
                    |> Option.map (fun x -> x.Comments)
                    |> Option.defaultValue []
                )
            }

        module rec RecoilTask =
            let rec comments = selectorFamily {
                key (nameof RecoilTask + "/" + nameof comments)
                get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                    Profiling.addCount (nameof RecoilTask + "/" + nameof comments)
                    getter.get(findTask taskId).Comments |> getter.get
                )
            }
            let rec priorityValue = selectorFamily {
                key (nameof RecoilTask + "/" + nameof priorityValue)
                get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                    Profiling.addCount (nameof RecoilTask + "/" + nameof priorityValue)
                    getter.get(findTask taskId).PriorityValue |> getter.get
                )
            }

        module rec RecoilCell =
            let rec status = selectorFamily {
                key (nameof RecoilCell + "/" + nameof status)
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    Profiling.addCount (nameof RecoilCell + "/" + nameof status)
                    getter.get(findCell cellId).Status |> getter.get
                )
            }
            let rec comments = selectorFamily {
                key (nameof RecoilCell + "/" + nameof comments)
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    Profiling.addCount (nameof RecoilCell + "/" + nameof comments)
                    getter.get(findCell cellId).Comments |> getter.get
                )
            }
            let rec sessions = selectorFamily {
                key (nameof RecoilCell + "/" + nameof sessions)
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    Profiling.addCount (nameof RecoilCell + "/" + nameof sessions)
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
                    Profiling.addCount (nameof RecoilCell + "/" + nameof selected)
                    getter.get(findCell cellId).Selected |> getter.get
                )
                set (fun (cellId: Atoms.RecoilCell.CellId) setter (newValue: bool) ->
                    Profiling.addCount (nameof RecoilCell + "/" + nameof selected + " (SET)")
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

//        let rec sortedLaneList = selector {
//            key (nameof sortedLaneList)
//            get (fun getter ->
//                printfn "sortedLaneList"
//                let view = getter.get Atoms.view
//                let dayStart = getter.get Atoms.dayStart
//                let now = getter.get Atoms.now
//                let taskOrderList = getter.get Atoms.taskOrderList
//                let filteredTaskStateList = getter.get filteredTaskStateList
//
//                let filteredLaneList =
//                    filteredTaskStateList
//                    |> List.map (fun taskState ->
//                        let taskId = Atoms.RecoilTask.taskId taskState.Task
//                        let cellId = Atoms.RecoilCell.cellId taskState.Task date
//                        let cell = Recoil.useValue (Atoms.RecoilCell.cellFamily cellId)
//                        let status = Recoil.useValue cell.Status
//
//                        Lane (taskState.Task, None)
//                    )
//
//                let result =
//                    match view with
//                    | View.Calendar ->
//                        filteredTaskStateList
//                        |> Sorting.sortLanesByFrequency
//                        |> Sorting.sortLanesByIncomingRecurrency dayStart now
//                        |> Sorting.sortLanesByTimeOfDay dayStart now taskOrderList
//                    | View.Groups ->
//                        let lanes =
//                            filteredLaneList
//                            |> Sorting.applyManualOrder taskOrderList
//
//                        getter.get Atoms.informationList
//                        |> List.map (fun information ->
//                            let lanes =
//                                lanes
//                                |> List.filter (fun (Lane (task, _)) -> task.Information = information)
//
//                            information, lanes
//                        )
//                        |> List.collect snd
//                    | View.Tasks ->
//                        filteredLaneList
//                        |> Sorting.applyManualOrder taskOrderList
//                        |> List.sortByDescending (fun (Lane (task, _)) ->
//                            let taskId = Atoms.RecoilTask.taskId task
//                            let priorityValue = Recoil.useValue (RecoilTask.priorityValue taskId)
//
//                            priorityValue |> ofTaskPriorityValue
//                        )
//                    | View.Week ->
//                        []
//
//                Profiling.addCount "sortedLaneList"
//                result
//            )
//        }
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
//                Profiling.addCount "cellFamily"
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

    let initState (_initializer: MutableSnapshot) =
//        let user = FakeBackend.getCurrentUser ()
//        let dayStart = FakeBackend.getDayStart ()
//        let dateSequence = dateSequence
//        let view = View.Calendar
//        let position = position
//
//        let tree =
//            FakeBackend.getTree
//                {| User = user
//                   DayStart = dayStart
//                   DateSequence = dateSequence
//                   View = view
//                   Position = position |}
        ()
//            initializer.set (Atoms.user, Some RootPrivateData.currentUser)
//
//        OldData.tempState.InformationList
//        |> List.iter (fun information ->
//            let comments =
//                OldData.tempState.InformationCommentsMap
//                |> Map.tryFind information
//                |> Option.defaultValue []
//
//            let informationId = Atoms.RecoilInformation.informationId information
//            let recoilInformation = Atoms.RecoilInformation.RecoilInformation.Create informationId
//
//            initializer.set (recoilInformation.Id, informationId)
//            initializer.set (recoilInformation.WrappedInformation, information)
//            initializer.set (recoilInformation.Comments, comments)
//        )
//
////            initializer.set (Atoms.informationList, OldData.tempState.InformationList)
////            OldData.tempState.InformationCommentsMap
////            |> Map.iter (fun information comments ->
////                let recoilInformation = Atoms.RecoilInformation.RecoilInformation.Create information
////                initializer.set (recoilInformation.WrappedInformation, information)
////                initializer.set (recoilInformation.Comments, comments)
////            )
//
//
//
//        let now = OldData.tempState.GetNow ()
//        let dayStart = OldData.tempState.DayStart
//
////                let a =
////                    FakeBackend.getTree
////                        {| User = None
////                           TreeId = TreeId ""
////                           View = view
////                           Position = None |}
//
//        initializer.set (Atoms.getNow, OldData.tempState.GetNow)
//        initializer.set (Atoms.now, now)
//        initializer.set (Atoms.dayStart, dayStart)
//        initializer.set (Atoms.taskOrderList, OldData.tempState.TaskOrderList)
//        initializer.set (Atoms.lastSessions, OldData.tempState.LastSessions)
//        initializer.set (Atoms.taskStateList, OldData.tempState.TaskStateList)
//
//
//        let dateSequence =
//            [ now.Date ]
//            |> Rendering.getDateSequence (45, 20)
//
//        OldData.tempState.TaskStateList
//        |> List.iter (fun taskState ->
//            let taskId = Atoms.RecoilTask.taskId taskState.Task
//            let task = Atoms.RecoilTask.RecoilTask.Create taskId
//            initializer.set (task.Id, taskId)
//            initializer.set (task.Comments, taskState.Comments)
//            initializer.set (task.PriorityValue, taskState.PriorityValue)
//
//            let cellMap =
//                let (Lane (_, cells)) =
//                    Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
//
//                cells
//                |> List.map (fun (Cell (address, status)) ->
//                    address.Date, status
//                )
//                |> Map.ofList
//
//            dateSequence
//            |> List.iter (fun date ->
//                let cellId = Atoms.RecoilCell.cellId taskId date
//                let cell = Atoms.RecoilCell.RecoilCell.Create cellId
//                let cellComments = taskState.CellCommentsMap |> Map.tryFind date |> Option.defaultValue []
//                let sessions =
//                    taskState.Sessions
//                    |> List.filter (fun (TaskSession start) -> isToday OldData.tempState.DayStart start date)
//
//                let status = cellMap |> Map.tryFind date |> Option.defaultValue Missed
//
//                initializer.set (cell.Id, cellId)
//                initializer.set (cell.Task, task)
//                initializer.set (cell.Date, date)
//                initializer.set (cell.Status, status)
//                initializer.set (cell.Comments, cellComments)
//                initializer.set (cell.Sessions, sessions)
//            )
//        )
