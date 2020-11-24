namespace Fluke.Shared

open FSharpPlus
open Fluke.Shared.Domain


module View =
    open Model
    open UserInteraction
    open State


    [<RequireQualifiedAccess>]
    type View =
        | HabitTracker
        | Priority
        | BulletJournal
        | Information

    let rec filterTaskStateList view dateRange (taskStateList: TaskState list) =
        match view with
        | View.HabitTracker
        | View.BulletJournal ->
            taskStateList
            |> List.filter (function
                | { Task = { Scheduling = Manual WithoutSuggestion } } as taskState ->
                    taskState.CellStateMap
                    |> Map.toSeq
                    |> Seq.exists (fun ((DateId referenceDay), cellState) ->
                        referenceDay.DateTime
                        >==< dateRange
                        && (cellState.Attachments
                            |> List.exists (function
                                | Attachment.Comment _ -> true
                                | _ -> false)
                            || cellState.Status <> Disabled))
                    || taskState.Sessions
                       |> List.exists (fun (TaskSession (start, _, _)) -> start.Date.DateTime >==< dateRange)
                | _ -> true)
        | View.Priority ->
            taskStateList
            |> List.filter (function
                | { Task = { Information = Archive _ } } -> false
                | { Task = { Priority = Some priority }; Sessions = [] } when (Priority.toTag priority) + 1 < 5 -> false
                | { Task = { Scheduling = Manual _ } } -> true
                | _ -> false)
        | View.Information ->
            taskStateList
            |> List.filter (function
                | { Task = { Scheduling = Manual WithoutSuggestion } } -> true
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

    let sortLanes (input: {| View: View
                             DayStart: FlukeTime
                             Position: FlukeDateTime
                             InformationStateList: InformationState list // TaskOrderList: TaskOrderEntry list
                             Lanes: (TaskState * (CellAddress * CellStatus) list) list |}) =
        match input.View with
        | View.HabitTracker ->
            input.Lanes
            |> Sorting.sortLanesByFrequency
            |> Sorting.sortLanesByIncomingRecurrency input.DayStart input.Position
            |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position //input.TaskOrderList
        | View.Priority ->
            input.Lanes
            //                |> Sorting.applyManualOrder input.TaskOrderList
            |> List.sortByDescending (fun (taskState, _) ->
                taskState.Task.Priority
                |> Option.map Priority.toTag
                |> Option.defaultValue -1)
        | View.BulletJournal -> input.Lanes
        | View.Information ->
            let lanes =
                input.Lanes
                //                    |> Sorting.applyManualOrder input.TaskOrderList
                |> List.groupBy (fun (taskState, _) -> taskState.Task.Information)
                |> Map.ofList

            input.InformationStateList
            |> List.map (fun informationState ->
                let lanes =
                    lanes
                    |> Map.tryFind informationState.Information
                    |> Option.defaultValue []

                informationState.Information, lanes)
            |> List.collect snd

    let getSessionData (input: {| Username: Username
                                  DayStart: FlukeTime
                                  DateSequence: FlukeDate list
                                  View: View
                                  Position: FlukeDateTime
                                  DatabaseStateMap: Map<DatabaseId, DatabaseState>
                                  SelectedDatabaseIds: Set<DatabaseId> |}) =
        //                                GetLivePosition: unit -> FlukeDateTime
        //            let selectedDatabaseIds =
//                input.State.Session.DatabaseSelection
//                |> Set.map (fun databaseState -> databaseState.Id)

        //
        let databaseStateList =
            input.SelectedDatabaseIds
            |> Set.toList
            |> List.choose (fun databaseId -> input.DatabaseStateMap |> Map.tryFind databaseId)

        let informationStateList =
            databaseStateList
            |> List.collect (fun databaseState ->
                databaseState.InformationStateMap
                |> Map.values
                |> Seq.distinctBy (fun informationState -> informationState.Information.Name)
                |> Seq.toList)

        let taskStateList =
            databaseStateList
            |> List.collect (fun databaseState ->
                databaseState.TaskStateMap
                |> Map.values
                |> Seq.toList
                |> List.map (fun taskState ->
                    let sessionsMap =
                        taskState.Sessions
                        |> List.map (fun (TaskSession (start, duration, breakDuration) as session) ->
                            let dateId = dateId input.DayStart start
                            dateId, session)
                        |> List.groupBy fst
                        |> Map.ofList
                        |> Map.mapValues (List.map snd)

                    let newCellStateMap =
                        sessionsMap
                        |> Map.keys
                        |> Seq.map (fun dateId ->
                            let cellState =
                                taskState.CellStateMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue
                                    {
                                        Status = Disabled
                                        Selected = Selection false
                                        Sessions = []
                                        Attachments = []
                                    }

                            let newSessions =
                                sessionsMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue []
                                |> List.append cellState.Sessions

                            dateId, { cellState with Sessions = newSessions })
                        |> Map.ofSeq

                    { taskState with
                        CellStateMap = mergeCellStateMap taskState.CellStateMap newCellStateMap
                    }))

        // TODO: this might be needed
        let informationStateMap, taskStateMap =
            ((Map.empty, Map.empty), databaseStateList)
            ||> List.fold (fun (informationStateMap, taskStateMap) databaseState ->
                    match databaseState with
                    | databaseState when hasAccess databaseState.Database input.Username ->
                        let newInformationStateMap =
                            mergeInformationStateMap informationStateMap databaseState.InformationStateMap

                        let newTaskStateMap = mergeTaskStateMap taskStateMap databaseState.TaskStateMap
                        newInformationStateMap, newTaskStateMap
                    | _ -> informationStateMap, taskStateMap)


        let dateRange =
            // TODO: handle
            let head =
                input.DateSequence
                |> List.head
                |> fun x -> x.DateTime

            let last =
                input.DateSequence
                |> List.last
                |> fun x -> x.DateTime

            head, last


        let filteredTaskStateList = filterTaskStateList input.View dateRange taskStateList

        let filteredLanes =
            filteredTaskStateList
            |> List.map (Rendering.renderLane input.DayStart input.Position input.DateSequence)

        //            let taskOrderList = RootPrivateData.databaseData.TaskOrderList // @ RootPrivateData.taskOrderList
//            let taskOrderList = [] // @ RootPrivateData.taskOrderList



        let sortedTaskStateList =
            sortLanes
                {|
                    View = input.View
                    DayStart = input.DayStart
                    Position = input.Position
                    InformationStateList = informationStateList
                    Lanes = filteredLanes
                |}
            |> List.map (fun (taskState, cells) ->
                let newCells =
                    cells
                    |> List.map (fun (address, status) -> address.DateId, status)
                    |> Map.ofList

                taskState, newCells)

        //                    let sortedTaskList =
//                        sortedTaskList
////                        |> List.sortByDescending (fun x -> x.StatusEntries.Length)
//                        |> List.take 50

        let newTaskStateList =
            sortedTaskStateList
            |> List.map (fun (taskState, statusMap) ->
                let newCellStateMap =
                    seq {
                        yield! taskState.CellStateMap |> Map.keys
                        yield! statusMap |> Map.keys
                    }
                    |> Seq.distinct
                    |> Seq.map (fun dateId ->
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
                                    Selected = Selection false
                                    Sessions = []
                                    Attachments = []
                                }

                        dateId, { cellState with Status = newStatus })
                    |> Map.ofSeq

                let newTaskState = { taskState with CellStateMap = newCellStateMap }

                newTaskState)

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

        //                    User = Some input.User
        let newSession =
            {
                InformationStateMap = newInformationStateMap
                TaskStateMap = newTaskStateMap
                TaskList = newTaskList
            }

        //            let newState = { Session = newSession }
//
//            newState
        newSession
