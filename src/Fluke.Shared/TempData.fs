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
        
           
    type TempCellEvent =
        | TempComment of comment:string
        | TempSession of start:FlukeDateTime
        | TempPriority of priority:TaskPriority
        | TempStatusEntry of date:FlukeDate * eventStatus:CellEventStatus
           
    let createManualTasksFromTree taskList taskTree =
        let createTaskMap taskList =
            let map =
                taskList
                |> List.map (fun x -> (x.Information, x.Name), x)
                |> Map.ofList
                
            let taskOrderList = taskList |> List.map (fun task -> { Task = task; Priority = Last })
                
            map, taskOrderList
            
        let oldTaskMap, oldTaskOrderList = createTaskMap taskList
        
        let newTaskStateList =
            taskTree
            |> List.collect (fun (information, tasks) -> 
                tasks
                |> List.map (fun (taskName, events) ->
                    let task =
                        oldTaskMap
                        |> Map.tryFind (information, taskName)
                        |> Option.defaultValue
                            { Task.Default with
                                Name = sprintf "> %s" taskName
                                Information = information }
                            
                    let comments =
                        events
                        |> List.choose (function
                            | TempComment comment ->
                                TaskComment (task, Comment comment) |> Some
                            | _ -> None
                        )
                        
                    let sessions =
                        events
                        |> List.choose (function
                            | TempSession { Date = date; Time = time } ->
                                TaskSession { Date = date; Time = time } |> Some
                            | _ -> None
                        )
                        |> List.sortBy (fun (TaskSession start) -> start.DateTime)
                        
                    let priority =
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
                            
                        events
                        |> List.choose (function
                            | TempPriority p ->
                                getPriorityValue p |> TaskPriorityValue |> Some
                            | _ -> None
                        )
                        |> List.tryHead
                        
                    { Task = task
                      Comments = comments |> List.map (ofTaskComment >> snd)
                      Sessions = sessions
                      PriorityValue = priority }
                )
            )
            
        let informationList =
            taskTree
            |> List.map fst
            |> List.distinct
            
        let newTaskMap, newTaskOrderList =
            newTaskStateList
            |> List.map (fun x -> x.Task)
            |> createTaskMap
        
        let notOnNewTaskMap task =
            not (newTaskMap.ContainsKey (task.Information, task.Name))
            
        let taskStateList =
            taskList
            |> List.filter notOnNewTaskMap
            |> List.map (fun task ->
                { Task = task
                  Comments = []
                  Sessions = []
                  PriorityValue = None })
            |> List.prepend newTaskStateList
        
        let initialTaskOrderList =
            taskStateList
            |> List.map (fun taskState -> { Task = taskState.Task; Priority = Last })
        
        let filteredOldTaskOrder = oldTaskOrderList |> List.filter (fun x -> notOnNewTaskMap x.Task)
        let taskOrderList = newTaskOrderList @ filteredOldTaskOrder
        
        {| TaskStateList = taskStateList
           TaskOrderList = initialTaskOrderList @ taskOrderList
           InformationList = informationList |}
           
           
    let createRenderLaneTestData (testData: {| CellEvents: (FlukeDate * CellEventStatus) list
                                               Data: (FlukeDate * CellStatus) list
                                               Now: FlukeDateTime
                                               Task: Task |}) =
        
        {| CellStatusEntries = testData.CellEvents |> Rendering.createCellStatusEntries testData.Task
           TaskList = [ testData.Task ]
           TaskOrderList = [ { Task = testData.Task; Priority = First } ]
           GetNow = fun () -> testData.Now |}
        
        
    let createSortLanesTestData (testData : {| Data: (Task * (FlukeDate * CellEventStatus) list) list
                                               Expected: string list
                                               Now: FlukeDateTime |}) =
        
        let cellStatusEntries =
            testData.Data
            |> List.collect (fun (task, events) -> Rendering.createCellStatusEntries task events)
            
        {| CellStatusEntries = cellStatusEntries
           TaskList = testData.Data |> List.map fst
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
            |> createManualTasksFromTree []
            
        RenderLaneTests = 
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = flukeTime 02 00 }
                           Data = [
                               flukeDate 2020 Month.March 09, Disabled
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Pending
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 10, Postponed (Some (flukeTime 23 00))
                           ] |}
                        |> createRenderLaneTestData
                        
        SortLanesTests =
                    {| Now = { Date = flukeDate 2020 Month.March 10
                               Time = flukeTime 14 00 }
                       Data = [
                           { Task.Default with Name = "01"; Scheduling = Manual WithSuggestion },
                           [] 
                           
                           { Task.Default with Name = "02"; Scheduling = Manual WithSuggestion },
                           [ flukeDate 2020 Month.March 10, Postponed None
                             flukeDate 2020 Month.March 08, Postponed None ]
                           
                           { Task.Default with Name = "03"; Scheduling = Manual WithoutSuggestion },
                           [ flukeDate 2020 Month.March 09, ManualPending ]
                           
                           { Task.Default with Name = "04"; Scheduling = Recurrency (Offset (Days 1));
                                                           PendingAfter = flukeTime 20 00 |> Some },
                           []
                           
                           { Task.Default with Name = "05"; Scheduling = Manual WithoutSuggestion },
                           [ flukeDate 2020 Month.March 10, ManualPending ]
                           
                           { Task.Default with Name = "06"; Scheduling = Manual WithoutSuggestion },
                           [ flukeDate 2020 Month.March 04, Postponed None
                             flukeDate 2020 Month.March 06, Dismissed ]
                           
                           { Task.Default with Name = "07"; Scheduling = Recurrency (Offset (Days 4)) },
                           [ flukeDate 2020 Month.March 08, Completed ]
                           
                           { Task.Default with Name = "08"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Completed ]
                           
                           { Task.Default with Name = "09"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Dismissed ]
                           
                           { Task.Default with Name = "10"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Postponed None ]
                           
                           { Task.Default with Name = "11"; Scheduling = Recurrency (Offset (Days 1)) },
                           [ flukeDate 2020 Month.March 10, Postponed (flukeTime 13 00 |> Some) ]
                           
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
                           [ flukeDate 2020 Month.March 07, Postponed None
                             flukeDate 2020 Month.March 09, Dismissed ]
                           
                           { Task.Default with Name = "16"; Scheduling = Recurrency (Offset (Days 1));
                                                            MissedAfter = (flukeTime 13 00 |> Some) },
                           []
                           
                           { Task.Default with Name = "17"; Scheduling = Recurrency (Offset (Days 1)) },
                           [ flukeDate 2020 Month.March 10, Postponed (flukeTime 15 00 |> Some) ]
                           
                           { Task.Default with Name = "18"; Scheduling = Recurrency (Offset (Days 1)) },
                           []
                       ]
                       Expected = [ "16"; "05"; "03"; "11"; "13"
                                    "18"; "17"; "04"; "01"; "02"
                                    "10"; "08"; "09"; "07"; "14"
                                    "15"; "12"; "06" ] |}
                    |> createSortLanesTestData
                    
    |}

