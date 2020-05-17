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
        ``app-fluke`` =
            { Project.Area = areas.workflow
              Project.Name = "app-fluke" }
            
        blog =
            { Project.Area = areas.writing
              Project.Name = "blog" }
            
        ``rebuild-website`` =
            { Project.Area = areas.programming
              Project.Name = "rebuild-website" }
    |}
    
    let private resources = {|
        agile =
            { Resource.Area = areas.programming
              Resource.Name = "agile" }
            
        ``artificial-intelligence`` =
            { Resource.Area = areas.programming
              Resource.Name = "artificial-intelligence" }
            
        cloud =
            { Resource.Area = areas.programming
              Resource.Name = "cloud" }
            
        communication = 
            { Resource.Area = areas.workflow
              Resource.Name = "communication" }
            
        docker = 
            { Resource.Area = areas.programming
              Resource.Name = "docker" }
            
        ``f#`` =
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
    
    let dayStart = flukeTime 00 00
    
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
                        |> List.choose (function TempComment comment -> TaskComment (task, Comment comment) |> Some | _ -> None)
                        
                    let sessions =
                        events
                        |> List.choose (function TempSession start -> TaskSession { Date = start.Date; Time = start.Time } |> Some | _ -> None)
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
                        |> List.choose (function TempPriority p -> getPriorityValue p |> TaskPriorityValue |> Some | _ -> None)
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
        
        let initialTaskOrderList = taskStateList |> List.map (fun taskState -> { Task = taskState.Task; Priority = Last })
        
        let filteredOldTaskOrder = oldTaskOrderList |> List.filter (fun x -> notOnNewTaskMap x.Task)
        let taskOrderList = newTaskOrderList @ filteredOldTaskOrder
        
        {| TaskStateList = taskStateList
           TaskOrderList = initialTaskOrderList @ taskOrderList
           InformationList = informationList |}
           
           
    let createRenderLaneTestData (testData: {| CellEvents: (FlukeDate * CellEventStatus) list
                                               Data: (FlukeDate * CellStatus) list
                                               Now: FlukeDateTime
                                               Task: Task |}) =
        
        {| CellEvents = testData.CellEvents |> Rendering.createCellStatusEntries testData.Task
           TaskList = [ testData.Task ]
           TaskOrderList = [ { Task = testData.Task; Priority = First } ]
           GetNow = fun () -> testData.Now |}
        
        
    let createSortLanesTestData (testData : {| Data: (Task * (FlukeDate * CellEventStatus) list) list
                                               Expected: string list
                                               Now: FlukeDateTime |}) =
        let cellEvents = testData.Data |> List.collect (fun (task, events) -> events |> Rendering.createCellStatusEntries task)
            
        {| CellEvents = cellEvents
           TaskList = testData.Data |> List.map fst
           TaskOrderList = testData.Data |> List.map (fun (task, _) -> { Task = task; Priority = Last })
           GetNow = fun () -> testData.Now |}
    
    
    let tempData = {|
        ManualTasks = 
            [
                Project projects.``app-fluke``, [
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
                Project projects.``rebuild-website``, [
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
                    "watch-movie-foobar", []
                ]
                Area areas.programming, []
                Area areas.travel, []
                Area areas.workflow, []
                Area areas.writing, []
                Resource resources.agile, []
                Resource resources.``artificial-intelligence``, []
                Resource resources.cloud, []
                Resource resources.communication, []
                Resource resources.docker, []
                Resource resources.``f#``, [
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
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 7, Disabled
                               flukeDate 2020 Month.March 8, EventStatus Completed
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Missed
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, EventStatus Completed
                               flukeDate 2020 Month.March 13, Disabled
                               flukeDate 2020 Month.March 14, Pending
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 8, Completed
                               flukeDate 2020 Month.March 12, Completed
                           ] |}
                        |> createRenderLaneTestData
                        
        SortLanesTests =
                    {| Now = { Date = flukeDate 2020 Month.March 10
                               Time = flukeTime 00 00 }
                       Data = [
                           { Task.Default with Name = "1"; Scheduling = Manual true },
                           [] 
                           
                           { Task.Default with Name = "2"; Scheduling = Manual true },
                           [ flukeDate 2020 Month.March 10, Postponed None
                             flukeDate 2020 Month.March 08, Postponed None ]
                           
                           { Task.Default with Name = "3"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 09, ManualPending ]
                           
                           { Task.Default with Name = "4"; Scheduling = Recurrency (Offset (Days 1));
                                                           PendingAfter = flukeTime 20 00 |> Some },
                           []
                           
                           { Task.Default with Name = "5"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 10, ManualPending ]
                           
                           { Task.Default with Name = "6"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 04, Postponed None
                             flukeDate 2020 Month.March 06, Dismissed ]
                           
                           { Task.Default with Name = "7"; Scheduling = Recurrency (Offset (Days 4)) },
                           [ flukeDate 2020 Month.March 08, Completed ]
                           
                           { Task.Default with Name = "8"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Completed ]
                           
                           { Task.Default with Name = "9"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Dismissed ]
                           
                           { Task.Default with Name = "10"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Postponed None ]
                           
                           { Task.Default with Name = "11"; Scheduling = Recurrency (Offset (Days 1)) },
                           []
                           
                           { Task.Default with Name = "12"; Scheduling = Manual false },
                           []
                           
                           { Task.Default with Name = "13"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ]) },
                           []
                           
                           { Task.Default with Name = "14"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) },
                           []
                           
                           { Task.Default with Name = "15"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ]) },
                           [ flukeDate 2020 Month.March 07, Postponed None
                             flukeDate 2020 Month.March 09, Dismissed ]
                       ]
                       Expected = [ "11"; "4"; "8"; "9"; "10"; "7"; "15"; "13"; "14"; "2"; "6"; "3"; "5"; "1"; "12" ] |}
                    |> createSortLanesTestData
                    
    |}

