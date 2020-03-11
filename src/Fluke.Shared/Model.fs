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
    
    type FlukeDateTime =
        { Date: FlukeDate
          Time: FlukeTime }
        static member inline FromDateTime (date: DateTime) =
            { Date = FlukeDate.FromDateTime date
              Time = FlukeTime.FromDateTime date }
            
    type InformationComment =
        { Information: InformationType
          Date: FlukeDate
          Comment: string }
    
    type TaskScheduling =
        | Disabled
        | Optional
        | OptionalDelayed of pendingAfter:FlukeTime
        | Recurrency of days:int
        | RecurrencyDelayed of days:int * pendingAfter:FlukeTime
    
        
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
        
    type Lane = Lane of task:Task * cells:Cell list
    
module Functions =
    open Model
    
    let sortLanes (today: FlukeDate) (lanes: Lane list) =
        let order = [
            Pending
            Optional
            EventStatus Postponed
            EventStatus Complete
            Missed
            Disabled
        ]
        
        lanes
        |> List.sortBy (fun (Lane (_, cells)) ->
            cells
            |> List.filter (fun cell -> cell.Date = today)
            |> List.map (fun cell -> order |> List.tryFindIndex ((=) cell.Status))
        )
    
    let getManualSortedTaskList (taskOrderList: TaskOrderEntry list) =
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

    
    let getDateSequence (paddingLeft, paddingRight) (cellDates: FlukeDate list) =
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
        |> Seq.map FlukeDate.FromDateTime
        |> Seq.toList
        
    let renderLane task (now: FlukeDateTime) dateSequence (cellEvents: CellEvent list) =
        let cellEventsByDate =
            cellEvents
            |> List.map (fun x -> x.Date, x)
            |> Map.ofList
            
        let compareTime now pendingAfter =
               now.Time.Hour > pendingAfter.Hour
            || now.Time.Hour = pendingAfter.Hour && now.Time.Minute >= pendingAfter.Minute
            
        let optionalStatus date pendingAfter =
            if now.Date = date && compareTime now pendingAfter
            then Pending
            else Optional
            
        let recurringStatus days date pendingAfter count =
            match date < now.Date, count with
            | true, 0 -> Missed, 0
            | true, _ -> Disabled, 1
            | false, _ when [ 0; days ] |> List.contains count ->
                let status =
                    if now.Date <> date || compareTime now pendingAfter
                    then Pending
                    else Optional
                status, 1
            | false, _ -> Disabled, count + 1
            
        let rec loop count dateSequence =
            match dateSequence with
            | head :: tail ->
                match cellEventsByDate |> Map.tryFind head with
                | Some ({ Status = Postponed _ } as cellEvent) -> (head, EventStatus cellEvent.Status) :: loop 0 tail
                | Some cellEvent -> (head, EventStatus cellEvent.Status) :: loop 1 tail
                | None ->
                    match task.Scheduling with
                    | TaskScheduling.Disabled -> (head, Disabled) :: loop count tail
                    | TaskScheduling.OptionalDelayed pendingAfter -> (head, optionalStatus head pendingAfter) :: loop count tail
                    | TaskScheduling.Optional -> (head, optionalStatus head { Hour = 24; Minute = 0 }) :: loop count tail
                    | TaskScheduling.RecurrencyDelayed (days, pendingAfter) ->
                        let status, count = recurringStatus days head pendingAfter count in 
                        (head, status) :: loop count tail
                    | TaskScheduling.Recurrency days ->
                        let status, count = recurringStatus days head { Hour = 0; Minute = 0 } count
                        (head, status) :: loop count tail
            | [] -> []
            
        let cells =
            loop 0 dateSequence
            |> List.map (fun (date, status) ->
                { Cell.Date = date
                  Cell.Status = status }
            )
        Lane (task, cells)
