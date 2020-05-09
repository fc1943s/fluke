namespace Fluke.Shared

open System
open System.Collections.Generic
open FSharpPlus
open Suigetsu.Core

module Model =
    
    [<StructuredFormatDisplay("{Name}")>]
    type Area =
        { Name: string }
        static member inline Default =
            { Name = "<null>" }
        
    [<StructuredFormatDisplay("{Area}/{Name}")>]
    type Project =
        { Area: Area
          Name: string }
        static member inline Default =
            { Name = "<null>"
              Area = Area.Default }
        
    [<StructuredFormatDisplay("{Area}/{Name}")>]
    type Resource =
        { Area: Area
          Name: string }
        static member inline Default =
            { Name = "<null>"
              Area = Area.Default }
        
    type Information =
        | Project of Project
        | Area of Area
        | Resource of Resource
        | Archive of Information
            
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
        
    [<StructuredFormatDisplay("{Year}-{Month}-{Day}")>]
    type FlukeDate =
        { Year: int
          Month: Month
          Day: int }
        member this.DateTime =
            DateTime (this.Year, int this.Month, this.Day, 12, 0, 0)
        static member inline FromDateTime (date: DateTime) =
            { Year = date.Year
              Month = Enum.Parse (typeof<Month>, string date.Month) :?> Month
              Day = date.Day }
    let flukeDate year month day = { Year = year; Month = month; Day = day }
            
    [<StructuredFormatDisplay("{Hour}:{Minute}")>]
    type FlukeTime =
        { Hour: int
          Minute: int }
        static member inline FromDateTime (date: DateTime) =
            { Hour = date.Hour
              Minute = date.Minute }
            
            
    let flukeTime hour minute = { Hour = hour; Minute = minute }
    let midnight = flukeTime 00 00
    
    [<StructuredFormatDisplay("{Date} {Time}")>]
    type FlukeDateTime =
        { Date: FlukeDate
          Time: FlukeTime }
        static member inline FromDateTime (date: DateTime) =
            { Date = FlukeDate.FromDateTime date
              Time = FlukeTime.FromDateTime date }
    let flukeDateTime year month day hour minute = { Date = flukeDate year month day; Time = flukeTime hour minute }
            
    type InformationComment =
        { Information: Information
          Comment: string }
        
    type FixedRecurrency =
        | Weekly of DayOfWeek
        | Monthly of day:int
        | Yearly of day:int * month:Month
        
    type TaskRecurrencyOffset =
        | Days of int
        | Weeks of int
        | Months of int
        
    type TaskRecurrency =
        | Offset of TaskRecurrencyOffset
        | Fixed of FixedRecurrency list
    
    type TaskScheduling =
        | Manual of suggested:bool
        | Recurrency of TaskRecurrency
    
    type Task =
        { Name: string
          Information: Information
          Scheduling: TaskScheduling
          PendingAfter: FlukeTime
          MissedAfter: FlukeTime
          Duration: int option }
        static member inline Default =
            { Name = "<null>"
              Information = Area Area.Default 
              PendingAfter = midnight
              MissedAfter = midnight
              Scheduling = Manual false
              Duration = None }
        
    type CellEventStatus =
        | Postponed of until:FlukeTime
        | Completed
        | Dismissed
        | ManualPending
        | Session of start:FlukeTime
    
    type CellStatus =
        | Disabled
        | Suggested 
        | Pending
        | Missed
        | MissedToday
        | EventStatus of CellEventStatus
        
    type CellAddress =
        { Task: Task
          Date: FlukeDate }
        
    type Cell = Cell of address:CellAddress * status:CellStatus
    
    type Comment = Comment of string
    
    type TaskComment = TaskComment of task:Task * comment:Comment
    
    type CellStatusEntry = CellStatusEntry of address:CellAddress * status:CellEventStatus
    
    type CellComment = CellComment of address:CellAddress * comment:Comment
    let ofCellComment = fun (CellComment (address, comment)) -> address, comment
    
    type CellSession = CellSession of address:CellAddress * start:FlukeTime
    let ofCellSession = fun (CellSession (address, start)) -> address, start
    
        
    type CellEvent =
        | StatusEvent of CellStatusEntry
        | CommentEvent of CellComment
        | SessionEvent of CellSession
           
        
    type TaskOrderPriority =
        | First
        | LessThan of Task
        | Last
        
    type TaskOrderEntry =
        { Task: Task
          Priority: TaskOrderPriority }
        
    type Lane = Lane of task:Task * cells:Cell list
    
    type TaskPriority =
        | Low1
        | Low2
        | Low3
        | Medium4
        | Medium5
        | Medium6
        | High7
        | High8
        | High9
        | Critical10
    
    
