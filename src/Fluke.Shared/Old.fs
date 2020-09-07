namespace Fluke.Shared

open System.Collections.Generic


module Old =
    open Model
    open Model.State

    //    type X =
//        | SameDay
//        | NextDay




    //    type UserComment_ = UserComment_ of user: User * comment: string
    //    type TaskSession = TaskSession of start: FlukeDateTime * duration: Minute * breakDuration: Minute
    //    type TaskSession = TaskSession of TaskInteraction
    //    type TaskStatusEntry = TaskStatusEntry of user: User * moment: FlukeDateTime * manualCellStatus: ManualCellStatus


    //    type TaskComment = TaskComment of task: Task * comment: UserComment

    type CellStatusEntry = CellStatusEntry of user: User * task: Task * moment: FlukeDateTime * manualCellStatus: ManualCellStatus

    //    type CellComment = CellComment of task: Task * moment: FlukeDateTime * comment: UserComment
    type CellSession = CellSession of task: Task * start: FlukeDateTime * duration: Minute

    //    type InformationComment =
    //        { Information: Information
    //          Comment: InformationInteraction }

    //    type CellEvent =
    //        | StatusEvent of CellStatusEntry
    //        | CommentEvent of CellComment
    //        | SessionEvent of CellSession




    type OldLane = OldLane of task: State.TaskState * cells: Cell list

    //    type TreeData =
//        {
//            GetLivePosition: (unit -> FlukeDateTime)
//            InformationList: Information list
//            TaskOrderList: TaskOrderEntry list
//            TaskStateList: (TaskState * UserInteraction list) list
//        }
//
    and TaskOrderEntry =
        {
            Task: Task
            Priority: TaskOrderPriority
        }

    and [<RequireQualifiedAccess>] TaskOrderPriority =
        | First
        | LessThan of Task
        | Last

    let ofLane (OldLane (taskState, cells)) = taskState, cells

    module Sorting =
        open Model
        open Model.State





        let getManualSortedTaskList (taskOrderList: TaskOrderEntry list) =
            let result = List<Task> ()

            let taskOrderList =
                taskOrderList
                |> Seq.rev
                |> Seq.distinctBy (fun x -> x.Task)
                |> Seq.rev
                |> Seq.toList

            for { Priority = priority; Task = task } in taskOrderList do
                match priority, result |> Seq.tryFindIndexBack ((=) task) with
                | TaskOrderPriority.First, None -> result.Insert (0, task)
                | TaskOrderPriority.Last, None -> result.Add task
                | TaskOrderPriority.LessThan lessThan, None ->
                    match result |> Seq.tryFindIndexBack ((=) lessThan) with
                    | None ->
                        seq {
                            task
                            lessThan
                        }
                        |> Seq.iter (fun x -> result.Insert (0, x))
                    | Some i -> result.Insert (i + 1, task)
                | _ -> ()

            for { Priority = priority; Task = task } in taskOrderList do
                match priority, result |> Seq.tryFindIndexBack ((=) task) with
                | TaskOrderPriority.First, None -> result.Insert (0, task)
                | TaskOrderPriority.Last, None -> result.Add task
                | _ -> ()

            result |> Seq.toList

        let applyManualOrder (taskOrderList: TaskOrderEntry list) lanes =
            let tasks =
                lanes
                |> List.map
                    (ofLane
                     >> fst
                     >> fun taskState -> taskState.Task)

            let tasksSet = tasks |> Set.ofList

            let orderEntriesOfTasks =
                taskOrderList
                |> List.filter (fun orderEntry -> tasksSet.Contains orderEntry.Task)

            let tasksWithOrderEntrySet =
                orderEntriesOfTasks
                |> List.map (fun x -> x.Task)
                |> Set.ofList

            let tasksWithoutOrderEntry =
                tasks
                |> List.filter (fun task -> not (tasksWithOrderEntrySet.Contains task))

            let orderEntriesMissing =
                tasksWithoutOrderEntry
                |> List.map (fun task ->
                    {
                        Task = task
                        Priority = TaskOrderPriority.Last
                    })

            let newTaskOrderList =
                orderEntriesMissing @ orderEntriesOfTasks

            let taskIndexMap =
                newTaskOrderList
                |> getManualSortedTaskList
                |> List.mapi (fun i task -> task, i)
                |> Map.ofList

            lanes
            |> List.sortBy (fun (OldLane (taskState, _)) -> taskIndexMap.[taskState.Task])
