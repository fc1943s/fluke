namespace Fluke.Shared

open System
open System.Collections.Generic
open FSharpPlus
open Suigetsu.Core


module TempData =
    open Model
    open Old

    module private Areas =
        let car = { Name = AreaName "car" }
        let career = { Name = AreaName "career" }
        let chores = { Name = AreaName "chores" }
        let finances = { Name = AreaName "finances" }
        let fitness = { Name = AreaName "fitness" }
        let food = { Name = AreaName "food" }
        let health = { Name = AreaName "health" }
        let leisure = { Name = AreaName "leisure" }
        let programming = { Name = AreaName "programming" }
        let travel = { Name = AreaName "travel" }
        let workflow = { Name = AreaName "workflow" }
        let writing = { Name = AreaName "writing" }


    module private Projects =
        let app_fluke =
            {
                Project.Area = Areas.workflow
                Project.Name = ProjectName "app_fluke"
            }

        let blog =
            {
                Project.Area = Areas.writing
                Project.Name = ProjectName "blog"
            }

        let rebuild_website =
            {
                Project.Area = Areas.programming
                Project.Name = ProjectName "rebuild_website"
            }


    module private Resources =
        let agile =
            {
                Area = Areas.programming
                Name = ResourceName "agile"
            }

        let artificial_intelligence =
            {
                Area = Areas.programming
                Name = ResourceName "artificial_intelligence"
            }

        let cloud =
            {
                Area = Areas.programming
                Name = ResourceName "cloud"
            }

        let communication =
            {
                Area = Areas.workflow
                Name = ResourceName "communication"
            }

        let docker =
            {
                Area = Areas.programming
                Name = ResourceName "docker"
            }

        let fsharp =
            {
                Area = Areas.programming
                Name = ResourceName "f#"
            }

        let linux =
            {
                Area = Areas.programming
                Name = ResourceName "linux"
            }

        let music =
            {
                Area = Areas.leisure
                Name = ResourceName "music"
            }

        let rust =
            {
                Area = Areas.programming
                Name = ResourceName "rust"
            }

        let vim =
            {
                Area = Areas.programming
                Name = ResourceName "vim"
            }

        let windows =
            {
                Area = Areas.programming
                Name = ResourceName "windows"
            }


    module Users =
        let rec fluke =
            {
                Username = nameof fluke
                Color = UserColor.Black
                WeekStart = DayOfWeek.Sunday
                DayStart = FlukeTime.Create 12 00
            }

        let rec fc1943s =
            {
                Username = nameof fc1943s
                Color = UserColor.Blue
                WeekStart = DayOfWeek.Sunday
                DayStart = FlukeTime.Create 07 00
            }

        let rec liryanne =
            {
                Username = nameof liryanne
                Color = UserColor.Pink
                WeekStart = DayOfWeek.Monday
                DayStart = FlukeTime.Create 07 00
            }

        let users = [ fluke; fc1943s; liryanne ]


    module Consts =
        let rootPath =
            """M:\Dropbox\home_encrypted\projects\app-fluke"""

        let dbPath = rootPath + """\db_v1"""

        [<Literal>]
        let sessionLength = 25.

        [<Literal>]
        let sessionBreakLength = 5.

        let defaultDate = FlukeDate.FromDateTime DateTime.MinValue
        let dayStart = FlukeTime.Create 07 00
        let defaultPosition = { Date = defaultDate; Time = dayStart }


    let getLivePosition () = FlukeDateTime.FromDateTime DateTime.Now




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
//                            let newStatus = UserStatus (user, Completed)
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




    let getTaskOrderList oldTaskOrderList (taskStateList: State.TaskState list) manualTaskOrder =
        let taskMap =
            taskStateList
            |> List.map (fun x -> (x.Task.Information, x.Task.Name), x)
            |> Map.ofList

        let newTaskOrderList =
            manualTaskOrder
            |> List.map (fun (information, taskName) ->
                taskMap
                |> Map.tryFind (information, TaskName taskName)
                |> function
                | None -> failwithf "Invalid task: '%A/%s'" information taskName
                | Some taskState ->
                    {
                        Task = taskState.Task
                        Priority = TaskOrderPriority.First
                    })

        oldTaskOrderList @ newTaskOrderList


    type DslTask =
        | DslTaskComment of comment: string
        | DslSession of start: FlukeDateTime
        | DslPriority of priority: Priority
        | DslInformationReferenceToggle of information: Information
        | DslStatusEntry of date: FlukeDate * manualCellStatus: ManualCellStatus
        | DslCellComment of date: FlukeDate * comment: string
        | DslTaskSet of taskSet: DslTaskSet
        | DslTaskSort of top: Task option * bottom: Task option

    and DslTaskSet =
        | DslSetScheduling of scheduling: Scheduling * start: FlukeDate option
        | DslSetPendingAfter of start: FlukeTime
        | DslSetMissedAfter of start: FlukeTime
        | DslSetDuration of duration: int

    let createTaskCommentInteraction user moment task comment =
        let interaction =
            Interaction.Task (task, TaskInteraction.Attachment (Attachment.Comment (user, Comment comment)))

        let userInteraction =
            UserInteraction (user, moment, interaction)

        userInteraction

    let createInformationCommentInteraction user moment information comment =
        let interaction =
            Interaction.Information
                (information, InformationInteraction.Attachment (Attachment.Comment (user, Comment comment)))

        let userInteraction =
            UserInteraction (user, moment, interaction)

        userInteraction

    let createCellCommentInteraction dayStart task moment user comment =
        let cellInteraction =
            Attachment.Comment (user, Comment comment)
            |> CellInteraction.Attachment

        let cellAddress =
            {
                Task = task
                DateId = dateId dayStart moment
            }

        let interaction =
            Interaction.Cell (cellAddress, cellInteraction)

        let userInteraction =
            UserInteraction (user, moment, interaction)

        userInteraction

    let createCellStatusChangeInteraction dayStart user task date manualCellStatus =
        let cellStatusChange =
            match manualCellStatus with
            | Completed -> CellStatusChange.Complete
            | Dismissed -> CellStatusChange.Dismiss
            | Postponed until -> CellStatusChange.Postpone until
            | ManualPending -> CellStatusChange.Schedule

        let cellInteraction =
            CellInteraction.StatusChange cellStatusChange

        let dateId = DateId date

        let cellAddress = { Task = task; DateId = dateId }

        let interaction =
            Interaction.Cell (cellAddress, cellInteraction)

        let moment = { Date = date; Time = dayStart }

        let userInteraction =
            UserInteraction (user, moment, interaction)

        userInteraction

    let createCellStatusChangeInteractions dayStart user (entries: (FlukeDate * (Task * ManualCellStatus) list) list) =
        entries
        |> List.collect (fun (date, events) ->
            events
            |> List.map (fun (task, manualCellStatus) ->
                createCellStatusChangeInteraction dayStart user task date manualCellStatus))


    let createTaskState dayStart position task (dslEntries: (DslTask * User) list) =

        let defaultTaskState: State.TaskState =
            {
                Task = task
                Sessions = []
                Attachments = []
                SortList = []
                CellStateMap = Map.empty
                InformationMap = Map.empty
            }

        let taskState, userInteractions =
            ((defaultTaskState, []), dslEntries)
            ||> List.fold (fun (taskState, userInteractions) (dslTask, user) ->
                    match dslTask with
                    | DslTaskComment comment ->
                        let interaction =
                            Interaction.Task
                                (task, TaskInteraction.Attachment (Attachment.Comment (user, Comment comment)))

                        let userInteraction =
                            UserInteraction (user, position, interaction)

                        let newUserInteractions = userInteractions @ [ userInteraction ]
                        taskState, newUserInteractions
                    | DslCellComment (date, comment) ->
                        let interaction =
                            Interaction.Cell
                                ({ Task = task; DateId = DateId date },
                                 CellInteraction.Attachment (Attachment.Comment (user, Comment comment)))

                        let userInteraction =
                            UserInteraction (user, position, interaction)

                        let newUserInteractions = userInteractions @ [ userInteraction ]
                        taskState, newUserInteractions
                    | (DslSession ({ Date = date; Time = time })) ->
                        let taskSession =
                            TaskSession
                                ({ Date = date; Time = time },
                                 Minute Consts.sessionLength,
                                 Minute Consts.sessionBreakLength)

                        let taskInteraction = TaskInteraction.Session taskSession
                        let interaction = Interaction.Task (task, taskInteraction)

                        let userInteraction =
                            UserInteraction (user, position, interaction)

                        let newUserInteractions = userInteractions @ [ userInteraction ]
                        taskState, newUserInteractions
                    | DslTaskSort (top, bottom) ->
                        let interaction =
                            Interaction.Task (task, TaskInteraction.Sort (top, bottom))

                        let userInteraction =
                            UserInteraction (user, position, interaction)

                        let newUserInteractions = userInteractions @ [ userInteraction ]
                        taskState, newUserInteractions
                    | DslStatusEntry (date, manualCellStatus) ->
                        let userInteraction =
                            createCellStatusChangeInteraction dayStart user task date manualCellStatus

                        let newUserInteractions = userInteractions @ [ userInteraction ]
                        taskState, newUserInteractions
                    | DslPriority priority ->
                        let newTaskState =
                            { taskState with
                                Task =
                                    { taskState.Task with
                                        Priority = Some priority
                                    }
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
                                    Task =
                                        { taskState.Task with
                                            Scheduling = scheduling
                                        }
                                }

                            newTaskState, userInteractions
                        | DslSetPendingAfter start ->
                            let newTaskState =
                                { taskState with
                                    Task =
                                        { taskState.Task with
                                            PendingAfter = Some start
                                        }
                                }

                            newTaskState, userInteractions
                        | DslSetMissedAfter start ->
                            let newTaskState =
                                { taskState with
                                    Task =
                                        { taskState.Task with
                                            MissedAfter = Some start
                                        }
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



    let treeDataWithUser user taskTree =
        taskTree
        |> List.map (Tuple2.mapItem2 (List.map (Tuple2.mapItem2 (List.map (fun event -> event, user)))))

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

    type State =
        {
            User: User option
            GetLivePosition: unit -> FlukeDateTime
            TreeStateMap: Map<State.TreeId, State.TreeState * bool>
        }

    type DslData =
        {
            GetLivePosition: (unit -> FlukeDateTime)
            InformationStateMap: Map<Information, State.InformationState>
            TaskOrderList: TaskOrderEntry list
            TaskStateList: (State.TaskState * UserInteraction list) list
        }

    let mergeInformationStateMap (oldMap: Map<Information, State.InformationState>)
                                 (newMap: Map<Information, State.InformationState>)
                                 =
        (oldMap, newMap)
        ||> Map.mapValues2 (fun oldValue newValue ->
                { oldValue with
                    Attachments = oldValue.Attachments @ newValue.Attachments
                    SortList = oldValue.SortList @ newValue.SortList
                })

    let mergeCellStateMap (oldMap: Map<DateId, State.CellState>) (newMap: Map<DateId, State.CellState>) =
        (oldMap, newMap)
        ||> Map.mapValues2 (fun oldValue newValue -> newValue)

    let mergeInformationMap (oldMap: Map<Information, unit>) (newMap: Map<Information, unit>) =
        (oldMap, newMap)
        ||> Map.mapValues2 (fun oldValue newValue -> newValue)

    let mergeTaskState (oldValue: State.TaskState) (newValue: State.TaskState) =
        { oldValue with
            Sessions = oldValue.Sessions @ newValue.Sessions
            Attachments = oldValue.Attachments @ newValue.Attachments
            SortList = oldValue.SortList @ newValue.SortList
            CellStateMap = mergeCellStateMap oldValue.CellStateMap newValue.CellStateMap
            InformationMap = mergeInformationMap oldValue.InformationMap newValue.InformationMap
        }

    let mergeTaskStateMap (oldMap: Map<Task, State.TaskState>) (newMap: Map<Task, State.TaskState>) =
        (oldMap, newMap) ||> Map.mapValues2 mergeTaskState


    let mergeTreeState (oldValue: State.TreeState) (newValue: State.TreeState) =
        { oldValue with
            InformationStateMap = mergeInformationStateMap oldValue.InformationStateMap newValue.InformationStateMap
            TaskStateMap = mergeTaskStateMap oldValue.TaskStateMap newValue.TaskStateMap
        }

    let mergeTreeStateMap (oldMap: Map<State.TreeId, State.TreeState>) (newMap: Map<State.TreeId, State.TreeState>) =

        (oldMap, newMap) ||> Map.mapValues2 mergeTreeState

    // How the HELL will I rewrite this? 🤦
    let dslDataFactory taskContainerFactory
                       (dslTreeGetter: ((string -> Task) -> (Information * (string * (DslTask * User) list) list) list))
                       =

        let taskDictionary = Dictionary<string, Task> ()

        let mutable dslDataMaybe = None
        let mutable taskGetter = fun _ -> Task.Default

        for n in [ 0; 1 ] do

            let taskGetterInternal taskName =
                let found, task = taskDictionary.TryGetValue taskName
                if found then
                    task
                else
                    printfn "task not found[%d]: %A" n taskName
                    Task.Default

            taskGetter <- taskGetterInternal

            let dslTree = dslTreeGetter taskGetterInternal

            let taskStateList =
                let rec informationLoop dslTree =
                    match dslTree with
                    | (information, tasks) :: tail ->
                        let rec tasksLoop tasks =
                            match tasks with
                            | (taskName, dslEntries) :: tail ->
                                let task =
                                    { Task.Default with
                                        Information = information
                                        Name = TaskName taskName
                                    }

                                taskDictionary.[taskName] <- task

                                let taskState =
                                    createTaskState Consts.dayStart Consts.defaultPosition task dslEntries

                                taskState :: tasksLoop tail
                            | [] -> []

                        tasksLoop tasks @ informationLoop tail
                    | [] -> []

                informationLoop dslTree

            let informationStateMap =
                dslTree
                |> List.map fst
                |> List.distinct
                |> State.informationListToStateMap

            let taskOrderList =
                taskStateList
                |> List.map (fun (taskState, _) ->
                    {
                        Task = taskState.Task
                        Priority = TaskOrderPriority.Last
                    })

            let dslData =
                {
                    TaskStateList = taskStateList
                    TaskOrderList = taskOrderList
                    InformationStateMap = informationStateMap
                    GetLivePosition = getLivePosition
                }

            dslDataMaybe <- Some dslData

        let dslData = dslDataMaybe.Value


        //        printfn "treeData %A" treeData

        let duplicated =
            dslData.TaskStateList
            //            |> List.filter (fun taskState -> taskState.Task <> Task.Default)
            |> List.map (fun (taskState, _) -> taskState.Task.Name)
            |> List.groupBy id
            |> List.filter
                (snd
                 >> List.length
                 >> fun n -> n > 1)
            |> List.map fst

        if not duplicated.IsEmpty then
            failwithf "Duplicated task names: %A" duplicated

        //        taskStateMap <-
//            treeData.TaskStateList
//            |> List.map (fun taskState -> taskState.Task.Name, taskState)
//            |> Map.ofList
//
//        treeDataMaybe <- Some treeData


        let tasks = taskContainerFactory taskGetter



        let taskOrderList =
            getTaskOrderList [] (dslData.TaskStateList |> List.map fst) []

        let newDslData =
            { dslData with
                TaskOrderList = taskOrderList
            }

        newDslData, tasks





    module Testing =
        module Consts =
            let testDayStart = FlukeTime.Create 12 00

        let dslDataToTreeState user dslData =
            let userInteractions =
                dslData.TaskStateList |> List.collect snd

            let treeStateWithoutInteractions: State.TreeState =
                {
                    Id =
                        State.TreeId
                        <| Guid "17A1AA3D-95C7-424E-BD6D-7C12B33CED37"
                    Name = State.TreeName "dslDataToState"
                    Owner = user
                    SharedWith = []
                    Position = None
                    InformationStateMap = dslData.InformationStateMap
                    TaskStateMap = Map.empty
                }

            treeStateWithoutInteractions
            |> State.treeStateWithInteractions userInteractions

        let createRenderLaneTestData (testData: {| Position: FlukeDateTime
                                                   Expected: (FlukeDate * CellStatus) list
                                                   Events: DslTask list
                                                   Task: Task |}) =
            let user = Users.fluke

            let eventsWithUser =
                testData.Events |> List.map (fun x -> x, user)

            let dslData =
                {
                    TaskStateList =
                        [
                            createTaskState Consts.testDayStart testData.Position testData.Task eventsWithUser
                        ]
                    TaskOrderList =
                        [
                            {
                                Task = testData.Task
                                Priority = TaskOrderPriority.First
                            }
                        ]
                    GetLivePosition = fun () -> testData.Position
                    InformationStateMap =
                        [ testData.Task.Information ]
                        |> State.informationListToStateMap
                }

            let getLivePosition = fun () -> testData.Position

            let treeState = dslDataToTreeState Users.fluke dslData

            let treeStateMap =
                [ treeState.Id, (treeState, true) ] |> Map.ofList

            let state =
                {
                    User = Some Users.fluke
                    GetLivePosition = getLivePosition
                    TreeStateMap = treeStateMap
                }

            dslData


        let createSortLanesTestData (testData: {| Position: FlukeDateTime
                                                  Data: (Task * DslTask list) list
                                                  Expected: string list |}) =
            let user = Users.fluke

            let dslData =
                let taskStateList =
                    testData.Data
                    |> List.map (fun (task, events) ->
                        events
                        |> List.map (fun dslTask -> dslTask, user)
                        |> createTaskState Consts.testDayStart testData.Position task)

                let taskOrderList =
                    testData.Data
                    |> List.map (fun (task, events) ->
                        {
                            Task = task
                            Priority = TaskOrderPriority.Last
                        })

                let getLivePosition = fun () -> testData.Position

                let informationStateMap =
                    taskStateList
                    |> List.map (fun (taskState, _) -> taskState.Task.Information)
                    |> List.distinct
                    |> State.informationListToStateMap

                {
                    TaskStateList = taskStateList
                    TaskOrderList = taskOrderList
                    GetLivePosition = getLivePosition
                    InformationStateMap = informationStateMap
                }

            let getLivePosition = fun () -> testData.Position

            let treeState = dslDataToTreeState Users.fluke dslData

            let treeStateMap =
                [ treeState.Id, (treeState, true) ] |> Map.ofList

            let state =
                {
                    User = Some Users.fluke
                    GetLivePosition = getLivePosition
                    TreeStateMap = treeStateMap
                }

            dslData

        let tempData =
            {|
                ManualTasks =
                    [
                        Project Projects.app_fluke,
                        [
                            "data management",
                            [
                                DslTaskComment "mutability", Users.fluke
                                DslTaskComment "initial default data (load the text first with tests)", Users.fluke
                            ]
                            "cell selection (mouse, vim navigation)", []
                            "data structures performance", []
                            "side panel (journal, comments)", []
                            "add task priority (for randomization)", []
                            "persistence",
                            [
                                DslTaskComment "data encryption", Users.fluke
                            ]
                            "vivaldi or firefox bookmark integration",
                            [
                                DslTaskComment "browser.html javascript injection or browser extension", Users.fluke
                            ]
                            "telegram integration (fast link sharing)", []
                            "mobile layout", []
                            "move fluke tasks to github issues", []
                        ]
                        Project Projects.blog, []
                        Project Projects.rebuild_website, [ "task1", [] ]
                        Area Areas.car, []
                        Area Areas.career, []
                        Area Areas.chores, []
                        Area Areas.fitness, []
                        Area Areas.food, []
                        Area Areas.finances, []
                        Area Areas.health, []
                        Area Areas.leisure, [ "watch_movie_foobar", [] ]
                        Area Areas.programming, []
                        Area Areas.travel, []
                        Area Areas.workflow, []
                        Area Areas.writing, []
                        Resource Resources.agile, []
                        Resource Resources.artificial_intelligence, []
                        Resource Resources.cloud, []
                        Resource Resources.communication, []
                        Resource Resources.docker, []
                        Resource Resources.fsharp,
                        [
                            "study: [choice, computation expressions]", []
                            "organize youtube playlists", []
                        ]
                        Resource Resources.linux, []
                        Resource Resources.music, []
                        Resource Resources.rust, []
                        Resource Resources.vim, []
                        Resource Resources.windows, []
                    ]
                RenderLaneTests =
                    {|
                        Task =
                            { Task.Default with
                                Scheduling =
                                    Recurrency
                                        (Fixed [
                                            Weekly DayOfWeek.Monday
                                            Weekly DayOfWeek.Tuesday
                                            Weekly DayOfWeek.Wednesday
                                            Weekly DayOfWeek.Thursday
                                            Weekly DayOfWeek.Friday
                                         ])
                                PendingAfter = Some (FlukeTime.Create 19 00)
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.August 26
                                Time = Consts.testDayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.August 25, Disabled
                                FlukeDate.Create 2020 Month.August 26, Suggested
                                FlukeDate.Create 2020 Month.August 27, Pending
                            ]
                        Events = []
                    |}
                    |> createRenderLaneTestData
                SortLanesTests =

                    {|
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 14 00
                            }
                        Data =
                            [
                                { Task.Default with
                                    Name = TaskName "01"
                                    Scheduling = Manual WithSuggestion
                                },
                                []

                                { Task.Default with
                                    Name = TaskName "02"
                                    Scheduling = Manual WithSuggestion
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Postponed None)
                                ]

                                { Task.Default with
                                    Name = TaskName "03"
                                    Scheduling = Manual WithoutSuggestion
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 09, ManualPending)
                                ]

                                { Task.Default with
                                    Name = TaskName "04"
                                    Scheduling = Recurrency (Offset (Days 1))
                                    PendingAfter = FlukeTime.Create 20 00 |> Some
                                },
                                []

                                { Task.Default with
                                    Name = TaskName "05"
                                    Scheduling = Manual WithoutSuggestion
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 10, ManualPending)
                                ]

                                { Task.Default with
                                    Name = TaskName "06"
                                    Scheduling = Manual WithoutSuggestion
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 04, Postponed None)
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 06, Dismissed)
                                ]

                                { Task.Default with
                                    Name = TaskName "07"
                                    Scheduling = Recurrency (Offset (Days 4))
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Completed)
                                ]

                                { Task.Default with
                                    Name = TaskName "08"
                                    Scheduling = Recurrency (Offset (Days 2))
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Completed)
                                ]

                                { Task.Default with
                                    Name = TaskName "09"
                                    Scheduling = Recurrency (Offset (Days 2))
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Dismissed)
                                ]

                                { Task.Default with
                                    Name = TaskName "10"
                                    Scheduling = Recurrency (Offset (Days 2))
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                ]

                                { Task.Default with
                                    Name = TaskName "11"
                                    Scheduling = Recurrency (Offset (Days 1))
                                },
                                [
                                    DslStatusEntry
                                        (FlukeDate.Create 2020 Month.March 10,
                                         Postponed (FlukeTime.Create 13 00 |> Some))
                                ]

                                { Task.Default with
                                    Name = TaskName "12"
                                    Scheduling = Manual WithoutSuggestion
                                },
                                []

                                { Task.Default with
                                    Name = TaskName "13"
                                    Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ])
                                },
                                []

                                { Task.Default with
                                    Name = TaskName "14"
                                    Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ])
                                },
                                []

                                { Task.Default with
                                    Name = TaskName "15"
                                    Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ])
                                },
                                [
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 07, Postponed None)
                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 09, Dismissed)
                                ]

                                { Task.Default with
                                    Name = TaskName "16"
                                    Scheduling = Recurrency (Offset (Days 1))
                                    MissedAfter = (FlukeTime.Create 13 00 |> Some)
                                },
                                []

                                { Task.Default with
                                    Name = TaskName "17"
                                    Scheduling = Recurrency (Offset (Days 1))
                                },
                                [
                                    DslStatusEntry
                                        (FlukeDate.Create 2020 Month.March 10,
                                         Postponed (FlukeTime.Create 15 00 |> Some))
                                ]

                                { Task.Default with
                                    Name = TaskName "18"
                                    Scheduling = Recurrency (Offset (Days 1))
                                },
                                []
                            ]
                        Expected =
                            [
                                "16"
                                "05"
                                "03"
                                "11"
                                "13"
                                "18"
                                "17"
                                "04"
                                "01"
                                "02"
                                "10"
                                "08"
                                "09"
                                "06"
                                "07"
                                "12"
                                "14"
                                "15"
                            ]
                    |}

                    |> createSortLanesTestData


            |}
