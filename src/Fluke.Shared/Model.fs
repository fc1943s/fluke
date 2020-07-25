namespace Fluke.Shared

open System
open System.Collections.Generic
open Suigetsu.Core

module Model =

    [<StructuredFormatDisplay "{Name}">]
    type Area =
        { Name: string }

    [<StructuredFormatDisplay "{Area}/{Name}">]
    type Project =
        { Area: Area
          Name: string }

    [<StructuredFormatDisplay "{Area}/{Name}">]
    type Resource =
        { Area: Area
          Name: string }

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

    [<StructuredFormatDisplay "{Year}-{Month}-{Day}">]
    type FlukeDate =
        { Year: int
          Month: Month
          Day: int }
        override this.ToString () =
            sprintf "%02i-%02i-%02i" this.Year (int this.Month) this.Day

    [<StructuredFormatDisplay "{Hour}h{Minute}m">]
    type FlukeTime =
        { Hour: int
          Minute: int }
        override this.ToString () =
            sprintf "%02i:%02i" this.Hour this.Minute

    [<StructuredFormatDisplay "{Date} {Time}">]
    type FlukeDateTime =
        { Date: FlukeDate
          Time: FlukeTime }
        override this.ToString () =
            sprintf "%A %A" this.Date this.Time

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

    type TaskManualScheduling =
        | WithSuggestion
        | WithoutSuggestion

    type TaskScheduling =
        | Manual of TaskManualScheduling
        | Recurrency of TaskRecurrency

    type X =
        | SameDay
        | NextDay

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


    type CellEventStatus =
        | Postponed of until:FlukeTime option
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

    [<RequireQualifiedAccess>]
    type UserColor =
        | Pink
        | Blue

    type User =
        { Username: string
          Color: UserColor }

    type DateId = DateId of FlukeDate
    type Comment = Comment of user:User * comment:string
    type TaskSession = TaskSession of start:FlukeDateTime
    type TaskStatusEntry = TaskStatusEntry of date:FlukeDate * eventStatus:CellEventStatus
    type TaskPriorityValue = TaskPriorityValue of value:int

    type CellState =
        { Status: CellStatus
          Comments: Comment list
          Sessions: TaskSession list }

    type Task =
        { Name: string
          Information: Information
          Scheduling: TaskScheduling
          PendingAfter: FlukeTime option
          MissedAfter: FlukeTime option
          Priority: TaskPriorityValue
          StatusEntries: TaskStatusEntry list
          Sessions: TaskSession list
          CellComments: (FlukeDate * Comment) list
          CellStateMap: Map<DateId, CellState>
          Comments: Comment list
          Duration: int option }

    type CellAddress =
        { Task: Task
          Date: FlukeDate }


    type Cell = Cell of address:CellAddress * status:CellStatus
    type TaskComment = TaskComment of task:Task * comment:Comment
    type CellStatusEntry = CellStatusEntry of address:CellAddress * eventStatus:CellEventStatus
    type CellComment = CellComment of address:CellAddress * comment:Comment
    type CellSession = CellSession of address:CellAddress * start:FlukeTime

    type InformationComment =
        { Information: Information
          Comment: Comment }

