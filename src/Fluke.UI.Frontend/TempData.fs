namespace Fluke.UI.Frontend

open Fluke.Shared

module TempData =
    open Model
    
    let mutable _hourOffset = 0
    
    let mutable _taskOrderList: TaskOrderEntry list = []
    
    let mutable _cellEvents: CellEvent list = []
    
    let mutable _cellComments: CellComment list = []
    
    