module Rendering =
    open Model
    
    let getDateSequence (paddingLeft, paddingRight) (cellDates: FlukeDate list) =
        let dates = cellDates |> List.map (fun x -> x.DateTime)
            
        let minDate =
            dates
            |> List.min
            |> fun x -> x.AddDays -(float paddingLeft)
            
        let maxDate =
            dates
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
        
        
    type LaneCellRenderState =
        | WaitingFirstEvent
        | WaitingEvent
        | DayMatch
        | Counting of int
        
    type LaneCellRenderOutput =
        | EmptyCell
        | StatusCell of CellStatus
        | TodayCell
        
    let createCellStatusEntries task (events: (FlukeDate * CellEventStatus) list) =
        events |> List.map (fun (date, eventStatus) -> CellStatusEntry ({ Task = task; Date = date }, eventStatus))
        
    let createCellComment task date comment =
        CellComment ({ Task = task; Date = date }, Comment comment)
        
    let isLate now time =
           now.Hour > time.Hour
        || now.Hour = time.Hour && now.Minute >= time.Minute
        
    let renderLane (now: FlukeDateTime) dateSequence task (cellStatusEntries: CellStatusEntry list) =
            
        let (|BeforeToday|Today|AfterToday|) (now: FlukeDate, date: FlukeDate) =
            match now.DateTime |> date.DateTime.CompareTo with
            | n when n < 0 -> BeforeToday
            | n when n = 0 -> Today
            | _ -> AfterToday
            
        let cellStatusEventsByDate =
            cellStatusEntries
            |> List.map (fun (CellStatusEntry (address, status)) -> address.Date, status)
            |> Map.ofList
            
        let rec loop renderState = function
            | date :: tail ->
                let event = cellStatusEventsByDate |> Map.tryFind date
                    
                let status, renderState =
                    match event with
                    | Some cellEvent ->
                        let renderState =
                            match cellEvent, (now.Date, date) with
                            | (Postponed _ | ManualPending), BeforeToday -> WaitingEvent
                            | _,                             _           -> Counting 1
                            
                        StatusCell (EventStatus cellEvent), renderState
                        
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
                        | Recurrency (Offset offset) ->
                            let days =
                                match offset with
                                | Days days -> days
                                | Weeks weeks -> weeks * 7
                                | Months months -> months * 28
                                
                            let renderState =
                                match renderState with
                                | Counting count when count = days -> DayMatch
                                | _ -> renderState
                                
                            getStatus renderState
                            
                        | Recurrency (Fixed recurrencyList) ->
                            let isDateMatched =
                                recurrencyList
                                |> List.map (function
                                    | Weekly dayOfWeek -> dayOfWeek = date.DateTime.DayOfWeek
                                    | Monthly day -> day = date.Day
                                    | Yearly (day, month) -> day = date.Day && month = date.Month
                                )
                                |> List.exists id
                                
                            match renderState, (now.Date, date) with
                            | WaitingFirstEvent, BeforeToday                     -> EmptyCell, WaitingFirstEvent
                            | _,                 Today        when isDateMatched -> StatusCell Pending, Counting 1
                            | WaitingFirstEvent, Today                           -> EmptyCell, Counting 1
                            | _,                 _            when isDateMatched -> getStatus WaitingEvent
                            | _,                 _                               -> getStatus renderState
                            
                        | Manual suggested ->
                            match renderState, (now.Date, date) with
                            | WaitingFirstEvent, Today when suggested && task.PendingAfter = midnight -> StatusCell Suggested, Counting 1
                            | WaitingFirstEvent, Today when suggested                                 -> TodayCell, Counting 1
                            | WaitingFirstEvent, Today                                                -> StatusCell Suggested, Counting 1
                            | _, _ -> 
                                let status, renderState =
                                    getStatus renderState

                                let status =
                                    match status with
                                    | EmptyCell when suggested -> StatusCell Suggested
                                    | TodayCell                -> StatusCell Pending
                                    | status                   -> status
                                    
                                status, renderState
                                
                let status =
                    match status with
                    | EmptyCell -> Disabled
                    | StatusCell status -> status
                    | TodayCell ->
                        match now.Time, task.PendingAfter, task.MissedAfter with
                        | now, _, missedAfter when missedAfter <> midnight && isLate now missedAfter -> MissedToday
                        | now, pendingAfter, _ when isLate now pendingAfter -> Pending
                        | _ -> Suggested
                
                (date, status) :: loop renderState tail
            | [] -> []
            
        let cells =
            loop WaitingFirstEvent dateSequence
            |> List.map (fun (date, cellStatus) -> Cell ({ Date = date; Task = task }, cellStatus)) 
        Lane (task, cells)

        
