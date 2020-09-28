namespace Fluke.Shared

open System
open FSharpPlus
open Suigetsu.Core


module TempData =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State
    open Templates

    let getPublicAreas () =
        {|
            car = { Name = AreaName "car" }
            career = { Name = AreaName "career" }
            chores = { Name = AreaName "chores" }
            finances = { Name = AreaName "finances" }
            fitness = { Name = AreaName "fitness" }
            food = { Name = AreaName "food" }
            health = { Name = AreaName "health" }
            leisure = { Name = AreaName "leisure" }
            programming = { Name = AreaName "programming" }
            travel = { Name = AreaName "travel" }
            workflow = { Name = AreaName "workflow" }
            writing = { Name = AreaName "writing" }
        |}


    let getPublicProjects () =
        let areas = getPublicAreas ()
        {|
            app_fluke =
                {
                    Project.Area = areas.workflow
                    Project.Name = ProjectName "app_fluke"
                }
            blog =
                {
                    Project.Area = areas.writing
                    Project.Name = ProjectName "blog"
                }
            rebuild_website =
                {
                    Project.Area = areas.programming
                    Project.Name = ProjectName "rebuild_website"
                }
        |}


    let getPublicResources () =
        let areas = getPublicAreas ()
        {|
            agile =
                {
                    Area = areas.programming
                    Name = ResourceName "agile"
                }
            artificial_intelligence =
                {
                    Area = areas.programming
                    Name = ResourceName "artificial_intelligence"
                }
            cloud =
                {
                    Area = areas.programming
                    Name = ResourceName "cloud"
                }
            communication =
                {
                    Area = areas.workflow
                    Name = ResourceName "communication"
                }
            docker =
                {
                    Area = areas.programming
                    Name = ResourceName "docker"
                }
            fsharp =
                {
                    Area = areas.programming
                    Name = ResourceName "f#"
                }
            linux =
                {
                    Area = areas.programming
                    Name = ResourceName "linux"
                }
            music =
                {
                    Area = areas.leisure
                    Name = ResourceName "music"
                }
            rust =
                {
                    Area = areas.programming
                    Name = ResourceName "rust"
                }
            vim =
                {
                    Area = areas.programming
                    Name = ResourceName "vim"
                }
            windows =
                {
                    Area = areas.programming
                    Name = ResourceName "windows"
                }
        |}


    let getUsers () =
        let rec result =
            {|
                fluke =
                    {
                        Username = Username <| nameof result.fluke
                        Color = UserColor.Black
                        WeekStart = DayOfWeek.Sunday
                        DayStart = FlukeTime.Create 12 00
                        SessionLength = Minute 25.
                        SessionBreakLength = Minute 5.
                    }
                fc1943s =
                    {
                        Username = Username <| nameof result.fc1943s
                        Color = UserColor.Blue
                        WeekStart = DayOfWeek.Sunday
                        DayStart = FlukeTime.Create 07 00
                        SessionLength = Minute 25.
                        SessionBreakLength = Minute 5.
                    }
                liryanne =
                    {
                        Username = Username <| nameof result.liryanne
                        Color = UserColor.Pink
                        WeekStart = DayOfWeek.Monday
                        DayStart = FlukeTime.Create 07 00
                        SessionLength = Minute 25.
                        SessionBreakLength = Minute 5.
                    }
            |}

        result
    //
//        let users = [ fluke; fc1943s; liryanne ]


    let getPublicConsts () =
        let rootPath = """M:\Dropbox\home_encrypted\projects\app-fluke"""

        let dbPath = rootPath + """\db_v1"""
        let dayStart = FlukeTime.Create 07 00

        let rec result =
            {|
                defaultPosition = { Date = FlukeDate.MinValue; Time = dayStart }
            |}

        result






    module Events =


        [<RequireQualifiedAccess>]
        type TempEvent =
            | CellStatus of user: User * task: Task * date: FlukeDateTime * manualCellStatus: ManualCellStatus
            | CellCompleted of user: User * task: Task * date: FlukeDateTime
        //            | CellCommented of user:User * task:Task * date:FlukeDateTime * comment:Comment

        type TaskName = TaskName of string
        type HashedTaskId = HashedTaskId of string

        type EventX =
            | TaskCreated of information: Information * name: TaskName
            | TaskRenamed of taskId: HashedTaskId * newName: TaskName


    //        let eventsFromCellComments user = ()


    //        module EventSourcingTemp =
