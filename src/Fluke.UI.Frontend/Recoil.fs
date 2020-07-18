namespace Fluke.UI.Frontend

#nowarn "40"

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
//        Browser.Dom.window?callCountClear <- fun () -> callCount.Clear ()
        Browser.Dom.window?oldJson <- ""
        let addCount id =
            if not (callCount.ContainsKey id) then
                callCount.[id] <- 0
            callCount.[id] <- callCount.[id] + 1

        Fable.Core.JS.setInterval (fun () ->
            let json = Fable.SimpleJson.SimpleJson.stringify Browser.Dom.window?callCount
            if json <> Browser.Dom.window?oldJson then
                mountById "diag" (str json)
                Browser.Dom.window?oldJson <- json
        ) 100 |> ignore

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

            let taskList =
                let treeData = RootPrivateData.treeData
                let sharedTreeData = RootPrivateData.sharedTreeData

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

            let dateRange =
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
                RootPrivateData.treeData.TaskOrderList// @ RootPrivateData.taskOrderList

            let sortedTaskList =
                sortLanes
                    {| View = input.View
                       DayStart = input.DayStart
                       Position = input.Position
                       TaskOrderList = taskOrderList
                       InformationList = informationList |> List.map (fun x -> x.Information)
                       Lanes = filteredLanes |}
                |> List.map ofLane
                |> List.map (Tuple2.mapSnd (fun cells ->
                    cells
                    |> List.map (fun (Cell (address, status)) ->
                        address.Date, status
                    )
                    |> Map.ofList
                ))

