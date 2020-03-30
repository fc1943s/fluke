namespace Fluke.UI.Frontend

open System
open Fluke.Shared

module TempData =
    open Model
    
    let areaList = [
        { Area.Name = "chores" }
        { Area.Name = "finances" }
        { Area.Name = "leisure" }
        { Area.Name = "workflow" }
    ]
    
    let areas = {|
        chores = areaList |> List.find (fun x -> x.Name = "chores")
        finances = areaList |> List.find (fun x -> x.Name = "finances")
        leisure = areaList |> List.find (fun x -> x.Name = "leisure")
        workflow = areaList |> List.find (fun x -> x.Name = "workflow")
    |}
    
    let defaultTask =
        { Name = "<blank>"
          InformationType = Area areas.workflow
          Comments = []
          PendingAfter = midnight
          Scheduling = TaskScheduling.Manual
          Duration = None }
        
    let mutable _hourOffset = 0
    
    let mutable _getNow = fun hourOffset ->
        let rawDate = DateTime.Now.AddHours -(float hourOffset)
        { Date = FlukeDate.FromDateTime rawDate
          Time = FlukeTime.FromDateTime rawDate }
    
    let mutable _taskList: Task list = []
    
    let mutable _taskOrderList: TaskOrderEntry list = []
    
    let mutable _cellEvents: CellEvent list = []
    
    let mutable _cellComments: CellComment list = []
    
    
    let loadRenderLaneTestData (testData: {| CellEvents: (FlukeDate * CellEventStatus) list
                                             Data: (FlukeDate * CellStatus) list
                                             Now: FlukeDateTime
                                             Task: Task |}) =
        
        _cellEvents <- (testData.CellEvents |> List.map (fun (date, status) -> { Task = testData.Task; Date = date; Status = status }))
        _taskList <- [ testData.Task ]
        _taskOrderList <- [ { Task = testData.Task; Priority = First } ]
        _getNow <- fun _ -> testData.Now
        
    let loadSortLanesTestData (testData : {| Data: (Task * (FlukeDate * CellEventStatus) list) list
                                             Expected: string list
                                             Now: FlukeDateTime |}) =
        let cellEvents =
            testData.Data
            |> List.collect (fun (task, events) ->
                events
                |> List.map (fun (date, status) ->
                    { Task = task
                      Date = date
                      Status = status }
                )
            )
            
        _cellEvents <- cellEvents
        _taskList <- testData.Data |> List.map fst
        _taskOrderList <- testData.Data |> List.map (fun (task, _) -> { Task = task; Priority = First })
        _getNow <- fun _ -> testData.Now
