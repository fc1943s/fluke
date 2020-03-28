namespace Fluke.Shared

open System
open System.Collections.Generic

module Temp =
    ()

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
            
    type Month =
        | January = 1
        | February = 2
        | March = 3
        | April = 4
        | May = 5
        | June = 6
        | July = 7
        | August = 8
        | September = 9
        | October = 10
        | November = 11
        | December = 12
        
    type FlukeDate =
        { Year: int
          Month: Month
          Day: int }
        override this.ToString () =
           sprintf "%d-%A-%d" this.Year this.Month this.Day
        member this.DateTime =
            DateTime (this.Year, int this.Month, this.Day, 12, 0, 0)
        static member inline FromDateTime (date: DateTime) =
            { Year = date.Year
              Month = Enum.Parse (typeof<Month>, string date.Month) :?> Month
              Day = date.Day }
            
    type FlukeTime =
        { Hour: int
          Minute: int }
        static member inline FromDateTime (date: DateTime) =
            { Hour = date.Hour
              Minute = date.Minute }
    let midnight = { Hour = 0; Minute = 0 }
    
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
        
        
    type FixedRecurrency =
        | Weekly of DayOfWeek
        | Monthly of day:int
        | Yearly of day:int * month:Month
        
    type TaskRecurrency =
        | Offset of days:int
        | Fixed of FixedRecurrency
    
    type TaskScheduling =
        | Disabled
        | Once
        | Optional
        | Recurrency of TaskRecurrency
    
        
    type Task =
        { Name: string
          InformationType: InformationType
          Comments: string list 
          Scheduling: TaskScheduling
          PendingAfter: FlukeTime
          Duration: int option }
        
    type CellEventStatus =
        | Postponed
        | Complete
        | Dropped
    
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
                | Dropped -> "#673ab7"
        
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
            EventStatus Dropped
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
        
        
    type LaneCellRenderingStatus =
        | Disabled
        | WaitingFirstEvent
        | WaitingEvent
        | Counting of int
        
    let renderLane task (now: FlukeDateTime) dateSequence (cellEvents: CellEvent list) =
        let isVisibleNow pendingAfter =
               now.Time.Hour > pendingAfter.Hour
            || now.Time.Hour = pendingAfter.Hour && now.Time.Minute >= pendingAfter.Minute
            
        let todayStatus pendingAfter =
            if isVisibleNow pendingAfter
            then Pending
            else Optional
            
        let (|BeforeToday|Today|AfterToday|) (now: FlukeDate, date: FlukeDate) =
            now.DateTime
            |> date.DateTime.CompareTo
            |> function
                | n when n < 0 -> BeforeToday
                | n when n = 0 -> Today
                | _ -> AfterToday
                
        let recurringStatus days (date: FlukeDate) pendingAfter lastCell =
            match (now.Date, date), lastCell with
            | _,           Disabled                         -> CellStatus.Disabled, Disabled
            
            | BeforeToday, WaitingFirstEvent                -> CellStatus.Disabled, WaitingFirstEvent
            | BeforeToday, WaitingEvent                     -> Missed, WaitingEvent
            | BeforeToday, Counting count when count = days -> Missed, WaitingEvent
            
            | Today,       WaitingFirstEvent                -> todayStatus pendingAfter, Counting 1
            | Today,       WaitingEvent                     -> todayStatus pendingAfter, Counting 1
            | Today,       Counting count when count = days -> todayStatus pendingAfter, Counting 1
            
            | AfterToday,  WaitingFirstEvent                -> Pending, Counting 1
            | AfterToday,  WaitingEvent                     -> Pending, Counting 1
            | AfterToday,  Counting count when count = days -> Pending, Counting 1
            
            | _,           Counting count                   -> CellStatus.Disabled, Counting (count + 1)
            
        let cellEventsByDate =
            cellEvents
            |> List.map (fun x -> x.Date, x)
            |> Map.ofList
            
        let rec loop lastCell = function
            | date :: tail ->
                match cellEventsByDate
                      |> Map.tryFind date with
                | Some ({ Status = Postponed _ } as cellEvent) ->
                    (date, EventStatus cellEvent.Status) :: loop WaitingEvent tail
                | Some ({ Status = Dropped _ } as cellEvent) ->
                    (date, EventStatus cellEvent.Status) :: loop Disabled tail
                | Some cellEvent ->
                    (date, EventStatus cellEvent.Status) :: loop (Counting 1) tail
                | None ->
                    match task.Scheduling with
                    | TaskScheduling.Disabled ->
                        (date, CellStatus.Disabled) :: loop Disabled tail
                        
                    | TaskScheduling.Optional ->
                        let pendingAfter =
                            if task.PendingAfter = midnight
                            then { Hour = 24; Minute = 0 }
                            else task.PendingAfter
                            
                        let status =
                            if date = now.Date
                            then todayStatus pendingAfter
                            else Optional
                            
                        (date, status) :: loop Disabled tail
                        
                    | TaskScheduling.Recurrency (Fixed recurrency) ->
                        let status =
                            match recurrency with
                            | Weekly dayOfWeek -> dayOfWeek = date.DateTime.DayOfWeek
                            | Monthly day -> day = date.Day
                            | Yearly (day, month) -> day = date.Day && month = date.Month
                            |> function
                                | false -> CellStatus.Disabled
                                | true ->
                                    if date = now.Date
                                    then todayStatus task.PendingAfter
                                    else Pending
                                    
                        (date, status) :: loop Disabled tail
                        
                    | TaskScheduling.Recurrency (Offset days) ->
                        let status, count = recurringStatus days date task.PendingAfter lastCell
                        (date, status) :: loop count tail
                        
                    | TaskScheduling.Once ->
                        let status, count = recurringStatus 0 date task.PendingAfter lastCell
                        (date, status) :: loop count tail
            | [] -> []
            
        let cells =
            loop WaitingFirstEvent dateSequence
            |> List.map (fun (date, status) ->
                { Cell.Date = date
                  Cell.Status = status }
            )
        Lane (task, cells)
