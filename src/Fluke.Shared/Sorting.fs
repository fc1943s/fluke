namespace Fluke.Shared

open FSharpPlus


module Sorting =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State


    let sortLanesByFrequency lanes =
        lanes
        |> List.sortBy (fun (taskState, cells) ->
            let disabledCellsCount =
                cells
                |> List.filter (function
                    | _,
                      (CellStatus.Disabled
                      | CellStatus.Suggested) -> true
                    | _ -> false)
                |> List.length

            let taskSessionsCount =
                taskState.CellStateMap
                |> Map.values
                |> Seq.collect (fun cellState -> cellState.Sessions)
                |> Seq.length

            disabledCellsCount - taskSessionsCount)

    let sortLanesByIncomingRecurrency dayStart position lanes =
        lanes
        |> List.sortBy (fun (_, cells) ->
            let lowPriority =
                cells
                |> List.exists (fun (address, status) ->
                    let today = isToday dayStart position address.DateId
                    match today, status with
                    | true,
                      (CellStatus.Disabled
                      | CellStatus.Suggested) -> true
                    | _ -> false)

            if not lowPriority then
                cells.Length
            else
                cells
                |> List.tryFindIndex (function
                    | _,
                      (CellStatus.Pending
                      | CellStatus.UserStatus (_, ManualCellStatus.ManualPending)) -> true
                    | _ -> false)
                |> Option.defaultValue cells.Length)

    type LaneSortType =
        | TaskOrderList
        | DefaultSort

    let sortLanesByTimeOfDay dayStart (position: FlukeDateTime) (*taskOrderList*) lanes =
        let currentDateId = dateId dayStart position

        let getGroup taskState ((address: CellAddress), status) =
            let (|PostponedUntil|Postponed|WasPostponed|NotPostponed|) =
                function
                | ManualCellStatus.Postponed None -> Postponed
                | ManualCellStatus.Postponed (Some until) when position.GreaterEqualThan dayStart address.DateId until ->
                    WasPostponed
                | ManualCellStatus.Postponed _ -> PostponedUntil
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
                [
                    (function
                    | CellStatus.MissedToday, _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (user, ManualCellStatus.ManualPending), _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | ((CellStatus.UserStatus (_, WasPostponed))
                      | CellStatus.Pending),
                      _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (user, PostponedUntil), _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.Suggested, SchedulingRecurrency -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.Suggested, ManualWithSuggestion -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (user, ManualCellStatus.Completed), _ -> Some DefaultSort
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (user, ManualCellStatus.Dismissed), _ -> Some DefaultSort
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (user, Postponed), _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | _, HasSessionToday -> Some DefaultSort
                    | _ -> None)
                    //                  (function Disabled,                                   SchedulingRecurrency    -> Some DefaultSort   | _ -> None)
                    //                  (function Suggested,                                  ManualWithoutSuggestion -> Some DefaultSort   | _ -> None)
                    (function
                    | _ -> Some DefaultSort)
                ]

            groupsIndexList
            |> List.map (fun orderFn -> orderFn (status, taskState))
            |> List.indexed
            |> List.choose (function
                | groupIndex, Some sortType -> Some (groupIndex, sortType)
                | _, None -> None)
            |> List.head

        lanes
        |> List.indexed
        |> List.groupBy (fun (_, (taskState, cells)) ->
            cells
            |> List.filter (fun (address, _) -> isToday dayStart position address.DateId)
            |> List.map (getGroup taskState)
            |> List.minBy fst)
        |> List.collect (fun ((groupIndex, sortType), indexedLanes) ->
            //            match sortType with
//            | TaskOrderList ->
//                indexedLanes
//                |> List.map snd
//                |> Testing.applyManualOrder taskOrderList
//                |> List.indexed
//            | DefaultSort -> indexedLanes
            indexedLanes
            |> List.map (fun (laneIndex, lane) -> (groupIndex * 1000) + laneIndex, lane))
        |> List.sortBy fst
        |> List.map snd
