namespace Fluke.Shared


module Sorting =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State


    let sortLanesByFrequency lanes =
        lanes
        |> List.sortBy
            (fun (taskState, statusMap) ->
                let disabledCellsCount =
                    statusMap
                    |> Map.values
                    |> Seq.filter
                        (function
                        | CellStatus.Disabled
                        | CellStatus.Suggested -> true
                        | _ -> false)
                    |> Seq.length

                let cellAttachmentCount =
                    taskState.CellStateMap
                    |> Map.values
                    |> Seq.collect (fun cellState -> cellState.AttachmentStateList)
                    |> Seq.length

                let cellSessionCount =
                    taskState.CellStateMap
                    |> Map.values
                    |> Seq.collect (fun cellState -> cellState.SessionList)
                    |> Seq.length

                (disabledCellsCount * 2)
                - cellSessionCount
                - cellAttachmentCount)

    let sortLanesByIncomingRecurrency dayStart position lanes =
        lanes
        |> List.sortBy
            (fun (_, statusMap) ->
                let lowPriority =
                    statusMap
                    |> Map.exists
                        (fun dateId status ->
                            let today = isToday dayStart position dateId

                            match today, status with
                            | true,
                              (CellStatus.Disabled
                              | CellStatus.Suggested) -> true
                            | _ -> false)

                if not lowPriority then
                    statusMap.Count
                else
                    statusMap
                    |> Seq.tryFindIndex
                        (function
                        | KeyValue (DateId referenceDay,
                                    (CellStatus.Pending
                                    | CellStatus.UserStatus (_, ManualCellStatus.Scheduled))) when
                            (referenceDay |> FlukeDate.DateTime) > (position.Date |> FlukeDate.DateTime)
                            ->
                            true
                        | _ -> false)
                    |> Option.defaultValue statusMap.Count)

    type LaneSortType =
        | TaskOrderList
        | DefaultSort

    let sortLanesByTimeOfDay dayStart (position: FlukeDateTime) lanes =
        let currentDateId = dateId dayStart position

        let getGroup taskState (dateId, status) =
            let (|PostponedUntil|Postponed|WasPostponed|NotPostponed|) =
                function
                | ManualCellStatus.Postponed None -> Postponed
                | ManualCellStatus.Postponed (Some until) when
                    position
                    |> FlukeDateTime.GreaterEqualThan dayStart dateId until
                    ->
                    WasPostponed
                | ManualCellStatus.Postponed _ -> PostponedUntil
                | _ -> NotPostponed

            let getSessionsTodayCount (cellStateMap: Map<DateId, CellState>) =
                cellStateMap
                |> Map.tryFind currentDateId
                |> Option.map (fun cellState -> cellState.SessionList)
                |> Option.defaultValue []
                |> fun sessions -> sessions.Length

            let (|SchedulingRecurrency|ManualWithSuggestion|ManualWithoutSuggestion|HasSessionToday|)
                (taskState: TaskState)
                =
                match taskState with
                | { Task = { Scheduling = Recurrency _ } } -> SchedulingRecurrency
                | { CellStateMap = cellStateMap } when getSessionsTodayCount cellStateMap > 0 -> HasSessionToday
                | {
                      Task = { Scheduling = Manual WithSuggestion }
                  } -> ManualWithSuggestion
                | {
                      Task = {
                                 Scheduling = Manual WithoutSuggestion
                             }
                  } -> ManualWithoutSuggestion


            let groupsIndexList =
                [
                    (function
                    | CellStatus.MissedToday, _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (_user, ManualCellStatus.Scheduled), _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | (CellStatus.UserStatus (_, WasPostponed)
                      | CellStatus.Pending),
                      _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (_user, PostponedUntil), _ -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.Suggested, SchedulingRecurrency -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.Suggested, ManualWithSuggestion -> Some TaskOrderList
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (_user, ManualCellStatus.Completed), _ -> Some DefaultSort
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (_user, ManualCellStatus.Dismissed), _ -> Some DefaultSort
                    | _ -> None)
                    (function
                    | CellStatus.UserStatus (_user, Postponed), _ -> Some TaskOrderList
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
            |> List.choose
                (function
                | groupIndex, Some sortType -> Some (groupIndex, sortType)
                | _, None -> None)
            |> List.head

        lanes
        |> List.indexed
        |> List.groupBy
            (fun (_, (taskState, stateMap)) ->
                stateMap
                |> Map.filter (fun dateId _ -> isToday dayStart position dateId)
                |> Seq.map (fun (KeyValue (dateId, status)) -> getGroup taskState (dateId, status))
                |> Seq.minBy fst)
        |> List.collect
            (fun ((groupIndex, _sortType), indexedLanes) ->
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
