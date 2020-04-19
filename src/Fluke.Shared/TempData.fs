namespace Fluke.Shared

open System
open FSharpPlus
open Suigetsu.Core

module TempData =
    open Model
    
    let mutable _areaList = [
        { Area.Name = "chores" }
        { Area.Name = "finances" }
        { Area.Name = "leisure" }
        { Area.Name = "programming" }
        { Area.Name = "workflow" }
    ]
    
    let getArea name =
        _areaList |> List.find (fun x -> x.Name = name)
    
    let areas = {|
        chores = getArea "chores"
        finances = getArea "finances"
        leisure = getArea "leisure"
        programming = getArea "programming"
        workflow = getArea "workflow"
    |}
    
    let mutable _projectList : Project list = [
        { Area = areas.workflow
          Name = "app-fluke" }
        
        { Area = areas.programming
          Name = "app-mechahaze" }
    ]
    
    let getProject name =
        _projectList |> List.find (fun x -> x.Name = name)
        
    let mutable _resourceList : Resource list = [
        { Area = areas.programming; Name = "f#" }
        { Area = areas.programming; Name = "rust" }
    ]
    
    let getResource name =
        _resourceList |> List.find (fun x -> x.Name = name)
        
    let mutable _hourOffset = 0
    
    let private getNow hourOffset =
        let rawDate = DateTime.Now.AddHours -(float hourOffset)
        { Date = FlukeDate.FromDateTime rawDate
          Time = FlukeTime.FromDateTime rawDate }
    
    let mutable _getNow = getNow
    
    let mutable _taskList: Task list = []
    let getTask name =
        _taskList |> List.find (fun x -> x.Name = name)
    
    let mutable _taskOrderList: TaskOrderEntry list = []
    
    let mutable _cellEvents: CellEvent list = []
    
    let mutable _cellComments: CellComment list = []
    
    let mutable _informationComments: InformationComment list = []
    
    let data () =
        {| AreaList = _areaList
           ProjectList = _projectList
           ResourceList = _resourceList
           TaskList = _taskList
           TaskOrderList = _taskOrderList
           CellEvents = _cellEvents
           CellComments = _cellComments
           InformationComments = _informationComments
           GetNow = _getNow
           HourOffset = _hourOffset |}
    
    let getInformationList () =
        [ _projectList |> List.map Project
          _areaList |> List.map Area
          _resourceList |> List.map Resource ]
        
    let loadRenderLaneTestData (testData: {| CellEvents: (FlukeDate * CellEventStatus) list
                                             Data: (FlukeDate * CellStatus) list
                                             Now: FlukeDateTime
                                             Task: Task |}) =
        
        _cellEvents <- testData.CellEvents |> LaneRendering.createCellEvents testData.Task
        _taskList <- [ testData.Task ]
        _taskOrderList <- [ { Task = testData.Task; Priority = First } ]
        _getNow <- fun _ ->
            if _taskList.Length = 1
            then testData.Now
            else getNow _hourOffset
        
    let loadSortLanesTestData (testData : {| Data: (Task * (FlukeDate * CellEventStatus) list) list
                                             Expected: string list
                                             Now: FlukeDateTime |}) =
        let cellEvents =
            testData.Data
            |> List.collect (fun (task, events) ->
                events
                |> LaneRendering.createCellEvents task
            )
            
        _cellEvents <- cellEvents
        _taskList <- testData.Data |> List.map fst
        _taskOrderList <- testData.Data |> List.map (fun (task, _) -> { Task = task; Priority = First })
        _getNow <- fun _ ->
            if _taskList.Length = testData.Data.Length
            then testData.Now
            else getNow _hourOffset
            
    let createManualTasksFromTree taskTree =
        let getInformationType informationTypeName informationName =
            match informationTypeName with
            | "projects" ->
                _projectList
                |> List.tryFind (fun x -> x.Name = informationName)
                |> Option.defaultValue { Project.Default with Name = informationName }
                |> Project
                |> Ok
                
            | "areas" ->
                _areaList
                |> List.tryFind (fun x -> x.Name = informationName)
                |> Option.defaultValue { Area.Default with Name = informationName }
                |> Area
                |> Ok
                
            | "resources" ->
                _resourceList
                |> List.tryFind (fun x -> x.Name = informationName)
                |> Option.defaultValue { Resource.Default with Name = informationName }
                |> Resource
                |> Ok
                
            | _ ->
                sprintf "Invalid information type: '%s'" informationTypeName |> Error
                
        let getOrCreateTask informationType taskName =
            _taskList
            |> List.tryFind (fun x -> x.Name = taskName)
            |> Option.defaultValue
                { Task.Default with
                    Name = taskName
                    InformationType = informationType }
                
        taskTree
        |> List.collect (fun (informationTypeName, informationType) ->
            informationType
            |> List.map (fun (informationName, information) ->
                getInformationType informationTypeName informationName
                |> Result.map (fun informationType ->
                    information
                    |> List.map (fun (taskName, comments) ->
                        let task = getOrCreateTask informationType taskName
                        match task with
                        | task when
                            [ Task.Default.InformationType; informationType ]
                            |> List.forall ((<>) task.InformationType) ->
                            sprintf "Task: %s. Invalid information type: (%s != %s)"
                                task.Name
                                (string task.InformationType)
                                (string informationType)
                            |> Error
                            
                        | task -> { task with Comments = task.Comments @ comments } |> Ok
                    )
                )
            )
        )
        |> List.map (Result.map (Result.fold (fun result next -> next :: result) (Ok [])) >> Result.flatten)
        |> Result.collect
        
    let loadTaskList taskList =
        taskList
        |> List.map (fun x -> x.InformationType)
        |> List.iter (function
            | Project project when _projectList |> List.exists (fun x -> x.Name = project.Name) |> not ->
                _projectList <- project :: _projectList
                
            | Area area when _areaList |> List.exists (fun x -> x.Name = area.Name) |> not ->
                _areaList <- area :: _areaList
                
            | Resource resource when _resourceList |> List.exists (fun x -> x.Name = resource.Name) |> not ->
                _resourceList <- resource :: _resourceList
                    
            | _ -> ()
        )
        
        _taskList <-
            _taskList
            |> List.filter (fun x -> taskList |> List.map (fun x -> x.Name) |> List.contains x.Name |> not)
            |> List.append taskList
        _taskOrderList <- _taskList |> List.map (fun task -> { Task = task; Priority = First })
        
    let loadTaskTree taskTree =
        taskTree
        |> createManualTasksFromTree
        |> fun (tasks, errors) ->
            match tasks, errors with
            | _, _ :: _ -> failwithf "Error creating tasks: %s" (String.Join ("\n", errors))
            | _ :: _, _ -> loadTaskList tasks
            | _ -> ()

    let loadTempManualTasks () =
        [
            "projects", [
                "app-fluke", [
                    "filesystem tests", []
                    "review onenote tasks", []
                ]
                "app-mechahaze", [
                    "multi dimensional separation", []
                    "create animations mathematically", []
                ]
            ]
            "areas", [
                "chores", [
                    "groceries", [
                        "food"
                        "beer"
                    ]
                ]
                "leisure", [
                    "watch-movie-foobar", []
                ]
            ]
            "resources", [
                "f#", [
                    "study: [choice, computation expressions]", []
                    "organize youtube playlists", []
                ]
                "rust", []
            ]
        ]
        |> loadTaskTree

