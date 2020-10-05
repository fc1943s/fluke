namespace Fluke.Shared

open FSharpPlus


module View =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State


    [<RequireQualifiedAccess>]
    type View =
        | Calendar
        | Groups
        | Tasks
        | Week

    let rec filterTaskStateList view dateRange (taskStateList: TaskState list) =
        match view with
        | View.Calendar
        | View.Week ->
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
        | View.Groups ->
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
        | View.Tasks ->
            taskStateList
            |> List.filter (function
                | { Task = { Information = Archive _ } } -> false
                | { Task = { Priority = Some priority }; Sessions = [] } when priority.Value < 5 -> false
                | { Task = { Scheduling = Manual _ } } -> true
                | _ -> false)

    let sortLanes (input: {| View: View
                             DayStart: FlukeTime
                             Position: FlukeDateTime
                             InformationStateList: InformationState list // TaskOrderList: TaskOrderEntry list
                             Lanes: (TaskState * (CellAddress * CellStatus) list) list |}) =
        match input.View with
        | View.Calendar ->
            input.Lanes
            |> Sorting.sortLanesByFrequency
            |> Sorting.sortLanesByIncomingRecurrency input.DayStart input.Position
            |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position //input.TaskOrderList
        | View.Groups ->
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
        | View.Tasks ->
            input.Lanes
            //                |> Sorting.applyManualOrder input.TaskOrderList
            |> List.sortByDescending (fun (taskState, _) ->
                taskState.Task.Priority
                |> Option.map (fun x -> x.Value)
                |> Option.defaultValue 0)
        | View.Week -> input.Lanes

    let getSessionData (input: {| User: User
                                  DateSequence: FlukeDate list
                                  View: View
                                  Position: FlukeDateTime
                                  TreeStateMap: Map<TreeId, TreeState>
                                  TreeSelectionIds: Set<TreeId> |}) =
        //                                GetLivePosition: unit -> FlukeDateTime
        //            let treeSelectionIds =
//                input.State.Session.TreeSelection
//                |> Set.map (fun treeState -> treeState.Id)
//
        let treeSelection =
            input.TreeSelectionIds
            |> Set.toList
            |> List.choose (fun treeId -> input.TreeStateMap |> Map.tryFind treeId)

        let informationStateList =
            treeSelection
            |> List.collect (fun treeState ->
                treeState.InformationStateMap
                |> Map.values
                |> Seq.distinctBy (fun informationState -> informationState.Information.Name)
                |> Seq.toList)

        let taskStateList =
            treeSelection
            |> List.collect (fun treeState ->
                treeState.TaskStateMap
                |> Map.values
                |> Seq.toList
                |> List.map (fun taskState ->
                    let sessionsMap =
                        taskState.Sessions
                        |> List.map (fun (TaskSession (start, duration, breakDuration) as session) ->
                            let dateId = dateId input.User.DayStart start
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
            ((Map.empty, Map.empty), treeSelection)
            ||> List.fold (fun (informationStateMap, taskStateMap) treeState ->
                    match treeState with
                    | treeState when hasAccess treeState input.User ->
                        let newInformationStateMap =
                            mergeInformationStateMap informationStateMap treeState.InformationStateMap

                        let newTaskStateMap = mergeTaskStateMap taskStateMap treeState.TaskStateMap
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

        printfn
            "FakeBackend.getSession -> (taskStateList.Length, filteredTaskStateList.Length) = %A"
            (taskStateList.Length, filteredTaskStateList.Length)

        let filteredLanes =
            filteredTaskStateList
            |> List.map (Rendering.renderLane input.User.DayStart input.Position input.DateSequence)

        //            let taskOrderList = RootPrivateData.treeData.TaskOrderList // @ RootPrivateData.taskOrderList
//            let taskOrderList = [] // @ RootPrivateData.taskOrderList



        let sortedTaskStateList =
            sortLanes
                {|
                    View = input.View
                    DayStart = input.User.DayStart
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
