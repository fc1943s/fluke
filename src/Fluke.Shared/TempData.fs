namespace Fluke.Shared

open System
open FSharpPlus
open Suigetsu.Core


module TempData =
    open Model
    
    let areaList : Area list = [
        { Name = "car" }
        { Name = "career" }
        { Name = "chores" }
        { Name = "finances" }
        { Name = "fitness" }
        { Name = "food" }
        { Name = "health" }
        { Name = "leisure" }
        { Name = "programming" }
        { Name = "travel" }
        { Name = "workflow" }
        { Name = "writing" }
    ]
    
    let private getArea name =
        areaList |> List.find (fun x -> x.Name = name)
    
    let private areas = {|
        chores = getArea "chores"
        finances = getArea "finances"
        leisure = getArea "leisure"
        programming = getArea "programming"
        workflow = getArea "workflow"
        writing = getArea "writing"
    |}
    
    let projectList : Project list = [
        { Area = areas.workflow;    Name = "app-fluke" }
        { Area = areas.writing;     Name = "blog" }
        { Area = areas.programming; Name = "rebuild-website" }
    ]
    
//    let private getProject name =
//        projectList |> List.find (fun x -> x.Name = name)
        
    let resourceList : Resource list = [
        { Area = areas.programming; Name = "agile" }
        { Area = areas.programming; Name = "artificial-intelligence" }
        { Area = areas.programming; Name = "cloud" }
        { Area = areas.workflow;    Name = "communication" }
        { Area = areas.programming; Name = "docker" }
        { Area = areas.programming; Name = "f#" }
        { Area = areas.programming; Name = "linux" }
        { Area = areas.leisure;     Name = "music" }
        { Area = areas.programming; Name = "rust" }
        { Area = areas.programming; Name = "vim" }
        { Area = areas.programming; Name = "windows" }
    ]
    
