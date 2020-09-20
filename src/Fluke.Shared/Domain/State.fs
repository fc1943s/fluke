namespace Fluke.Shared.Domain

open System


module State =
    open Information
    open UserInteraction

    type State =
        {
            Session: Session
            TaskList: Task list
            InformationStateMap: Map<Information, InformationState>
            TaskStateMap: Map<Task, TaskState>
            TreeStateMap: Map<TreeId, TreeState>
        }

    and Session =
        {
            User: User option
            GetLivePosition: unit -> FlukeDateTime
            TreeSelection: TreeState list
        }

    and TreeState =
        {
            Id: TreeId
            Name: TreeName
            Owner: User
            SharedWith: TreeAccess
            Position: FlukeDateTime option
            InformationStateMap: Map<Information, InformationState>
            TaskStateMap: Map<Task, TaskState>
        }

    and TreeId = TreeId of guid: Guid

    and TreeName = TreeName of name: string

    and [<RequireQualifiedAccess>] TreeAccess =
        | Public
        | Private of TreeAccessItem list

    and [<RequireQualifiedAccess>] TreeAccessItem =
        | Admin of user: User
        | ReadOnly of user: User

    and InformationState =
        {
            Information: Information
            Attachments: Attachment list
            SortList: (Information option * Information option) list
        }

    and TaskState =
        {
            Task: Task
            Sessions: TaskSession list
            Attachments: Attachment list
            SortList: (Task option * Task option) list
            CellStateMap: Map<DateId, CellState>
            InformationMap: Map<Information, unit>
        }
    //        type Cell = Cell of address: CellAddress * status: CellStatus
    and CellState =
        {
            Status: CellStatus
            Attachments: Attachment list
            Sessions: TaskSession list
        }

    and CellStatus =
        | Disabled
        | Suggested
        | Pending
        | Missed
        | MissedToday
        | UserStatus of user: User * status: ManualCellStatus

    and ManualCellStatus =
        | Postponed of until: FlukeTime option
        | Completed
        | Dismissed
        | ManualPending


    and TreeState with
        static member inline Create (id, name, owner, ?sharedWith): TreeState =
            {
                Id = id
                Name = name
                Owner = owner
                SharedWith = defaultArg sharedWith (TreeAccess.Private [])
                Position = None
                InformationStateMap = Map.empty
                TaskStateMap = Map.empty
            }

    let informationListToStateMap informationList =
        informationList
        |> List.map (fun information ->
            let informationState: InformationState =
                {
                    Information = information
                    Attachments = []
                    SortList = []
                }

            information, informationState)
        |> Map.ofList

    let hasAccess treeState user =
        match treeState with
        | { Owner = owner } when owner = user -> true
        | { SharedWith = TreeAccess.Public } -> true
        | { SharedWith = TreeAccess.Private accessList } ->
            accessList
            |> List.exists (function
                | TreeAccessItem.Admin dbUser
                | TreeAccessItem.ReadOnly dbUser -> dbUser = user)

    let treeStateWithInteractions (userInteractionList: UserInteraction list) (treeState: TreeState) =

        //            let diag =