//    type CellEvent =
//        | StatusEvent of CellStatusEntry
//        | CommentEvent of CellComment
//        | SessionEvent of CellSession

    [<RequireQualifiedAccess>]
    type TaskOrderPriority =
        | First
        | LessThan of Task
        | Last

    type TaskOrderEntry =
        { Task: Task
          Priority: TaskOrderPriority }

    type OldLane = OldLane of task:Task * cells:Cell list




    [<RequireQualifiedAccess>]
    type TreeAccess =
        | Admin of user:User
        | ReadOnly of user:User

    type Tree =
        { Owner: User
          SharedWith: TreeAccess list
          Position: FlukeDateTime
          InformationList: Information list
          TaskList: Task list }



    type Area with
        static member inline Default =
            { Name = "<null>" }

    type Project with
        static member inline Default =
            { Name = "<null>"
              Area = Area.Default }

    type Resource with
        static member inline Default =
            { Name = "<null>"
              Area = Area.Default }

    type FlukeDate with
        member this.DateTime =
            DateTime (this.Year, int this.Month, this.Day, 12, 0, 0)
        static member inline FromDateTime (date: DateTime) =
            { Year = date.Year
              Month = Enum.Parse (typeof<Month>, string date.Month) :?> Month
              Day = date.Day }

    let flukeDate year month day = { Year = year; Month = month; Day = day }

    type FlukeTime with
        static member inline FromDateTime (date: DateTime) =
            { Hour = date.Hour
              Minute = date.Minute }
        member this.GreaterEqualThan time =
               this.Hour > time.Hour
            || this.Hour = time.Hour && this.Minute >= time.Minute

    let flukeTime hour minute = { Hour = hour; Minute = minute }

    type FlukeDateTime with
        member this.DateTime =
            DateTime (this.Date.Year, int this.Date.Month, this.Date.Day, this.Time.Hour, this.Time.Minute, 0)
        static member inline FromDateTime (date: DateTime) =
            { Date = FlukeDate.FromDateTime date
              Time = FlukeTime.FromDateTime date }
        member this.GreaterEqualThan (dayStart: FlukeTime) (date: FlukeDate) time =
            let testingAfterMidnight = dayStart.GreaterEqualThan time
            let currentlyBeforeMidnight = this.Time.GreaterEqualThan dayStart

            let newDate =
                if testingAfterMidnight && currentlyBeforeMidnight
                then date.DateTime.AddDays 1. |> FlukeDate.FromDateTime
                else date

            let dateToCompare = { Date = newDate; Time = time }

            this.DateTime >= dateToCompare.DateTime

    let flukeDateTime year month day hour minute =
        { Date = flukeDate year month day; Time = flukeTime hour minute }

    type Information with
        member this.KindName =
            match this with
            | Project _  -> "projects"
            | Area _     -> "areas"
            | Resource _ -> "resources"
            | Archive _  -> "archives"
        member this.Order =
            match this with
            | Project _  -> 1
            | Area _     -> 2
            | Resource _ -> 3
            | Archive  _ -> 4


    type Task with
        static member inline Default =
            { Name = "<null>"
              Information = Area Area.Default
              PendingAfter = None
              MissedAfter = None
              Scheduling = Manual WithoutSuggestion
              Priority = TaskPriorityValue 0
              StatusEntries = []
              Sessions = []
              CellComments = []
              CellStateMap = Map.empty
              Comments = []
              Duration = None }

    let ofLane = fun (OldLane (task, cells)) -> task, cells
    let ofTaskSession = fun (TaskSession start) -> start
    let ofComment = fun (Comment (user, comment)) -> user, comment
    let ofTaskComment = fun (TaskComment (task, comment)) -> task, comment
    let ofCellComment = fun (CellComment (address, comment)) -> address, comment
    let ofCellSession = fun (CellSession (address, start)) -> address, start
    let ofTaskPriorityValue = fun (TaskPriorityValue value) -> value
    let ofTaskStatusEntry = fun (TaskStatusEntry (date, entries)) -> date, entries


    let createTaskStatusEntries task cellStatusEntries =
        cellStatusEntries
        |> List.filter (fun (CellStatusEntry (address, _)) -> address.Task = task)
        |> List.map (fun (CellStatusEntry (address, entries)) -> TaskStatusEntry (address.Date, entries))
        |> List.sortBy (fun (TaskStatusEntry (date, _)) -> date)

    let createCellComment task date user comment =
        CellComment ({ Task = task; Date = date }, Comment (user, comment))

    let (|BeforeToday|Today|AfterToday|) (dayStart: FlukeTime, position:FlukeDateTime, date:FlukeDate) =
        let dateStart = { Date = date; Time = dayStart }.DateTime
        let dateEnd = dateStart.AddDays 1.

        match position.DateTime with
        | position when position >=< (dateStart, dateEnd) -> Today
        | position when dateStart < position -> BeforeToday
        | _ -> AfterToday

    let (|StartOfMonth|StartOfWeek|NormalDay|) = function
        | { Day = 1 } -> StartOfMonth
        | date when date.DateTime.DayOfWeek = DayOfWeek.Monday -> StartOfWeek
        | _ -> NormalDay

    let isToday dayStart position date =
        match (dayStart, position, date) with
        | Today -> true
        | _ -> false

    let dateId dayStart position =
        match isToday dayStart position position.Date with
        | true -> position.Date
        | false -> position.Date.DateTime.AddDays -1. |> FlukeDate.FromDateTime
        |> DateId

