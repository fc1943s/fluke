namespace Fluke.Shared

open System
open System.Collections.Generic
open Suigetsu.Core

module Model =

    [<StructuredFormatDisplay "{Name}">]
    type Area = { Name: string }

    [<StructuredFormatDisplay "{Area}/{Name}">]
    type Project = { Area: Area; Name: string }

    [<StructuredFormatDisplay "{Area}/{Name}">]
    type Resource = { Area: Area; Name: string }

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
        override this.ToString() =
            sprintf "%02i-%02i-%02i" this.Year (int this.Month) this.Day

    [<StructuredFormatDisplay "{Hour}h{Minute}m">]
    type FlukeTime =
        { Hour: int
          Minute: int }
        override this.ToString() =
            sprintf "%02i:%02i" this.Hour this.Minute

    [<StructuredFormatDisplay "{Date} {Time}">]
    type FlukeDateTime =
        { Date: FlukeDate
          Time: FlukeTime }
        override this.ToString() = sprintf "%A %A" this.Date this.Time


    type Task =
        { Name: string
          Information: Information
          Scheduling: TaskScheduling
          PendingAfter: FlukeTime option
          MissedAfter: FlukeTime option
          Priority: TaskPriorityValue
          Duration: int option }

    and TaskScheduling =
        | Manual of TaskManualScheduling
        | Recurrency of TaskRecurrency

    and TaskManualScheduling =
        | WithSuggestion
        | WithoutSuggestion

    and TaskRecurrency =
        | Offset of TaskRecurrencyOffset
        | Fixed of FixedRecurrency list

    and FixedRecurrency =
        | Weekly of DayOfWeek
        | Monthly of day: int
        | Yearly of day: int * month: Month

    and TaskRecurrencyOffset =
        | Days of int
        | Weeks of int
        | Months of int

    and TaskPriorityValue = TaskPriorityValue of value: int

    type Comment = Comment of comment: string

    type DateId = DateId of referenceDay: FlukeDate

    type CellAddress = { Task: Task; DateId: DateId }

    [<RequireQualifiedAccess>]
    type UserColor =
        | Pink
        | Blue

    type User = { Username: string; Color: UserColor }

    // Link: Auto:[Title, Favicon, Screenshot]
    // Image: Embed
    [<RequireQualifiedAccess>]
    type Attachment =
        | Comment of user:User * comment: Comment
        | Link
        | Video
        | Image
        | Attachment of attachment: Attachment

    [<RequireQualifiedAccess>]
    type AttachmentInteraction = | Pin

    [<RequireQualifiedAccess>]
    type InformationInteraction =
        | Attachment of attachment: Attachment
        | Sort of top: Information option * bottom: Information option

    [<RequireQualifiedAccess>]
    type TaskInteraction =
        | Attachment of attachment: Attachment
        | Archive
        | Session
        | Sort of top: Task option * bottom: Task option

    [<RequireQualifiedAccess>]
    type CellInteraction =
        | Attachment of attachment: Attachment
        | StatusChange of cellStatusChange: CellStatusChange
    //            | Sort of top:Cell option * bottom:Cell option
    and [<RequireQualifiedAccess>] CellStatusChange =
        | Postpone of until: FlukeTime option
        | Complete
        | Dismiss
        | Schedule

    [<RequireQualifiedAccess>]
    type Interaction =
        | Information of information: Information * interaction: InformationInteraction
        | Task of task: Task * interaction: TaskInteraction
        | Cell of cellAddress: CellAddress * interaction: CellInteraction

    type UserInteraction = UserInteraction of user: User * moment: FlukeDateTime * interaction: Interaction

    type UserComment_ = UserComment_ of user: User * comment: string









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



    type ManualCellStatus =
        | Postponed of until: FlukeTime option
        | Completed
        | Dismissed
        | ManualPending

    type CellStatus =
        | Disabled
        | Suggested
        | Pending
        | Missed
        | MissedToday
        | UserStatus of user: User * status: ManualCellStatus


    type TaskId = TaskId of informationName: string * taskName: string
    type TaskSession = TaskSession of start: FlukeDateTime
    type TaskStatusEntry = TaskStatusEntry of user: User * moment: FlukeDateTime * manualCellStatus: ManualCellStatus

    type CellState =
        { Status: CellStatus
          CellInteractions: CellInteraction list
          Sessions: TaskSession list }



    type Cell = Cell of address: CellAddress * status: CellStatus