//    let private getResource name =
//        resourceList |> List.find (fun x -> x.Name = name)
        
    let hourOffset = 0
    
    let getNow hourOffset =
        let rawDate = DateTime.Now.AddHours -(float hourOffset)
        { Date = FlukeDate.FromDateTime rawDate
          Time = FlukeTime.FromDateTime rawDate }
    
    let private taskList : Task list = []
    
    let getTask name =
        taskList
        |> List.tryFind (fun x -> x.Name = name)
        |> Option.toResultWith (sprintf "Error getting task '%s'" name)
        |> Result.okOrThrow
    
    
    let getInformationList (projectList, areaList, resourceList) =
        [ projectList |> List.map Project
          areaList |> List.map Area
          resourceList |> List.map Resource ]
        
    let createRenderLaneTestData (testData: {| CellEvents: (FlukeDate * CellEventStatus) list
                                               Data: (FlukeDate * CellStatus) list
                                               Now: FlukeDateTime
                                               Task: Task |}) =
        
        {| CellEvents = testData.CellEvents |> LaneRendering.createCellEvents testData.Task
           TaskList = [ testData.Task ]
           TaskOrderList = [ { Task = testData.Task; Priority = First } ]
           GetNow = fun (_: int) -> testData.Now |}
        
    let createSortLanesTestData (testData : {| Data: (Task * (FlukeDate * CellEventStatus) list) list
                                               Expected: string list
                                               Now: FlukeDateTime |}) =
        let cellEvents = testData.Data |> List.collect (fun (task, events) -> LaneRendering.createCellEvents task events)
            
        {| CellEvents = cellEvents
           TaskList = testData.Data |> List.map fst
           TaskOrderList = testData.Data |> List.map (fun (task, _) -> { Task = task; Priority = Last })
           GetNow = fun (_: int) -> testData.Now |}
           
    let getInformationMap (projectList: Project list, areaList: Area list, resourceList: Resource list) =
        let projectMap = projectList |> List.map (fun x -> x.Name, x) |> Map.ofList
        let areaMap = areaList |> List.map (fun x -> x.Name, x) |> Map.ofList
        let resourceMap = resourceList |> List.map (fun x -> x.Name, x) |> Map.ofList
        
        projectMap, areaMap, resourceMap
            
    let mergeTaskList (projectList, areaList, resourceList) taskList =
        let projectMap, areaMap, resourceMap = getInformationMap (projectList, areaList, resourceList)
        
        let (|ProjectMissing|AreaMissing|ResourceMissing|Other|) = function
            | Project project when projectMap.ContainsKey project.Name |> not -> ProjectMissing project
            | Area area when areaMap.ContainsKey area.Name |> not -> AreaMissing area
            | Resource resource when resourceMap.ContainsKey resource.Name |> not -> ResourceMissing resource
            | _ -> Other
            
        let rec loop (projectList, areaList, resourceList) = function
            | ProjectMissing project :: tail -> loop (project :: projectList, areaList, resourceList) tail
            | AreaMissing area :: tail -> loop (projectList, area :: areaList, resourceList) tail
            | ResourceMissing resource :: tail -> loop (projectList, areaList, resource :: resourceList) tail
            | _ -> projectList, areaList, resourceList
            
        let projectList, areaList, resourceList =
            taskList
            |> List.map (fun x -> x.InformationType)
            |> loop (projectList, areaList, resourceList)
            
        let taskOrderList = taskList |> List.map (fun task -> { Task = task; Priority = Last })
            
        {| TaskList = taskList
           TaskOrderList = taskOrderList
           TaskComments = []
           ProjectList = projectList
           AreaList = areaList
           ResourceList = resourceList |}
           
    let createManualTasksFromTree (projectList, areaList, resourceList) taskList taskComments taskTree = Core.result {
        let projectMap, areaMap, resourceMap = getInformationMap (projectList, areaList, resourceList)
        
        let createTaskMap taskList =
            let map =
                taskList
                |> List.map (fun x -> (x.InformationType, x.Name), x)
                |> Map.ofList
                
            let taskOrderList = taskList |> List.map (fun task -> { Task = task; Priority = Last })
                
            map, taskOrderList
            
        let oldTaskMap, oldTaskOrderList = createTaskMap taskList
        
        let! newTaskList, newTaskComments =
            let getInformationType informationTypeName informationName =
                match informationTypeName with
                | "projects" -> projectMap |> Map.tryFind informationName |> Option.map Project
                | "areas" -> areaMap |> Map.tryFind informationName |> Option.map Area
                | "resources" -> resourceMap |> Map.tryFind informationName |> Option.map Resource
                | _ -> None
                |> Option.toResultWith (sprintf "Invalid information type: '%s/%s'" informationTypeName informationName)
                
            taskTree
            |> List.collect (fun (informationTypeName, informationType) ->
                informationType
                |> List.map (fun (informationName, information) -> 
                    getInformationType informationTypeName informationName
                    |> Result.map (fun informationType ->
                        information
                        |> List.map (Tuple2.mapFst (fun taskName ->
                            oldTaskMap
                            |> Map.tryFind (informationType, taskName)
                            |> Option.defaultValue
                                { Task.Default with
                                    Name = sprintf "> %s" taskName
                                    InformationType = informationType }
                        ))
                    )
                )
            )
            |> Result.fold List.append (Ok [])
            |> Result.map List.unzip
            
        let newTaskMap, newTaskOrderList = createTaskMap newTaskList
        
        let notOnNewTaskMap task =
            newTaskMap.ContainsKey (task.InformationType, task.Name) |> not
            
        let filteredOldTaskList = taskList |> List.filter notOnNewTaskMap
        let taskList =  filteredOldTaskList @ newTaskList
        
        let result = taskList |> mergeTaskList (projectList, areaList, resourceList)
        
        let filteredOldTaskOrder = oldTaskOrderList |> List.filter (fun x -> notOnNewTaskMap x.Task)
        let taskOrderList = newTaskOrderList @ filteredOldTaskOrder
        
        return
            {| result with
                TaskOrderList = result.TaskOrderList @ taskOrderList
                TaskComments = taskComments @ newTaskComments |}
    }
            
        
    let tempData<'T> = {|
        ManualTasks = 
            [
                "projects", [
                    "app-fluke", [
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
                    "blog", []
                    "rebuild-website", [
                        "task1", []
                    ]
                ]
                "areas", [
                    "car", []
                    "career", []
                    "chores", [
                        "groceries", [
                            "food"
                            "beer"
                        ]
                    ]
                    "fitness", []
                    "food", []
                    "finances", []
                    "health", []
                    "leisure", [
                        "watch-movie-foobar", []
                    ]
                    "programming", []
                    "travel", []
                    "workflow", []
                    "writing", []
                ]
                "resources", [
                    "agile", []
                    "artificial-intelligence", []
                    "cloud", []
                    "communication", []
                    "docker", []
                    "f#", [
                        "study: [choice, computation expressions]", []
                        "organize youtube playlists", []
                    ]
                    "linux", []
                    "music", []
                    "rust", []
                    "vim", []
                    "windows", []
                ]
            ]
            |> createManualTasksFromTree (projectList, areaList, resourceList) taskList []
            
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

