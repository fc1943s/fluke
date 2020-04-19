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
    
    let areas = {|
        chores = _areaList |> List.find (fun x -> x.Name = "chores")
        finances = _areaList |> List.find (fun x -> x.Name = "finances")
        leisure = _areaList |> List.find (fun x -> x.Name = "leisure")
        programming = _areaList |> List.find (fun x -> x.Name = "programming")
        workflow = _areaList |> List.find (fun x -> x.Name = "workflow")
    |}
    
    let mutable _projectList : Project list = [
        { Area = areas.workflow
          Name = "app-fluke" }
        
        { Area = areas.programming
          Name = "app-mechahaze" }
    ]
    let mutable _resourceList : Resource list = [
        { Area = areas.programming; Name = "f#" }
        { Area = areas.programming; Name = "rust" }
    ]
    let mutable _hourOffset = 0
    
    let private getNow = fun hourOffset ->
        let rawDate = DateTime.Now.AddHours -(float hourOffset)
        { Date = FlukeDate.FromDateTime rawDate
          Time = FlukeTime.FromDateTime rawDate }
    
    let mutable _getNow = getNow
    
    let mutable _taskList: Task list = []
    
    let mutable _taskOrderList: TaskOrderEntry list = []
    
    let mutable _cellEvents: CellEvent list = []
    
    let mutable _cellComments: CellComment list = []
    
    let mutable _informationComments: InformationComment list = []
    
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
        let mapDict fn =
            Seq.map (|KeyValue|) // let (|KeyValue|) (kvp:KeyValuePair<_,_>) = kvp.Key,kvp.Value
            >> Map.ofSeq
            >> Map.map fn
            
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
            
        taskTree
        |> mapDict (fun informationTypeName informationType ->
            informationType
            |> mapDict (fun informationName information ->
                getInformationType informationTypeName informationName
                |> Result.map (fun informationType ->
                    information
                    |> mapDict (fun taskName comments ->
                        _taskList
                        |> List.tryFind (fun x -> x.Name = taskName)
                        |> Option.defaultValue
                            { Task.Default with
                                Name = taskName
                                InformationType = informationType }
                        |> function
                            | task when
                                [ Task.Default.InformationType; informationType ]
                                |> List.forall ((<>) task.InformationType) ->
                                sprintf "Task: %s. Invalid information type: (%s != %s)"
                                    task.Name
                                    (string task.InformationType)
                                    (string informationType)
                                |> Error
                                
                            | task ->
                                { task with
                                    Comments = task.Comments |> List.append comments }
                                |> Ok
                    )
                )
            )
        )
        |> Map.values
        |> Seq.collect Map.values
        |> Seq.map (Result.map Map.values)
        |> Seq.map (Result.map (Result.fold (fun result next -> next :: result) (Ok [])) >> Result.flatten)
        |> Result.collect

    let loadTempManualTasks () =
        let taskLists = dict [
            "projects", dict [
                "app-fluke", dict [
                    "task1", []
                    "task2", []
                ]
                "app-mechahaze", dict [
                    "task3", []
                    "task4", []
                ]
            ]
            "areas", dict [
                "chores", dict [
                    "groceries", [
                        "food"
                        "beer"
                    ]
                ]
                "leisure", dict [
                    "watch-movie-foobar", []
                ]
            ]
            "resources", dict [
                "f#", dict [
                    "study: [choice, computation expressions]", []
                    "organize youtube playlists", []
                ]
                "rust", dict []
            ]
        ]
        
        taskLists
        |> createManualTasksFromTree
        |> fun (tasks, errors) ->
            match tasks, errors with
            | _, _ :: _ -> failwithf "Error creating tasks: %s" (String.Join ("\n", errors))
            | _ :: _, _ ->
                printfn "%A" tasks
                
                tasks
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
                
                _taskList <- tasks
                _taskOrderList <- tasks |> List.map (fun task -> { Task = task; Priority = First })
            | _ -> ()

