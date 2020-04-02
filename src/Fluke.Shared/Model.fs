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
        
    let sortLanes (today: FlukeDate) (lanes: Lane list) =
        
        let getIndex task (cell: Cell) =
            [ function EventStatus ManualPending, _                             -> true | _ -> false
              function Pending,                   _                             -> true | _ -> false
              function Suggested,                 { Scheduling = Recurrency _ } -> true | _ -> false
              function Suggested,                 { Scheduling = Manual true }  -> true | _ -> false
              function EventStatus Postponed,     _                             -> true | _ -> false
              function EventStatus Complete,      _                             -> true | _ -> false
              
              function EventStatus Dropped,       _                             -> true | _ -> false
              function Disabled,                  { Scheduling = Recurrency _ } -> true | _ -> false
              function Suggested,                 { Scheduling = Manual false } -> true | _ -> false
              function _,                         _                             -> true ]
            |> List.mapi (fun i v -> i, v (cell.Status, task))
            |> List.filter (fun (_, ok) -> ok)
            |> List.map fst
        
        lanes
        |> List.sortBy (fun (Lane (task, cells)) ->
            cells
            |> List.filter (fun cell -> cell.Date = today)
            |> List.map (getIndex task)
        )
    
    type LaneCellRenderState =
        | WaitingFirstEvent
        | WaitingEvent
        | DayMatch
        | Counting of int
        
    type LaneCellRenderOutput =
        | EmptyCell
        | StatusCell of CellStatus
        | TodayCell
                    
        
    let renderLane task (now: FlukeDateTime) dateSequence (cellEvents: CellEvent list) =
            
        let (|BeforeToday|Today|AfterToday|) (now: FlukeDate, date: FlukeDate) =
            now.DateTime
            |> date.DateTime.CompareTo
            |> function
                | n when n < 0 -> BeforeToday
                | n when n = 0 -> Today
                | _ -> AfterToday
                
        let cellEventsByDate =
            cellEvents
            |> List.map (fun x -> x.Date, x)
            |> Map.ofList
            
        let rec loop renderState = function
            | date :: tail ->
                let event = 
                    cellEventsByDate
                    |> Map.tryFind date
                    
                let status, renderState =
                    match event with
                    | Some cellEvent ->
                        let renderState =
                            match cellEvent.Status, (now.Date, date) with
                            | (Postponed | ManualPending), BeforeToday -> WaitingEvent
                            | _ -> Counting 1
                            
                        StatusCell (EventStatus cellEvent.Status), renderState
                        
                    | None ->
                        let getStatus renderState =
                            match renderState, (now.Date, date) with
                            | WaitingFirstEvent, BeforeToday -> EmptyCell, WaitingFirstEvent
                            | DayMatch,          BeforeToday -> StatusCell Missed, WaitingEvent
                            | WaitingEvent,      BeforeToday -> StatusCell Missed, WaitingEvent

                            | WaitingFirstEvent, Today       -> TodayCell, Counting 1
                            | DayMatch,          Today       -> TodayCell, Counting 1
                            | WaitingEvent,      Today       -> TodayCell, Counting 1

                            | WaitingFirstEvent, AfterToday  -> EmptyCell, WaitingFirstEvent
                            | DayMatch,          AfterToday  -> StatusCell Pending, Counting 1
                            | WaitingEvent,      AfterToday  -> StatusCell Pending, Counting 1

                            | Counting count,    _           -> EmptyCell, Counting (count + 1)
                            
                        match task.Scheduling with
                        | Recurrency (Offset days) ->
                            let renderState =
                                match renderState with
                                | Counting count when count = days -> DayMatch
                                | _ -> renderState
                                
                            getStatus renderState
                            
                        | Recurrency (Fixed recurrency) ->
                            let isDateMatched =
                                match recurrency with
                                | Weekly dayOfWeek -> dayOfWeek = date.DateTime.DayOfWeek
                                | Monthly day -> day = date.Day
                                | Yearly (day, month) -> day = date.Day && month = date.Month
                                
                            match renderState, (now.Date, date) with
                            | WaitingFirstEvent, BeforeToday                     -> EmptyCell, WaitingFirstEvent
                            | _,                 Today        when isDateMatched -> StatusCell Pending, Counting 1
                            | WaitingFirstEvent, Today                           -> EmptyCell, Counting 1
                            | _,                 _            when isDateMatched -> getStatus WaitingEvent
                            | _,                 _                               -> getStatus renderState
                            
                        | Manual suggested ->
                            
                            let status, renderStatus =
                                match renderState, (now.Date, date) with
//                                | WaitingFirstEvent, BeforeToday                     -> EmptyCell, WaitingFirstEvent
                                | WaitingFirstEvent, Today       when suggested && task.PendingAfter = midnight -> StatusCell Suggested, Counting 1
                                | WaitingFirstEvent, Today       when suggested -> TodayCell, Counting 1
                                | WaitingFirstEvent, Today                          -> StatusCell Suggested, Counting 1
                                | _,                 _                               -> 
                                    let oldRenderState = renderState
                                    let status, renderState =
                                        getStatus renderState
                                    
//                                    printfn "AA: %A %A %A->%A" date status oldRenderState renderState
                                    let status =
                                        match renderState, status with
                                        | _,          EmptyCell when suggested -> StatusCell Suggested
                                        | Counting _, TodayCell                -> StatusCell Pending
                                        | _,          TodayCell                -> StatusCell Suggested
                                        | _,          status                   -> status
                                        
                                    status, renderState
                                
                            status, renderStatus
//                            match renderState with
//                            | WaitingFirstEvent -> EmptyCell, WaitingFirstEvent
//                            | WaitingEvent -> StatusCell Pending, Counting 1
//                            | _ ->
//                                let oldRenderState = renderState
//                                let status, renderState =
//                                    getStatus renderState
//                                
//                                printfn "AA: %A %A %A->%A" date status oldRenderState renderState
//                                let status =
//                                    match renderState, status with
//                                    | _,          EmptyCell when suggested -> StatusCell Suggested
//                                    | Counting _, TodayCell                -> StatusCell Pending
//                                    | _,          TodayCell                -> StatusCell Suggested
//                                    | _,          status                   -> status
//                                    
//                                status, renderState
                                
                let status =
                    match status with
                    | EmptyCell -> Disabled
                    | StatusCell status -> status
                    | TodayCell ->
                        let isPendingNow =
                               now.Time.Hour > task.PendingAfter.Hour
                            || now.Time.Hour = task.PendingAfter.Hour && now.Time.Minute >= task.PendingAfter.Minute
                
                        match isPendingNow with
                        | true -> Pending
                        | false -> Suggested
//                            match renderStatus, pendingAfter, isVisibleNow with
//                            | WaitingEvent, _,            _                                  -> Pending
//                            | _,            _,            true                               -> Pending
//                            | _,            _,            false                              -> Suggested
//                            | _,            _, _     when pendingAfter = midnight -> Suggested
                
                (date, status) :: loop renderState tail
            | [] -> []
            
        let cells =
            loop WaitingFirstEvent dateSequence
            |> List.map (fun (date, status) ->
                { Cell.Date = date
                  Cell.Status = status }
            )
        Lane (task, cells)