//    type TaskComment = TaskComment of task: Task * comment: UserComment

    type CellStatusEntry = CellStatusEntry of user: User * task: Task * moment: FlukeDateTime * manualCellStatus: ManualCellStatus

//    type CellComment = CellComment of task: Task * moment: FlukeDateTime * comment: UserComment
    type CellSession = CellSession of task: Task * start: FlukeDateTime

//    type InformationComment =
//        { Information: Information
//          Comment: InformationInteraction }

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

    type TaskState =
        { Task: Task
          Sessions: TaskSession list
          UserInteractions: UserInteraction list
          CellInteractions: (FlukeDate * CellInteraction) list
          CellStateMap: Map<DateId, CellState> }

    type OldLane = OldLane of task: TaskState * cells: Cell list




    [<RequireQualifiedAccess>]
    type TreeAccess =
        | Admin of user: User
        | ReadOnly of user: User

    type Tree =
        { Owner: User
          SharedWith: TreeAccess list
          Position: FlukeDateTime
          InformationList: Information list
          TaskStateList: TaskState list }



    type Area with
        static member inline Default = { Name = "<null>" }

    type Project with
        static member inline Default = { Name = "<null>"; Area = Area.Default }

    type Resource with
        static member inline Default = { Name = "<null>"; Area = Area.Default }

    type FlukeDate with
        member this.DateTime =
            DateTime(this.Year, int this.Month, this.Day, 12, 0, 0)

        static member inline FromDateTime(date: DateTime) =
            { Year = date.Year
              Month = Enum.Parse(typeof<Month>, string date.Month) :?> Month
              Day = date.Day }

    let flukeDate year month day =
        { Year = year
          Month = month
          Day = day }

    type FlukeTime with
        static member inline FromDateTime(date: DateTime) =
            { Hour = date.Hour
              Minute = date.Minute }

        member this.GreaterEqualThan time =
            this.Hour > time.Hour
            || this.Hour = time.Hour
               && this.Minute >= time.Minute

    let flukeTime hour minute = { Hour = hour; Minute = minute }

    type FlukeDateTime with
        member this.DateTime =
            DateTime(this.Date.Year, int this.Date.Month, this.Date.Day, this.Time.Hour, this.Time.Minute, 0)

        static member inline FromDateTime(date: DateTime) =
            { Date = FlukeDate.FromDateTime date
              Time = FlukeTime.FromDateTime date }

        member this.GreaterEqualThan (dayStart: FlukeTime) (DateId referenceDay) time =
            let testingAfterMidnight = dayStart.GreaterEqualThan time
            let currentlyBeforeMidnight = this.Time.GreaterEqualThan dayStart

            let newDate =
                if testingAfterMidnight && currentlyBeforeMidnight then
                    referenceDay.DateTime.AddDays 1.
                    |> FlukeDate.FromDateTime
                else
                    referenceDay

            let dateToCompare = { Date = newDate; Time = time }

            this.DateTime >= dateToCompare.DateTime

    let flukeDateTime year month day hour minute =
        { Date = flukeDate year month day
          Time = flukeTime hour minute }

    type Information with
        member this.Name =
            match this with
            | Project project -> project.Name
            | Area area -> area.Name
            | Resource resource -> resource.Name
            | Archive archive -> sprintf "[%s]" archive.Name

        member this.KindName =
            match this with
            | Project _ -> "projects"
            | Area _ -> "areas"
            | Resource _ -> "resources"
            | Archive _ -> "archives"

        member this.Order =
            match this with
            | Project _ -> 1
            | Area _ -> 2
            | Resource _ -> 3
            | Archive _ -> 4


    type Task with
        static member inline Default =
            { Name = "<null>"
              Information = Area Area.Default
              PendingAfter = None
              MissedAfter = None
              Scheduling = Manual WithoutSuggestion
              Priority = TaskPriorityValue 0
              Duration = None }

    let ofLane =
        fun (OldLane (taskState, cells)) -> taskState, cells

    let ofTaskSession = fun (TaskSession start) -> start

    let ofDateId =
        fun (DateId referenceDay) -> referenceDay

    let ofAttachmentComment = function
        | Attachment.Comment (user, comment) -> Some (user, comment)
        | _ -> None

