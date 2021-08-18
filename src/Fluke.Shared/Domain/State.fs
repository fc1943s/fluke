namespace Fluke.Shared.Domain

open FsCore
open System
open FsCore.BaseModel


module State =
    open Model
    open UserInteraction

    type DatabaseState =
        {
            Database: Database
            InformationStateMap: Map<Information, InformationState>
            TaskStateMap: Map<TaskId, TaskState>
            FileMap: Map<FileId, string>
        }

    and Database =
        {
            Id: DatabaseId
            Name: DatabaseName
            Owner: Username
            SharedWith: DatabaseAccess
            Position: FlukeDateTime option
        }

    and DatabaseId = DatabaseId of guid: Guid

    and DatabaseName = DatabaseName of name: string

    and [<RequireQualifiedAccess>] DatabaseAccess =
        | Public
        | Private of accessList: (Username * Access) list

    and [<RequireQualifiedAccess>] Access =
        | ReadWrite
        | ReadOnly

    and InformationState =
        {
            Information: Information
            AttachmentStateList: AttachmentState list
            SortList: (Information option * Information option) list
        }

    and TaskState =
        {
            Task: Task
            Archived: bool
            SessionList: Session list
            AttachmentStateList: AttachmentState list
            SortList: (TaskId option * TaskId option) list
            CellStateMap: Map<DateId, CellState>
        }

    and CellState =
        {
            Status: CellStatus
            AttachmentStateList: AttachmentState list
            SessionList: Session list
        }

    and AttachmentState =
        {
            Timestamp: FlukeDateTime
            Attachment: Attachment
            Archived: bool
        }

    and CellStatus =
        | Disabled
        | Suggested
        | Pending
        | Missed
        | MissedToday
        | UserStatus of username: Username * status: ManualCellStatus

    and ManualCellStatus =
        | Completed
        | Postponed of until: FlukeTime option
        | Dismissed
        | Scheduled


    and DatabaseId with
        static member inline NewId () = DatabaseId (Guid.newTicksGuid ())
        static member inline Value (DatabaseId guid) = guid

    and DatabaseName with
        static member inline Value value =
            try
                match value with
                | DatabaseName name -> Some name
            with
            | ex ->
                eprintfn $"DatabaseName.Value error value={value} ex={ex}"
                None

        static member inline ValueOrDefault = DatabaseName.Value >> Option.defaultValue ""

    and DatabaseState with
        static member inline Create (name, owner, ?position, ?sharedWith, ?id) =
            {
                Database =
                    {
                        Id = id |> Option.defaultWith DatabaseId.NewId
                        Name = name
                        Owner = owner
                        SharedWith = defaultArg sharedWith (DatabaseAccess.Private [])
                        Position = position
                    }
                InformationStateMap = Map.empty
                TaskStateMap = Map.empty
                FileMap = Map.empty
            }

    and Database with
        static member inline Default =
            {
                Id = DatabaseId Guid.Empty
                Name = DatabaseName ""
                Owner = Username ""
                SharedWith = DatabaseAccess.Private []
                Position = None
            }

    and InformationState with
        static member inline Default =
            {
                Information = Task.Default.Information
                AttachmentStateList = []
                SortList = []
            }

    and TaskState with
        static member inline Default =
            {
                Task = Task.Default
                Archived = false
                SessionList = []
                AttachmentStateList = []
                SortList = []
                CellStateMap = Map.empty
            }

    and CellState with
        static member inline Default =
            {
                Status = Disabled
                AttachmentStateList = []
                SessionList = []
            }


    let inline informationListToStateMap informationList =
        informationList
        |> List.map
            (fun information ->
                information,
                { InformationState.Default with
                    Information = information
                })
        |> Map.ofSeq

    let inline getAccess database username =
        match database with
        | { Owner = owner } when owner = username -> Some Access.ReadWrite
        | { SharedWith = DatabaseAccess.Public } -> Some Access.ReadWrite
        | {
              SharedWith = DatabaseAccess.Private accessList
          } ->
            accessList
            |> List.choose (fun (username', access) -> if username' <> username then None else Some access)
            |> List.sortWith
                (fun x y ->
                    match x, y with
                    | Access.ReadWrite, Access.ReadWrite -> 2
                    | Access.ReadWrite, _
                    | _, Access.ReadWrite -> 1
                    | _ -> 0)
            |> List.tryHead


    let inline databaseStateWithInteractions
        (userInteractionList: UserInteraction list)
        (databaseState: DatabaseState)
        =
        (databaseState, userInteractionList)
        ||> List.fold
                (fun databaseState (UserInteraction (moment, user, interaction)) ->
                    match interaction with
                    | Interaction.Information (information, informationInteraction) ->
                        let informationState =
                            databaseState.InformationStateMap
                            |> Map.tryFind information
                            |> Option.defaultValue
                                { InformationState.Default with
                                    Information = information
                                }

                        let newInformationState =
                            match informationInteraction with
                            | InformationInteraction.Attachment attachment ->
                                { informationState with
                                    AttachmentStateList =
                                        {
                                            Timestamp = moment
                                            Archived = false
                                            Attachment = attachment
                                        }
                                        :: informationState.AttachmentStateList
                                }
                            | InformationInteraction.Sort (top, bottom) ->
                                { informationState with
                                    SortList = (top, bottom) :: informationState.SortList
                                }

                        { databaseState with
                            InformationStateMap =
                                databaseState.InformationStateMap
                                |> Map.add information newInformationState
                        }

                    | Interaction.Task (taskId, taskInteraction) ->
                        let taskState = databaseState.TaskStateMap.[taskId]

                        let newTaskState =
                            match taskInteraction with
                            | TaskInteraction.Attachment attachment ->
                                { taskState with
                                    AttachmentStateList =
                                        {
                                            Timestamp = moment
                                            Archived = false
                                            Attachment = attachment
                                        }
                                        :: taskState.AttachmentStateList
                                }

                            | TaskInteraction.Sort (top, bottom) ->
                                { taskState with
                                    SortList = (top, bottom) :: taskState.SortList
                                }
                            | TaskInteraction.Session session ->
                                { taskState with
                                    SessionList = session :: taskState.SessionList
                                }
                            | TaskInteraction.Archive -> taskState

                        { databaseState with
                            TaskStateMap =
                                databaseState.TaskStateMap
                                |> Map.add taskId newTaskState
                        }
                    | Interaction.Cell (taskId, dateId, cellInteraction) ->
                        let taskState = databaseState.TaskStateMap.[taskId]

                        let cellState =
                            taskState.CellStateMap
                            |> Map.tryFind dateId
                            |> Option.defaultValue CellState.Default

                        let newCellState =
                            match cellInteraction with
                            | CellInteraction.Attachment attachment ->
                                { cellState with
                                    AttachmentStateList =
                                        {
                                            Timestamp = moment
                                            Archived = false
                                            Attachment = attachment
                                        }
                                        :: cellState.AttachmentStateList
                                }
                            | CellInteraction.StatusChange cellStatusChange ->
                                let manualCellStatus =
                                    match cellStatusChange with
                                    | CellStatusChange.Complete -> ManualCellStatus.Completed
                                    | CellStatusChange.Dismiss -> ManualCellStatus.Dismissed
                                    | CellStatusChange.Postpone until -> ManualCellStatus.Postponed until
                                    | CellStatusChange.Schedule -> ManualCellStatus.Scheduled

                                { cellState with
                                    Status = CellStatus.UserStatus (user, manualCellStatus)
                                }

                        let newTaskState =
                            { taskState with
                                CellStateMap =
                                    taskState.CellStateMap
                                    |> Map.add dateId newCellState
                            }

                        { databaseState with
                            TaskStateMap =
                                databaseState.TaskStateMap
                                |> Map.add taskId newTaskState
                        })

    let inline mergeInformationStateMap
        (oldMap: Map<Information, InformationState>)
        (newMap: Map<Information, InformationState>)
        =
        (oldMap, newMap)
        ||> Map.unionWith
                (fun oldValue newValue ->
                    { oldValue with
                        AttachmentStateList =
                            oldValue.AttachmentStateList
                            @ newValue.AttachmentStateList
                        SortList = oldValue.SortList @ newValue.SortList
                    })

    let inline mergeCellStateMap (oldMap: Map<DateId, CellState>) (newMap: Map<DateId, CellState>) =
        (oldMap, newMap)
        ||> Map.unionWith
                (fun oldValue newValue ->
                    { oldValue with
                        SessionList = oldValue.SessionList @ newValue.SessionList
                        AttachmentStateList =
                            oldValue.AttachmentStateList
                            @ newValue.AttachmentStateList
                        Status = newValue.Status
                    })

    let inline mergeInformationMap (oldMap: Map<Information, unit>) (newMap: Map<Information, unit>) =
        oldMap |> Map.union newMap

    let inline mergeTaskState (oldValue: TaskState) (newValue: TaskState) =
        {
            Task = newValue.Task
            Archived = newValue.Archived
            SessionList = oldValue.SessionList @ newValue.SessionList
            AttachmentStateList =
                oldValue.AttachmentStateList
                @ newValue.AttachmentStateList
            SortList = oldValue.SortList @ newValue.SortList
            CellStateMap = mergeCellStateMap oldValue.CellStateMap newValue.CellStateMap
        }

    let inline mergeTaskStateMap (oldMap: Map<TaskId, TaskState>) (newMap: Map<TaskId, TaskState>) =
        Map.unionWith mergeTaskState oldMap newMap
