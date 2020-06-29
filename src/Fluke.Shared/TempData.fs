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


    let [<Literal>] sessionLength = 25.
    let [<Literal>] sessionBreakLength = 5.
    let dayStart = flukeTime 07 00
    let testDayStart = flukeTime 12 00

    let testUser =
        { Username = "Test"
          Color = UserColor.Blue }

    let getNow () =
        FlukeDateTime.FromDateTime DateTime.Now

    module Users =
        let fc1943s =
            { Username = "fc1943s"
              Color = UserColor.Blue }

        let liryanne =
            { Username = "liryanne"
              Color = UserColor.Pink }

        let users = [
            fc1943s
            liryanne
        ]


    let getTaskOrderList oldTaskOrderList (tasks: Task list) manualTaskOrder =
        let taskMap =
            tasks
            |> List.map (fun x -> (x.Information, x.Name), x)
            |> Map.ofList

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
        |> List.append oldTaskOrderList

    type TempTaskEventField =
        | TempTaskFieldScheduling of scheduling:TaskScheduling
        | TempTaskFieldPendingAfter of start:FlukeTime
        | TempTaskFieldMissedAfter of start:FlukeTime
        | TempTaskFieldDuration of minutes:int

    type TempTaskEvent =
        | TempComment of comment:string
        | TempSession of start:FlukeDateTime
        | TempPriority of priority:TaskPriority
        | TempStatusEntry of date:FlukeDate * eventStatus:CellEventStatus
        | TempCellComment of date:FlukeDate * comment:string
        | TempTaskField of field:TempTaskEventField

    let applyTaskEvents task events =

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
        let comments, cellCommentsMap, sessions, statusEntries, priority, scheduling, pendingAfter, missedAfter, duration =
            let rec loop comments cellComments sessions statusEntries priority scheduling pendingAfter missedAfter duration = function
                | TempComment comment :: tail ->
                    let comment = Comment (testUser, comment)
                    loop (comment :: comments) cellComments sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                | TempCellComment (date, comment) :: tail ->
                    let cellComment = date, Comment (testUser, comment)
                    loop comments (cellComment :: cellComments) sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                | TempSession { Date = date; Time = time } :: tail ->
                    let session = TaskSession { Date = date; Time = time }
                    loop comments cellComments (session :: sessions) statusEntries priority scheduling pendingAfter missedAfter duration tail

                | TempStatusEntry (date, eventStatus) :: tail ->
                    let statusEntry = TaskStatusEntry (date, eventStatus)
                    loop comments cellComments sessions (statusEntry :: statusEntries) priority scheduling pendingAfter missedAfter duration tail

                | TempPriority priority :: tail ->
                    let priority = TaskPriorityValue (getPriorityValue priority) |> Some
                    loop comments cellComments sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                | TempTaskField field :: tail ->
                    match field with
                    | TempTaskFieldScheduling scheduling ->
                        loop comments cellComments sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                    | TempTaskFieldPendingAfter start ->
                        loop comments cellComments sessions statusEntries priority scheduling (Some start) missedAfter duration tail

                    | TempTaskFieldMissedAfter start ->
                        loop comments cellComments sessions statusEntries priority scheduling pendingAfter (Some start) duration tail

                    | TempTaskFieldDuration minutes ->
                        loop comments cellComments sessions statusEntries priority scheduling pendingAfter missedAfter (Some minutes) tail

                | [] ->
                    let sortedComments = comments |> List.rev
                    let cellCommentsMap =
                        cellComments
                        |> List.rev
                        |> List.groupBy fst
                        |> Map.ofList
                        |> Map.mapValues (List.map snd)
                    let sortedSessions = sessions |> List.sortBy (fun (TaskSession start) -> start.DateTime)
                    let sortedStatusEntries = statusEntries |> List.rev
                    let priority = priority |> Option.defaultValue (TaskPriorityValue 0)
                    sortedComments, cellCommentsMap, sortedSessions, sortedStatusEntries, priority, scheduling, pendingAfter, missedAfter, duration

            loop [] [] [] [] None task.Scheduling task.PendingAfter task.MissedAfter task.Duration events

        let laneMap =
            cellCommentsMap
            |> Map.mapValues (fun comments ->
                { Comments = comments
                  Status = Disabled }
            )

        { task with
            Scheduling = scheduling
            PendingAfter = pendingAfter
            MissedAfter = missedAfter
            Duration = duration
            Comments = comments
            LaneMap = laneMap
            Sessions = sessions
            StatusEntries = statusEntries
            Priority = priority }


    let transformTreeData taskTree =
        let taskList =
            taskTree
            |> List.collect (fun (information, tasks) ->
                tasks
                |> List.map (fun (taskName, events) ->
                    let task =
                        { Task.Default with
                            Name = taskName
                            Information = information }

                    applyTaskEvents task events
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

        {| TaskList = [ applyTaskEvents testData.Task testData.Events ]
           TaskOrderList = [ { Task = testData.Task; Priority = TaskOrderPriority.First } ]
           GetNow = fun () -> testData.Now |}


    let createSortLanesTestData (testData : {| Now: FlukeDateTime
                                               Data: (Task * TempTaskEvent list) list
                                               Expected: string list |}) =

        {| TaskList = testData.Data |> List.map (fun (task, events) -> applyTaskEvents task events)
           TaskOrderList =
               testData.Data
               |> List.map (fun (task, _) -> { Task = task; Priority = TaskOrderPriority.Last })
           GetNow = fun () -> testData.Now |}


    let tempData = {|
        ManualTasks =
            [
                Project Projects.app_fluke, [
                    "data management", [
                        TempComment "mutability"
                        TempComment "initial default data (load the text first with tests)"
                    ]
                    "cell selection (mouse, vim navigation)", []
                    "data structures performance", []
                    "side panel (journal, comments)", []
                    "add task priority (for randomization)", []
                    "persistence", [
                        TempComment "data encryption"
                    ]
                    "vivaldi or firefox bookmark integration", [
                        TempComment "browser.html javascript injection or browser extension"
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
            |> transformTreeData

        RenderLaneTests =
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 04
                                   Time = testDayStart }
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


