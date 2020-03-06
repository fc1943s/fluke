namespace Fluke.Shared

open System
open System.Collections.Generic

module Temp =
    type WatchSource =
        | Filmow
        | Trakt
    

module Model =
    let hourOffset = 7
    
    let utcHour hour =
        hour// - TimeZoneInfo.Local.BaseUtcOffset.Hours
    
    type Area =
        { Name: string }
        
    type Project =
        { Area: Area
          Name: string }
        
    type Resource =
        { Area: Area
          Name: string }
        
        
    type InformationType =
        | Project of Project
        | Area of Area
        | Resource of Resource
        | Archive of InformationType
        member this.Name =
            match this with
            | Project project -> project.Name
            | Area area -> area.Name
            | Resource resource -> resource.Name
            | Archive archive -> sprintf "[%s]" archive.Name
        member this.Color =
            match this with
            | Project _ -> "#999"
            | Area _ -> "#666"
            | Resource _ -> "#333"
            | Archive archive -> sprintf "[%s]" archive.Color
            
    type InformationComment =
        { Information: InformationType
          Date: DateTime
          Comment: string }
    
    type TaskScheduling =
        | Disabled
        | Optional
        | Recurrency of int
        
    type Time =
        { Hour: int
          Minute: int }
        
    type Task =
        { Name: string
          InformationType: InformationType
          Comments: string list 
          Scheduling: TaskScheduling
          Duration: int option
          VisibleAfter: Time option }
        
    type CellEventStatus =
        | Missed
        | Postponed
        | Complete
    
    type CellStatus =
        | Disabled
        | Optional
        | Pending
        | EventStatus of CellEventStatus
        member this.CellColor =
            match this with
            | Disabled -> "#595959"
            | Optional -> "#4c664e"
            | Pending -> "#262626"
            | EventStatus status ->
                match status with
                | Missed -> "#339933"
                | Postponed -> "#b08200"
                | Complete -> "#339933"
        
    type Cell =
        { Date: DateTime
          Status: CellStatus }
        
    type CellEvent = {
        Task: Task
        Date: DateTime
        Status: CellEventStatus
    }
    
    type CellComment = {
        Task: Task
        Comment: string
    }
        
    
    type TaskOrderPriority =
        | First
        | LessThan of Task
        | Last
        
    type TaskOrderEntry =
        { Task: Task
          Priority: TaskOrderPriority }
    
    
    let getTaskList (taskOrderList: TaskOrderEntry list) =
        let result = List<Task> ()
        
        let taskOrderList =
            taskOrderList
            |> Seq.rev
            |> Seq.distinctBy (fun x -> x.Task.Name)
            |> Seq.rev
            |> Seq.toList
        
        for { Priority = priority; Task = task } in taskOrderList do
            match priority, result |> Seq.tryFindIndexBack ((=) task) with
            | First, None -> result.Insert (0, task)
            | Last, None -> result.Add task
            | LessThan lessThan, None ->
                match result |> Seq.tryFindIndexBack ((=) lessThan) with
                | None -> seq { task; lessThan } |> Seq.iter (fun x -> result.Insert (0, x))
                | Some i -> result.Insert (i + 1, task)
            | _ -> ()
            
        for { Priority = priority; Task = task } in taskOrderList do
            match priority, result |> Seq.tryFindIndexBack ((=) task) with
            | First, None -> result.Insert (0, task)
            | Last, None -> result.Add task
            | _ -> ()
            
        result |> Seq.toList

    
    