//            type Command = CompleteCell of user: User * task: Task * date: FlukeDateTime
//            type Event = CellCompleted of user: User * task: Task * date: FlukeDateTime
//
//
//            let a =
//                let b: UserInteraction list = []
//                b
//                |> List.map (fun userInteraction -> ())
//                |> ignore
//
////            let initialState =
////                {
////                    User = None
////                    TaskMap = Map.empty
////                    TaskIdList = []
////                }
//
//            let private apply (state: State) (event: Event): State =
//                match event with
//                | CellCompleted (user, task, moment) ->
//                    { state with
//                        TaskMap =
//                            let taskId = taskId task
//                            let dateId = dateId state.DayStart moment
//                            let newStatus = UserStatus (user, CellStatus.Completed)
//
//                            let cellMap =
//                                state.TaskMap
//                                |> Map.tryFind taskId
//                                |> Option.defaultValue Map.empty
//                                |> Map.add dateId newStatus
//
//                            state.TaskMap |> Map.add taskId cellMap
//                    }
//
//            let execute (state: State) (command: Command): Event list =
//                [
//                    match command, state with
//                    | CompleteCell (user, task, moment), state when state.TaskMap
//                                                                    |> Map.tryFind (taskId task)
//                                                                    |> Option.defaultValue Map.empty
//                                                                    |> Map.tryFind (dateId state.DayStart moment)
//                                                                    |> Option.defaultValue Disabled
//                                                                    |> (=) Pending -> CellCompleted (user, task, moment)
//
//                    | CompleteCell (user, task, eventDate), state -> CellCompleted (user, task, eventDate)
//
//                    | _ -> ()
//                ]
//
//            let build = List.fold apply
//            let rebuld = build initialState






    let createTaskCommentInteraction user moment task comment =
        let interaction = Interaction.Task (task, TaskInteraction.Attachment (Attachment.Comment (user, comment)))

        let userInteraction = UserInteraction (moment, user, interaction)

        userInteraction

    let createInformationCommentInteraction user moment information comment =
        let interaction =
            Interaction.Information
                (information, InformationInteraction.Attachment (Attachment.Comment (user, comment)))

        let userInteraction = UserInteraction (moment, user, interaction)

        userInteraction

    let createCellCommentInteraction dayStart task moment user comment =
        let cellInteraction =
            Attachment.Comment (user, comment)
            |> CellInteraction.Attachment

        let cellAddress = { Task = task; DateId = dateId dayStart moment }

        let interaction = Interaction.Cell (cellAddress, cellInteraction)

        let userInteraction = UserInteraction (moment, user, interaction)

        userInteraction

    let createCellStatusChangeInteraction (user: User) task date manualCellStatus =
        let cellStatusChange =
            match manualCellStatus with
            | ManualCellStatus.Completed -> CellStatusChange.Complete
            | ManualCellStatus.Dismissed -> CellStatusChange.Dismiss
            | ManualCellStatus.Postponed until -> CellStatusChange.Postpone until
            | ManualCellStatus.ManualPending -> CellStatusChange.Schedule

        let cellInteraction = CellInteraction.StatusChange cellStatusChange

        let dateId = DateId date

        let cellAddress = { Task = task; DateId = dateId }

        let interaction = Interaction.Cell (cellAddress, cellInteraction)

        let moment = { Date = date; Time = user.DayStart }

        let userInteraction = UserInteraction (moment, user, interaction)

        userInteraction

    let createCellStatusChangeInteractions user (entries: (FlukeDate * (Task option * ManualCellStatus) list) list) =
        entries
        |> List.collect (fun (date, events) ->
            events
            |> List.choose (fun (task, manualCellStatus) ->
                task
                |> Option.map (fun task -> createCellStatusChangeInteraction user task date manualCellStatus)))


    let createTaskState moment task (sortTaskMap: Map<TaskName, Task> option) (dslTasks: (DslTask * User) list) =

        let defaultTaskState: TaskState =
            {
                Task = task
                Sessions = []
                Attachments = []
                SortList = []
                CellStateMap = Map.empty
                InformationMap = Map.empty
            }

        let taskState, userInteractions =
            ((defaultTaskState, []), dslTasks)
            ||> List.fold (fun (taskState, userInteractions) (dslTask, user) ->
                    match dslTask with
                    | DslTaskComment comment ->
                        let interaction =
                            Interaction.Task
                                (task, TaskInteraction.Attachment (Attachment.Comment (user, Comment.Comment comment)))

                        let userInteraction = UserInteraction (moment, user, interaction)

                        let newUserInteractions =
                            userInteractions
                            @ [
                                userInteraction
                            ]

                        taskState, newUserInteractions
                    | DslCellComment (date, comment) ->
                        let interaction =
                            Interaction.Cell
                                ({ Task = task; DateId = DateId date },
                                 CellInteraction.Attachment (Attachment.Comment (user, Comment.Comment comment)))

                        let userInteraction = UserInteraction (moment, user, interaction)

                        let newUserInteractions =
                            userInteractions
                            @ [
                                userInteraction
                            ]

                        taskState, newUserInteractions
                    | DslSession start ->
                        let taskSession = TaskSession (start, user.SessionLength, user.SessionBreakLength)

                        let taskInteraction = TaskInteraction.Session taskSession
                        let interaction = Interaction.Task (task, taskInteraction)

                        let userInteraction = UserInteraction (moment, user, interaction)

                        let newUserInteractions =
                            userInteractions
                            @ [
                                userInteraction
                            ]

                        taskState, newUserInteractions
                    | DslTaskSort (top, bottom) ->
                        let newUserInteractions =
                            match sortTaskMap with
                            | Some sortTaskMap ->
                                let getTask taskName =
                                    taskName
                                    |> Option.map (fun taskName ->
                                        sortTaskMap
                                        |> Map.tryFind taskName
                                        |> function
                                        | Some task -> task
                                        | None ->
                                            failwithf
                                                "DslTaskSort. Task not found: %A. Map length: %A"
                                                taskName
                                                sortTaskMap.Count)

                                let interaction =
                                    Interaction.Task (task, TaskInteraction.Sort (getTask top, getTask bottom))

                                let userInteraction = UserInteraction (moment, user, interaction)

                                userInteractions
                                @ [
                                    userInteraction
                                ]
                            | None -> userInteractions

                        taskState, newUserInteractions
                    | DslStatusEntry (date, manualCellStatus) ->
                        let userInteraction = createCellStatusChangeInteraction user task date manualCellStatus

                        let newUserInteractions =
                            userInteractions
                            @ [
                                userInteraction
                            ]

                        taskState, newUserInteractions
                    | DslPriority priority ->
                        let newTaskState =
                            { taskState with
                                Task = { taskState.Task with Priority = Some priority }
                            }

                        newTaskState, userInteractions
                    | DslInformationReferenceToggle information ->
                        let newTaskState =
                            { taskState with
                                InformationMap = taskState.InformationMap |> Map.add information ()
                            }

                        newTaskState, userInteractions
                    | DslTaskSet set ->
                        match set with
                        | DslSetScheduling (scheduling, start) ->
                            let newTaskState =
                                { taskState with
                                    Task = { taskState.Task with Scheduling = scheduling }
                                }

                            newTaskState, userInteractions
                        | DslSetPendingAfter start ->
                            let newTaskState =
                                { taskState with
                                    Task = { taskState.Task with PendingAfter = Some start }
                                }

                            newTaskState, userInteractions
                        | DslSetMissedAfter start ->
                            let newTaskState =
                                { taskState with
                                    Task = { taskState.Task with MissedAfter = Some start }
                                }

                            newTaskState, userInteractions

                        | DslSetDuration minutes ->
                            let newTaskState =
                                { taskState with
                                    Task =
                                        { taskState.Task with
                                            Duration = Some (Minute (float minutes))
                                        }
                                }

                            newTaskState, userInteractions

                    )

        taskState, userInteractions



    let dslTreeWithUser (user: User) (dslTree: (Information * (string * DslTask list) list) list) =
        dslTree
        |> List.map (fun (information, tasks) ->
            let newTasks =
                tasks
                |> List.map (fun (taskName, dslTasks) ->
                    taskName,
                    dslTasks
                    |> List.map (fun dslTask -> dslTask, user))

            information, newTasks)

    //        taskTree