module Rendering =
    open Model

    let getDateSequence (paddingLeft, paddingRight) (cellDates: FlukeDate list) =
        let dates =
            cellDates
            |> Seq.map (fun x -> x.DateTime)
            |> Seq.sort
            |> Seq.toArray

        let minDate =
            dates
            |> Array.head
            |> fun x -> x.AddDays -(float paddingLeft)

        let maxDate =
            dates
            |> Array.last
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


    let renderLane dayStart (position: FlukeDateTime) (dateSequence: FlukeDate list) task =

        let cellStatusEventsByDate =
            task.StatusEntries
            |> List.map ofTaskStatusEntry
            |> Map.ofList

        let firstDateRange = dateSequence |> List.head
        let lastDateRange = dateSequence |> List.last

        let entriesDateSequence =
            let dates =
                cellStatusEventsByDate
                |> Seq.map (fun (KeyValue (k, _)) -> k.DateTime) // Map.keys
                |> Seq.sort
                |> Seq.toArray

            match dates with
            | [||] -> dateSequence
            | dates ->
                [ dates |> Array.head |> min firstDateRange.DateTime
                  dates |> Array.last |> max lastDateRange.DateTime ]
                |> List.map FlukeDate.FromDateTime
                |> getDateSequence (0, 0)


        let rec loop renderState = function
            | date :: tail ->
                let event = cellStatusEventsByDate |> Map.tryFind date

                let status, renderState =
                    match event with
                    | Some cellEvent ->
                        let renderState =
                            match cellEvent, (dayStart, position, date) with
                            | Postponed (Some _),               BeforeToday -> renderState
                            | (Postponed None | ManualPending), BeforeToday -> WaitingEvent
                            | Postponed None,                   Today       -> DayMatch
                            | _                                             -> Counting 1

                        let event =
                            match cellEvent, (dayStart, position, date) with
                            | Postponed (Some until), Today
                                when position.GreaterEqualThan dayStart date until -> Pending
                            | _                                               -> EventStatus cellEvent

                        StatusCell event, renderState

                    | None ->
                        let getStatus renderState =
                            match renderState, (dayStart, position, date) with
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
                                | Days days     -> days
                                | Weeks weeks   -> weeks * 7
                                | Months months -> months * 28

                            let renderState =
                                match renderState with
                                | Counting count when count = days -> DayMatch
                                | _                                -> renderState

                            getStatus renderState

                        | Recurrency (Fixed recurrencyList) ->
                            let isDateMatched =
                                recurrencyList
                                |> List.map (function
                                    | Weekly dayOfWeek    -> dayOfWeek = date.DateTime.DayOfWeek
                                    | Monthly day         -> day = date.Day
                                    | Yearly (day, month) -> day = date.Day && month = date.Month
                                )
                                |> List.exists id

                            match renderState, (dayStart, position, date) with
                            | WaitingFirstEvent, BeforeToday                     -> EmptyCell, WaitingFirstEvent
                            | _,                 Today        when isDateMatched -> StatusCell Pending, Counting 1
                            | WaitingFirstEvent, Today                           -> EmptyCell, Counting 1
                            | _,                 _            when isDateMatched -> getStatus WaitingEvent
                            | _,                 _                               -> getStatus renderState

                        | Manual suggestion ->
                            match renderState, (dayStart, position, date), suggestion with
                            | WaitingFirstEvent, Today, WithSuggestion
                                when task.PendingAfter = None          -> StatusCell Suggested, Counting 1
                            | WaitingFirstEvent, Today, WithSuggestion -> TodayCell, Counting 1
                            | WaitingFirstEvent, Today, _              -> StatusCell Suggested, Counting 1
                            | _                                        ->
                                let status, renderState = getStatus renderState

                                let status =
                                    match status, suggestion with
                                    | EmptyCell, WithSuggestion -> StatusCell Suggested
                                    | TodayCell, _              -> StatusCell Pending
                                    | status,    _              -> status

                                status, renderState

                let status =
                    match status with
                    | EmptyCell -> Disabled
                    | StatusCell status -> status
                    | TodayCell ->
                        match position, task.MissedAfter, task.PendingAfter with
                        | position, Some missedAfter, _
                            when position.GreaterEqualThan dayStart date missedAfter  -> MissedToday
                        | position, _,                Some pendingAfter
                            when position.GreaterEqualThan dayStart date pendingAfter -> Pending
                        | _,   _,                     None                            -> Pending
                        | _                                                           -> Suggested

                (date, status) :: loop renderState tail
            | [] -> []

        let cells =
            loop WaitingFirstEvent entriesDateSequence
            |> List.filter (fun (date, _) -> date >==< (firstDateRange, lastDateRange))
            |> List.map (fun (date, cellStatus) -> Cell ({ Date = date; Task = task }, cellStatus))
        OldLane (task, cells)