module Sorting =
    open Model
    
    let getManualSortedTaskList taskOrderList =
        let result = List<Task> ()
        
        let taskOrderList =
            taskOrderList
            |> Seq.rev
            |> Seq.distinctBy (fun x -> x.Task)
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
        
    let applyManualOrder taskOrderList lanes =
        let tasks = lanes |> List.map (fun (Lane (task, _)) -> task)
        let tasksSet = tasks |> Set.ofList
        let orderEntriesOfTasks = taskOrderList |> List.filter (fun orderEntry -> tasksSet.Contains orderEntry.Task)
        
        let tasksWithOrderEntrySet =
            orderEntriesOfTasks
            |> List.map (fun x -> x.Task)
            |> Set.ofList
        
        let tasksWithoutOrderEntry = tasks |> List.filter (fun task -> not (tasksWithOrderEntrySet.Contains task))
        let orderEntriesMissing = tasksWithoutOrderEntry |> List.map (fun task -> { Task = task; Priority = Last })
        let newTaskOrderList = orderEntriesMissing @ orderEntriesOfTasks
            
        let taskIndexMap =
            newTaskOrderList
            |> getManualSortedTaskList
            |> List.mapi (fun i task -> task, i)
            |> Map.ofList
            
        lanes |> List.sortBy (fun (Lane (task, _)) -> taskIndexMap.[task])
        
    let sortLanesByFrequency lanes =
        lanes
        |> List.sortBy (fun (Lane (_, cells)) ->
            cells
            |> List.filter (function Cell (_, (Disabled | Suggested)) -> true | _ -> false)
            |> List.length
        )
        
    let sortLanesByIncomingRecurrency today lanes =
        lanes
        |> List.sortBy (fun (Lane (_, cells)) ->
            cells
            |> List.exists (fun (Cell (address, status)) -> address.Date = today && status = Disabled)
            |> function
                | true ->
                    cells
                    |> List.tryFindIndex (fun (Cell (_, status)) -> status = Pending)
                    |> Option.defaultValue cells.Length
                | false -> cells.Length
        )
        
    type LaneSortType =
        | TaskOrderList
        | DefaultSort
        
    let sortLanesByTimeOfDay (now: FlukeDateTime) taskOrderList lanes =
        let (|PostponedTemp|Postponed|WasPostponed|None|) = function
            | Postponed until when until = midnight -> Postponed
            | Postponed until when Rendering.isLate now.Time until -> WasPostponed
            | Postponed _ -> PostponedTemp
            | _ -> None
        
        let order =
            [ (function MissedToday,               _                             -> true | _ -> false), TaskOrderList
              (function EventStatus ManualPending, _                             -> true | _ -> false), TaskOrderList
              (function (EventStatus WasPostponed
                       | Pending),                 _                             -> true | _ -> false), TaskOrderList
              (function EventStatus PostponedTemp, _                             -> true | _ -> false), TaskOrderList
              (function Suggested,                 { Scheduling = Recurrency _ } -> true | _ -> false), TaskOrderList
              (function Suggested,                 { Scheduling = Manual true }  -> true | _ -> false), TaskOrderList
              (function EventStatus Postponed,     _                             -> true | _ -> false), TaskOrderList
              (function EventStatus Completed,     _                             -> true | _ -> false), DefaultSort
              (function EventStatus Dismissed,     _                             -> true | _ -> false), DefaultSort
              (function Disabled,                  { Scheduling = Recurrency _ } -> true | _ -> false), DefaultSort
              (function Suggested,                 { Scheduling = Manual false } -> true | _ -> false), DefaultSort
              (function _,                         _                             -> true)             , DefaultSort ]
        
        let getGroup task (Cell (_, status)) =
            order
            |> List.map (Tuple2.mapFst (fun orderFn -> orderFn (status, task)))
            |> List.indexed
            |> List.filter (snd >> fst)
            |> List.map (Tuple2.mapSnd snd)
            |> List.head
            
        lanes
        |> List.indexed
        |> List.groupBy (fun (_, Lane (task, cells)) ->
            cells
            |> List.filter (fun (Cell (address, _)) -> address.Date = now.Date)
            |> List.map (getGroup task)
            |> List.minBy fst
        )
        |> List.collect (fun ((groupIndex, sortType), indexedLanes) ->
            match sortType with
            | TaskOrderList ->
                indexedLanes
                |> List.map snd
                |> applyManualOrder taskOrderList
                |> List.indexed
            | DefaultSort -> indexedLanes
            |> List.map (fun (laneIndex, lane) -> (groupIndex * 1000) + laneIndex, lane)
        )
        |> List.sortBy fst
        |> List.map snd
        
module Temp =
    
    type Mode =
        | Navigation
        | Selection
        | Editing
    