//    let ofUserComment =
//        fun (UserComment (user, comment)) -> user, comment
//
//    let ofTaskComment =
//        fun (TaskComment (task, userComment)) -> task, userComment
//
//    let ofCellComment =
//        fun (CellComment (task, moment, userComment)) -> task, moment, userComment

    let ofCellSession =
        fun (CellSession (task, start)) -> task, start

    let ofTaskPriorityValue = fun (TaskPriorityValue value) -> value

    let ofTaskStatusEntry =
        fun (TaskStatusEntry (user, moment, manualCellStatus)) -> user, moment, manualCellStatus


    let (|BeforeToday|Today|AfterToday|) (dayStart: FlukeTime, position: FlukeDateTime, DateId referenceDay) =
        let dateStart =
            { Date = referenceDay; Time = dayStart }.DateTime

        let dateEnd = dateStart.AddDays 1.

        match position.DateTime with
        | position when position >=< (dateStart, dateEnd) -> Today
        | position when dateStart < position -> BeforeToday
        | _ -> AfterToday

    let (|StartOfMonth|StartOfWeek|NormalDay|) (weekStart, date) =
        match date with
        | { Day = 1 } -> StartOfMonth
        | date when date.DateTime.DayOfWeek = weekStart -> StartOfWeek
        | _ -> NormalDay

    let isToday dayStart position dateId =
        match (dayStart, position, dateId) with
        | Today -> true
        | _ -> false

    let dateId dayStart position =
        match isToday dayStart position (DateId position.Date) with
        | true -> position.Date
        | false ->
            position.Date.DateTime.AddDays -1.
            |> FlukeDate.FromDateTime
        |> DateId

    let taskId (task: Task) = TaskId(task.Information.Name, task.Name)

    let createTaskStatusEntries task cellStatusEntries =
        cellStatusEntries
        |> List.filter (fun (CellStatusEntry (user, task', moment, manualCellStatus)) -> task' = task)
        |> List.map (fun (CellStatusEntry (user, task', moment, entries)) -> TaskStatusEntry(user, moment, entries))
        |> List.sortBy (fun (TaskStatusEntry (user, date, _)) -> date)

    let createCellComment dayStart task moment user (comment: string) =
        let cellInteraction =
            CellInteraction.Attachment <| Attachment.Comment (user, Comment comment)

        let cellAddress =
            { Task = task
              DateId = dateId dayStart moment }

        let interaction =
            Interaction.Cell(cellAddress, cellInteraction)

        let userInteraction =
            UserInteraction(user, moment, interaction)

        userInteraction
//        CellComment (task, moment, UserComment (user, comment))



module Rendering =
    open Model

    let getDateSequence (paddingLeft, paddingRight) (cellDates: FlukeDate list) =

        let rec dateLoop (date: DateTime) (maxDate: DateTime) =
            seq {
                if date <= maxDate then
                    yield date
                    yield! dateLoop (date.AddDays 1.) maxDate
            }

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
            |> fun x -> x.AddDays(float paddingRight)

        dateLoop minDate maxDate
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


    let renderLane dayStart (position: FlukeDateTime) (dateSequence: FlukeDate list) (taskState: TaskState) =
        let convertManualCellStatus cellStatusChange =
            match cellStatusChange with
            | CellStatusChange.Complete -> Completed
            | CellStatusChange.Dismiss -> Dismissed
            | CellStatusChange.Postpone until -> Postponed until
            | CellStatusChange.Schedule -> ManualPending

        let cellStatusEventsByDateId =
            taskState.UserInteractions
            |> List.choose (fun (UserInteraction (user, moment, interaction)) ->
                match interaction with
                | Interaction.Cell ({ DateId = (DateId referenceDay) }, CellInteraction.StatusChange statusChange) ->
                    Some(dateId dayStart moment, (user, moment, convertManualCellStatus statusChange))
                | _ -> None)
            |> Map.ofList

        let firstDateRange, lastDateRange =
            //            let x x =
//                let rec loop x = function
//                    | () -> ()
//                loop x
//            let a = x dateSequence
//            a |> ignore

            let firstDateRange =
                dateSequence
                |> List.head
                |> fun date -> { Date = date; Time = dayStart }

            let lastDateRange =
                dateSequence
                |> List.last
                |> fun date -> { Date = date; Time = dayStart }

            firstDateRange, lastDateRange

        let dateSequenceWithEntries =
            let dates =
                cellStatusEventsByDateId
                |> Seq.map (fun (KeyValue ((DateId referenceDay), (user, moment, manualCellStatus))) ->
                    referenceDay.DateTime) // Map.keys
                |> Seq.sort
                |> Seq.toArray

            match dates with
            | [||] -> dateSequence
            | dates ->
                [ dates |> Array.head |> min firstDateRange.DateTime
                  dates |> Array.last |> max lastDateRange.DateTime ]
                |> List.map FlukeDate.FromDateTime
                |> getDateSequence (0, 0)
            |> List.map (fun date -> { Date = date; Time = dayStart })


        let rec loop renderState =
            function
            | moment :: tail ->
                let dateId = dateId dayStart moment

                let cellStatus =
                    cellStatusEventsByDateId |> Map.tryFind dateId

                let group = dayStart, position, dateId

                let status, renderState =
                    match cellStatus with
                    | Some (user, moment, manualCellStatus) ->
                        let renderState =
                            match manualCellStatus, group with
                            | Postponed (Some _), BeforeToday -> renderState
                            | (Postponed None
                              | ManualPending),
                              BeforeToday -> WaitingEvent
                            | Postponed None, Today -> DayMatch
                            | _ -> Counting 1

                        let event =
                            match manualCellStatus, group with
                            | Postponed (Some until), Today when position.GreaterEqualThan dayStart dateId until ->
                                Pending
                            | _ -> UserStatus(user, manualCellStatus)

                        StatusCell event, renderState

                    | None ->
                        let getStatus renderState =
                            match renderState, group with
                            | WaitingFirstEvent, BeforeToday -> EmptyCell, WaitingFirstEvent
                            | DayMatch, BeforeToday -> StatusCell Missed, WaitingEvent
                            | WaitingEvent, BeforeToday -> StatusCell Missed, WaitingEvent

                            | WaitingFirstEvent, Today -> TodayCell, Counting 1
                            | DayMatch, Today -> TodayCell, Counting 1
                            | WaitingEvent, Today -> TodayCell, Counting 1

                            | WaitingFirstEvent, AfterToday -> EmptyCell, WaitingFirstEvent
                            | DayMatch, AfterToday -> StatusCell Pending, Counting 1
                            | WaitingEvent, AfterToday -> StatusCell Pending, Counting 1

                            | Counting count, _ -> EmptyCell, Counting(count + 1)

                        match taskState.Task.Scheduling with
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
                                    | Weekly dayOfWeek -> dayOfWeek = moment.DateTime.DayOfWeek
                                    | Monthly day -> day = moment.Date.Day
                                    | Yearly (day, month) -> day = moment.Date.Day && month = moment.Date.Month)
                                |> List.exists id

                            match renderState, group with
                            | WaitingFirstEvent, BeforeToday -> EmptyCell, WaitingFirstEvent
                            | _, Today when isDateMatched -> TodayCell, Counting 1
                            | WaitingFirstEvent, Today -> EmptyCell, Counting 1
                            | _, _ when isDateMatched -> getStatus WaitingEvent
                            | _, _ -> getStatus renderState

                        | Manual suggestion ->
                            match renderState, group, suggestion with
                            | WaitingFirstEvent, Today, WithSuggestion when taskState.Task.PendingAfter = None ->
                                StatusCell Suggested, Counting 1
                            | WaitingFirstEvent, Today, WithSuggestion -> TodayCell, Counting 1
                            | WaitingFirstEvent, Today, _ -> StatusCell Suggested, Counting 1
                            | _ ->
                                let status, renderState = getStatus renderState

                                let status =
                                    match status, suggestion with
                                    | EmptyCell, WithSuggestion -> StatusCell Suggested
                                    | TodayCell, _ -> StatusCell Pending
                                    | status, _ -> status

                                status, renderState

                let status =
                    match status with
                    | EmptyCell -> Disabled
                    | StatusCell status -> status
                    | TodayCell ->
                        match taskState.Task.MissedAfter, taskState.Task.PendingAfter with
                        | Some missedAfter, _ when position.GreaterEqualThan dayStart dateId missedAfter -> MissedToday

                        | _, Some pendingAfter when position.GreaterEqualThan dayStart dateId pendingAfter -> Pending

                        | _, None -> Pending

                        | _ -> Suggested

                (moment, status) :: loop renderState tail
            | [] -> []

        let cells =
            loop WaitingFirstEvent dateSequenceWithEntries
            |> List.filter (fun (moment, _) -> moment >==< (firstDateRange, lastDateRange))
            |> List.map (fun (moment, cellStatus) ->
                Cell
                    ({ DateId = dateId dayStart moment
                       Task = taskState.Task },
                     cellStatus))

        OldLane(taskState, cells)



module Sorting =
    open Model

    let getManualSortedTaskList (taskOrderList: TaskOrderEntry list) =
        let result = List<Task>()

        let taskOrderList =
            taskOrderList
            |> Seq.rev
            |> Seq.distinctBy (fun x -> x.Task)
            |> Seq.rev
            |> Seq.toList

        for { Priority = priority; Task = task } in taskOrderList do
            match priority, result |> Seq.tryFindIndexBack ((=) task) with
            | TaskOrderPriority.First, None -> result.Insert(0, task)
            | TaskOrderPriority.Last, None -> result.Add task
            | TaskOrderPriority.LessThan lessThan, None ->
                match result |> Seq.tryFindIndexBack ((=) lessThan) with
                | None ->
                    seq {
                        task
                        lessThan
                    }
                    |> Seq.iter (fun x -> result.Insert(0, x))
                | Some i -> result.Insert(i + 1, task)
            | _ -> ()

        for { Priority = priority; Task = task } in taskOrderList do
            match priority, result |> Seq.tryFindIndexBack ((=) task) with
            | TaskOrderPriority.First, None -> result.Insert(0, task)
            | TaskOrderPriority.Last, None -> result.Add task
            | _ -> ()

        result |> Seq.toList

    let applyManualOrder (taskOrderList: TaskOrderEntry list) lanes =
        let tasks =
            lanes
            |> List.map
                (ofLane
                 >> fst
                 >> fun taskState -> taskState.Task)

        let tasksSet = tasks |> Set.ofList

        let orderEntriesOfTasks =
            taskOrderList
            |> List.filter (fun orderEntry -> tasksSet.Contains orderEntry.Task)

        let tasksWithOrderEntrySet =
            orderEntriesOfTasks
            |> List.map (fun x -> x.Task)
            |> Set.ofList

        let tasksWithoutOrderEntry =
            tasks
            |> List.filter (fun task -> not (tasksWithOrderEntrySet.Contains task))

        let orderEntriesMissing =
            tasksWithoutOrderEntry
            |> List.map (fun task ->
                { Task = task
                  Priority = TaskOrderPriority.Last })

        let newTaskOrderList =
            orderEntriesMissing @ orderEntriesOfTasks

        let taskIndexMap =
            newTaskOrderList
            |> getManualSortedTaskList
            |> List.mapi (fun i task -> task, i)
            |> Map.ofList

        lanes
        |> List.sortBy (fun (OldLane (taskState, _)) -> taskIndexMap.[taskState.Task])

    let sortLanesByFrequency lanes =
        lanes
        |> List.sortBy (fun (OldLane (taskState, cells)) ->
            let disabledCellsCount =
                cells
                |> List.filter (function
                    | Cell (_,
                            (Disabled
                            | Suggested)) -> true
                    | _ -> false)
                |> List.length

            disabledCellsCount - taskState.Sessions.Length)

    let sortLanesByIncomingRecurrency dayStart position lanes =
        lanes
        |> List.sortBy (fun (OldLane (_, cells)) ->
            let lowPriority =
                cells
                |> List.exists (fun (Cell (address, status)) ->
                    let today = isToday dayStart position address.DateId
                    match today, status with
                    | true,
                      (Disabled
                      | Suggested) -> true
                    | _ -> false)

            if not lowPriority then
                cells.Length
            else
                cells
                |> List.tryFindIndex (function
                    | Cell (_,
                            (Pending
                            | UserStatus (_, ManualPending))) -> true
                    | _ -> false)
                |> Option.defaultValue cells.Length)

    type LaneSortType =
        | TaskOrderList
        | DefaultSort

    let sortLanesByTimeOfDay dayStart (position: FlukeDateTime) taskOrderList lanes =
        let currentDateId = dateId dayStart position

        let getGroup taskState (Cell (address, status)) =
            let (|PostponedUntil|Postponed|WasPostponed|NotPostponed|) =
                function
                | Postponed None -> Postponed
                | Postponed (Some until) when position.GreaterEqualThan dayStart address.DateId until -> WasPostponed
                | Postponed _ -> PostponedUntil
                | _ -> NotPostponed

            let getSessionsTodayCount (cellStateMap: Map<DateId, CellState>) =
                cellStateMap
                |> Map.tryFind currentDateId
                |> Option.map (fun cellState -> cellState.Sessions)
                |> Option.defaultValue []
                |> fun sessions -> sessions.Length

            let (|SchedulingRecurrency|ManualWithSuggestion|ManualWithoutSuggestion|HasSessionToday|) (taskState: TaskState) =
                match taskState with
                | { Task = { Scheduling = Recurrency _ } } -> SchedulingRecurrency
                | { CellStateMap = cellStateMap } when getSessionsTodayCount cellStateMap > 0 -> HasSessionToday
                | { Task = { Scheduling = Manual WithSuggestion } } -> ManualWithSuggestion
                | { Task = { Scheduling = Manual WithoutSuggestion } } -> ManualWithoutSuggestion


            let groupsIndexList =
                [ (function
                  | MissedToday, _ -> Some TaskOrderList
                  | _ -> None)
                  (function
                  | UserStatus (user, ManualPending), _ -> Some TaskOrderList
                  | _ -> None)
                  (function
                  | ((UserStatus (_, WasPostponed))
                    | Pending),
                    _ -> Some TaskOrderList
                  | _ -> None)
                  (function
                  | UserStatus (user, PostponedUntil), _ -> Some TaskOrderList
                  | _ -> None)
                  (function
                  | Suggested, SchedulingRecurrency -> Some TaskOrderList
                  | _ -> None)
                  (function
                  | Suggested, ManualWithSuggestion -> Some TaskOrderList
                  | _ -> None)
                  (function
                  | UserStatus (user, Completed), _ -> Some DefaultSort
                  | _ -> None)
                  (function
                  | UserStatus (user, Dismissed), _ -> Some DefaultSort
                  | _ -> None)
                  (function
                  | UserStatus (user, Postponed), _ -> Some TaskOrderList
                  | _ -> None)
                  (function
                  | _, HasSessionToday -> Some DefaultSort
                  | _ -> None)
                  //                  (function Disabled,                                   SchedulingRecurrency    -> Some DefaultSort   | _ -> None)
                  //                  (function Suggested,                                  ManualWithoutSuggestion -> Some DefaultSort   | _ -> None)
                  (function
                  | _ -> Some DefaultSort) ]

            groupsIndexList
            |> List.map (fun orderFn -> orderFn (status, taskState))
            |> List.indexed
            |> List.choose (function
                | groupIndex, Some sortType -> Some(groupIndex, sortType)
                | _, None -> None)
            |> List.head

        lanes
        |> List.indexed
        |> List.groupBy (fun (_, (OldLane (taskState, cells))) ->
            cells
            |> List.filter (fun (Cell (address, _)) -> isToday dayStart position address.DateId)
            |> List.map (getGroup taskState)
            |> List.minBy fst)
        |> List.collect (fun ((groupIndex, sortType), indexedLanes) ->
            match sortType with
            | TaskOrderList ->
                indexedLanes
                |> List.map snd
                |> applyManualOrder taskOrderList
                |> List.indexed
            | DefaultSort -> indexedLanes
            |> List.map (fun (laneIndex, lane) -> (groupIndex * 1000) + laneIndex, lane))
        |> List.sortBy fst
        |> List.map snd

module Temp =

    type Mode =
        | Navigation
        | Selection
        | Editing
