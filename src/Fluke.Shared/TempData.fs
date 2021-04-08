namespace Fluke.Shared

open Fluke.Shared
open System


module LintTests =
    type ExampleInterface =
        abstract print : unit -> unit

module TempData =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State
    open Templates

    let rec testUser =
        {
            Username = Username (nameof testUser)
            Color = UserColor.Black
            WeekStart = DayOfWeek.Sunday
            DayStart = FlukeTime.Create 12 0
            SessionLength = Minute 25.
            SessionBreakLength = Minute 5.
        }


    module Events =


        [<RequireQualifiedAccess>]
        type TempEvent =
            | CellStatus of user: User * task: Task * date: FlukeDateTime * manualCellStatus: ManualCellStatus
            | CellCompleted of user: User * task: Task * date: FlukeDateTime
        //            | CellCommented of user:User * task:Task * date:FlukeDateTime * comment:Comment

        type TaskName = TaskName of string
        type HashedTaskId = HashedTaskId of string

        type EventX =
            | TaskCreated of information: Information * name: TaskName
            | TaskRenamed of taskId: HashedTaskId * newName: TaskName


    //        let eventsFromCellComments user = ()


    //        module EventSourcingTemp =
//            type Command = CompleteCell of user: User * task: Task * date: FlukeDateTime
//            type Event = CellCompleted of user: User * task: Task * date: FlukeDateTime
//
//
//            let a =
//                let b: UserInteraction list = []
//                b
//                |> List.map (fun userInteraction -> ())
//                |> ignore
//
////            let initialState =
////                {
////                    User = None
////                    TaskMap = Map.empty
////                    TaskIdList = []
////                }
//
//            let private apply (state: State) (event: Event): State =
//                match event with
//                | CellCompleted (user, task, moment) ->
//                    { state with
//                        TaskMap =
//                            let taskId = taskId task
//                            let dateId = dateId state.DayStart moment
//                            let newStatus = UserStatus (user, CellStatus.Completed)
//
//                            let cellMap =
//                                state.TaskMap
//                                |> Map.tryFind taskId
//                                |> Option.defaultValue Map.empty
//                                |> Map.add dateId newStatus
//
//                            state.TaskMap |> Map.add taskId cellMap
//                    }
//
//            let execute (state: State) (command: Command): Event list =
//                [
//                    match command, state with
//                    | CompleteCell (user, task, moment), state when state.TaskMap
//                                                                    |> Map.tryFind (taskId task)
//                                                                    |> Option.defaultValue Map.empty
//                                                                    |> Map.tryFind (dateId state.DayStart moment)
//                                                                    |> Option.defaultValue Disabled
//                                                                    |> (=) Pending -> CellCompleted (user, task, moment)
//
//                    | CompleteCell (user, task, eventDate), state -> CellCompleted (user, task, eventDate)
//
//                    | _ -> ()
//                ]
//
//            let build = List.fold apply
//            let rebuld = build initialState






    let createTaskCommentInteraction user moment task comment =
        let interaction = Interaction.Task (task, TaskInteraction.Attachment (Attachment.Comment (user, comment)))

        let userInteraction = UserInteraction (moment, user, interaction)

        userInteraction

    let createInformationCommentInteraction user moment information comment =
        let interaction =
            Interaction.Information (
                information,
                InformationInteraction.Attachment (Attachment.Comment (user, comment))
            )

        let userInteraction = UserInteraction (moment, user, interaction)

        userInteraction

    let createCellCommentInteraction dayStart task moment user comment =
        let cellInteraction =
            Attachment.Comment (user, comment)
            |> CellInteraction.Attachment

        let cellAddress =
            {
                Task = task
                DateId = dateId dayStart moment
            }

        let interaction = Interaction.Cell (cellAddress, cellInteraction)

        let userInteraction = UserInteraction (moment, user, interaction)

        userInteraction


    let createCellStatusChangeInteractions user (entries: (FlukeDate * (Task option * ManualCellStatus) list) list) =
        entries
        |> List.collect
            (fun (date, events) ->
                events
                |> List.choose
                    (fun (task, manualCellStatus) ->
                        task
                        |> Option.map (fun task -> createCellStatusChangeInteraction user task date manualCellStatus)))





    let dslDatabaseWithUser (user: User) (dslDatabase: (Information * (string * DslTask list) list) list) =
        dslDatabase
        |> List.map
            (fun (information, tasks) ->
                let newTasks =
                    tasks
                    |> List.map
                        (fun (taskName, dslTasks) ->
                            taskName,
                            dslTasks
                            |> List.map (fun dslTask -> dslTask, user))

                information, newTasks)

    //        taskDatabase
