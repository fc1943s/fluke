namespace Fluke.Shared

open System
open FSharpPlus
open Suigetsu.Core


module TempData =
    open Model

    let private areas = {|
        car = { Name = "car" }
        career = { Name = "career" }
        chores = { Name = "chores" }
        finances = { Name = "finances" }
        fitness = { Name = "fitness" }
        food = { Name = "food" }
        health = { Name = "health" }
        leisure = { Name = "leisure" }
        programming = { Name = "programming" }
        travel = { Name = "travel" }
        workflow = { Name = "workflow" }
        writing = { Name = "writing" }
    |}

    let private projects = {|
        app_fluke =
            { Project.Area = areas.workflow
              Project.Name = "app_fluke" }

        blog =
            { Project.Area = areas.writing
              Project.Name = "blog" }

        rebuild_website =
            { Project.Area = areas.programming
              Project.Name = "rebuild_website" }
    |}

    let private resources = {|
        agile =
            { Resource.Area = areas.programming
              Resource.Name = "agile" }

        artificial_intelligence =
            { Resource.Area = areas.programming
              Resource.Name = "artificial_intelligence" }

        cloud =
            { Resource.Area = areas.programming
              Resource.Name = "cloud" }

        communication =
            { Resource.Area = areas.workflow
              Resource.Name = "communication" }

        docker =
            { Resource.Area = areas.programming
              Resource.Name = "docker" }

        fsharp =
            { Resource.Area = areas.programming
              Resource.Name = "f#" }

        linux =
            { Resource.Area = areas.programming
              Resource.Name = "linux" }

        music =
            { Resource.Area = areas.leisure
              Resource.Name = "music" }

        rust =
            { Resource.Area = areas.programming
              Resource.Name = "rust" }

        vim =
            { Resource.Area = areas.programming
              Resource.Name = "vim" }

        windows =
            { Resource.Area = areas.programming
              Resource.Name = "windows" }

    |}

    let sessionLength = 25.
    let sessionBreakLength = 5.
    let dayStart = flukeTime 05 00
    let testDayStart = flukeTime 12 00

    let getNow () =
        let rawDate = DateTime.Now
        { Date = FlukeDate.FromDateTime rawDate
          Time = FlukeTime.FromDateTime rawDate }


    let getTaskOrderList oldTaskOrderList tasks manualTaskOrder =
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
                      Priority = First }
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
        | TempTaskField of field:TempTaskEventField

    let createTaskState task events =

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

        let comments, sessions, statusEntries, priority, scheduling, pendingAfter, missedAfter, duration =
            let rec loop comments sessions statusEntries priority scheduling pendingAfter
                missedAfter duration = function
                | TempComment comment :: tail ->
                    let item = Comment comment
                    loop (item :: comments)
                        sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                | TempSession { Date = date; Time = time } :: tail ->
                    let item = TaskSession { Date = date; Time = time }
                    loop comments (item :: sessions)
                        statusEntries priority scheduling pendingAfter missedAfter duration tail

                | TempStatusEntry (date, eventStatus) :: tail ->
                    let item = TaskStatusEntry (date, eventStatus)
                    loop comments sessions (item :: statusEntries)
                        priority scheduling pendingAfter missedAfter duration tail

                | TempPriority priority :: tail ->
                    loop comments sessions statusEntries (TaskPriorityValue (getPriorityValue priority) |> Some)
                        scheduling pendingAfter missedAfter duration tail

                | TempTaskField field :: tail ->
                    match field with
                    | TempTaskFieldScheduling scheduling ->
                        loop comments sessions statusEntries priority scheduling pendingAfter missedAfter duration tail

                    | TempTaskFieldPendingAfter start ->
                        loop comments sessions statusEntries priority scheduling (Some start) missedAfter duration tail

                    | TempTaskFieldMissedAfter start ->
                        loop comments sessions statusEntries priority scheduling pendingAfter (Some start) duration tail

                    | TempTaskFieldDuration minutes ->
                        loop comments sessions statusEntries priority scheduling pendingAfter missedAfter (Some minutes)
                            tail

                | [] ->
                    let sortedComments = comments |> List.rev
                    let sortedSessions = sessions |> List.sortBy (fun (TaskSession start) -> start.DateTime)
                    let sortedStatusEntries = statusEntries |> List.rev
                    sortedComments, sortedSessions, sortedStatusEntries, priority, scheduling, pendingAfter,
                    missedAfter, duration

            loop [] [] [] None task.Scheduling task.PendingAfter task.MissedAfter task.Duration events

        { Task =
            { task with
                Scheduling = scheduling
                PendingAfter = pendingAfter
                MissedAfter = missedAfter
                Duration = duration }
          Comments = comments
          Sessions = sessions
          StatusEntries = statusEntries
          PriorityValue = priority }


    let createManualTasksFromTree taskTree =
        let taskStateList =
            taskTree
            |> List.collect (fun (information, tasks) ->
                tasks
                |> List.map (fun (taskName, events) ->
                    let task =
                        { Task.Default with
                            Name = taskName
                            Information = information }

                    createTaskState task events
                )
            )

        let informationList =
            taskTree
            |> List.map fst
            |> List.distinct

        let taskOrderList =
            taskStateList
            |> List.map (fun taskState -> { Task = taskState.Task; Priority = Last })

        {| TaskStateList = taskStateList
           TaskOrderList = taskOrderList
           InformationList = informationList |}


    let createRenderLaneTestData (testData: {| Now: FlukeDateTime
                                               Expected: (FlukeDate * CellStatus) list
                                               Events: TempTaskEvent list
                                               Task: Task |}) =

        {| TaskStateList = [ createTaskState testData.Task testData.Events ]
           TaskOrderList = [ { Task = testData.Task; Priority = First } ]
           GetNow = fun () -> testData.Now |}


    let createSortLanesTestData (testData : {| Now: FlukeDateTime
                                               Data: (Task * TempTaskEvent list) list
                                               Expected: string list |}) =

        {| TaskStateList = testData.Data |> List.map (fun (task, events) -> createTaskState task events)
           TaskOrderList = testData.Data |> List.map (fun (task, _) -> { Task = task; Priority = Last })
           GetNow = fun () -> testData.Now |}


    let tempData = {|
        ManualTasks =
            [
                Project projects.app_fluke, [
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
                Project projects.blog, []
                Project projects.rebuild_website, [
                    "task1", []
                ]
                Area areas.car, []
                Area areas.career, []
                Area areas.chores, []
                Area areas.fitness, []
                Area areas.food, []
                Area areas.finances, []
                Area areas.health, []
                Area areas.leisure, [
                    "watch_movie_foobar", []
                ]
                Area areas.programming, []
                Area areas.travel, []
                Area areas.workflow, []
                Area areas.writing, []
                Resource resources.agile, []
                Resource resources.artificial_intelligence, []
                Resource resources.cloud, []
                Resource resources.communication, []
                Resource resources.docker, []
                Resource resources.fsharp, [
                    "study: [choice, computation expressions]", []
                    "organize youtube playlists", []
                ]
                Resource resources.linux, []
                Resource resources.music, []
                Resource resources.rust, []
                Resource resources.vim, []
                Resource resources.windows, []
            ]
            |> createManualTasksFromTree

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


