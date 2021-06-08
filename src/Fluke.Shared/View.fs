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

    let rec filterTaskStateSeq view dateSequence (taskStateSeq: seq<TaskState>) =
        let dateRangeStart =
            dateSequence
            |> List.tryHead
            |> Option.map FlukeDate.DateTime

        let dateRangeEnd =
            dateSequence
            |> List.tryLast
            |> Option.map FlukeDate.DateTime

        match dateRangeStart, dateRangeEnd, view with
        | _, _, View.Information ->
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
        | Some dateRangeStart,
          Some dateRangeEnd,
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
                            dateId |> DateId.Value |> FlukeDate.DateTime
                            >==< (dateRangeStart, dateRangeEnd)
                            && (cellState.Attachments
                                |> List.exists
                                    (function
                                    | Attachment.Comment _ -> true
                                    | _ -> false)
                                || cellState.Status <> Disabled))
                    || taskState.Sessions
                       |> List.exists
                           (fun (TaskSession (start, _, _)) ->
                               (start.Date |> FlukeDate.DateTime)
                               >==< (dateRangeStart, dateRangeEnd))
                | _ -> true)
        | _, _, View.Priority ->
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
                   InformationStateList: InformationState list // TaskOrderList: TaskOrderEntry list
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
                |> Map.ofList

            input.InformationStateList
            |> List.map
                (fun informationState ->
                    let lanes =
                        lanes
                        |> Map.tryFind informationState.Information
                        |> Option.defaultValue []

                    informationState.Information, lanes)
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