//        |> List.map (fun x -> x |> Tuple2.mapItem2 (fun x -> x |> List.map (fun x -> x |> Tuple2.mapItem2 (fun (a,b) -> x |> List.map (fun (event:DslTask) -> event, user)))))

    //    let createDatabaseData dayStart position taskDatabase =
//        let taskStateList =
//            taskDatabase
//            |> List.collect (fun (information, tasks) ->
//                tasks
//                |> List.map (fun (taskName, events: (DslTask * User) list) ->
//                    let task =
//                        { Task.Default with
//                            Name = TaskName taskName
//                            Information = information
//                        }
//
//                    createTaskState dayStart position task events))
//
//        let informationList =
//            taskDatabase |> List.map fst |> List.distinct
//
//        let taskOrderList =
//            taskStateList
//            |> List.map (fun (taskState, userInteractions) ->
//                {
//                    Task = taskState.Task
//                    Priority = TaskOrderPriority.Last
//                })
//
//        {
//            TaskStateList = taskStateList
//            TaskOrderList = taskOrderList
//            InformationList = informationList
//            GetLivePosition = getLivePosition
//        }

    //    type SessionState =
//        {
//            User: User option
//            GetLivePosition: unit -> FlukeDateTime
//            DatabaseStateMap: Map<DatabaseState, bool>
//        }



    let mergeDatabaseState (oldValue: DatabaseState) (newValue: DatabaseState) =
        { oldValue with
            InformationStateMap = mergeInformationStateMap oldValue.InformationStateMap newValue.InformationStateMap
            TaskStateMap = mergeTaskStateMap oldValue.TaskStateMap newValue.TaskStateMap
        }

    let mergeDatabaseStateMap (oldMap: Map<DatabaseId, DatabaseState>) (newMap: Map<DatabaseId, DatabaseState>) =
        Map.unionWith mergeDatabaseState oldMap newMap



    let createDslData
        moment
        taskContainerFactory
        (dslDatabase: (Information * (string * (DslTask * User) list) list) list)
        =

        let taskMap, taskStateList =
            let (|FirstPass|SecondPass|) =
                function
                | 0 -> FirstPass
                | _ -> SecondPass

            let rec loop pass taskMap =
                let newTaskMap, newTaskStateList =
                    ((taskMap, []), dslDatabase)
                    ||> List.fold
                            (fun (taskMap, taskStateList) (information, tasks) ->
                                ((taskMap, taskStateList), tasks)
                                ||> List.fold
                                        (fun (taskMap, taskStateList) (taskName, dslTasks) ->
                                            let oldTask : Task option = taskMap |> Map.tryFind taskName

                                            let newTask =
                                                match oldTask with
                                                | Some task -> { task with Information = information }
                                                | None ->
                                                    { Task.Default with
                                                        Information = information
                                                        Name = TaskName taskName
                                                    }

                                            let fakeTaskMap =
                                                match pass with
                                                | FirstPass -> None
                                                | SecondPass ->
                                                    taskMap
                                                    |> Map.toSeq
                                                    |> Seq.map (fun (taskName, task) -> TaskName taskName, task)
                                                    |> Map.ofSeq
                                                    |> Some

                                            let taskState, userInteractions =
                                                createTaskState moment newTask fakeTaskMap dslTasks

                                            let newTaskMap = taskMap |> Map.add taskName taskState.Task

                                            let newTaskStateList =
                                                taskStateList
                                                @ [
                                                    taskState, userInteractions
                                                ]

                                            newTaskMap, newTaskStateList))

                match pass with
                | 0 -> loop 1 newTaskMap
                | _ -> newTaskMap, newTaskStateList

            loop 0 Map.empty

        //        printfn
//            "taskMap taskStateList ### %A ### ### %A ###"
//            (taskMap|>Map.tryPick (fun k v -> if k.Contains "week view" then Some v else None))
//            (taskStateList |> List.filter (fun (taskState, interactions) -> let (TaskName taskName) = taskState.Task.Name in taskName.Contains "week view"))

        let informationStateMap =
            dslDatabase
            |> List.map fst
            |> List.distinct
            |> informationListToStateMap

        //        let taskOrderList =
