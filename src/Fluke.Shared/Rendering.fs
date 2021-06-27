namespace Fluke.Shared

open System
open Domain.Model
open Domain.UserInteraction
open Fluke.Shared.Domain.State


module Rendering =
    let getDateSequence (paddingLeft, paddingRight) (cellDates: FlukeDate list) =

        let rec dateLoop (date: DateTime) (maxDate: DateTime) =
            seq {
                if date <= maxDate then
                    yield date
                    yield! dateLoop (date.AddDays 1.) maxDate
            }

        let dates =
            cellDates
            |> List.map FlukeDate.DateTime
            |> List.sort
            |> List.toArray

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

    let renderTaskStatusMap dayStart position dateSequence taskState =
        let firstDateRange = FlukeDateTime.Create (dateSequence |> List.head, dayStart)
        let lastDateRange = FlukeDateTime.Create (dateSequence |> List.last, dayStart)

//        printfn $"renderTaskStatusMap
//        dayStart={dayStart|>FlukeTime.Stringify}
//        dateSequence={dateSequence|>List.map FlukeDate.Stringify}
//        taskState={taskState}
//        "
//
        let dateSequenceWithEntries =
            let dates =
                taskState.CellStateMap
                |> Map.keys
                |> Seq.map (DateId.Value >> FlukeDate.DateTime)
                |> Seq.sort
                |> Seq.toArray

            match dates with
            | [||] -> dateSequence
            | dates ->
                let firstDate =
                    dates
                    |> Array.head
                    |> min (firstDateRange |> FlukeDateTime.DateTime)
                    |> FlukeDate.FromDateTime

                let lastDate =
                    dates
                    |> Array.last
                    |> max (lastDateRange |> FlukeDateTime.DateTime)
                    |> FlukeDate.FromDateTime

                getDateSequence
                    (0, 0)
                    [
                        firstDate
                        lastDate
                    ]
            |> List.map (fun date -> FlukeDateTime.Create (date, dayStart))

        let rec loop renderState =
            function
            | moment :: tail ->
                let DateId referenceDay as dateId = dateId dayStart moment

                let cellState = taskState.CellStateMap |> Map.tryFind dateId

                let group = dayStart, position, dateId

                let tempStatus, renderState =
                    match cellState with
                    | Some {
                               Status = UserStatus (_user, manualCellStatus) as userStatus
                           } ->
                        let renderState =
                            match manualCellStatus, group with
                            | Postponed (Some _), BeforeToday -> renderState
                            | (Postponed None
                              | Scheduled),
                              BeforeToday -> WaitingEvent
                            | Postponed None, Today -> DayMatch
                            | _ -> Counting 1

                        let cellStatus =
                            match manualCellStatus, group with
                            | Postponed (Some until), Today when
                                position
                                |> FlukeDateTime.GreaterEqualThan dayStart dateId until -> Pending
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
                            let days = RecurrencyOffset.DayCount offset

                            let renderState =
                                match renderState with
                                | Counting count when count = days -> DayMatch
                                | _ -> renderState

                            getStatus renderState

                        | Recurrency (Fixed recurrencyList) ->
                            let isDateMatched =
                                recurrencyList
                                |> List.map
                                    (function
                                    | Weekly dayOfWeek -> dayOfWeek = (referenceDay |> FlukeDate.DateTime).DayOfWeek
                                    | Monthly day -> day = referenceDay.Day
                                    | Yearly (day, month) ->
                                        day = referenceDay.Day
                                        && month = referenceDay.Month)
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
                        | Some missedAfter, _ when
                            position
                            |> FlukeDateTime.GreaterEqualThan dayStart dateId missedAfter -> MissedToday
                        | _, Some pendingAfter when
                            position
                            |> FlukeDateTime.GreaterEqualThan dayStart dateId pendingAfter -> Pending
                        | _, None -> Pending
                        | _ -> Suggested

//                printfn
//                    $"dateId={referenceDay |> FlukeDate.Stringify} moment={moment |> FlukeDateTime.Stringify} status={
//                                                                                                                          status
//                    }"

                (moment, status) :: loop renderState tail
            | [] -> []

        loop WaitingFirstEvent dateSequenceWithEntries
        |> List.filter (fun (moment, _) -> moment >==< (firstDateRange, lastDateRange))
        |> List.map (fun (moment, cellStatus) -> dateId dayStart moment, cellStatus)
        |> Map.ofSeq