module Sorting =
    open Model

    let getManualSortedTaskList (taskOrderList: TaskOrderEntry list) =
        let result = List<Task> ()

        let taskOrderList =
            taskOrderList
            |> Seq.rev
            |> Seq.distinctBy (fun x -> x.Task)
            |> Seq.rev
            |> Seq.toList

        for { Priority = priority; Task = task } in taskOrderList do
            match priority, result |> Seq.tryFindIndexBack ((=) task) with
            | TaskOrderPriority.First, None             -> result.Insert (0, task)
            | TaskOrderPriority.Last, None              -> result.Add task
            | TaskOrderPriority.LessThan lessThan, None ->
                match result |> Seq.tryFindIndexBack ((=) lessThan) with
                | None   -> seq { task; lessThan } |> Seq.iter (fun x -> result.Insert (0, x))
                | Some i -> result.Insert (i + 1, task)
            | _ -> ()

        for { Priority = priority; Task = task } in taskOrderList do
            match priority, result |> Seq.tryFindIndexBack ((=) task) with
            | TaskOrderPriority.First, None -> result.Insert (0, task)
            | TaskOrderPriority.Last, None  -> result.Add task
            | _ -> ()

        result |> Seq.toList

    let applyManualOrder (taskOrderList: TaskOrderEntry list) lanes =
        let tasks = lanes |> List.map (ofLane >> fst)
        let tasksSet = tasks |> Set.ofList
        let orderEntriesOfTasks = taskOrderList |> List.filter (fun orderEntry -> tasksSet.Contains orderEntry.Task)

        let tasksWithOrderEntrySet =
            orderEntriesOfTasks
            |> List.map (fun x -> x.Task)
            |> Set.ofList

        let tasksWithoutOrderEntry = tasks |> List.filter (fun task -> not (tasksWithOrderEntrySet.Contains task))
        let orderEntriesMissing =
            tasksWithoutOrderEntry |> List.map (fun task -> { Task = task; Priority = TaskOrderPriority.Last })
        let newTaskOrderList = orderEntriesMissing @ orderEntriesOfTasks

        let taskIndexMap =
            newTaskOrderList
            |> getManualSortedTaskList
            |> List.mapi (fun i task -> task, i)
            |> Map.ofList

        lanes |> List.sortBy (fun (OldLane (task, _)) -> taskIndexMap.[task])

    let sortLanesByFrequency lanes =
        lanes
        |> List.sortBy (fun (OldLane (_, cells)) ->
            cells
            |> List.filter (function Cell (_, (Disabled | Suggested)) -> true | _ -> false)
            |> List.length
        )

    let sortLanesByIncomingRecurrency dayStart position lanes =
        lanes
        |> List.sortBy (fun (OldLane (_, cells)) ->
            cells
            |> List.exists (fun (Cell (address, status)) -> isToday dayStart position address.Date && status = Disabled)
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

    let sortLanesByTimeOfDay dayStart (position: FlukeDateTime) taskOrderList lanes =

        let getGroup task (Cell (address, status)) =
            let (|PostponedUntil|Postponed|WasPostponed|NotPostponed|) = function
                | Postponed None                                                                    -> Postponed
                | Postponed (Some until) when position.GreaterEqualThan dayStart address.Date until -> WasPostponed
                | Postponed _                                                                       -> PostponedUntil
                | _                                                                                 -> NotPostponed

            let (|SchedulingRecurrency|ManualWithSuggestion|ManualWithoutSuggestion|) = function
                | { Scheduling = Recurrency _ } -> SchedulingRecurrency
                | { Scheduling = Manual WithSuggestion } -> ManualWithSuggestion
                | { Scheduling = Manual WithoutSuggestion } -> ManualWithoutSuggestion

            [ (function MissedToday,                          _                       -> Some TaskOrderList | _ -> None)
              (function EventStatus ManualPending,            _                       -> Some TaskOrderList | _ -> None)
              (function (EventStatus WasPostponed | Pending), _                       -> Some TaskOrderList | _ -> None)
              (function EventStatus PostponedUntil,           _                       -> Some TaskOrderList | _ -> None)
              (function Suggested,                            SchedulingRecurrency    -> Some TaskOrderList | _ -> None)
              (function Suggested,                            ManualWithSuggestion    -> Some TaskOrderList | _ -> None)
              (function EventStatus Postponed,                _                       -> Some TaskOrderList | _ -> None)
              (function EventStatus Completed,                _                       -> Some DefaultSort   | _ -> None)
              (function EventStatus Dismissed,                _                       -> Some DefaultSort   | _ -> None)
              (function Disabled,                             SchedulingRecurrency    -> Some DefaultSort   | _ -> None)
              (function Suggested,                            ManualWithoutSuggestion -> Some DefaultSort   | _ -> None)
              (function _                                                             -> Some DefaultSort) ]
            |> List.map (fun orderFn -> orderFn (status, task))
            |> List.indexed
            |> List.choose (function groupIndex, Some sortType -> Some (groupIndex, sortType) | _, None -> None)
            |> List.head

        lanes
        |> List.indexed
        |> List.groupBy (fun (_, OldLane (task, cells)) ->
            cells
            |> List.filter (fun (Cell (address, _)) -> isToday dayStart position address.Date)
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