//                    let sortedTaskList =
//                        sortedTaskList
////                        |> List.sortByDescending (fun x -> x.StatusEntries.Length)
//                        |> List.take 50

            let taskList =
                sortedTaskList
                |> List.map (fun (task, statusMap) ->
                    let mergedCellStateMap =
                        statusMap
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
//            let lastSessions =
//                taskList
//                |> List.filter (fun task -> not task.Sessions.IsEmpty)
//                |> List.map (fun task -> task, task.Sessions)
//                |> List.map (Tuple2.mapSnd (fun sessions ->
//                    sessions
//                    |> List.sortByDescending (fun (TaskSession start) -> start.DateTime)
//                    |> List.head
//                ))

            {|
                Owner = input.User
                SharedWith = []
                InformationList = informationList
                TaskList = taskList
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
            type TaskId = TaskId of id:string
            type RecoilTask =
                { Id: RecoilValue<TaskId, ReadWrite>
                  InformationId: RecoilValue<RecoilInformation.InformationId, ReadWrite>
                  Name: RecoilValue<string, ReadWrite>
                  Comments: RecoilValue<Comment list, ReadWrite>
                  Sessions: RecoilValue<TaskSession list, ReadWrite>
                  Priority: RecoilValue<TaskPriorityValue, ReadWrite> }
            let rec idFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof idFamily))
                def (fun (_taskId: TaskId) -> TaskId "")
            }
            let rec informationIdFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof informationIdFamily))
                def (fun (_taskId: TaskId) ->
                    RecoilInformation.InformationId "")
            }
            let rec nameFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof nameFamily))
                def (fun (_taskId: TaskId) -> "")
            }
            let rec commentsFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof commentsFamily))
                def (fun (_taskId: TaskId) -> [])
            }
            let rec sessionsFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof sessionsFamily))
                def (fun (_taskId: TaskId) -> [])
            }
            let rec priorityFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof priorityFamily))
                def (fun (_taskId: TaskId) -> TaskPriorityValue 0)
            }
            type RecoilTask with
                static member internal Create taskId =
                    { Id = idFamily taskId
                      InformationId = informationIdFamily taskId
                      Name = nameFamily taskId
                      Comments = commentsFamily taskId
                      Sessions = sessionsFamily taskId
                      Priority = priorityFamily taskId }
            let taskId (task: Task) =
                TaskId (sprintf "%s/%s" task.Information.Name task.Name)
            let rec taskFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof taskFamily))
                def (fun (taskId: TaskId) -> RecoilTask.Create taskId)
            }

        module RecoilCell =
            type CellId = CellId of id:string
            type RecoilCell =
                { Id: RecoilValue<CellId, ReadWrite>
                  TaskId: RecoilValue<RecoilTask.TaskId, ReadWrite>
                  Date: RecoilValue<FlukeDate, ReadWrite>
                  Status: RecoilValue<CellStatus, ReadWrite>
                  Comments: RecoilValue<Comment list, ReadWrite>
                  Sessions: RecoilValue<TaskSession list, ReadWrite>
                  Selected: RecoilValue<bool, ReadWrite> }

            let rec idFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof idFamily))
                def (fun (_cellId: CellId) -> CellId "")
            }
            let rec taskIdFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof taskIdFamily))
                def (fun (_cellId: CellId) -> RecoilTask.TaskId "")
            }
            let rec dateFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof dateFamily))
                def (fun (_cellId: CellId) -> flukeDate 0000 Month.January 01)
            }
            let rec statusFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof statusFamily))
                def (fun (_cellId: CellId) -> Disabled)
            }
            let rec commentsFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof commentsFamily))
                def (fun (_cellId: CellId) -> [] : Comment list)
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
            let cellId (RecoilTask.TaskId taskId) (date: FlukeDate) =
                CellId (sprintf "%s/%s" taskId (date.DateTime.Format "yyyy-MM-dd"))

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
                  TaskIdList: RecoilValue<RecoilTask.TaskId list, ReadWrite> }
            let rec idFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilTree) (nameof idFamily))
                def (fun (_treeId: TreeId) -> TreeId "")
            }
            let rec ownerFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof ownerFamily))
                def (fun (_treeId: TreeId) -> TempData.testUser)
            }
            let rec sharedWithFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof sharedWithFamily))
                def (fun (_treeId: TreeId) -> [])
            }
            let rec positionFamily = atomFamily {
                key (sprintf "%s/%s" (nameof RecoilInformation) (nameof positionFamily))
                def (fun (_treeId: TreeId) -> flukeDateTime 0000 Month.January 01 00 00)
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
        let rec isTodayFamily = selectorFamily {
            key ("selectorFamily/" + nameof isTodayFamily)
            get (fun (date: FlukeDate) getter ->
                let dayStart = getter.get Atoms.dayStart
                let position = getter.get position
                Profiling.addCount (nameof isTodayFamily)
                isToday dayStart position date
            )
        }
        let rec dateIdFamily = selectorFamily { // dateReferenceFamily
            key ("selectorFamily/" + nameof dateIdFamily)
            get (fun (date: FlukeDate) getter ->
                let dayStart = getter.get Atoms.dayStart
                let position = getter.get position
                Profiling.addCount (nameof dateIdFamily)
                isToday dayStart position date
            )
        }
        let rec selection = selector {
            key ("selector/" + nameof selection)
            get (fun getter ->
                let selection = getter.get Atoms.selection
                Profiling.addCount (nameof selection)
                selection
            )
            set (fun setter (newSelection: Map<Atoms.RecoilTask.TaskId, Set<FlukeDate>>) ->
                let selection = setter.get Atoms.selection

                selection
                |> Seq.iter (fun (KeyValue (taskId, dates)) ->
                    let newSelectionTask =
                        newSelection
                        |> Map.tryFind taskId
                        |> Option.defaultValue Set.empty
                    dates
                    |> Seq.filter (fun date ->
                        newSelectionTask.Contains date
                        |> not
                    )
                    |> Seq.iter (fun date ->
                        let cellId = Atoms.RecoilCell.cellId taskId date
                        let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                        setter.set (cell.Selected, false)
                    )
                )

                newSelection
                |> Seq.iter (fun (KeyValue (taskId, dates)) ->
                    let selectionTask =
                        selection
                        |> Map.tryFind taskId
                        |> Option.defaultValue Set.empty
                    dates
                    |> Seq.filter (fun date ->
                        selectionTask.Contains date
                        |> not
                    )
                    |> Seq.iter (fun date ->
                        let cellId = Atoms.RecoilCell.cellId taskId date
                        let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                        setter.set (cell.Selected, true)
                    )
                )

                setter.set (Atoms.selection, newSelection)
                Profiling.addCount (nameof selection + "(SET)")
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
            key ("selector/" + nameof taskList)
            get (fun getter ->
                let taskIdList = getter.get Atoms.taskIdList

                let taskList =
                    taskIdList
                    |> List.map (fun taskId ->
                        let task = getter.get (Atoms.RecoilTask.taskFamily taskId)
                        let informationId = getter.get task.InformationId
                        let priority = getter.get task.Priority
                        let name = getter.get task.Name

                        let information = getter.get (Atoms.RecoilInformation.informationFamily informationId)
                        let wrappedInformation = getter.get information.WrappedInformation
                        let informationComments = getter.get information.Comments

                        {| Id = taskId
                           Name = name
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
            let rec lastSessionFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof lastSessionFamily))
                get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                    let task = getter.get (Atoms.RecoilTask.taskFamily taskId)
                    let sessions = getter.get task.Sessions

                    Profiling.addCount (nameof lastSessionFamily)
                    sessions
                    |> List.sortByDescending (fun (TaskSession start) -> start.DateTime)
                    |> List.tryHead
                )
            }
            let rec activeSessionFamily = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilTask) (nameof activeSessionFamily))
                get (fun (taskId: Atoms.RecoilTask.TaskId) getter ->
                    let position = getter.get position
                    let lastSession = getter.get (lastSessionFamily taskId)

                    Profiling.addCount (nameof activeSessionFamily)

                    lastSession
                    |> Option.bind (fun (TaskSession start) ->
                        let durationMinutes = (position.DateTime - start.DateTime).TotalMinutes
                        let active = durationMinutes < TempData.sessionLength + TempData.sessionBreakLength
                        match active with
                        | true -> Some durationMinutes
                        | false -> None
                    )
                )
            }

        module rec RecoilCell =
            let rec selected = selectorFamily {
                key (sprintf "%s/%s" (nameof RecoilCell) (nameof selected))
                get (fun (cellId: Atoms.RecoilCell.CellId) getter ->
                    let cell = getter.get (Atoms.RecoilCell.cellFamily cellId)

                    Profiling.addCount (sprintf "%s/%s" (nameof RecoilCell) (nameof selected))
                    getter.get cell.Selected
                )
                set (fun (cellId: Atoms.RecoilCell.CellId) setter (newValue: bool) ->
                    let ctrlPressed = setter.get Atoms.ctrlPressed

                    let cell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                    let date = setter.get cell.Date
                    let taskId = setter.get cell.TaskId

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
                    Profiling.addCount (sprintf "%s/%s (SET)" (nameof RecoilCell) (nameof selected))
                )
            }

        let rec activeSessions = selector {
            key ("selector/" + nameof activeSessions)
            get (fun getter ->
                let taskList = getter.get taskList
                taskList
                |> List.map (fun task ->
                    let duration = getter.get (RecoilTask.activeSessionFamily task.Id)
                    duration
                    |> Option.map (fun duration -> ActiveSession (task.Name, duration))
                )
                |> List.choose id
            )
        }
        let rec private tree = selector {
            key ("selector/" + nameof tree)
            get (fun getter ->
                let user = getter.get Atoms.user
                let dayStart = getter.get Atoms.dayStart
                let dateSequence = getter.get dateSequence
                let position = getter.get position
                let view = getter.get Atoms.view

                let tree =
                    FakeBackend.getTree
                        {| User = user
                           DayStart = dayStart
                           DateSequence = dateSequence
                           View = view
                           Position = position |}

                Profiling.addCount (nameof tree)
                tree
            )
        }
        let rec treeUpdater = selector {
            key ("selector/" + nameof treeUpdater)
            get (fun _ -> ())
            set (fun setter _ ->
                let dateSequence = setter.get dateSequence
                let tree = setter.get tree

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

                setter.set (Atoms.taskIdList, taskIdList)

                taskIdList
                |> List.iter (fun taskId ->
                    dateSequence
                    |> List.iter (fun date ->
                        let cellId = Atoms.RecoilCell.cellId taskId date
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
                    setter.set (recoilTask.Comments, task.Comments)
                    setter.set (recoilTask.Sessions, task.Sessions)
                    setter.set (recoilTask.Priority, task.Priority)

                    task.CellStateMap
                    |> Map.iter (fun date cellState ->
                        let cellId = Atoms.RecoilCell.cellId taskId date
                        let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)
                        setter.set (recoilCell.Status, cellState.Status)
                        setter.set (recoilCell.Comments, cellState.Comments)
                        setter.set (recoilCell.Sessions, [])
                        setter.set (recoilCell.Selected, false)
                    )
                )

                let treeId = Atoms.RecoilTree.treeId tree.Owner "default"
                let recoilTree = setter.get (Atoms.RecoilTree.treeFamily treeId)
                setter.set (recoilTree.Owner, tree.Owner)
                setter.set (recoilTree.SharedWith, tree.SharedWith)
                setter.set (recoilTree.InformationIdList, recoilInformationList |> List.map snd)
                setter.set (recoilTree.TaskIdList, taskIdList)

                Profiling.addCount (nameof treeUpdater)
            )
        }

