namespace Fluke.Shared

open System
open FSharpPlus


module Rendering =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

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
            |> fun x -> x.AddDays (float paddingRight)

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
        //        let convertManualCellStatus cellStatusChange =
//            match cellStatusChange with
//            | CellStatusChange.Complete -> Completed
//            | CellStatusChange.Dismiss -> Dismissed
//            | CellStatusChange.Postpone until -> Postponed until
//            | CellStatusChange.Schedule -> ManualPending

        //        let dateId = dateId dayStart position
//        let cellStatus =
//            taskState.CellStateMap
//            |> Map.tryFind dateId

        //        let cellStatusEventsByDateId =
//            taskUserInteractions
//            |> List.choose (fun (UserInteraction (user, moment, interaction)) ->
//                match interaction with
//                | Cell ({ DateId = (DateId referenceDay) }, CellStatusChange statusChange) ->
//                    Some (dateId dayStart moment, (user, moment, convertManualCellStatus statusChange))
//                | _ -> None)
//            |> Map.ofList

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
                taskState.CellStateMap
                |> Map.keys
                |> Seq.map (fun (DateId referenceDay) -> referenceDay.DateTime)
                |> Seq.sort
                |> Seq.toArray

            match dates with
            | [||] -> dateSequence
            | dates ->
                [
                    dates |> Array.head |> min firstDateRange.DateTime
                    dates |> Array.last |> max lastDateRange.DateTime
                ]
                |> List.map FlukeDate.FromDateTime
                |> getDateSequence (0, 0)
            |> List.map (fun date -> { Date = date; Time = dayStart })


        let rec loop renderState =
            function
            | moment :: tail ->
                let dateId = dateId dayStart moment

                let cellState = taskState.CellStateMap |> Map.tryFind dateId

                let group = dayStart, position, dateId

                let tempStatus, renderState =
                    match cellState with
                    | Some { Status = UserStatus (user, manualCellStatus) as userStatus } ->
                        let renderState =
                            match manualCellStatus, group with
                            | Postponed (Some _), BeforeToday -> renderState
                            | (Postponed None
                              | ManualPending),
                              BeforeToday -> WaitingEvent
                            | Postponed None, Today -> DayMatch
                            | _ -> Counting 1

                        let cellStatus =
                            match manualCellStatus, group with
                            | Postponed (Some until), Today when position.GreaterEqualThan dayStart dateId until ->
                                Pending
                            | _ -> userStatus

                        StatusCell cellStatus, renderState

                    | _ ->
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

                            | Counting count, _ -> EmptyCell, Counting (count + 1)

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
                    match tempStatus with
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
                {
                    DateId = dateId dayStart moment
                    Task = taskState.Task
                },
                cellStatus)

        taskState, cells
