namespace Fluke.UI.Frontend

open System
open Fluke.Shared

module TempData =
    open Model
    
    let mutable _hourOffset = 0
    
    let mutable _getNow = fun hourOffset ->
        let rawDate = DateTime.Now.AddHours -(float hourOffset)
        { Date = FlukeDate.FromDateTime rawDate
          Time = FlukeTime.FromDateTime rawDate }
    
    let mutable _taskOrderList: TaskOrderEntry list = []
    
    let mutable _cellEvents: CellEvent list = []
    
    let mutable _cellComments: CellComment list = []
    
    

