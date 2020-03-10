namespace Fluke.Shared

open System
open System.Collections.Generic

module Temp =
    type WatchSource =
        | Filmow
        | Trakt
    

module Model =
    
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
            
    type FlukeDate =
        { Year: int
          Month: int
          Day: int }
        override this.ToString () =
           sprintf "%d-%d-%d" this.Year this.Month this.Day
        member this.DateTime =
            DateTime (this.Year, this.Month, this.Day, 12, 0, 0)
        static member inline FromDateTime (date: DateTime) =
            { Year = date.Year
              Month = date.Month
              Day = date.Day }
            
    type InformationComment =
        { Information: InformationType
          Date: FlukeDate
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
                | Missed -> "#990022"
                | Postponed -> "#b08200"
                | Complete -> "#339933"
        
    type Cell =
        { Date: FlukeDate
          Status: CellStatus }
        
    type CellEvent =
        { Task: Task
          Date: FlukeDate
          Status: CellEventStatus }
    
    type CellComment =
        { Task: Task
          Date: FlukeDate
          Comment: string }
        
    
    type TaskOrderPriority =
        | First
        | LessThan of Task
        | Last
        
    type TaskOrderEntry =
        { Task: Task
          Priority: TaskOrderPriority }
    
module Functions =
    
    let getTaskList (taskOrderList: Model.TaskOrderEntry list) =
        let result = List<Model.Task> ()
        
        let taskOrderList =
            taskOrderList
            |> Seq.rev
            |> Seq.distinctBy (fun x -> x.Task.Name)
            |> Seq.rev
            |> Seq.toList
        
        for { Priority = priority; Task = task } in taskOrderList do
            match priority, result |> Seq.tryFindIndexBack ((=) task) with
            | Model.First, None -> result.Insert (0, task)
            | Model.Last, None -> result.Add task
            | Model.LessThan lessThan, None ->
                match result |> Seq.tryFindIndexBack ((=) lessThan) with
                | None -> seq { task; lessThan } |> Seq.iter (fun x -> result.Insert (0, x))
                | Some i -> result.Insert (i + 1, task)
            | _ -> ()
            
        for { Priority = priority; Task = task } in taskOrderList do
            match priority, result |> Seq.tryFindIndexBack ((=) task) with
            | Model.First, None -> result.Insert (0, task)
            | Model.Last, None -> result.Add task
            | _ -> ()
            
        result |> Seq.toList

    
    let getDateSequence (paddingLeft, paddingRight) (cellDates: Model.FlukeDate list) =
        let minDate =
            cellDates
            |> List.map (fun x -> x.DateTime)
            |> List.min
            |> fun x -> x.AddDays -(float paddingLeft)
            
        let maxDate =
            cellDates
            |> List.map (fun x -> x.DateTime)
            |> List.max
            |> fun x -> x.AddDays (float paddingRight)
            
        let rec loop date = seq {
            if date < maxDate then
                yield date
                yield! loop (date.AddDays 1.)
        }
        
        minDate
        |> loop
        |> Seq.map Model.FlukeDate.FromDateTime
        |> Seq.toList
            
    let renderLane (task: Model.Task) today dateSequence (cellEvents: Model.CellEvent list) =
        let cellEventsByDate =
            cellEvents
            |> List.map (fun x -> x.Date, x)
            |> Map.ofList
            
        let rec loop count dateSequence =
            match dateSequence with
            | h :: t ->
                match task.Scheduling, cellEventsByDate |> Map.tryFind h with
                | _, Some cellEvent ->
                    (h, Model.EventStatus cellEvent.Status) :: loop 1 t
                    
                | Model.Disabled, _ ->
                    (h, Model.CellStatus.Disabled) :: loop count t
                    
                | Model.Optional, _ ->
                    (h, Model.CellStatus.Optional) :: loop count t
                    
                | Model.Recurrency _, _ when h < today ->
                    (h, Model.CellStatus.Disabled) :: loop count t
                    
                | Model.Recurrency interval, _ when count = 0 || count = interval -> 
                    (h, Model.CellStatus.Pending) :: loop 1 t
                    
                | Model.Recurrency _, _ -> 
                    (h, Model.CellStatus.Disabled) :: loop (count + 1) t
                    
            | [] -> []
        loop 0 dateSequence
