namespace Fluke.Shared

open System
open FSharpPlus
open Suigetsu.Core


module TempData =
    open Model

    module private Areas =
        let car = { Name = "car" }
        let career = { Name = "career" }
        let chores = { Name = "chores" }
        let finances = { Name = "finances" }
        let fitness = { Name = "fitness" }
        let food = { Name = "food" }
        let health = { Name = "health" }
        let leisure = { Name = "leisure" }
        let programming = { Name = "programming" }
        let travel = { Name = "travel" }
        let workflow = { Name = "workflow" }
        let writing = { Name = "writing" }


    module private Projects =
        let app_fluke =
            { Project.Area = Areas.workflow
              Project.Name = "app_fluke" }

        let blog =
            { Project.Area = Areas.writing
              Project.Name = "blog" }

        let rebuild_website =
            { Project.Area = Areas.programming
              Project.Name = "rebuild_website" }


    module private Resources =
        let agile =
            { Resource.Area = Areas.programming
              Resource.Name = "agile" }

        let artificial_intelligence =
            { Resource.Area = Areas.programming
              Resource.Name = "artificial_intelligence" }

        let cloud =
            { Resource.Area = Areas.programming
              Resource.Name = "cloud" }

        let communication =
            { Resource.Area = Areas.workflow
              Resource.Name = "communication" }

        let docker =
            { Resource.Area = Areas.programming
              Resource.Name = "docker" }

        let fsharp =
            { Resource.Area = Areas.programming
              Resource.Name = "f#" }

        let linux =
            { Resource.Area = Areas.programming
              Resource.Name = "linux" }

        let music =
            { Resource.Area = Areas.leisure
              Resource.Name = "music" }

        let rust =
            { Resource.Area = Areas.programming
              Resource.Name = "rust" }

        let vim =
            { Resource.Area = Areas.programming
              Resource.Name = "vim" }

        let windows =
            { Resource.Area = Areas.programming
              Resource.Name = "windows" }


    module Users =
        let fc1943s =
            { Username = "fc1943s"
              Color = UserColor.Blue }

        let liryanne =
            { Username = "liryanne"
              Color = UserColor.Pink }

        let testUser =
            { Username = "Test"
              Color = UserColor.Blue }

        let users = [
            fc1943s
            liryanne
        ]


    module Consts =
        let [<Literal>] sessionLength = 25.
        let [<Literal>] sessionBreakLength = 5.
        let defaultDate = flukeDate 0001 Month.January 01
        let dayStart = flukeTime 07 00
        let testDayStart = flukeTime 12 00


    module Events =

        [<RequireQualifiedAccess>]
        type TempEvent =
            | CellStatus of user:User * task:Task * date:FlukeDateTime * manualCellStatus:ManualCellStatus
            | CellCompleted of user:User * task:Task * date:FlukeDateTime
            | CellCommented of user:User * task:Task * date:FlukeDateTime * comment:Comment

        let eventsFromStatusEntries user (entries: (FlukeDate * (Task * ManualCellStatus) list) list) =
            let newEvents =
                entries
                |> List.collect (fun (date, events) ->
                    events
                    |> List.map (fun (task, userStatus) ->
                        TempEvent.CellStatus (user, task, { Date = date; Time = Consts.dayStart }, userStatus)
                    )
                )

            let oldEvents =
                entries
                |> List.collect (fun (date, events) ->
                    events
                    |> List.map (fun (task, manualCellStatus) ->
                        CellStatusEntry (user, task, { Date = date; Time = Consts.dayStart }, manualCellStatus)
                    )
                )

            oldEvents, newEvents

        let eventsFromCellComments user =
            ()

        module Temp =
            type Command =
                | CompleteCell of user:User * task:Task * date:FlukeDateTime
            type Event =
                | CellCompleted of user:User * task:Task * date:FlukeDateTime
            type State =
                { DayStart: FlukeTime
                  TaskMap: Map<TaskId, Map<DateId, CellStatus>>
                  TaskIdList: TaskId list }
            let initialState =
                { DayStart = Consts.dayStart
                  TaskMap = Map.empty
                  TaskIdList = [] }
            let private apply (state: State) (event: Event) : State = // Apply/Evolve
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
                            state.TaskMap
                            |> Map.add taskId cellMap
                    }
            let execute (state: State) (command: Command) : Event list = [ // Execute/Decide
                match command, state with
                | CompleteCell (user, task, moment), state
                    when
                        state.TaskMap
                        |> Map.tryFind (taskId task)
                        |> Option.defaultValue Map.empty
                        |> Map.tryFind (dateId state.DayStart moment)
                        |> Option.defaultValue Disabled
                        |> (=) Missed ->
                    CellCompleted (user, task, moment)

                | CompleteCell (user, task, eventDate), state ->
                    CellCompleted (user, task, eventDate)

                | _ -> ()
            ]
            let build = List.fold apply
            let rebuld = build initialState


    let getNow () =
        FlukeDateTime.FromDateTime DateTime.Now

    let getTaskOrderList oldTaskOrderList (tasks: Task list) manualTaskOrder =
        let taskMap =
            tasks
            |> List.map (fun x -> (x.Information, x.Name), x)
            |> Map.ofList

        let newTaskOrderList =
            manualTaskOrder
            |> List.map (fun (information, taskName) ->
                taskMap
                |> Map.tryFind (information, taskName)
                |> function
                    | None -> failwithf "Invalid task: '%A/%s'" information taskName
                    | Some task ->
                        { Task = task
                          Priority = TaskOrderPriority.First }
            )
        oldTaskOrderList @ newTaskOrderList

    type TempTaskEventField =
        | TempTaskFieldScheduling of scheduling:TaskScheduling * start:FlukeDate option
        | TempTaskFieldPendingAfter of start:FlukeTime
        | TempTaskFieldMissedAfter of start:FlukeTime
        | TempTaskFieldDuration of minutes:int

    type TempTaskEvent =
        | TempComment of comment:string
        | TempSession of start:FlukeDateTime
        | TempPriority of priority:TaskPriority
        | TempStatusEntry of date:FlukeDate * manualCellStatus:ManualCellStatus
        | TempCellComment of date:FlukeDate * comment:string
        | TempTaskField of field:TempTaskEventField

    let applyTaskEvents dayStart task (events: (TempTaskEvent * User) list) =

        let getPriorityValue = function
            | Low1 -> 1
            | Low2 -> 2
            | Low3 -> 3
            | Medium4 -> 4
            | Medium5 -> 5
            | Medium6 -> 6
            | High7 -> 7
            | High8 -> 8
            | High9 -> 9
            | Critical10 -> 10

        // TODO: how the hell do i rewrite this without losing performance?
        let comments, cellComments, sessions, statusEntries, priority, scheduling, pendingAfter, missedAfter, duration =
            let rec loop comments cellComments sessions statusEntries priority scheduling pendingAfter missedAfter duration = function
                | (TempComment comment, user) :: tail ->
                    let comment = UserComment (user, comment)
                    loop (comment :: comments) cellComments sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                | (TempCellComment (date, comment), user) :: tail ->
                    let cellComment = date, UserComment (user, comment)
                    loop comments (cellComment :: cellComments) sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                | (TempSession { Date = date; Time = time }, user) :: tail ->
                    let session = TaskSession { Date = date; Time = time }
                    loop comments cellComments (session :: sessions) statusEntries priority scheduling pendingAfter missedAfter duration tail

                | (TempStatusEntry (date, manualCellStatus), user) :: tail ->
                    let statusEntry = TaskStatusEntry (user, { Date = date; Time = dayStart }, manualCellStatus)
                    loop comments cellComments sessions (statusEntry :: statusEntries) priority scheduling pendingAfter missedAfter duration tail

                | (TempPriority priority, user) :: tail ->
                    let priority = TaskPriorityValue (getPriorityValue priority) |> Some
                    loop comments cellComments sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                | (TempTaskField field, user) :: tail ->
                    match field with
                    | TempTaskFieldScheduling (scheduling, start) ->
                        loop comments cellComments sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                    | TempTaskFieldPendingAfter start ->
                        loop comments cellComments sessions statusEntries priority scheduling (Some start) missedAfter duration tail

                    | TempTaskFieldMissedAfter start ->
                        loop comments cellComments sessions statusEntries priority scheduling pendingAfter (Some start) duration tail

                    | TempTaskFieldDuration minutes ->
                        loop comments cellComments sessions statusEntries priority scheduling pendingAfter missedAfter (Some minutes) tail

                | [] ->
                    let sortedComments = comments |> List.rev
                    let sortedCellComments = cellComments |> List.rev
                    let sortedSessions = sessions |> List.sortBy (fun (TaskSession start) -> start.DateTime)
                    let sortedStatusEntries = statusEntries |> List.rev
                    let priority = priority |> Option.defaultValue (TaskPriorityValue 0)
                    sortedComments, sortedCellComments, sortedSessions, sortedStatusEntries, priority, scheduling, pendingAfter, missedAfter, duration

            loop [] [] [] [] None task.Scheduling task.PendingAfter task.MissedAfter task.Duration events

        { task with
            Scheduling = scheduling
            PendingAfter = pendingAfter
            MissedAfter = missedAfter
            Duration = duration
            Comments = comments
            CellComments = cellComments
            Sessions = sessions
            StatusEntries = statusEntries
            Priority = priority }


    let treeDataWithUser user taskTree =
        taskTree |> List.map (Tuple2.mapSnd (List.map (Tuple2.mapSnd (List.map (fun event -> event, user)))))

    let transformTreeData dayStart taskTree =
        let taskList =
            taskTree
            |> List.collect (fun (information, tasks) ->
                tasks
                |> List.map (fun (taskName, (events: (TempTaskEvent * User) list)) ->
                    let task =
                        { Task.Default with
                            Name = taskName
                            Information = information }

                    applyTaskEvents dayStart task events
                )
            )

        let informationList =
            taskTree
            |> List.map fst
            |> List.distinct

        let taskOrderList =
            taskList
            |> List.map (fun task -> { Task = task; Priority = TaskOrderPriority.Last })

        {| TaskList = taskList
           TaskOrderList = taskOrderList
           InformationList = informationList |}


    let createRenderLaneTestData (testData: {| Now: FlukeDateTime
                                               Expected: (FlukeDate * CellStatus) list
                                               Events: TempTaskEvent list
                                               Task: Task |}) =
        let eventsWithUser = testData.Events |> List.map (fun x -> x, Users.testUser)
        {| TaskList = [ applyTaskEvents Consts.testDayStart testData.Task eventsWithUser ]
           TaskOrderList = [ { Task = testData.Task; Priority = TaskOrderPriority.First } ]
           GetNow = fun () -> testData.Now |}


    let createSortLanesTestData (testData : {| Now: FlukeDateTime
                                               Data: (Task * TempTaskEvent list) list
                                               Expected: string list |}) =
        {| TaskList =
               testData.Data
               |> List.map (fun (task, events) ->
                   events
                   |> List.map (fun x -> x, Users.testUser)
                   |> applyTaskEvents Consts.testDayStart task
                )
           TaskOrderList =
               testData.Data
               |> List.map (fun (task, events) -> { Task = task; Priority = TaskOrderPriority.Last })
           GetNow = fun () -> testData.Now |}


    let tempData = {|
        ManualTasks =
            [
                Project Projects.app_fluke, [
                    "data management", [
                        TempComment "mutability", Users.testUser
                        TempComment "initial default data (load the text first with tests)", Users.testUser
                    ]
                    "cell selection (mouse, vim navigation)", []
                    "data structures performance", []
                    "side panel (journal, comments)", []
                    "add task priority (for randomization)", []
                    "persistence", [
                        TempComment "data encryption", Users.testUser
                    ]
                    "vivaldi or firefox bookmark integration", [
                        TempComment "browser.html javascript injection or browser extension", Users.testUser
                    ]
                    "telegram integration (fast link sharing)", []
                    "mobile layout", []
                    "move fluke tasks to github issues", []
                ]
                Project Projects.blog, []
                Project Projects.rebuild_website, [
                    "task1", []
                ]
                Area Areas.car, []
                Area Areas.career, []
                Area Areas.chores, []
                Area Areas.fitness, []
                Area Areas.food, []
                Area Areas.finances, []
                Area Areas.health, []
                Area Areas.leisure, [
                    "watch_movie_foobar", []
                ]
                Area Areas.programming, []
                Area Areas.travel, []
                Area Areas.workflow, []
                Area Areas.writing, []
                Resource Resources.agile, []
                Resource Resources.artificial_intelligence, []
                Resource Resources.cloud, []
                Resource Resources.communication, []
                Resource Resources.docker, []
                Resource Resources.fsharp, [
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
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 04
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.February 29, Disabled
                               flukeDate 2020 Month.March 1, Disabled
                               flukeDate 2020 Month.March 2, Disabled
                               flukeDate 2020 Month.March 3, Disabled
                               flukeDate 2020 Month.March 4, Pending
                               flukeDate 2020 Month.March 5, Pending
                               flukeDate 2020 Month.March 6, Pending
                               flukeDate 2020 Month.March 7, Pending
                               flukeDate 2020 Month.March 8, Disabled
                           ]
                           Events = [
                               TempSession (flukeDateTime 2020 Month.March 01 11 00)
                               TempSession (flukeDateTime 2020 Month.March 01 13 00)
                               TempSession (flukeDateTime 2020 Month.March 08 11 00)
                               TempSession (flukeDateTime 2020 Month.March 08 13 00)
                           ] |}
                        |> createRenderLaneTestData

        SortLanesTests =
                    {| Now = { Date = flukeDate 2020 Month.March 10
                               Time = flukeTime 14 00 }
                       Data = [
                           { Task.Default with Name = "01"; Scheduling = Manual WithSuggestion },
                           []

                           { Task.Default with Name = "02"; Scheduling = Manual WithSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 08, Postponed None) ]

                           { Task.Default with Name = "03"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 09, ManualPending) ]

                           { Task.Default with Name = "04"; Scheduling = Recurrency (Offset (Days 1));
                                                           PendingAfter = flukeTime 20 00 |> Some },
                           []

                           { Task.Default with Name = "05"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, ManualPending) ]

                           { Task.Default with Name = "06"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 04, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 06, Dismissed) ]

                           { Task.Default with Name = "07"; Scheduling = Recurrency (Offset (Days 4)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 08, Completed) ]

                           { Task.Default with Name = "08"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Completed) ]

                           { Task.Default with Name = "09"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Dismissed) ]

                           { Task.Default with Name = "10"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None) ]

                           { Task.Default with Name = "11"; Scheduling = Recurrency (Offset (Days 1)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (flukeTime 13 00 |> Some)) ]

                           { Task.Default with Name = "12"; Scheduling = Manual WithoutSuggestion },
                           []

                           { Task.Default with Name = "13"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ]) },
                           []

                           { Task.Default with Name = "14"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) },
                           []

                           { Task.Default with Name = "15"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ]) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 07, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 09, Dismissed) ]

                           { Task.Default with Name = "16"; Scheduling = Recurrency (Offset (Days 1));
                                                            MissedAfter = (flukeTime 13 00 |> Some) },
                           []

                           { Task.Default with Name = "17"; Scheduling = Recurrency (Offset (Days 1)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (flukeTime 15 00 |> Some)) ]

                           { Task.Default with Name = "18"; Scheduling = Recurrency (Offset (Days 1)) },
                           []
                       ]
                       Expected = [ "16"; "05"; "03"; "11"; "13"
                                    "18"; "17"; "04"; "01"; "02"
                                    "10"; "08"; "09"; "07"; "14"
                                    "15"; "12"; "06" ] |}
                    |> createSortLanesTestData

    |}


