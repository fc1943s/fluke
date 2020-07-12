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

    module private Profiling =
        let private callCount = Dictionary<string, int>()
        Browser.Dom.window?callCount <- callCount
        Browser.Dom.window?callCountClear <- fun () -> callCount.Clear ()
        let addCount id =
            let add = async {
                if not (callCount.ContainsKey id) then
                    callCount.[id] <- 0
                callCount.[id] <- callCount.[id] + 1
                mountById "diag" (str (Fable.SimpleJson.SimpleJson.stringify Browser.Dom.window?callCount))
            }

            Fable.Core.JS.setTimeout (fun () ->
                add |> Async.StartImmediate
            ) 0 |> ignore
            ()

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

    module private FakeBackend =
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

        let getLivePosition () =
            FlukeDateTime.FromDateTime DateTime.Now

        let getCurrentUser () =
            RootPrivateData.currentUser

        let getDayStart () =
            TempData.dayStart

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
            | View.Calendar ->
                taskList
                |> List.filter (function
                    | { Scheduling = Manual WithoutSuggestion
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
                Owner = TempData.Users.fc1943s
                SharedWith = []
                InformationList = informationList
                TaskList =
                    let taskList =
                        let treeData = RootPrivateData.treeData
                        let sharedTreeData = RootPrivateData.treeData

                        let applyEvents statusEntries comments (task: Task) =
                            let newCellCommentMap =
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
                                CellStateMap =
                                    task.CellStateMap
                                    |> Map.map (fun date cellState ->
                                        let newComments =
                                            newCellCommentMap
                                            |> Map.tryFind date
                                            |> Option.defaultValue []
                                        { cellState with
                                            Comments =
                                                cellState.Comments
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

                    let cellStatusMap =
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
                        let mergedCellStateMap =
                            cellStatusMap
                            |> Map.tryFind task
                            |> Option.defaultValue Map.empty
                            |> Map.map (fun date status ->
                                let cellState =
                                    task.CellStateMap
                                    |> Map.tryFind date
                                    |> Option.defaultValue
                                        { Comments = []
                                          Status = Disabled }
                                { cellState with Status = status }
                            )
                        let newCellStateMap =
                            task.CellStateMap
                            |> Map.union mergedCellStateMap

                        { task with CellStateMap = newCellStateMap }
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
                  Name: RecoilValue<string, ReadWrite>
                  Information: RecoilValue<RecoilInformation.RecoilInformation, ReadWrite>
                  Comments: RecoilValue<Comment list, ReadWrite>
                  Priority: RecoilValue<TaskPriorityValue, ReadWrite> }
            let rec idFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof idFamily)
                def (fun (_taskId: TaskId) -> TaskId "")
            }
            let rec nameFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof nameFamily)
                def (fun (_taskId: TaskId) -> "")
            }
            let rec informationFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof informationFamily)
                def (fun (_taskId: TaskId) ->
                    RecoilInformation.RecoilInformation.Create (RecoilInformation.InformationId ""))
            }
            let rec commentsFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof commentsFamily)
                def (fun (_taskId: TaskId) -> [])
            }
            let rec priorityFamily = atomFamily {
                key (nameof RecoilTask + "/" + nameof priorityFamily)
                def (fun (_taskId: TaskId) -> TaskPriorityValue 0)
            }
            type RecoilTask with
                static member internal Create taskId =
                    { Id = idFamily taskId
                      Name = nameFamily taskId
                      Information = informationFamily taskId
                      Comments = commentsFamily taskId
                      Priority = priorityFamily taskId }
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
                CellId (sprintf "%s/%s" taskId (date.DateTime.Format "yyyy-MM-dd"))

            let rec cellFamily = atomFamily {
                key (nameof RecoilCell + "/" + nameof cellFamily)
                def (fun (cellId: CellId) -> RecoilCell.Create cellId)
            }
        module RecoilTree =
            type TreeId = TreeId of id:string
            type RecoilTree =
                { Id: RecoilValue<TreeId, ReadWrite>
                  Owner: RecoilValue<User, ReadWrite>
                  SharedWith: RecoilValue<TreeAccess list, ReadWrite>
                  Position: RecoilValue<FlukeDateTime, ReadWrite>
                  InformationList: RecoilValue<RecoilInformation.RecoilInformation list, ReadWrite>
                  TaskList: RecoilValue<RecoilTask.RecoilTask list, ReadWrite> }
            let rec idFamily = atomFamily {
                key (nameof RecoilTree + "/" + nameof idFamily)
                def (fun (_treeId: TreeId) -> TreeId "")
            }
            let rec ownerFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof ownerFamily)
                def (fun (_treeId: TreeId) -> TempData.testUser)
            }
            let rec sharedWithFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof sharedWithFamily)
                def (fun (_treeId: TreeId) -> [])
            }
            let rec positionFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof positionFamily)
                def (fun (_treeId: TreeId) -> flukeDateTime 0000 Month.January 01 00 00)
            }
            let rec informationListFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof informationListFamily)
                def (fun (_treeId: TreeId) -> [])
            }
            let rec taskListFamily = atomFamily {
                key (nameof RecoilInformation + "/" + nameof taskListFamily)
                def (fun (_treeId: TreeId) -> [])
            }
            type RecoilTree with
                static member internal Create treeId =
                    { Id = idFamily treeId
                      Owner = ownerFamily treeId
                      SharedWith = sharedWithFamily treeId
                      Position = positionFamily treeId
                      InformationList = informationListFamily treeId
                      TaskList = taskListFamily treeId }
            let treeId owner name =
                TreeId (sprintf "%s/%s" owner.Username name)
            let rec treeFamily = atomFamily {
                key (nameof RecoilTree + "/" + nameof treeFamily)
                def (fun (treeId: TreeId) -> RecoilTree.Create treeId)
            }

        let rec user = atom {
            key ("atom/" + nameof user)
            def (FakeBackend.getCurrentUser ())
        }
        let rec view = atom {
            key ("atom/" + nameof view)
            def View.Calendar
            local_storage
        }
        let rec dayStart = atom {
            key ("atom/" + nameof dayStart)
            def (FakeBackend.getDayStart ())
        }
        let rec selection = atom {
            key ("atom/" + nameof selection)
            def (Map.empty : Map<RecoilTask.TaskId, Set<FlukeDate>>)
        }
        let rec ctrlPressed = atom {
            key ("atom/" + nameof ctrlPressed)
            def false
        }
        let rec activeSessions = atom {
            key ("atom/" + nameof activeSessions)
            def ([] : ActiveSession list)
        }
        let rec positionTrigger = atom {
            key ("atom/" + nameof positionTrigger)
            def 0
        }
        let rec taskIdList = atom {
            key ("atom/" + nameof taskIdList)
            def ([] : RecoilTask.TaskId list)
        }

    module Selectors =
        let rec position = selector {
            key ("selector/" + nameof position)
            get (fun getter ->
                let positionTrigger = getter.get Atoms.positionTrigger
                Profiling.addCount (nameof position)
                let newPosition = FakeBackend.getLivePosition ()
                printfn "NEWPOSITION: %A. TRIGGER: %A" newPosition positionTrigger
                newPosition
            )
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
        let rec isTodayFamily = selectorFamily {
            key ("selectorFamily/" + nameof isTodayFamily)
            get (fun (date: FlukeDate) getter ->
                let dayStart = getter.get Atoms.dayStart
                let position = getter.get position
                Profiling.addCount (nameof isTodayFamily)
                isToday dayStart position date
            )
        }
        let rec selection = selector {
            key (nameof selection)
            get (fun getter ->
                let selection = getter.get Atoms.selection
                Profiling.addCount (nameof selection)
                selection
            )
            set (fun setter (newSelection: Map<Atoms.RecoilTask.TaskId, Set<FlukeDate>>) ->
                let selection = setter.get Atoms.selection

                selection
                |> Seq.iter (fun (KeyValue (taskId, dates)) ->
                    dates
                    |> Seq.iter (fun date ->
                        let cellId = Atoms.RecoilCell.cellId taskId date
                        let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                        setter.set (cell.Selected, false)
                    )
                )

                newSelection
                |> Seq.iter (fun (KeyValue (taskId, dates)) ->
                    dates
                    |> Seq.iter (fun date ->
                        let cellId = Atoms.RecoilCell.cellId taskId date
                        let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                        setter.set (cell.Selected, true)
                    )
                )

                setter.set (Atoms.selection, newSelection)
                Profiling.addCount "selection (SET)"
            )
        }
        let rec dateMap = selector {
            key ("selector/" + nameof dateMap)
            get (fun getter ->
                let dateSequence = getter.get dateSequence
                let selection = getter.get selection

                let selectionSet =
                    selection
                    |> Map.values
                    |> Set.unionMany

                let dateMap =
                    dateSequence
                    |> List.map (fun date ->
                        let isToday = getter.get (isTodayFamily date)
                        let info =
                            {| IsSelected = selectionSet.Contains date
                               IsToday = isToday |}
                        date, info
                    )
                    |> Map.ofList

                Profiling.addCount (nameof dateMap)
                dateMap
            )
        }
        let rec taskList = selector {
            key (nameof taskList)
            get (fun getter ->
                let taskIdList = getter.get Atoms.taskIdList

                let taskList =
                    taskIdList
                    |> List.map (fun taskId ->
                        let task = getter.get (Atoms.RecoilTask.taskFamily taskId)

                        let information = getter.get task.Information
                        let priority = getter.get task.Priority
                        let wrappedInformation = getter.get information.WrappedInformation
                        let informationComments = getter.get information.Comments

                        {| Id = taskId
                           Priority = priority
                           Information = wrappedInformation
                           InformationComments = informationComments |}
                    )

                Profiling.addCount (nameof taskList)
                taskList
            )
        }

        module private rec RecoilInformation =
            ()

        module private rec RecoilTask =
            ()

        module rec RecoilCell =
            let rec selected = selectorFamily {
                key (nameof RecoilCell + "/" + nameof selected)
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    Profiling.addCount (nameof RecoilCell + "/" + nameof selected)
                    let cell = getter.get (Atoms.RecoilCell.cellFamily cellId)
                    getter.get cell.Selected
                )
                set (fun (cellId: Atoms.RecoilCell.CellId) setter (newValue: bool) ->
                    Profiling.addCount (nameof RecoilCell + "/" + nameof selected + " (SET)")
                    let ctrlPressed = setter.get Atoms.ctrlPressed

                    let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                    let date = setter.get cell.Date
                    let task = setter.get cell.Task
                    let taskId = setter.get task.Id

                    printfn "A: %A; %A; %A" cellId taskId date

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

                    setter.set (selection, newSelection)
                )
            }

        let rec tree = recoil {
            let! user = Atoms.user
            let! dayStart = Atoms.dayStart
            let! dateSequence = dateSequence
            let! position = position
            let! view = Atoms.view

            printfn "TREE: %A" (user.Username, dayStart, dateSequence.Length, view, position)

            Profiling.addCount (nameof tree)
            let tree =
                FakeBackend.getTree
                    {| User = user
                       DayStart = dayStart
                       DateSequence = dateSequence
                       View = view
                       Position = position |}
            return tree
        }
        let rec treeUpdater = selector {
            key (nameof treeUpdater)
            get (fun _ -> ())
            set (fun setter _ ->
                Profiling.addCount ("treeupdater1")
                let tree = setter.get tree

                let recoilInformationList =
                    tree.InformationList
                    |> List.map (fun information ->
                        let informationId = Atoms.RecoilInformation.informationId information.Information
                        let recoilInformation = setter.get (Atoms.RecoilInformation.informationFamily informationId)
                        setter.set (recoilInformation.Id, informationId)
                        setter.set (recoilInformation.WrappedInformation, information.Information)
                        setter.set (recoilInformation.Comments, information.Comments)
                        information.Information, recoilInformation
                    )

                let recoilInformationMap =
                    recoilInformationList
                    |> Map.ofList

                let recoilTaskList =
                    tree.TaskList
                    |> List.map (fun task ->
                        let taskId = Atoms.RecoilTask.taskId task
                        let recoilTask = setter.get (Atoms.RecoilTask.taskFamily taskId)
                        setter.set (recoilTask.Id, taskId)
                        setter.set (recoilTask.Name, task.Name)
                        setter.set (recoilTask.Information, recoilInformationMap.[task.Information])
                        setter.set (recoilTask.Comments, task.Comments)
                        setter.set (recoilTask.Priority, task.Priority)

                        task.CellStateMap
                        |> Map.iter (fun date cellState ->
                            let cellId = Atoms.RecoilCell.cellId taskId date
                            let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                            setter.set (recoilCell.Id, cellId)
                            setter.set (recoilCell.Task, recoilTask)
                            setter.set (recoilCell.Date, date)
                            setter.set (recoilCell.Status, cellState.Status)
                            setter.set (recoilCell.Comments, cellState.Comments)
                            setter.set (recoilCell.Sessions, [])
                            setter.set (recoilCell.Selected, false)
                        )

                        recoilTask
                    )

                let taskIdList =
                    recoilTaskList
                    |> List.map (fun x -> setter.get x.Id)

                setter.set (Atoms.taskIdList, taskIdList)

                let treeId = Atoms.RecoilTree.treeId tree.Owner "default"
                let recoilTree = setter.get (Atoms.RecoilTree.treeFamily treeId)
                setter.set (recoilTree.Owner, tree.Owner)
                setter.set (recoilTree.SharedWith, tree.SharedWith)
                setter.set (recoilTree.InformationList, recoilInformationList |> List.map snd)
                setter.set (recoilTree.TaskList, recoilTaskList)

                Profiling.addCount "treeupdater2"
            )
        }

