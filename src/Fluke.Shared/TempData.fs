namespace Fluke.Shared

open System

module TempData =
    open Model
    
    let mutable _areaList = [
        { Area.Name = "chores" }
        { Area.Name = "finances" }
        { Area.Name = "leisure" }
        { Area.Name = "workflow" }
    ]
    
    let areas = {|
        chores = _areaList |> List.find (fun x -> x.Name = "chores")
        finances = _areaList |> List.find (fun x -> x.Name = "finances")
        leisure = _areaList |> List.find (fun x -> x.Name = "leisure")
        workflow = _areaList |> List.find (fun x -> x.Name = "workflow")
    |}
    
    let mutable _projectList : Project list = []
    let mutable _resourceList : Resource list = []
    
    let defaultTask =
        { Name = "<blank>"
          InformationType = Area areas.workflow
          Comments = []
          PendingAfter = midnight
          Scheduling = Manual false
          Duration = None }
        
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
