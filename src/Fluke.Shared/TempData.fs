namespace Fluke.Shared

open System
open FSharpPlus
open Suigetsu.Core


module TempData =
    open Model

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
        let fc1943s =
            {
                Username = "fc1943s"
                Color = UserColor.Blue
            }

        let liryanne =
            {
                Username = "liryanne"
                Color = UserColor.Pink
            }

        let testUser =
            {
                Username = "Test"
                Color = UserColor.Blue
            }

        let users = [ fc1943s; liryanne ]


    module Consts =
        let rootPath =
            """M:\Dropbox\home_encrypted\projects\app-fluke"""

        let dbPath = rootPath + """\db_v1"""

        [<Literal>]
        let sessionLength = 25.

        [<Literal>]
        let sessionBreakLength = 5.

        let defaultDate = FlukeDate.FromDateTime DateTime.MinValue
        let dayStart = flukeTime 07 00
        let defaultPosition = { Date = defaultDate; Time = dayStart }


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


        let eventsFromCellComments user = ()


        module EventSourcingTemp =
            type Command = CompleteCell of user: User * task: Task * date: FlukeDateTime
            type Event = CellCompleted of user: User * task: Task * date: FlukeDateTime

            type State =
                {
                    DayStart: FlukeTime
                    TaskMap: Map<TaskId, Map<DateId, CellStatus>>
                    TaskIdList: TaskId list
                }

            let initialState =
                {
                    DayStart = Consts.dayStart
                    TaskMap = Map.empty
                    TaskIdList = []
                }

            let private apply (state: State) (event: Event): State =
                match event with
                | CellCompleted (user, task, moment) ->
                    { state with
                        TaskMap =
                            let taskId = taskId task
                            let dateId = dateId state.DayStart moment
                            let newStatus = UserStatus (user, Completed)

                            let cellMap =
                                state.TaskMap
                                |> Map.tryFind taskId
                                |> Option.defaultValue Map.empty
                                |> Map.add dateId newStatus

                            state.TaskMap |> Map.add taskId cellMap
                    }

            let execute (state: State) (command: Command): Event list =
                [
                    match command, state with
                    | CompleteCell (user, task, moment), state when state.TaskMap
                                                                    |> Map.tryFind (taskId task)
                                                                    |> Option.defaultValue Map.empty
                                                                    |> Map.tryFind (dateId state.DayStart moment)
                                                                    |> Option.defaultValue Disabled
                                                                    |> (=) Missed -> CellCompleted (user, task, moment)

                    | CompleteCell (user, task, eventDate), state -> CellCompleted (user, task, eventDate)

                    | _ -> ()
                ]

            let build = List.fold apply
            let rebuld = build initialState


    let getLivePosition () = FlukeDateTime.FromDateTime DateTime.Now

    let getTaskOrderList oldTaskOrderList (taskStateList: TaskState list) manualTaskOrder =
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
        | DslTaskInteraction of interaction: TaskInteraction

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


    let applyTaskEvents dayStart task (events: (DslTask * User) list) =
        let defaultTaskState =
            {
                Task = Task.Default
                Sessions = []
                CellInteractions = []
                UserInteractions = []
                InformationMap = Map.empty
                CellStateMap = Map.empty
            }

        let interactions =
            let moment = Consts.defaultPosition
            (defaultTaskState, events)
            ||> List.fold (fun taskState (event, user) -> // TODO: Why start?
                    match event with
                    | DslTaskInteraction taskInteraction ->
                        let interaction = Interaction.Task (task, taskInteraction)

                        let userInteraction =
                            UserInteraction (user, moment, interaction)

                        { taskState with
                            UserInteractions = taskState.UserInteractions @ [ userInteraction ]
                        }
                    | DslTaskComment comment ->
                        let interaction =
                            Interaction.Task
                                (task, TaskInteraction.Attachment (Attachment.Comment (user, Comment comment)))

                        let userInteraction =
                            UserInteraction (user, moment, interaction)

                        { taskState with
                            UserInteractions = taskState.UserInteractions @ [ userInteraction ]
                        }
                    | DslCellComment (date, comment) ->
                        let interaction =
                            Interaction.Cell
                                ({ Task = task; DateId = DateId date },
                                 CellInteraction.Attachment (Attachment.Comment (user, Comment comment)))

                        let userInteraction =
                            UserInteraction (user, moment, interaction)

                        { taskState with
                            UserInteractions = taskState.UserInteractions @ [ userInteraction ]
                        }
                    | (DslSession ({ Date = date; Time = time })) ->
                        let session =
                            TaskInteraction.Session
                                ({ Date = date; Time = time },
                                 Minute Consts.sessionLength,
                                 Minute Consts.sessionBreakLength)

                        { taskState with
                            Sessions = taskState.Sessions @ [ session ]
                        }
                    | DslStatusEntry (date, manualCellStatus) ->
                        let userInteraction =
                            createCellStatusChangeInteraction Consts.dayStart user task date manualCellStatus

                        { taskState with
                            UserInteractions = taskState.UserInteractions @ [ userInteraction ]
                        }
                    | DslPriority priority ->
                        { taskState with
                            Task =
                                { taskState.Task with
                                    Priority = Some priority
                                }
                        }
                    | DslInformationReferenceToggle information ->
                        { taskState with
                            InformationMap =
                                taskState.InformationMap
                                |> Map.add information true
                        }

                    | DslTaskSet set ->
                        match set with
                        | DslSetScheduling (scheduling, start) ->
                            { taskState with
                                Task =
                                    { taskState.Task with
                                        Scheduling = scheduling
                                    }
                            }
                        | DslSetPendingAfter start ->
                            { taskState with
                                Task =
                                    { taskState.Task with
                                        PendingAfter = Some start
                                    }
                            }
                        | DslSetMissedAfter start ->
                            { taskState with
                                Task =
                                    { taskState.Task with
                                        MissedAfter = Some start
                                    }
                            }

                        | DslSetDuration minutes ->
                            { taskState with
                                Task =
                                    { taskState.Task with
                                        Duration = Some (Minute (float minutes))
                                    }
                            })

        interactions



    let treeDataWithUser user taskTree =
        taskTree
        |> List.map (Tuple2.mapItem2 (List.map (Tuple2.mapItem2 (List.map (fun event -> event, user)))))

    let transformTreeData dayStart taskTree =
        let taskStateList =
            taskTree
            |> List.collect (fun (information, tasks) ->
                tasks
                |> List.map (fun (taskName, events: (DslTask * User) list) ->
                    printfn "TASKNAME %A" taskName

                    let task =
                        { Task.Default with
                            Name = TaskName taskName
                            Information = information
                        }

                    applyTaskEvents dayStart task events))

        let informationList =
            taskTree |> List.map fst |> List.distinct

        let taskOrderList =
            taskStateList
            |> List.map (fun taskState ->
                {
                    Task = taskState.Task
                    Priority = TaskOrderPriority.Last
                })

        {|
            TaskStateList = taskStateList
            TaskOrderList = taskOrderList
            InformationList = informationList
            GetLivePosition = getLivePosition
        |}

    // How the HELL will I rewrite this? 🤦
    let treeDataFactory taskContainerFactory dslTree =
        let mutable taskStateMap: Map<TaskName, TaskState> = Map.empty
        let mutable taskStateList = []

        let mutable treeDataMaybe: {| GetLivePosition: unit -> FlukeDateTime
                                      InformationList: Information list
                                      TaskOrderList: TaskOrderEntry list
                                      TaskStateList: TaskState list |} option = None

        let getTask fail name =
            let taskState =
                taskStateMap |> Map.tryFind (TaskName name)

            let result =
                match taskState, fail with
                | Some taskState, _ when taskState.Task <> Task.Default -> taskState.Task
                | None, false -> Task.Default
                | _ -> failwithf "error searching task %A" name

            printfn "getTask %A; result: %A" name result.Name
            result

        for n in [ 0; 1 ] do
            printfn "loop %A" n
            printfn "taskStateMap %A" taskStateMap

            let treeData =
                dslTree (getTask (n = 1))
                |> transformTreeData Consts.dayStart

            printfn "treeData %A" treeData

            let duplicated =
                treeData.TaskStateList
                |> List.filter (fun taskState -> taskState.Task <> Task.Default)
                |> List.map (fun taskState -> taskState.Task.Name)
                |> List.groupBy id
                |> List.filter
                    (snd
                     >> List.length
                     >> fun n -> n > 1)
                |> List.map fst

            if not duplicated.IsEmpty then
                failwithf "Duplicated task names: %A" duplicated

            taskStateMap <-
                treeData.TaskStateList
                |> List.map (fun taskState -> taskState.Task.Name, taskState)
                |> Map.ofList

            treeDataMaybe <- Some treeData



        let tasks = taskContainerFactory (getTask true)

        let taskOrderList = getTaskOrderList [] taskStateList []

        let newTreeData =
            treeDataMaybe
            |> Option.map (fun treeData ->
                {| treeData with
                    TaskOrderList = taskOrderList
                |})

        newTreeData.Value, tasks





    module Testing =
        module Consts =
            let testDayStart = flukeTime 12 00

        let createRenderLaneTestData (testData: {| Position: FlukeDateTime
                                                   Expected: (FlukeDate * CellStatus) list
                                                   Events: DslTask list
                                                   Task: Task |}) =
            let eventsWithUser =
                testData.Events
                |> List.map (fun x -> x, Users.testUser)

            {|
                TaskStateList =
                    [
                        applyTaskEvents Consts.testDayStart testData.Task eventsWithUser
                    ]
                TaskOrderList =
                    [
                        {
                            Task = testData.Task
                            Priority = TaskOrderPriority.First
                        }
                    ]
                GetLivePosition = fun () -> testData.Position
                InformationList = [ testData.Task.Information ]
            |}


        let createSortLanesTestData (testData: {| Position: FlukeDateTime
                                                  Data: (Task * DslTask list) list
                                                  Expected: string list |}) =
            let taskStateList =
                testData.Data
                |> List.map (fun (task, events) ->
                    events
                    |> List.map (fun x -> x, Users.testUser)
                    |> applyTaskEvents Consts.testDayStart task)

            {|
                TaskStateList = taskStateList
                TaskOrderList =
                    testData.Data
                    |> List.map (fun (task, events) ->
                        {
                            Task = task
                            Priority = TaskOrderPriority.Last
                        })
                GetLivePosition = fun () -> testData.Position
                InformationList =
                    taskStateList
                    |> List.map (fun x -> x.Task.Information)
                    |> List.distinct
            |}

        let tempData =
            {|
                ManualTasks =
                    [
                        Project Projects.app_fluke,
                        [
                            "data management",
                            [
                                DslTaskComment "mutability", Users.testUser
                                DslTaskComment "initial default data (load the text first with tests)", Users.testUser
                            ]
                            "cell selection (mouse, vim navigation)", []
                            "data structures performance", []
                            "side panel (journal, comments)", []
                            "add task priority (for randomization)", []
                            "persistence",
                            [
                                DslTaskComment "data encryption", Users.testUser
                            ]
                            "vivaldi or firefox bookmark integration",
                            [
                                DslTaskComment "browser.html javascript injection or browser extension", Users.testUser
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
                    |> transformTreeData Consts.testDayStart
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
                                PendingAfter = Some (flukeTime 19 00)
                            }
                        Position =
                            {
                                Date = flukeDate 2020 Month.August 26
                                Time = Consts.testDayStart
                            }
                        Expected =
                            [
                                flukeDate 2020 Month.August 25, Disabled
                                flukeDate 2020 Month.August 26, Suggested
                                flukeDate 2020 Month.August 27, Pending
                            ]
                        Events = []
                    |}
                    |> createRenderLaneTestData
                SortLanesTests =

                    {|
                        Position =
                            {
                                Date = flukeDate 2020 Month.March 10
                                Time = flukeTime 14 00
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
                                    DslStatusEntry (flukeDate 2020 Month.March 10, Postponed None)
                                    DslStatusEntry (flukeDate 2020 Month.March 08, Postponed None)
                                ]

                                { Task.Default with
                                    Name = TaskName "03"
                                    Scheduling = Manual WithoutSuggestion
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 09, ManualPending)
                                ]

                                { Task.Default with
                                    Name = TaskName "04"
                                    Scheduling = Recurrency (Offset (Days 1))
                                    PendingAfter = flukeTime 20 00 |> Some
                                },
                                []

                                { Task.Default with
                                    Name = TaskName "05"
                                    Scheduling = Manual WithoutSuggestion
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 10, ManualPending)
                                ]

                                { Task.Default with
                                    Name = TaskName "06"
                                    Scheduling = Manual WithoutSuggestion
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 04, Postponed None)
                                    DslStatusEntry (flukeDate 2020 Month.March 06, Dismissed)
                                ]

                                { Task.Default with
                                    Name = TaskName "07"
                                    Scheduling = Recurrency (Offset (Days 4))
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 08, Completed)
                                ]

                                { Task.Default with
                                    Name = TaskName "08"
                                    Scheduling = Recurrency (Offset (Days 2))
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 10, Completed)
                                ]

                                { Task.Default with
                                    Name = TaskName "09"
                                    Scheduling = Recurrency (Offset (Days 2))
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 10, Dismissed)
                                ]

                                { Task.Default with
                                    Name = TaskName "10"
                                    Scheduling = Recurrency (Offset (Days 2))
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 10, Postponed None)
                                ]

                                { Task.Default with
                                    Name = TaskName "11"
                                    Scheduling = Recurrency (Offset (Days 1))
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 10, Postponed (flukeTime 13 00 |> Some))
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
                                    DslStatusEntry (flukeDate 2020 Month.March 07, Postponed None)
                                    DslStatusEntry (flukeDate 2020 Month.March 09, Dismissed)
                                ]

                                { Task.Default with
                                    Name = TaskName "16"
                                    Scheduling = Recurrency (Offset (Days 1))
                                    MissedAfter = (flukeTime 13 00 |> Some)
                                },
                                []

                                { Task.Default with
                                    Name = TaskName "17"
                                    Scheduling = Recurrency (Offset (Days 1))
                                },
                                [
                                    DslStatusEntry (flukeDate 2020 Month.March 10, Postponed (flukeTime 15 00 |> Some))
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
