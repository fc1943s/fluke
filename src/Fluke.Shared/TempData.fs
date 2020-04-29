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
    
    let hourOffset = 0
    
    let getNow hourOffset =
        let rawDate = DateTime.Now.AddHours -(float hourOffset)
        { Date = FlukeDate.FromDateTime rawDate
          Time = FlukeTime.FromDateTime rawDate }
    
    let createRenderLaneTestData (testData: {| CellEvents: (FlukeDate * CellEventStatusType) list
                                               Data: (FlukeDate * CellStatusType) list
                                               Now: FlukeDateTime
                                               Task: Task |}) =
        
        {| CellEvents = testData.CellEvents |> LaneRendering.createCellEvents testData.Task
           TaskList = [ testData.Task ]
           TaskOrderList = [ { Task = testData.Task; Priority = First } ]
           GetNow = fun (_: int) -> testData.Now |}
        
    let createSortLanesTestData (testData : {| Data: (Task * (FlukeDate * CellEventStatusType) list) list
                                               Expected: string list
                                               Now: FlukeDateTime |}) =
        let cellEvents = testData.Data |> List.collect (fun (task, events) -> LaneRendering.createCellEvents task events)
            
        {| CellEvents = cellEvents
           TaskList = testData.Data |> List.map fst
           TaskOrderList = testData.Data |> List.map (fun (task, _) -> { Task = task; Priority = Last })
           GetNow = fun (_: int) -> testData.Now |}
           
           
    let createManualTasksFromTree taskList taskTree =
        let now = (getNow hourOffset).Date
        
        let createTaskMap taskList =
            let map =
                taskList
                |> List.map (fun x -> (x.InformationType, x.Name), x)
                |> Map.ofList
                
            let taskOrderList = taskList |> List.map (fun task -> { Task = task; Priority = Last })
                
            map, taskOrderList
            
        let oldTaskMap, oldTaskOrderList = createTaskMap taskList
        
        let newTaskList, newTaskComments =
            taskTree
            |> List.collect (fun (informationType, tasks) -> 
                tasks
                |> List.map (fun (taskName, comments) ->
                    let task =
                        oldTaskMap
                        |> Map.tryFind (informationType, taskName)
                        |> Option.defaultValue
                            { Task.Default with
                                Name = sprintf "> %s" taskName
                                InformationType = informationType }
                    let comments =
                        comments
                        |> List.map (fun comment ->
                            { Task = task
                              Comment = comment }
                        )
                    task, comments
                )
            )
            |> List.unzip
        
        let informationList =
            taskTree
            |> List.map fst
            |> List.distinct
            
        let newTaskMap, newTaskOrderList = createTaskMap newTaskList
        
        let notOnNewTaskMap task =
            newTaskMap.ContainsKey (task.InformationType, task.Name) |> not
            
        let filteredOldTaskList = taskList |> List.filter notOnNewTaskMap
        let taskList =  filteredOldTaskList @ newTaskList
        
        let initialTaskOrderList = taskList |> List.map (fun task -> { Task = task; Priority = Last })
        
        let filteredOldTaskOrder = oldTaskOrderList |> List.filter (fun x -> notOnNewTaskMap x.Task)
        let taskOrderList = newTaskOrderList @ filteredOldTaskOrder
        
        {| TaskList = taskList
           TaskOrderList = initialTaskOrderList @ taskOrderList
           TaskComments = newTaskComments |> List.collect id
           InformationList = informationList |}
    
    let tempData = {|
        ManualTasks = 
            [
                Project projects.``app-fluke``, [
                    "data management", [ "mutability"; "initial default data (load the text first with tests)" ]
                    "cell selection (mouse, vim navigation)", []
                    "data structures performance", []
                    "side panel (journal, comments)", []
                    "add task priority (for randomization)", []
                    "persistence", [ "data encryption" ]
                    "vivaldi or firefox bookmark integration", [ "browser.html javascript injection or browser extension" ]
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
                Area areas.chores, [
                    "groceries", [
                        "food"
                        "beer"
                    ]
                ]
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
                        {| Task = { Task.Default with Scheduling = Manual true
                                                      PendingAfter = flukeTime 20 0 }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 21 0 }
                           Data = [
                               flukeDate 2020 Month.March 9, Suggested
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Suggested
                           ]
                           CellEvents = [] |}
                        |> createRenderLaneTestData
                        
        SortLanesTests =
                    {| Now = { Date = flukeDate 2020 Month.March 10
                               Time = midnight }
                       Data = [
                           { Task.Default with Name = "1"; Scheduling = Manual true },
                           [] 
                           
                           { Task.Default with Name = "2"; Scheduling = Manual true },
                           [ flukeDate 2020 Month.March 10, Postponed midnight
                             flukeDate 2020 Month.March 8, Postponed midnight ]
                           
                           { Task.Default with Name = "3"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 9, ManualPending ]
                           
                           { Task.Default with Name = "4"; Scheduling = Recurrency (Offset (Days 1));
                                                           PendingAfter = flukeTime 20 0 },
                           []
                           
                           { Task.Default with Name = "5"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 10, ManualPending ]
                           
                           { Task.Default with Name = "6"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 4, Postponed midnight
                             flukeDate 2020 Month.March 6, Dropped ]
                           
                           { Task.Default with Name = "7"; Scheduling = Recurrency (Offset (Days 4)) },
                           [ flukeDate 2020 Month.March 8, Completed ]
                           
                           { Task.Default with Name = "8"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Completed ]
                           
                           { Task.Default with Name = "9"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Dropped ]
                           
                           { Task.Default with Name = "10"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Postponed midnight ]
                           
                           { Task.Default with Name = "11"; Scheduling = Recurrency (Offset (Days 1)) },
                           []
                           
                           { Task.Default with Name = "12"; Scheduling = Manual false },
                           []
                           
                           { Task.Default with Name = "13"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ]) },
                           []
                           
                           { Task.Default with Name = "14"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) },
                           []
                           
                           { Task.Default with Name = "15"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ]) },
                           [ flukeDate 2020 Month.March 7, Postponed midnight
                             flukeDate 2020 Month.March 9, Dropped ]
                       ]
                       Expected = [ "5"; "3"; "11"; "13"; "4"; "1"; "2"; "10"; "8"; "9"; "7"; "14"; "15"; "12"; "6" ] |}
                    |> createSortLanesTestData
                    
    |}