//        |> List.map (fun x -> x |> Tuple2.mapItem2 (fun x -> x |> List.map (fun x -> x |> Tuple2.mapItem2 (fun (a,b) -> x |> List.map (fun (event:DslTask) -> event, user)))))

    //    let createTreeData dayStart position taskTree =
//        let taskStateList =
//            taskTree
//            |> List.collect (fun (information, tasks) ->
//                tasks
//                |> List.map (fun (taskName, events: (DslTask * User) list) ->
//                    let task =
//                        { Task.Default with
//                            Name = TaskName taskName
//                            Information = information
//                        }
//
//                    createTaskState dayStart position task events))
//
//        let informationList =
//            taskTree |> List.map fst |> List.distinct
//
//        let taskOrderList =
//            taskStateList
//            |> List.map (fun (taskState, userInteractions) ->
//                {
//                    Task = taskState.Task
//                    Priority = TaskOrderPriority.Last
//                })
//
//        {
//            TaskStateList = taskStateList
//            TaskOrderList = taskOrderList
//            InformationList = informationList
//            GetLivePosition = getLivePosition
//        }

    //    type SessionState =
//        {
//            User: User option
//            GetLivePosition: unit -> FlukeDateTime
//            TreeStateMap: Map<TreeState, bool>
//        }

    type DslData =
        {
            //            GetLivePosition: (unit -> FlukeDateTime)
            InformationStateMap: Map<Information, InformationState>
            //            TaskOrderList: TaskOrderEntry list
            TaskStateList: (TaskState * UserInteraction list) list
        }

    let mergeInformationStateMap (oldMap: Map<Information, InformationState>)
                                 (newMap: Map<Information, InformationState>)
                                 =
        (oldMap, newMap)
        ||> Map.unionWith (fun oldValue newValue ->
                { oldValue with
                    Attachments = oldValue.Attachments @ newValue.Attachments
                    SortList = oldValue.SortList @ newValue.SortList
                })

    let mergeCellStateMap (oldMap: Map<DateId, CellState>) (newMap: Map<DateId, CellState>) = oldMap |> Map.union newMap

    let mergeInformationMap (oldMap: Map<Information, unit>) (newMap: Map<Information, unit>) =
        oldMap |> Map.union newMap

    let mergeTaskState (oldValue: TaskState) (newValue: TaskState) =
        { oldValue with
            Task = oldValue.Task
            Sessions = oldValue.Sessions @ newValue.Sessions
            Attachments = oldValue.Attachments @ newValue.Attachments
            SortList = oldValue.SortList @ newValue.SortList
            CellStateMap = mergeCellStateMap oldValue.CellStateMap newValue.CellStateMap
            InformationMap = mergeInformationMap oldValue.InformationMap newValue.InformationMap
        }

    let mergeTaskStateMap (oldMap: Map<Task, TaskState>) (newMap: Map<Task, TaskState>) =
        Map.unionWith mergeTaskState oldMap newMap


    let mergeTreeState (oldValue: TreeState) (newValue: TreeState) =
        { oldValue with
            InformationStateMap = mergeInformationStateMap oldValue.InformationStateMap newValue.InformationStateMap
            TaskStateMap = mergeTaskStateMap oldValue.TaskStateMap newValue.TaskStateMap
        }

    let mergeTreeStateMap (oldMap: Map<TreeId, TreeState>) (newMap: Map<TreeId, TreeState>) =
        Map.unionWith mergeTreeState oldMap newMap



    let createDslData moment taskContainerFactory (dslTree: (Information * (string * (DslTask * User) list) list) list) =

        let taskMap, taskStateList =
            let (|FirstPass|SecondPass|) =
                function
                | 0 -> FirstPass
                | _ -> SecondPass

            let rec loop pass taskMap =
                let newTaskMap, newTaskStateList =
                    ((taskMap, []), dslTree)
                    ||> List.fold (fun (taskMap, taskStateList) (information, tasks) ->
                            ((taskMap, taskStateList), tasks)
                            ||> List.fold (fun (taskMap, taskStateList) (taskName, dslTasks) ->
                                    let oldTask: Task option = taskMap |> Map.tryFind taskName

                                    let newTask =
                                        match oldTask with
                                        | Some task -> { task with Information = information }
                                        | None ->
                                            { Task.Default with
                                                Information = information
                                                Name = TaskName taskName
                                            }

                                    let fakeTaskMap =
                                        match pass with
                                        | FirstPass -> None
                                        | SecondPass ->
                                            taskMap
                                            |> Map.toSeq
                                            |> Seq.map (fun (taskName, task) -> TaskName taskName, task)
                                            |> Map.ofSeq
                                            |> Some

                                    let taskState, userInteractions =
                                        createTaskState moment newTask fakeTaskMap dslTasks

                                    let newTaskMap = taskMap |> Map.add taskName taskState.Task

                                    let newTaskStateList =
                                        taskStateList
                                        @ [
                                            taskState, userInteractions
                                        ]

                                    newTaskMap, newTaskStateList))

                match pass with
                | 0 -> loop 1 newTaskMap
                | _ -> newTaskMap, newTaskStateList

            loop 0 Map.empty

        //        printfn
