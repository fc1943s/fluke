namespace Fluke.Shared

open System
open System.Collections.Generic
open FSharpPlus

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
        
    type InformationType =
        | Project of Project
        | Area of Area
        | Resource of Resource
        | Archive of InformationType
            
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
    let midnight = flukeTime 0 0
    
    [<StructuredFormatDisplay("{Date} {Time}")>]
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
          InformationType: InformationType
          Scheduling: TaskScheduling
          PendingAfter: FlukeTime
          MissedAfter: FlukeTime
          Duration: int option }
        static member inline Default =
            { Name = "<null>"
              InformationType = Area Area.Default 
              PendingAfter = midnight
              MissedAfter = midnight
              Scheduling = Manual false
              Duration = None }
        
    // TODO: this is overlapping with CellComment
    type TaskComment =
        { Task: Task
          Date: FlukeDate
          Comment: string }
        
    type CellEventStatus =
        | Postponed of FlukeTime
        | Completed
        | Dropped
        | ManualPending
    
    type CellStatus =
        | Disabled
        | Suggested 
        | Pending
        | Missed
        | EventStatus of CellEventStatus
        
    type Cell =
        { Date: FlukeDate
          Task: Task
          Status: CellStatus }
        
    type CellEvent =
        { Cell: Cell
          Status: CellEventStatus }
    
    type CellComment =
        { Cell: Cell
          Comment: string }
        
    
    type TaskOrderPriority =
        | First
        | LessThan of Task
        | Last
        
    type TaskOrderEntry =
        { Task: Task
          Priority: TaskOrderPriority }
        
    type Lane = Lane of task:Task * cells:Cell list
    
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
        let tasksSet =
            lanes
            |> List.map (fun (Lane (task, _)) -> task)
            |> Set.ofList
            
        let activeTaskOrderList = taskOrderList |> List.filter (fun orderEntry -> tasksSet.Contains orderEntry.Task)
        
        let filteredTaskOrderSet =
            activeTaskOrderList
            |> List.map (fun x -> x.Task)
            |> Set.ofList
            
        let remainingTaskOrderList =
            tasksSet
            |> Set.filter (fun task -> filteredTaskOrderSet.Contains task |> not)
            |> Set.toList
            |> List.map (fun task -> { Task = task; Priority = Last })
        
        let newTaskOrderList = remainingTaskOrderList @ activeTaskOrderList
            
        let indexMap =
            newTaskOrderList
            |> getManualSortedTaskList
            |> List.mapi (fun i task -> task, i)
            |> Map.ofList
            
        lanes |> List.sortBy (fun (Lane (task, _)) -> indexMap.[task])
        
        
    let applyPendingManualOrder today taskOrderList lanes =
        let lanesMap =
            lanes
            |> List.groupBy (fun (Lane (_, cells)) ->
                match cells |> List.tryFind (fun cell -> cell.Date = today) with
                | Some { Status = EventStatus ManualPending } -> Some (EventStatus ManualPending)
                | Some { Status = Pending } -> Some Pending
                | _ -> None
            )
            |> Map.ofList
            
        let getLaneGroup status =
            lanesMap
            |> Map.tryFind status
            |> Option.defaultValue []
            
        [ getLaneGroup (Some (EventStatus ManualPending)) |> applyManualOrder taskOrderList
          getLaneGroup (Some Pending) |> applyManualOrder taskOrderList
          getLaneGroup None ]
        |> List.concat
        
        
    let sortLanesByFrequency lanes =
        lanes
        |> List.sortBy (fun (Lane (_, cells)) ->
            cells
            |> List.filter (fun x -> x.Status = Disabled || x.Status = Suggested)
            |> List.length
        )
        
    let sortLanesByIncomingRecurrency today lanes =
        lanes
        |> List.sortBy (fun (Lane (_, cells)) ->
            cells
            |> List.exists (fun x -> x.Date = today && x.Status = Disabled)
            |> function
                | true ->
                    cells
                    |> List.tryFindIndex (fun x -> x.Status = Pending)
                    |> Option.defaultValue cells.Length
                | false -> cells.Length
        )
        
    
    let sortLanesByToday today lanes =
        let order =
            [ function EventStatus ManualPending, _                             -> true | _ -> false
              function Pending,                   _                             -> true | _ -> false
              function Suggested,                 { Scheduling = Recurrency _ } -> true | _ -> false
              function Suggested,                 { Scheduling = Manual true }  -> true | _ -> false
              function EventStatus (Postponed _), _                             -> true | _ -> false
              function EventStatus Completed,     _                             -> true | _ -> false
              function EventStatus Dropped,       _                             -> true | _ -> false
              function Disabled,                  { Scheduling = Recurrency _ } -> true | _ -> false
              function Suggested,                 { Scheduling = Manual false } -> true | _ -> false
              function _,                         _                             -> true ]
        
        let getIndex task (cell: Cell) =
            order
            |> List.map (fun v -> v (cell.Status, task))
            |> List.indexed
            |> List.filter snd
            |> List.map fst
            |> List.head
        
        lanes
        |> List.indexed
        |> List.sortBy (fun (i, Lane (task, cells)) ->
            cells
            |> List.filter (fun cell -> cell.Date = today)
            |> List.map (getIndex task)
            |> List.map ((*) 1000)
            |> List.map ((+) i)
        )
        |> List.map snd
    
    
module LaneRendering =
    open Model
    
    type LaneCellRenderState =
        | WaitingFirstEvent
        | WaitingEvent
        | DayMatch
        | Counting of int
        
    type LaneCellRenderOutput =
        | EmptyCell
        | StatusCell of CellStatus
        | TodayCell
        
        
    let createCellEvents task events =
        events
        |> List.map (fun (date, status) ->
            { Cell = { Task = task
                       Date = date
                       Status = Disabled }
              Status = status }
        )
        
        
    let renderLane (now: FlukeDateTime) dateSequence task (cellEvents: CellEvent list) =
            
        let (|BeforeToday|Today|AfterToday|) (now: FlukeDate, date: FlukeDate) =
            match now.DateTime |> date.DateTime.CompareTo with
            | n when n < 0 -> BeforeToday
            | n when n = 0 -> Today
            | _ -> AfterToday
                
        let cellEventsByDate =
            cellEvents
            |> List.map (fun x -> x.Cell.Date, x)
            |> Map.ofList
            
        let rec loop renderState = function
            | date :: tail ->
                let event = cellEventsByDate |> Map.tryFind date
                    
                let status, renderState =
                    match event with
                    | Some cellEvent ->
                        let renderState =
                            match cellEvent.Status, (now.Date, date) with
                            | (Postponed _ | ManualPending), BeforeToday -> WaitingEvent
                            | _,                             _           -> Counting 1
                            
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
                        let isPendingNow =
                               now.Time.Hour > task.PendingAfter.Hour
                            || now.Time.Hour = task.PendingAfter.Hour && now.Time.Minute >= task.PendingAfter.Minute
                
                        match isPendingNow with
                        | true -> Pending
                        | false -> Suggested
                
                (date, status) :: loop renderState tail
            | [] -> []
            
        let cells =
            loop WaitingFirstEvent dateSequence
            |> List.map (fun (date, status) ->
                { Date = date
                  Task = task
                  Status = status }
            )
        Lane (task, cells)
        
module Temp =
    
    type Mode =
        | Navigation
        | Selection
        | Editing
    