//            taskStateList
//            |> List.map (fun (taskState, _) ->
//                {
//                    Task = taskState.Task
//                    Priority = TaskOrderPriority.Last
//                })

        let duplicated =
            taskStateList
            |> List.map (fun (taskState, _) -> taskState.Task.Name)
            |> List.groupBy id
            |> List.filter (snd >> List.length >> fun n -> n > 1)
            |> List.map fst

        if not duplicated.IsEmpty then
            failwithf $"Duplicated task names: {duplicated}"

        let tasks =
            taskContainerFactory
                (fun taskName ->
                    taskMap
                    |> Map.tryFind taskName
                    |> Option.orElseWith (fun () -> failwithf $"createDslData. Task not found: {taskName}"))


        //        let taskOrderList = getTaskOrderList [] (taskStateList |> List.map fst) []

        let dslData =
            {
                TaskStateList = taskStateList
                //                TaskOrderList = taskOrderList
                InformationStateMap = informationStateMap
            }

        dslData, tasks





    module Testing =

        //        let dslDataToDatabaseState user dslData =
//            let userInteractions =
//                dslData.TaskStateList |> List.collect snd
//
//            let databaseStateWithoutInteractions: DatabaseState =
//                {
//                    Id =
//                        DatabaseId
//                        <| Guid "17A1AA3D-95C7-424E-BD6D-7C12B33CED37"
//                    Name = State.DatabaseName "dslDataToState"
//                    Owner = user
//                    SharedWith = []
//                    Position = None
//                    InformationStateMap = dslData.InformationStateMap
//                    TaskStateMap = Map.empty
//                }
//
//            databaseStateWithoutInteractions
//            |> DatabaseStateWithInteractions userInteractions

        let createLaneRenderingDslData
            (input: {| User: User
                       Position: FlukeDateTime
                       Task: Task
                       Events: DslTask list |})
            =
            let eventsWithUser = input.Events |> List.map (fun x -> x, input.User)

            let dslData =
                {
                    TaskStateList =
                        [
                            createTaskState input.Position input.Task None eventsWithUser
                        ]
                    //                    TaskOrderList =
//                        [
//                            {
//                                Task = input.Task
//                                Priority = TaskOrderPriority.First
//                            }
//                        ]
//                    //                    GetLivePosition = fun () -> input.Position
                    InformationStateMap =
                        [
                            input.Task.Information
                        ]
                        |> informationListToStateMap
                }

            //            let getLivePosition = fun () -> input.Position

            //            let databaseState = dslDataToDatabaseState input.User dslData

            //            let databaseStateMap =
//                [ databaseState.Id, (databaseState, true) ] |> Map.ofList

            //            let state =
//                {
//                    User = Some input.User
//                    GetLivePosition = getLivePosition
//                    DatabaseStateMap = databaseStateMap
//                }

            dslData


        let createLaneSortingDslData
            (input: {| User: User
                       Position: FlukeDateTime
                       Data: (Task * DslTask list) list
                       Expected: string list |})
            =
            let dslData =
                let taskStateList =
                    input.Data
                    |> List.map
                        (fun (task, events) ->
                            events
                            |> List.map (fun dslTask -> dslTask, input.User)
                            |> createTaskState input.Position task None)

                //                let taskOrderList =
//                    input.Data
//                    |> List.map (fun (task, events) -> { Task = task; Priority = TaskOrderPriority.Last })

                let _getLivePosition = fun () -> input.Position

                let informationStateMap =
                    taskStateList
                    |> List.map (fun (taskState, _) -> taskState.Task.Information)
                    |> List.distinct
                    |> informationListToStateMap

                {
                    TaskStateList = taskStateList
                    //                    TaskOrderList = taskOrderList
                    //                    GetLivePosition = getLivePosition
                    InformationStateMap = informationStateMap
                }

            //            let getLivePosition = fun () -> input.Position
//
//            let databaseState = dslDataToDatabaseState input.User dslData
//
//            let databaseStateMap =
//                [ databaseState.Id, (databaseState, true) ] |> Map.ofList
//
//            let state =
//                {
//                    User = Some input.User
//                    GetLivePosition = getLivePosition
//                    DatabaseStateMap = databaseStateMap
//                }

            dslData