//            "taskMap taskStateList ### %A ### ### %A ###"
//            (taskMap|>Map.tryPick (fun k v -> if k.Contains "week view" then Some v else None))
//            (taskStateList |> List.filter (fun (taskState, interactions) -> let (TaskName taskName) = taskState.Task.Name in taskName.Contains "week view"))

        let informationStateMap =
            dslTree
            |> List.map fst
            |> List.distinct
            |> informationListToStateMap

        //        let taskOrderList =
//            taskStateList
//            |> List.map (fun (taskState, _) ->
//                {
//                    Task = taskState.Task
//                    Priority = TaskOrderPriority.Last
//                })

        let duplicated =
            taskStateList
            |> List.map (fun (taskState, _) -> taskState.Task.Name)
            |> List.groupBy id
            |> List.filter
                (snd
                 >> List.length
                 >> fun n -> n > 1)
            |> List.map fst

        if not duplicated.IsEmpty then
            failwithf "Duplicated task names: %A" duplicated

        let tasks =
            taskContainerFactory (fun taskName ->
                taskMap
                |> Map.tryFind taskName
                |> Option.orElseWith (fun () -> failwithf "createDslData. Task not found: %A" taskName))


        //        let taskOrderList = getTaskOrderList [] (taskStateList |> List.map fst) []

        let dslData =
            {
                TaskStateList = taskStateList
                //                TaskOrderList = taskOrderList
                InformationStateMap = informationStateMap
            }

        dslData, tasks

    let mergeDslDataIntoTreeState (dslData: DslData) (treeState: TreeState) =

        let newInformationStateMap = mergeInformationStateMap treeState.InformationStateMap dslData.InformationStateMap

        let taskStateList, userInteractionsBundle = dslData.TaskStateList |> List.unzip


        let userInteractions = userInteractionsBundle |> List.collect id

        let newTreeState = treeStateWithInteractions userInteractions treeState

        let newTaskStateMap =
            (newTreeState.TaskStateMap, taskStateList)
            ||> List.fold (fun taskStateMap taskState ->
                    let oldTaskState =
                        newTreeState.TaskStateMap
                        |> Map.tryFind taskState.Task

                    let newTaskState =
                        match oldTaskState with
                        | Some oldTaskState -> mergeTaskState oldTaskState taskState
                        | None -> taskState

                    taskStateMap
                    |> Map.add taskState.Task newTaskState)

        let result =
            { newTreeState with
                InformationStateMap = newInformationStateMap
                TaskStateMap = newTaskStateMap
            }

        result




    module Testing =

        //        let dslDataToTreeState user dslData =