//                treeState.TaskStateMap
//                    |> Map.tryPick (fun k v -> if k.Name = TaskName "seethrus" then Some v else None)
//            match diag with
//            | Some diag -> printfn "treeStateWithInteractions A %A B %A C %A" userInteractionList.Length treeState.TaskStateMap.Count diag
//            | None -> ()

        let newTreeState =
            (treeState, userInteractionList)
            ||> List.fold (fun treeState (UserInteraction (moment, user, interaction)) ->
                    match interaction with
                    | Interaction.Information (information, informationInteraction) ->
                        let informationState =
                            treeState.InformationStateMap
                            |> Map.tryFind information
                            |> Option.defaultValue
                                {
                                    Information = information
                                    Attachments = []
                                    SortList = []
                                }

                        let newInformationState =
                            match informationInteraction with
                            | InformationInteraction.Attachment attachment ->
                                let attachments = attachment :: informationState.Attachments
                                { informationState with
                                    Attachments = attachments
                                }
                            | InformationInteraction.Sort (top, bottom) ->
                                let sortList = (top, bottom) :: informationState.SortList
                                { informationState with SortList = sortList }

                        let newInformationStateMap =
                            treeState.InformationStateMap
                            |> Map.add information newInformationState

                        { treeState with
                            InformationStateMap = newInformationStateMap
                        }

                    | Interaction.Task (task, taskInteraction) ->
                        let taskState =
                            treeState.TaskStateMap
                            |> Map.tryFind task
                            |> Option.defaultValue
                                {
                                    Task = task
                                    Sessions = []
                                    Attachments = []
                                    SortList = []
                                    CellStateMap = Map.empty
                                    InformationMap = Map.empty
                                }

                        let newTaskState =
                            match taskInteraction with
                            | TaskInteraction.Attachment attachment ->
                                let newAttachments = attachment :: taskState.Attachments

                                { taskState with Attachments = newAttachments }

                            | TaskInteraction.Sort (top, bottom) ->
                                let newSortList = (top, bottom) :: taskState.SortList

                                { taskState with SortList = newSortList }
                            | TaskInteraction.Session (TaskSession (start, duration, breakDuration) as session) ->
                                let newSessions = session :: taskState.Sessions

                                { taskState with Sessions = newSessions }
                            //                                    let cellState =
//                                        taskState.CellStateMap
//                                        |> Map.tryFind dateId
//                                        |> Option.defaultValue
//                                            {
//                                                Status = Disabled
//                                                Attachments = []
//                                                Sessions = []
//                                            }
//
//                                    let newSessions = session :: cellState.Sessions
//
//                                    let newCellState =
//                                        {cellState with Sessions = newSessions}
//
//                                    let newCellStateMap =
//                                        taskState.CellStateMap
//                                        |> Map.add dateId newCellState
//
//                                    { taskState with CellStateMap = newCellStateMap }
                            | TaskInteraction.Archive -> taskState

                        let newTaskStateMap =
                            treeState.TaskStateMap
                            |> Map.add task newTaskState

                        { treeState with TaskStateMap = newTaskStateMap }
                    | Interaction.Cell ({ Task = task; DateId = dateId } as cellAddress, cellInteraction) ->
                        let taskState =
                            treeState.TaskStateMap
                            |> Map.tryFind task
                            |> Option.defaultValue
                                {
                                    Task = task
                                    Sessions = []
                                    Attachments = []
                                    SortList = []
                                    CellStateMap = Map.empty
                                    InformationMap = Map.empty
                                }

                        let cellState =
                            taskState.CellStateMap
                            |> Map.tryFind dateId
                            |> Option.defaultValue
                                {
                                    Status = CellStatus.Disabled
                                    Attachments = []
                                    Sessions = []
                                }


                        let newCellState =
                            match cellInteraction with
                            | CellInteraction.Attachment attachment ->
                                let attachments = attachment :: cellState.Attachments

                                let newCellState = { cellState with Attachments = attachments }

                                newCellState
                            | CellInteraction.StatusChange cellStatusChange ->
                                let manualCellStatus =
                                    match cellStatusChange with
                                    | CellStatusChange.Complete -> ManualCellStatus.Completed
                                    | CellStatusChange.Dismiss -> ManualCellStatus.Dismissed
                                    | CellStatusChange.Postpone until -> ManualCellStatus.Postponed until
                                    | CellStatusChange.Schedule -> ManualCellStatus.ManualPending

                                let newCellState =
                                    { cellState with
                                        Status = CellStatus.UserStatus (user, manualCellStatus)
                                    }

                                newCellState

                        let newTaskState =
                            { taskState with
                                CellStateMap =
                                    taskState.CellStateMap
                                    |> Map.add dateId newCellState
                            }

                        let newTaskStateMap =
                            treeState.TaskStateMap
                            |> Map.add task newTaskState

                        { treeState with TaskStateMap = newTaskStateMap })

        //            let diag =
//                newTreeState.TaskStateMap
//                    |> Map.tryPick (fun k v -> if k.Name = TaskName "seethrus" then Some v else None)
//            printfn "diag2 %A" diag
        newTreeState
