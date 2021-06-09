namespace Fluke.Shared.Domain

open Fluke.Shared
open System


module State =
    open Model
    open UserInteraction

    //    type State =
//        {
//            Session: Session
//        }

    type SessionData =
        {
            //            User: User option
//            SelectedDatabaseIds: Set<DatabaseState>
            TaskList: Task list
            InformationStateMap: Map<Information, InformationState>
            TaskStateMap: Map<Task, TaskState>
            UnfilteredTaskCount: int
        }

    and Database =
        {
            Id: DatabaseId
            Name: DatabaseName
            Owner: Username
            SharedWith: DatabaseAccess
            Position: FlukeDateTime option
            DayStart: FlukeTime
        }

    and DatabaseId = DatabaseId of guid: Guid

    and DatabaseName = DatabaseName of name: string

    and DatabaseState =
        {
            Database: Database
            InformationStateMap: Map<Information, InformationState>
            TaskStateMap: Map<TaskId, TaskState>
        }

    and [<RequireQualifiedAccess>] DatabaseAccess =
        | Public
        | Private of accessList: (Username * Access) list

    and [<RequireQualifiedAccess>] Access =
        | ReadWrite
        | ReadOnly

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
        }

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
        | UserStatus of username: Username * status: ManualCellStatus

    and ManualCellStatus =
        | Completed
        | Postponed of until: FlukeTime option
        | Dismissed
        | Scheduled


    and DatabaseId with
        static member inline NewId () = DatabaseId (Guid.NewGuid ())
        static member inline Value (DatabaseId guid) = guid

    and DatabaseName with
        static member inline Value (DatabaseName name) = name

    and Database with
        static member inline Default =
            {
                Id = DatabaseId Guid.Empty
                Name = DatabaseName ""
                Owner = Username ""
                SharedWith = DatabaseAccess.Private []
                Position = None
                DayStart = FlukeTime.Create 7 0
            }

    and TaskState with
        static member inline Default =
            {
                Task = Task.Default
                Sessions = []
                Attachments = []
                SortList = []
                CellStateMap = Map.empty
            }

    and DatabaseState with
        static member inline Create (name, owner, dayStart, ?id, ?sharedWith, ?position) =
            {
                Database =
                    {
                        Id = id |> Option.defaultWith DatabaseId.NewId
                        Name = name
                        Owner = owner
                        SharedWith = defaultArg sharedWith (DatabaseAccess.Private [])
                        Position = position
                        DayStart = dayStart
                    }
                InformationStateMap = Map.empty
                TaskStateMap = Map.empty
            }

    let informationListToStateMap informationList =
        informationList
        |> List.map
            (fun information ->
                let informationState : InformationState =
                    {
                        Information = information
                        Attachments = []
                        SortList = []
                    }

                information, informationState)
        |> Map.ofList

    let getAccess database username =
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


    let databaseStateWithInteractions (userInteractionList: UserInteraction list) (databaseState: DatabaseState) =
        let newDatabaseState =
            (databaseState, userInteractionList)
            ||> List.fold
                    (fun databaseState (UserInteraction (_moment, user, interaction)) ->
                        match interaction with
                        | Interaction.Information (information, informationInteraction) ->
                            let informationState =
                                databaseState.InformationStateMap
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

                                    { informationState with
                                        SortList = sortList
                                    }

                            let newInformationStateMap =
                                databaseState.InformationStateMap
                                |> Map.add information newInformationState

                            { databaseState with
                                InformationStateMap = newInformationStateMap
                            }

                        | Interaction.Task (taskId, taskInteraction) ->
                            let taskState = databaseState.TaskStateMap.[taskId]

                            let newTaskState =
                                match taskInteraction with
                                | TaskInteraction.Attachment attachment ->
                                    let newAttachments = attachment :: taskState.Attachments

                                    { taskState with
                                        Attachments = newAttachments
                                    }

                                | TaskInteraction.Sort (top, bottom) ->
                                    let newSortList = (top, bottom) :: taskState.SortList

                                    { taskState with
                                        SortList = newSortList
                                    }
                                | TaskInteraction.Session (TaskSession (_start, _duration, _breakDuration) as session) ->
                                    let newSessions = session :: taskState.Sessions

                                    { taskState with
                                        Sessions = newSessions
                                    }
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
                                databaseState.TaskStateMap
                                |> Map.add taskId newTaskState

                            { databaseState with
                                TaskStateMap = newTaskStateMap
                            }
                        | Interaction.Cell (taskId, dateId, cellInteraction) ->
                            let taskState = databaseState.TaskStateMap.[taskId]

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

                                    let newCellState =
                                        { cellState with
                                            Attachments = attachments
                                        }

                                    newCellState
                                | CellInteraction.StatusChange cellStatusChange ->
                                    let manualCellStatus =
                                        match cellStatusChange with
                                        | CellStatusChange.Complete -> ManualCellStatus.Completed
                                        | CellStatusChange.Dismiss -> ManualCellStatus.Dismissed
                                        | CellStatusChange.Postpone until -> ManualCellStatus.Postponed until
                                        | CellStatusChange.Schedule -> ManualCellStatus.Scheduled

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
                                databaseState.TaskStateMap
                                |> Map.add taskId newTaskState

                            { databaseState with
                                TaskStateMap = newTaskStateMap
                            })

        newDatabaseState

    let mergeInformationStateMap
        (oldMap: Map<Information, InformationState>)
        (newMap: Map<Information, InformationState>)
        =
        (oldMap, newMap)
        ||> Map.unionWith
                (fun oldValue newValue ->
                    { oldValue with
                        Attachments = oldValue.Attachments @ newValue.Attachments
                        SortList = oldValue.SortList @ newValue.SortList
                    })

    let mergeCellStateMap (oldMap: Map<DateId, CellState>) (newMap: Map<DateId, CellState>) = oldMap |> Map.union newMap

    let mergeInformationMap (oldMap: Map<Information, unit>) (newMap: Map<Information, unit>) =
        oldMap |> Map.union newMap

    let mergeTaskState (oldValue: TaskState) (newValue: TaskState) =
        {
            Task = oldValue.Task
            Sessions = oldValue.Sessions @ newValue.Sessions
            Attachments = oldValue.Attachments @ newValue.Attachments
            SortList = oldValue.SortList @ newValue.SortList
            CellStateMap = mergeCellStateMap oldValue.CellStateMap newValue.CellStateMap
        }

    let mergeTaskStateMap (oldMap: Map<TaskId, TaskState>) (newMap: Map<TaskId, TaskState>) =
        Map.unionWith mergeTaskState oldMap newMap