//            let userInteractions =
//                dslData.TaskStateList |> List.collect snd
//
//            let treeStateWithoutInteractions: TreeState =
//                {
//                    Id =
//                        TreeId
//                        <| Guid "17A1AA3D-95C7-424E-BD6D-7C12B33CED37"
//                    Name = State.TreeName "dslDataToState"
//                    Owner = user
//                    SharedWith = []
//                    Position = None
//                    InformationStateMap = dslData.InformationStateMap
//                    TaskStateMap = Map.empty
//                }
//
//            treeStateWithoutInteractions
//            |> TreeStateWithInteractions userInteractions

        let createLaneRenderingDslData (input: {| User: User
                                                  Position: FlukeDateTime
                                                  Task: Task
                                                  Events: DslTask list
                                                  Expected: (FlukeDate * CellStatus) list |}) =
            let eventsWithUser = input.Events |> List.map (fun x -> x, input.User)

            let dslData =
                {
                    TaskStateList =
                        [
                            createTaskState input.Position input.Task None eventsWithUser
                        ]
                    //                    TaskOrderList =
//                        [
//                            {
//                                Task = input.Task
//                                Priority = TaskOrderPriority.First
//                            }
//                        ]
//                    //                    GetLivePosition = fun () -> input.Position
                    InformationStateMap =
                        [
                            input.Task.Information
                        ]
                        |> informationListToStateMap
                }

            //            let getLivePosition = fun () -> input.Position

            //            let treeState = dslDataToTreeState input.User dslData

            //            let treeStateMap =
