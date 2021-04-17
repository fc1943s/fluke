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
            TaskStateMap: Map<Task, TaskState>
        }

    and [<RequireQualifiedAccess>] DatabaseAccess =
        | Public
        | Private of DatabaseAccessItem list

    and [<RequireQualifiedAccess>] DatabaseAccessItem =
        | Admin of user: Username
        | ReadOnly of user: Username

    and InformationState =
        {
            Information: Information
            Attachments: Attachment list
            SortList: (Information option * Information option) list
        }

    and TaskState =
        {
            TaskId: TaskId
            Task: Task
            Sessions: TaskSession list
            Attachments: Attachment list
            SortList: (Task option * Task option) list
            CellStateMap: Map<DateId, CellState>
            InformationMap: Map<Information, unit>
        }

    and TaskId = TaskId of guid: Guid
    //        type Cell = Cell of address: CellAddress * status: CellStatus
    and CellState =
        {
            Status: CellStatus
            Selected: Selection
            Attachments: Attachment list
            Sessions: TaskSession list
        }

    and CellStatus =
        | Disabled
        | Suggested
        | Pending
        | Missed
        | MissedToday
        | UserStatus of user: Username * status: ManualCellStatus

    and ManualCellStatus =
        | Completed
        | Postponed of until: FlukeTime option
        | Dismissed
        | Scheduled


    and DatabaseId with
        static member inline NewId () = DatabaseId (Guid.NewGuid ())

    and TaskId with
        static member inline NewId () = TaskId (Guid.NewGuid ())

    and DatabaseState with
        static member inline Create (name, owner, dayStart, ?id, ?sharedWith, ?position) =
            {
                Database =
                    {
                        Id = id |> Option.defaultValue (DatabaseId.NewId ())
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

    let hasAccess database username =
        match database with
        | { Owner = owner } when owner = username -> true
        | { SharedWith = DatabaseAccess.Public } -> true
        | {
              SharedWith = DatabaseAccess.Private accessList
          } ->
            accessList
            |> List.exists
                (function
                | DatabaseAccessItem.Admin dbUser
                | DatabaseAccessItem.ReadOnly dbUser -> dbUser = username)


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

                        | Interaction.Task (task, taskInteraction) ->
                            let taskState =
                                databaseState.TaskStateMap
                                |> Map.tryFind task
                                |> Option.defaultValue
                                    {
                                        TaskId = TaskId.NewId ()
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
                                |> Map.add task newTaskState

                            { databaseState with
                                TaskStateMap = newTaskStateMap
                            }
                        | Interaction.Cell ({ Task = task; DateId = dateId } as _cellAddress, cellInteraction) ->
                            let taskState =
                                databaseState.TaskStateMap
                                |> Map.tryFind task
                                |> Option.defaultValue
                                    {
                                        TaskId = TaskId.NewId ()
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
                                        Selected = Selection false
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
                                | CellInteraction.Selection selected ->
                                    let newCellState = { cellState with Selected = selected }
                                    newCellState

                            let newTaskState =
                                { taskState with
                                    CellStateMap =
                                        taskState.CellStateMap
                                        |> Map.add dateId newCellState
                                }

                            let newTaskStateMap =
                                databaseState.TaskStateMap
                                |> Map.add task newTaskState

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
        { oldValue with
            Task = oldValue.Task
            Sessions = oldValue.Sessions @ newValue.Sessions
            Attachments = oldValue.Attachments @ newValue.Attachments
            SortList = oldValue.SortList @ newValue.SortList
            CellStateMap = mergeCellStateMap oldValue.CellStateMap newValue.CellStateMap
            InformationMap = mergeInformationMap oldValue.InformationMap newValue.InformationMap
        }

    let mergeTaskStateMap (oldMap: Map<Task, TaskState>) (newMap: Map<Task, TaskState>) =
        Map.unionWith mergeTaskState oldMap newMap
