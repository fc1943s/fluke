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
        | Manual of suggested:bool
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
        | ManualPending
    
    type CellStatus =
        | Disabled
        | Suggested 
        | Pending
        | Missed
        | EventStatus of CellEventStatus
        member this.CellColor =
            match this with
            | Disabled -> "#595959"
            | Suggested -> "#4c664e"
            | Pending -> "#262626"
            | Missed -> "#990022"
            | EventStatus status ->
                match status with
                | Postponed -> "#b08200"
                | Complete -> "#339933"
                | Dropped -> "#673ab7"
                | ManualPending -> "#003038"
        
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
        
        let getIndex task (cells: Cell list) (cell: Cell) =
            let (|Manual|RecOffset|RecFixed|) = function
                | { Scheduling = Manual suggested } -> Manual suggested
                | { Scheduling = Recurrency (Offset _) } -> RecOffset
                | { Scheduling = Recurrency (Fixed _) } -> RecFixed
                
            let dropped =
                cells
                |> List.filter (function
                    { Date = date; Status = EventStatus _ }
                        when date.DateTime <= today.DateTime -> true | _ -> false)
                |> List.tryLast
                |> function
                    | Some { Status = EventStatus Dropped } -> true
                    | _ -> false
                
            [ function EventStatus ManualPending, _                                 -> true | _ -> false
              function Pending,                   _                                 -> true | _ -> false
              function Suggested,                 (RecFixed | RecOffset)            -> true | _ -> false
              function Suggested,                 Manual true                       -> true | _ -> false
              function EventStatus Postponed,     _                                 -> true | _ -> false
              function EventStatus Complete,      _                                 -> true | _ -> false
              
              function EventStatus Dropped,       _                                 -> true | _ -> false
              function Disabled,                  RecOffset                         -> true | _ -> false
              function Disabled,                  RecFixed                          -> true | _ -> false
              function Suggested,                 Manual false                      -> true | _ -> false
//              function Optional,       Manual           when dropped     -> true | _ -> false
//              function _,                         _                when dropped     -> true | _ -> false
              function _,                         _                                 -> true ]
            |> List.mapi (fun i v -> i, v (cell.Status, task))
            |> List.filter (fun (_, ok) -> ok)
            |> List.map fst
        
        lanes
        |> List.sortBy (fun (Lane (task, cells)) ->
            cells
            |> List.filter (fun cell -> cell.Date = today)
            |> List.map (fun cell -> getIndex task cells cell)
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
        
        
    type LaneCellRenderStatus =
        | WaitingFirstEvent
        | WaitingEvent
        | Counting of int
        
    let renderLane task (now: FlukeDateTime) dateSequence (cellEvents: CellEvent list) =
                
        let (|BeforeToday|Today|AfterToday|) (now: FlukeDate, date: FlukeDate) =
            now.DateTime
            |> date.DateTime.CompareTo
            |> function
                | n when n < 0 -> BeforeToday
                | n when n = 0 -> Today
                | _ -> AfterToday
                
        let isVisibleNow pendingAfter =
               now.Time.Hour > pendingAfter.Hour
            || now.Time.Hour = pendingAfter.Hour && now.Time.Minute >= pendingAfter.Minute
            
        let todayStatus pendingAfter =
            if isVisibleNow pendingAfter
            then Pending
            else Suggested
            
        let defaultCellStatus =
            match task.Scheduling with
            | Manual true -> Suggested
            | _ -> Disabled
            
        let getStatus days (date: FlukeDate) pendingAfter renderStatus =
            match renderStatus, (now.Date, date) with
            | WaitingFirstEvent, BeforeToday                   -> defaultCellStatus, WaitingFirstEvent
            | Counting count,    BeforeToday when count = days -> Missed, WaitingEvent
            | WaitingEvent,      BeforeToday                   -> Missed, WaitingEvent
                                                                           
//            | WaitingFirstEvent, Today             when task.Scheduling = Manual true            -> Missed, Counting 1
            | WaitingFirstEvent, Today                         -> todayStatus pendingAfter, Counting 1
            | Counting count,    Today       when count = days -> todayStatus pendingAfter, Counting 1
            | WaitingEvent,      Today                         -> todayStatus pendingAfter, Counting 1
                                                                           
            | WaitingFirstEvent, AfterToday                    -> defaultCellStatus, WaitingFirstEvent
            | Counting count,    AfterToday  when count = days -> Pending, Counting 1
            | WaitingEvent,      AfterToday                    -> Pending, Counting 1
                                                                           
            | Counting count,    _                             -> defaultCellStatus, Counting (count + 1)
            
        let cellEventsByDate =
            cellEvents
            |> List.map (fun x -> x.Date, x)
            |> Map.ofList
            
        let rec loop renderStatus = function
            | date :: tail ->
                match cellEventsByDate
                      |> Map.tryFind date with
                | Some ({ Status = Postponed _ } as cellEvent) ->
                    (date, EventStatus cellEvent.Status) :: loop WaitingEvent tail
                    
                | Some ({ Status = ManualPending _ } as cellEvent) ->
                    let status, renderStatus =
                        if date.DateTime < now.Date.DateTime
                        then getStatus 0 date task.PendingAfter WaitingEvent
                        else EventStatus cellEvent.Status, Counting 1
                        
                    (date, status) :: loop renderStatus tail
                    
                | Some cellEvent ->
                    (date, EventStatus cellEvent.Status) :: loop (Counting 1) tail
                    
                | None ->
                    match task.Scheduling with
                    | Recurrency (Fixed recurrency) ->
                        
                        let isDateMatched =
                            match recurrency with
                            | Weekly dayOfWeek -> dayOfWeek = date.DateTime.DayOfWeek
                            | Monthly day -> day = date.Day
                            | Yearly (day, month) -> day = date.Day && month = date.Month
                            
                        let status, renderStatus =
                            if date = now.Date && isDateMatched then
                                Pending, Counting 1
                            elif date = now.Date && renderStatus = WaitingFirstEvent then
                                Disabled, Counting 1
                            else
                                match renderStatus with
                                | WaitingFirstEvent -> WaitingFirstEvent
                                | _ when isDateMatched -> WaitingEvent
                                | _ -> renderStatus
                                |> getStatus 0 date task.PendingAfter
                        
//                        printfn "AA: %A - %A - %A - %A" date status renderStatus renderStatus
//                        
                        (date, status) :: loop renderStatus tail
                        
                    | Recurrency (Offset days) ->
                        let status, renderStatus = getStatus days date task.PendingAfter renderStatus
                        (date, status) :: loop renderStatus tail
                        
                    | Manual _ ->
                        let pendingAfter =
                            if    task.PendingAfter = midnight
                               && renderStatus <> WaitingEvent
                            then { Hour = 24; Minute = 0 }
                            else task.PendingAfter
                            
//                        let pendingAfter = task.PendingAfter
                            
                        let status, renderStatus =
                            getStatus 0 date pendingAfter renderStatus
                            
                        (date, status) :: loop renderStatus tail
            | [] -> []
            
        let cells =
            loop WaitingFirstEvent dateSequence
            |> List.map (fun (date, status) ->
                { Cell.Date = date
                  Cell.Status = status }
            )
        Lane (task, cells)