//                [ treeState.Id, (treeState, true) ] |> Map.ofList

            //            let state =
//                {
//                    User = Some input.User
//                    GetLivePosition = getLivePosition
//                    TreeStateMap = treeStateMap
//                }

            dslData


        let createLaneSortingDslData (input: {| User: User
                                                Position: FlukeDateTime
                                                Data: (Task * DslTask list) list
                                                Expected: string list |}) =
            let dslData =
                let taskStateList =
                    input.Data
                    |> List.map (fun (task, events) ->
                        events
                        |> List.map (fun dslTask -> dslTask, input.User)
                        |> createTaskState input.Position task None)

                //                let taskOrderList =
//                    input.Data
//                    |> List.map (fun (task, events) -> { Task = task; Priority = TaskOrderPriority.Last })

                let getLivePosition = fun () -> input.Position

                let informationStateMap =
                    taskStateList
                    |> List.map (fun (taskState, _) -> taskState.Task.Information)
                    |> List.distinct
                    |> informationListToStateMap

                {
                    TaskStateList = taskStateList
                    //                    TaskOrderList = taskOrderList
                    //                    GetLivePosition = getLivePosition
                    InformationStateMap = informationStateMap
                }

            //            let getLivePosition = fun () -> input.Position
//
//            let treeState = dslDataToTreeState input.User dslData
//
//            let treeStateMap =
//                [ treeState.Id, (treeState, true) ] |> Map.ofList
//
//            let state =
//                {
//                    User = Some input.User
//                    GetLivePosition = getLivePosition
//                    TreeStateMap = treeStateMap
//                }

            dslData

    module PublicData =
        open Domain.Information
        open Domain.UserInteraction
        open Domain.State

        let getPublicData () =
            let users = getUsers ()
            let projects = getPublicProjects ()
            let areas = getPublicAreas ()
            let resources = getPublicResources ()

            let tempData =
                {|
                    ManualTasks =
                        [
                            Project (projects.app_fluke, []),
                            [
                                "data management",
                                [
                                    DslTaskComment "mutability", users.fluke
                                    DslTaskComment "initial default data (load the text first with tests)", users.fluke
                                ]
                                "cell selection (mouse, vim navigation)", []
                                "data structures performance", []
                                "side panel (journal, comments)", []
                                "add task priority (for randomization)", []
                                "persistence",
                                [
                                    DslTaskComment "data encryption", users.fluke
                                ]
                                "vivaldi or firefox bookmark integration",
                                [
                                    DslTaskComment "browser.html javascript injection or browser extension", users.fluke
                                ]
                                "telegram integration (fast link sharing)", []
                                "mobile layout", []
                                "move fluke tasks to github issues", []
                            ]
                            Project (projects.blog, []), []
                            Project (projects.rebuild_website, []),
                            [
                                "task1", []
                            ]
                            Area (areas.car, []), []
                            Area (areas.career, []), []
                            Area (areas.chores, []), []
                            Area (areas.fitness, []), []
                            Area (areas.food, []), []
                            Area (areas.finances, []), []
                            Area (areas.health, []), []
                            Area (areas.leisure, []),
                            [
                                "watch_movie_foobar", []
                            ]
                            Area (areas.programming, []), []
                            Area (areas.travel, []), []
                            Area (areas.workflow, []), []
                            Area (areas.writing, []), []
                            Resource (resources.agile, []), []
                            Resource (resources.artificial_intelligence, []), []
                            Resource (resources.cloud, []), []
                            Resource (resources.communication, []), []
                            Resource (resources.docker, []), []
                            Resource (resources.fsharp, []),
                            [
                                "study: [choice, computation expressions]", []
                                "organize youtube playlists", []
                            ]
                            Resource (resources.linux, []), []
                            Resource (resources.music, []), []
                            Resource (resources.rust, []), []
                            Resource (resources.vim, []), []
                            Resource (resources.windows, []), []
                        ]
                |}

            tempData
