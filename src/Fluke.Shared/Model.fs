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
            
    type FlukeTime =
        { Hour: int
          Minute: int }
        static member inline FromDateTime (date: DateTime) =
            { Hour = date.Hour
              Minute = date.Minute }
            
    type InformationComment =
        { Information: InformationType
          Date: FlukeDate
          Comment: string }
    
    type TaskScheduling =
        | Disabled
        | Optional
        | Delayed of pendingAfter: FlukeTime
        | Recurrency of days: int
    
        
    type Task =
        { Name: string
          InformationType: InformationType
          Comments: string list 
          Scheduling: TaskScheduling
          Duration: int option }
        
    type CellEventStatus =
        | Postponed
        | Complete
    
    type CellStatus =
        | Disabled
        | Optional 
        | Pending
        | Missed
        | EventStatus of CellEventStatus
        member this.CellColor =
            match this with
            | Disabled -> "#595959"
            | Optional -> "#4c664e"
            | Pending -> "#262626"
            | Missed -> "#990022"
            | EventStatus status ->
                match status with
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
        
    type Lane = Lane of Task * Cell list
    
module Functions =
    
    let sortLanes (today: Model.FlukeDate) (lanes: Model.Lane list) =
        let order = [
            Model.Pending
            Model.CellStatus.Optional
            Model.EventStatus Model.Postponed
            Model.EventStatus Model.Complete
            Model.CellStatus.Missed
            Model.CellStatus.Disabled
        ]
        
        lanes
        |> List.sortBy (fun (Model.Lane (_, cells)) ->
            cells
            |> List.filter (fun cell -> cell.Date = today)
            |> List.map (fun cell -> order |> List.tryFindIndex ((=) cell.Status))
        )
    
    let getManualSortedTaskList (taskOrderList: Model.TaskOrderEntry list) =
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
            if date <= maxDate then
                yield date
                yield! loop (date.AddDays 1.)
        }
        
        minDate
        |> loop
        |> Seq.map Model.FlukeDate.FromDateTime
        |> Seq.toList
            
    let renderLane (task: Model.Task)
                   (today: Model.FlukeDate)
                   (now: Model.FlukeTime)
                   (dateSequence: Model.FlukeDate list)
                   (cellEvents: Model.CellEvent list) =
        let cellEventsByDate =
            cellEvents
            |> List.map (fun x -> x.Date, x)
            |> Map.ofList
            
        let rec loop count dateSequence =
            match dateSequence with
            | head :: tail ->
                match cellEventsByDate |> Map.tryFind head with
                | Some { Status = Model.Postponed as status } ->
                    (head, Model.EventStatus status) :: loop 0 tail
                    
                | Some { Status = status } ->
                    (head, Model.EventStatus status) :: loop 1 tail
                    
                | None ->
                    match task.Scheduling with
                    | Model.Disabled ->
                        (head, Model.CellStatus.Disabled) :: loop count tail
                        
                    | Model.Optional ->
                        (head, Model.CellStatus.Optional) :: loop count tail
                        
                    | Model.Delayed pendingAfter
                          when today = head
                            && now.Hour > pendingAfter.Hour
                            || now.Hour = pendingAfter.Hour && now.Minute >= pendingAfter.Minute ->
                        (head, Model.CellStatus.Pending) :: loop count tail
                        
                    | Model.Delayed _ ->
                        (head, Model.CellStatus.Optional) :: loop count tail
                        
                    | Model.Recurrency interval ->
                        let status, count =
                            match head < today, count with
                            | true, 0 -> Model.CellStatus.Missed, 0
                            | true, _ -> Model.CellStatus.Disabled, 1
                            | _, 0 -> Model.CellStatus.Pending, 1
                            | _, _ when count = interval -> Model.CellStatus.Pending, 1
                            | _, _ -> Model.CellStatus.Disabled, count + 1
                        (head, status) :: loop count tail
            | [] -> []
            
        let cells =
            loop 0 dateSequence
            |> List.map (fun (date, status) ->
                { Model.Cell.Date = date
                  Model.Cell.Status = status }
            )
        Model.Lane (task, cells)
