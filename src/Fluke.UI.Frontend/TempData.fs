namespace Fluke.UI.Frontend

open System
open Fluke.Shared

module TempData =
    open Model
    
    let areas = {|
        chores =
            { Area.Name = "chores" }
            
        finances =
            { Area.Name = "finances" }
            
        leisure =
            { Area.Name = "leisure" }
            
        workflow =
            { Area.Name = "workflow" }
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
    
    let mutable _taskOrderList: TaskOrderEntry list = []
    
    let mutable _cellEvents: CellEvent list = []
    
    let mutable _cellComments: CellComment list = []
    
    
    let loadTestData () =
        let testData =
            {| Task = { defaultTask with Scheduling = TaskScheduling.Optional }
               Now = { Date = { Year = 2020; Month = Month.March; Day = 28 }
                       Time = midnight }
               Data = [
                   { Year = 2020; Month = Month.March; Day = 24 }, Optional
                   { Year = 2020; Month = Month.March; Day = 25 }, Missed
                   { Year = 2020; Month = Month.March; Day = 26 }, EventStatus Complete
                   { Year = 2020; Month = Month.March; Day = 27 }, Optional
                   { Year = 2020; Month = Month.March; Day = 28 }, Optional
                   { Year = 2020; Month = Month.March; Day = 29 }, Optional
               ]
               CellEvents = [
                   { Year = 2020; Month = Month.March; Day = 25 }, ManualPending
                   { Year = 2020; Month = Month.March; Day = 26 }, Complete
               ] |}
        
        _cellEvents <- (testData.CellEvents |> List.map (fun (date, status) -> { Task = testData.Task; Date = date; Status = status }))
        _taskOrderList <- [ { Task = testData.Task; Priority = First } ]
        _getNow <- fun _ -> testData.Now
        
    // loadTestData ()
