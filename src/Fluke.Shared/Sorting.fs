namespace Fluke.Shared

open Suigetsu.Core


module Sorting =
    open Model
    open Old


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

            let getSessionsTodayCount (cellStateMap: Map<DateId, State.CellState>) =
                cellStateMap
                |> Map.tryFind currentDateId
                |> Option.map (fun cellState -> cellState.Sessions)
                |> Option.defaultValue []
                |> fun sessions -> sessions.Length

            let (|SchedulingRecurrency|ManualWithSuggestion|ManualWithoutSuggestion|HasSessionToday|) (taskState: State.TaskState) =
                match taskState with
                | { Task = { Scheduling = Recurrency _ } } -> SchedulingRecurrency
                | { CellStateMap = cellStateMap } when getSessionsTodayCount cellStateMap > 0 -> HasSessionToday
                | { Task = { Scheduling = Manual WithSuggestion } } -> ManualWithSuggestion
                | { Task = { Scheduling = Manual WithoutSuggestion } } -> ManualWithoutSuggestion


            let groupsIndexList =
                [
                    (function
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
                |> Sorting.applyManualOrder taskOrderList
                |> List.indexed
            | DefaultSort -> indexedLanes
            |> List.map (fun (laneIndex, lane) -> (groupIndex * 1000) + laneIndex, lane))
        |> List.sortBy fst
        |> List.map snd

