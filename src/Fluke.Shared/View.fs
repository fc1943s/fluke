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

    let rec filterTaskStateList view dateRange (taskStateList: TaskState list) =
        match view with
        | View.Information ->
            taskStateList
            |> List.filter
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
        | View.HabitTracker
        | View.BulletJournal ->
            taskStateList
            |> List.filter
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
                            >==< dateRange
                            && (cellState.Attachments
                                |> List.exists
                                    (function
                                    | Attachment.Comment _ -> true
                                    | _ -> false)
                                || cellState.Status <> Disabled))
                    || taskState.Sessions
                       |> List.exists
                           (fun (TaskSession (start, _, _)) -> (start.Date |> FlukeDate.DateTime) >==< dateRange)
                | _ -> true)
        | View.Priority ->
            taskStateList
            |> List.filter
                (function
                | { Task = { Information = Archive _ } } -> false
                | {
                      Task = { Priority = Some priority }
                      Sessions = []
                  } when (Priority.toTag priority) + 1 < 5 -> false
                | { Task = { Scheduling = Manual _ } } -> true
                | _ -> false)

    let sortLanes
        (input: {| View: View
                   DayStart: FlukeTime
                   Position: FlukeDateTime
                   InformationStateList: InformationState list // TaskOrderList: TaskOrderEntry list
                   Lanes: (TaskState * (CellAddress * CellStatus) list) list |})
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

    let getSessionData
        (input: {| Username: Username
                   DayStart: FlukeTime
                   DateSequence: FlukeDate list
                   FilterTasksByView: bool
                   View: View
                   Position: FlukeDateTime option
                   TaskStateList: TaskState list |})
        =
        let informationStateList =
            input.TaskStateList
            |> List.map (fun taskState -> taskState.Task.Information)
            |> List.distinct
            |> List.map
                (fun information ->
                    {
                        Information = information
                        Attachments = []
                        SortList = []
                    })

        let taskStateList =
            input.TaskStateList
            |> List.map
                (fun taskState ->
                    let sessionsMap =
                        taskState.Sessions
                        |> List.map
                            (fun session ->
                                match session with
                                | TaskSession (start, _duration, _breakDuration) as session ->
                                    let dateId = dateId input.DayStart start
                                    dateId, session)
                        |> List.groupBy fst
                        |> Map.ofList
                        |> Map.mapValues (List.map snd)

                    let newCellStateMap =
                        sessionsMap
                        |> Map.keys
                        |> Seq.map
                            (fun dateId ->
                                let cellState =
                                    taskState.CellStateMap
                                    |> Map.tryFind dateId
                                    |> Option.defaultValue
                                        {
                                            Status = Disabled
                                            Sessions = []
                                            Attachments = []
                                        }

                                let newSessions =
                                    sessionsMap
                                    |> Map.tryFind dateId
                                    |> Option.defaultValue []
                                    |> List.append cellState.Sessions

                                dateId,
                                { cellState with
                                    Sessions = newSessions
                                })
                        |> Map.ofSeq

                    { taskState with
                        CellStateMap = mergeCellStateMap taskState.CellStateMap newCellStateMap
                    })

        let dateSequence =
            match input.View, input.Position with
            | (View.Information
              | View.Priority),
              Some position ->
                [
                    position.Date
                ]
            | _ -> input.DateSequence

        let head =
            dateSequence
            |> List.tryHead
            |> Option.map FlukeDate.DateTime

        let last =
            dateSequence
            |> List.tryLast
            |> Option.map FlukeDate.DateTime

        let newTaskStateList =
            match input.Position, head, last with
            | Some position, Some head, Some last ->
                let dateRange = head, last

                let filteredTaskStateList =
                    if input.FilterTasksByView then
                        filterTaskStateList input.View dateRange taskStateList
                    else
                        taskStateList

                let filteredLanes =
                    filteredTaskStateList
                    |> List.map (Rendering.renderLane input.DayStart position dateSequence)

                //            let taskOrderList = RootPrivateData.databaseData.TaskOrderList // @ RootPrivateData.taskOrderList
                //            let taskOrderList = [] // @ RootPrivateData.taskOrderList

                let sortedTaskStateList =
                    sortLanes
                        {|
                            View = input.View
                            DayStart = input.DayStart
                            Position = position
                            InformationStateList = informationStateList
                            Lanes = filteredLanes
                        |}
                    |> List.map
                        (fun (taskState, cells) ->
                            let newCells =
                                cells
                                |> List.map (fun (address, status) -> address.DateId, status)
                                |> Map.ofList

                            taskState, newCells)

                //                    let sortedTaskList =
                //                        sortedTaskList
                ////                        |> List.sortByDescending (fun x -> x.StatusEntries.Length)
                //                        |> List.take 50

                sortedTaskStateList
                |> List.map
                    (fun (taskState, statusMap) ->
                        let newCellStateMap =
                            seq {
                                yield! taskState.CellStateMap |> Map.keys
                                yield! statusMap |> Map.keys
                            }
                            |> Seq.distinct
                            |> Seq.map
                                (fun dateId ->
                                    let newStatus =
                                        statusMap
                                        |> Map.tryFind dateId
                                        |> Option.defaultValue Disabled

                                    let cellState =
                                        taskState.CellStateMap
                                        |> Map.tryFind dateId
                                        |> Option.defaultValue
                                            {
                                                Status = Disabled
                                                Sessions = []
                                                Attachments = []
                                            }

                                    dateId, { cellState with Status = newStatus })
                            |> Map.ofSeq

                        let newTaskState =
                            { taskState with
                                CellStateMap = newCellStateMap
                            }

                        newTaskState)
            | _ -> []

        let newInformationStateMap =
            informationStateList
            |> List.map (fun informationState -> informationState.Information, informationState)
            |> Map.ofList

        let newTaskStateMap =
            newTaskStateList
            |> List.map (fun taskState -> taskState.Task, taskState)
            |> Map.ofList

        let newTaskList =
            newTaskStateList
            |> List.map (fun taskState -> taskState.Task)

        let newSession =
            {
                InformationStateMap = newInformationStateMap
                TaskStateMap = newTaskStateMap
                TaskList = newTaskList
                UnfilteredTaskCount = taskStateList.Length
            }

        newSession
