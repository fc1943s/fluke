namespace Fluke.Shared

open Fluke.Shared
open Fluke.Shared.Domain


module View =
    open Model
    open UserInteraction
    open State


    [<RequireQualifiedAccess>]
    type View =
        | Information
        | HabitTracker
        | Priority
        | BulletJournal

    let getDateRange dateSequence =
        let dateRangeStart =
            dateSequence
            |> List.tryHead
            |> Option.map FlukeDate.DateTime

        let dateRangeEnd =
            dateSequence
            |> List.tryLast
            |> Option.map FlukeDate.DateTime

        match dateRangeStart, dateRangeEnd with
        | Some dateRangeStart, Some dateRangeEnd -> Some (dateRangeStart, dateRangeEnd)
        | _ -> None

    let rec filterTaskStateSeq view dateSequence (taskStateSeq: seq<TaskState>) =
        let dateRange = getDateRange dateSequence

        match dateRange, view with
        | _, View.Information ->
            taskStateSeq
            |> Seq.filter
                (function
                | {
                      Task = {
                                 Scheduling = Manual WithoutSuggestion
                             }
                  } -> true
                | _ -> false)
        //                |> List.filter (fun task ->
//                    task.StatusEntries
//                    |> List.filter (function
//                        | TaskStatusEntry (date, _) when date.DateTime >==< dateRange -> true
//                        | _ -> false
//                    )
//                    |> List.tryLast
//                    |> function Some (TaskStatusEntry (_, Dismissed)) -> false | _ -> true
//                )
        | Some (dateRangeStart, dateRangeEnd),
          (View.HabitTracker
          | View.BulletJournal) ->
            taskStateSeq
            |> Seq.filter
                (function
                | {
                      Task = {
                                 Scheduling = Manual WithoutSuggestion
                             }
                  } as taskState ->
                    taskState.CellStateMap
                    |> Map.toSeq
                    |> Seq.exists
                        (fun (dateId, cellState) ->
                            let dateInRange =
                                dateId |> DateId.Value |> FlukeDate.DateTime
                                >==< (dateRangeStart, dateRangeEnd)

                            let hasRelevantData =
                                not cellState.Sessions.IsEmpty
                                || not cellState.Attachments.IsEmpty
                                || cellState.Status <> Disabled

                            dateInRange && hasRelevantData)
                | _ -> true)
        | _, View.Priority ->
            taskStateSeq
            |> Seq.filter
                (function
                | { Task = { Information = Archive _ } } -> false
                | {
                      Task = { Priority = Some priority }
                      Sessions = []
                  } when (Priority.toTag priority) + 1 < 5 -> false
                | { Task = { Scheduling = Manual _ } } -> true
                | _ -> false)
        | _ -> Seq.empty

    let sortLanes
        (input: {| View: View
                   DayStart: FlukeTime
                   Position: FlukeDateTime
                   InformationSet: Set<Information>
                   Lanes: (TaskState * Map<DateId, CellStatus>) list |})
        =
        let lanes =
            input.Lanes
            |> List.sortBy (fun (taskState, _) -> taskState.Task.Name |> TaskName.Value)

        match input.View with
        | View.Information ->
            let lanes =
                lanes
                //                    |> Sorting.applyManualOrder input.TaskOrderList
                |> List.groupBy (fun (taskState, _) -> taskState.Task.Information)
                |> Map.ofSeq

            input.InformationSet
            |> Set.toList
            |> List.map
                (fun information ->
                    let lanes =
                        lanes
                        |> Map.tryFind information
                        |> Option.defaultValue []

                    information, lanes)
            |> List.collect snd
        | View.HabitTracker ->
            lanes
            |> Sorting.sortLanesByFrequency
            |> Sorting.sortLanesByIncomingRecurrency input.DayStart input.Position
            |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position //input.TaskOrderList
        | View.Priority ->
            lanes
            //                |> Sorting.applyManualOrder input.TaskOrderList
            |> List.sortByDescending
                (fun (taskState, _) ->
                    taskState.Task.Priority
                    |> Option.map Priority.toTag
                    |> Option.defaultValue -1)
        | View.BulletJournal -> lanes
