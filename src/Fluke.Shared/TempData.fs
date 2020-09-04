namespace Fluke.Shared

open System
open FSharpPlus
open Suigetsu.Core


module TempData =
    open Model

    module private Areas =
        let car = { Name = "car" }
        let career = { Name = "career" }
        let chores = { Name = "chores" }
        let finances = { Name = "finances" }
        let fitness = { Name = "fitness" }
        let food = { Name = "food" }
        let health = { Name = "health" }
        let leisure = { Name = "leisure" }
        let programming = { Name = "programming" }
        let travel = { Name = "travel" }
        let workflow = { Name = "workflow" }
        let writing = { Name = "writing" }


    module private Projects =
        let app_fluke =
            { Project.Area = Areas.workflow
              Project.Name = "app_fluke" }

        let blog =
            { Project.Area = Areas.writing
              Project.Name = "blog" }

        let rebuild_website =
            { Project.Area = Areas.programming
              Project.Name = "rebuild_website" }


    module private Resources =
        let agile =
            { Resource.Area = Areas.programming
              Resource.Name = "agile" }

        let artificial_intelligence =
            { Resource.Area = Areas.programming
              Resource.Name = "artificial_intelligence" }

        let cloud =
            { Resource.Area = Areas.programming
              Resource.Name = "cloud" }

        let communication =
            { Resource.Area = Areas.workflow
              Resource.Name = "communication" }

        let docker =
            { Resource.Area = Areas.programming
              Resource.Name = "docker" }

        let fsharp =
            { Resource.Area = Areas.programming
              Resource.Name = "f#" }

        let linux =
            { Resource.Area = Areas.programming
              Resource.Name = "linux" }

        let music =
            { Resource.Area = Areas.leisure
              Resource.Name = "music" }

        let rust =
            { Resource.Area = Areas.programming
              Resource.Name = "rust" }

        let vim =
            { Resource.Area = Areas.programming
              Resource.Name = "vim" }

        let windows =
            { Resource.Area = Areas.programming
              Resource.Name = "windows" }


    module Users =
        let fc1943s =
            { Username = "fc1943s"
              Color = UserColor.Blue }

        let liryanne =
            { Username = "liryanne"
              Color = UserColor.Pink }

        let testUser =
            { Username = "Test"
              Color = UserColor.Blue }

        let users = [ fc1943s; liryanne ]


    module Consts =
        let rootPath =
            """M:\Dropbox\home_encrypted\projects\app-fluke"""

        let dbPath = rootPath + """\db_v1"""

        [<Literal>]
        let sessionLength = 25.

        [<Literal>]
        let sessionBreakLength = 5.

        let defaultDate = FlukeDate.FromDateTime DateTime.MinValue
        let dayStart = flukeTime 07 00
        let defaultPosition = { Date = defaultDate; Time = dayStart }


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

        let eventsFromStatusEntries user (entries: (FlukeDate * (Task * ManualCellStatus) list) list) =
            let oldEvents =
                entries
                |> List.collect (fun (date, events) ->
                    events
                    |> List.map (fun (task, manualCellStatus) ->
                        let cellStatusChange =
                            match manualCellStatus with
                            | Completed -> CellStatusChange.Complete
                            | Dismissed -> CellStatusChange.Dismiss
                            | Postponed until -> CellStatusChange.Postpone until
                            | ManualPending -> CellStatusChange.Schedule

                        let cellInteraction =
                            CellInteraction.StatusChange cellStatusChange

                        let cellAddress = { Task = task; DateId = DateId date }

                        let interaction =
                            Interaction.Cell(cellAddress, cellInteraction)

                        let moment = { Date = date; Time = Consts.dayStart }

                        let userInteraction =
                            UserInteraction(user, moment, interaction)

                        userInteraction))

            oldEvents

        let eventsFromCellComments user = ()


        module EventSourcingTemp =
            type Command = CompleteCell of user: User * task: Task * date: FlukeDateTime
            type Event = CellCompleted of user: User * task: Task * date: FlukeDateTime

            type State =
                { DayStart: FlukeTime
                  TaskMap: Map<TaskId, Map<DateId, CellStatus>>
                  TaskIdList: TaskId list }

            let initialState =
                { DayStart = Consts.dayStart
                  TaskMap = Map.empty
                  TaskIdList = [] }

            let private apply (state: State) (event: Event): State =
                match event with
                | CellCompleted (user, task, moment) ->
                    { state with
                          TaskMap =
                              let taskId = taskId task
                              let dateId = dateId state.DayStart moment
                              let newStatus = UserStatus(user, Completed)

                              let cellMap =
                                  state.TaskMap
                                  |> Map.tryFind taskId
                                  |> Option.defaultValue Map.empty
                                  |> Map.add dateId newStatus

                              state.TaskMap |> Map.add taskId cellMap }

            let execute (state: State) (command: Command): Event list =
                [ match command, state with
                  | CompleteCell (user, task, moment), state when state.TaskMap
                                                                  |> Map.tryFind (taskId task)
                                                                  |> Option.defaultValue Map.empty
                                                                  |> Map.tryFind (dateId state.DayStart moment)
                                                                  |> Option.defaultValue Disabled
                                                                  |> (=) Missed -> CellCompleted(user, task, moment)

                  | CompleteCell (user, task, eventDate), state -> CellCompleted(user, task, eventDate)

                  | _ -> () ]

            let build = List.fold apply
            let rebuld = build initialState


    let getLivePosition () = FlukeDateTime.FromDateTime DateTime.Now

    let getTaskOrderList oldTaskOrderList (taskStateList: TaskState list) manualTaskOrder =
        let taskMap =
            taskStateList
            |> List.map (fun x -> (x.Task.Information, x.Task.Name), x)
            |> Map.ofList

        let newTaskOrderList =
            manualTaskOrder
            |> List.map (fun (information, taskName) ->
                taskMap
                |> Map.tryFind (information, taskName)
                |> function
                | None -> failwithf "Invalid task: '%A/%s'" information taskName
                | Some taskState ->
                    { Task = taskState.Task
                      Priority = TaskOrderPriority.First })

        oldTaskOrderList @ newTaskOrderList


    type TempTaskEvent =
        | TempComment of comment: string
        | TempSession of start: FlukeDateTime
        | TempPriority of priority: TaskPriority
        | TempTag of information: Information
        | TempStatusEntry of date: FlukeDate * manualCellStatus: ManualCellStatus
        | TempCellComment of date: FlukeDate * comment: string
        | TempTaskField of field: TempTaskEventField
        | TempInteraction of interaction: TaskInteraction

    and TempTaskEventField =
        | TempTaskFieldScheduling of scheduling: TaskScheduling * start: FlukeDate option
        | TempTaskFieldPendingAfter of start: FlukeTime
        | TempTaskFieldMissedAfter of start: FlukeTime
        | TempTaskFieldDuration of minutes: int

    let getCellStatusChangeUserInteraction dayStart user task date manualCellStatus =
        let cellStatusChange =
            match manualCellStatus with
            | Completed -> CellStatusChange.Complete
            | Dismissed -> CellStatusChange.Dismiss
            | Postponed until -> CellStatusChange.Postpone until
            | ManualPending -> CellStatusChange.Schedule

        let cellInteraction =
            CellInteraction.StatusChange cellStatusChange

        let cellAddress = { Task = task; DateId = DateId date }

        let interaction =
            Interaction.Cell(cellAddress, cellInteraction)

        let moment = { Date = date; Time = dayStart }

        let userInteraction =
            UserInteraction(user, moment, interaction)

        userInteraction

    let applyTaskEvents dayStart task (events: (TempTaskEvent * User) list) =

        let getPriorityValue =
            function
            | Low1 -> 1
            | Low2 -> 2
            | Low3 -> 3
            | Medium4 -> 4
            | Medium5 -> 5
            | Medium6 -> 6
            | High7 -> 7
            | High8 -> 8
            | High9 -> 9
            | Critical10 -> 10

        // TODO: how the hell do i rewrite this without losing performance?
        let userInteractions, sessions, priority, scheduling, pendingAfter, missedAfter, duration =
            let rec loop (state: {| Duration: int option
                                    MissedAfter: FlukeTime option
                                    PendingAfter: FlukeTime option
                                    Priority: TaskPriorityValue option
                                    Scheduling: TaskScheduling
                                    Sessions: TaskSession list
                                    UserInteractions: UserInteraction list |}) =
                function
                | (TempInteraction interaction, user) :: tail ->
                    let moment = Consts.defaultPosition
                    let interaction = Interaction.Task(task, interaction)

                    let userInteraction =
                        UserInteraction(user, moment, interaction)

                    loop
                        {| state with
                               UserInteractions = state.UserInteractions @ [ userInteraction ] |}
                        tail

                | (TempComment comment, user) :: tail ->
                    let moment = Consts.defaultPosition

                    let interaction =
                        Interaction.Task(task, TaskInteraction.Attachment(Attachment.Comment(user, Comment comment)))

                    let userInteraction =
                        UserInteraction(user, moment, interaction)

                    loop
                        {| state with
                               UserInteractions = state.UserInteractions @ [ userInteraction ] |}
                        tail

                | (TempCellComment (date, comment), user) :: tail ->
                    let moment = Consts.defaultPosition

                    let interaction =
                        Interaction.Cell({Task = task; DateId = DateId date},
                                         CellInteraction.Attachment(Attachment.Comment(user, Comment comment)))

                    let userInteraction =
                        UserInteraction(user, moment, interaction)

                    loop
                        {| state with
                               UserInteractions = state.UserInteractions @ [ userInteraction ] |}
                        tail

                | (TempSession { Date = date; Time = time }, user) :: tail ->
                    let session = TaskSession { Date = date; Time = time }
                    loop
                        {| state with
                               Sessions = session :: state.Sessions |}
                        tail

                | (TempStatusEntry (date, manualCellStatus), user) :: tail ->
                    let userInteraction =
                        getCellStatusChangeUserInteraction Consts.dayStart user task date manualCellStatus

                    loop
                        {| state with
                               UserInteractions = state.UserInteractions @ [ userInteraction ] |}
                        tail

                | (TempPriority priority, user) :: tail ->
                    let priority =
                        TaskPriorityValue(getPriorityValue priority)
                        |> Some

                    loop {| state with Priority = priority |} tail

                | (TempTag information, user) :: tail -> loop state tail

                | (TempTaskField field, user) :: tail ->
                    match field with
                    | TempTaskFieldScheduling (scheduling, start) -> loop {| state with Scheduling = scheduling |} tail

                    | TempTaskFieldPendingAfter start ->
                        loop
                            {| state with
                                   PendingAfter = Some start |}
                            tail

                    | TempTaskFieldMissedAfter start ->
                        loop
                            {| state with
                                   MissedAfter = Some start |}
                            tail

                    | TempTaskFieldDuration minutes -> loop {| state with Duration = Some minutes |} tail

                | [] ->
                    let sortedSessions =
                        state.Sessions
                        |> List.sortBy (fun (TaskSession start) -> start.DateTime)

                    let priority =
                        state.Priority
                        |> Option.defaultValue (TaskPriorityValue 0)

                    state.UserInteractions,
                    sortedSessions,
                    priority,
                    state.Scheduling,
                    state.PendingAfter,
                    state.MissedAfter,
                    state.Duration

            let state =
                {| UserInteractions = []
                   Sessions = []
                   Priority = None
                   Scheduling = task.Scheduling
                   PendingAfter = task.PendingAfter
                   MissedAfter = task.MissedAfter
                   Duration = task.Duration |}

            loop state events

        { Task =
              { task with
                    Scheduling = scheduling
                    PendingAfter = pendingAfter
                    MissedAfter = missedAfter
                    Duration = duration
                    Priority = priority }
          Sessions = sessions
          CellInteractions = []
          UserInteractions = userInteractions
          CellStateMap = Map.empty }




    let treeDataWithUser user taskTree =
        taskTree
        |> List.map (Tuple2.mapItem2 (List.map (Tuple2.mapItem2 (List.map (fun event -> event, user)))))

    let transformTreeData dayStart taskTree =
        let taskStateList =
            taskTree
            |> List.collect (fun (information, tasks) ->
                tasks
                |> List.map (fun (taskName, events: (TempTaskEvent * User) list) ->
                    let task =
                        { Task.Default with
                              Name = taskName
                              Information = information }

                    applyTaskEvents dayStart task events))

        let informationList =
            taskTree |> List.map fst |> List.distinct

        let taskOrderList =
            taskStateList
            |> List.map (fun taskState ->
                { Task = taskState.Task
                  Priority = TaskOrderPriority.Last })

        {| TaskStateList = taskStateList
           TaskOrderList = taskOrderList
           InformationList = informationList
           GetLivePosition = getLivePosition |}

    // How the HELL will I rewrite this? 🤦
    let transformTasks currentUser rawTreeData getTaskLinks =
        let mutable taskStateMap: Map<string, TaskState> = Map.empty
        let mutable taskStateList = []

        let mutable treeDataMaybe: {| GetLivePosition: unit -> FlukeDateTime
                                      InformationList: Information list
                                      TaskOrderList: TaskOrderEntry list
                                      TaskStateList: TaskState list |} option = None

        let getTask name =
            taskStateMap
            |> Map.tryFind name
            |> Option.map (fun x -> x.Task)
            |> Option.defaultValue Task.Default

        for _ in [ 0; 1 ] do
            let treeData =
                rawTreeData getTask
                |> treeDataWithUser currentUser
                |> transformTreeData Consts.dayStart
            //        let taskList = treeData.TaskStateList |> List.map (fun x -> x.Task)
            let taskStateList = treeData.TaskStateList

            let duplicated =
                taskStateList
                |> List.map (fun x -> x.Task.Name)
                |> List.groupBy id
                |> List.filter
                    (snd
                     >> List.length
                     >> fun n -> n > 1)
                |> List.map fst

            if not duplicated.IsEmpty
            then failwithf "Duplicated task names: %A" duplicated

            taskStateMap <-
                taskStateList
                |> List.map (fun x -> x.Task.Name, x)
                |> Map.ofList

            treeDataMaybe <- Some treeData



        let tasks = getTaskLinks getTask

        let taskOrderList = getTaskOrderList [] taskStateList []

        let newTreeData =
            treeDataMaybe
            |> Option.map (fun treeData ->
                {| treeData with
                       TaskOrderList = taskOrderList |})

        newTreeData.Value, tasks





    module Testing =
        module Consts =
            let testDayStart = flukeTime 12 00

        let createRenderLaneTestData (testData: {| Position: FlukeDateTime
                                                   Expected: (FlukeDate * CellStatus) list
                                                   Events: TempTaskEvent list
                                                   Task: Task |}) =
            let eventsWithUser =
                testData.Events
                |> List.map (fun x -> x, Users.testUser)

            {| TaskStateList = [ applyTaskEvents Consts.testDayStart testData.Task eventsWithUser ]
               TaskOrderList =
                   [ { Task = testData.Task
                       Priority = TaskOrderPriority.First } ]
               GetLivePosition = fun () -> testData.Position
               InformationList = [ testData.Task.Information ] |}


        let createSortLanesTestData (testData: {| Position: FlukeDateTime
                                                  Data: (Task * TempTaskEvent list) list
                                                  Expected: string list |}) =
            let taskStateList =
                testData.Data
                |> List.map (fun (task, events) ->
                    events
                    |> List.map (fun x -> x, Users.testUser)
                    |> applyTaskEvents Consts.testDayStart task)

            {| TaskStateList = taskStateList
               TaskOrderList =
                   testData.Data
                   |> List.map (fun (task, events) ->
                       { Task = task
                         Priority = TaskOrderPriority.Last })
               GetLivePosition = fun () -> testData.Position
               InformationList =
                   taskStateList
                   |> List.map (fun x -> x.Task.Information)
                   |> List.distinct |}

        let tempData =
            {| ManualTasks =
                   [ Project Projects.app_fluke,
                     [ "data management",
                       [ TempComment "mutability", Users.testUser
                         TempComment "initial default data (load the text first with tests)", Users.testUser ]
                       "cell selection (mouse, vim navigation)", []
                       "data structures performance", []
                       "side panel (journal, comments)", []
                       "add task priority (for randomization)", []
                       "persistence", [ TempComment "data encryption", Users.testUser ]
                       "vivaldi or firefox bookmark integration",
                       [ TempComment "browser.html javascript injection or browser extension", Users.testUser ]
                       "telegram integration (fast link sharing)", []
                       "mobile layout", []
                       "move fluke tasks to github issues", [] ]
                     Project Projects.blog, []
                     Project Projects.rebuild_website, [ "task1", [] ]
                     Area Areas.car, []
                     Area Areas.career, []
                     Area Areas.chores, []
                     Area Areas.fitness, []
                     Area Areas.food, []
                     Area Areas.finances, []
                     Area Areas.health, []
                     Area Areas.leisure, [ "watch_movie_foobar", [] ]
                     Area Areas.programming, []
                     Area Areas.travel, []
                     Area Areas.workflow, []
                     Area Areas.writing, []
                     Resource Resources.agile, []
                     Resource Resources.artificial_intelligence, []
                     Resource Resources.cloud, []
                     Resource Resources.communication, []
                     Resource Resources.docker, []
                     Resource Resources.fsharp,
                     [ "study: [choice, computation expressions]", []
                       "organize youtube playlists", [] ]
                     Resource Resources.linux, []
                     Resource Resources.music, []
                     Resource Resources.rust, []
                     Resource Resources.vim, []
                     Resource Resources.windows, [] ]
                   |> transformTreeData Consts.testDayStart
               RenderLaneTests =
                   {| Task =
                          { Task.Default with
                                Scheduling =
                                    Recurrency
                                        (Fixed [ Weekly DayOfWeek.Monday
                                                 Weekly DayOfWeek.Tuesday
                                                 Weekly DayOfWeek.Wednesday
                                                 Weekly DayOfWeek.Thursday
                                                 Weekly DayOfWeek.Friday ])
                                PendingAfter = Some(flukeTime 19 00) }
                      Position =
                          { Date = flukeDate 2020 Month.August 26
                            Time = Consts.testDayStart }
                      Expected =
                          [ flukeDate 2020 Month.August 25, Disabled
                            flukeDate 2020 Month.August 26, Suggested
                            flukeDate 2020 Month.August 27, Pending ]
                      Events = [] |}
                   |> createRenderLaneTestData
               SortLanesTests =

                   {| Position =
                          { Date = flukeDate 2020 Month.March 10
                            Time = flukeTime 14 00 }
                      Data =
                          [ { Task.Default with
                                  Name = "01"
                                  Scheduling = Manual WithSuggestion },
                            []

                            { Task.Default with
                                  Name = "02"
                                  Scheduling = Manual WithSuggestion },
                            [ TempStatusEntry(flukeDate 2020 Month.March 10, Postponed None)
                              TempStatusEntry(flukeDate 2020 Month.March 08, Postponed None) ]

                            { Task.Default with
                                  Name = "03"
                                  Scheduling = Manual WithoutSuggestion },
                            [ TempStatusEntry(flukeDate 2020 Month.March 09, ManualPending) ]

                            { Task.Default with
                                  Name = "04"
                                  Scheduling = Recurrency(Offset(Days 1))
                                  PendingAfter = flukeTime 20 00 |> Some },
                            []

                            { Task.Default with
                                  Name = "05"
                                  Scheduling = Manual WithoutSuggestion },
                            [ TempStatusEntry(flukeDate 2020 Month.March 10, ManualPending) ]

                            { Task.Default with
                                  Name = "06"
                                  Scheduling = Manual WithoutSuggestion },
                            [ TempStatusEntry(flukeDate 2020 Month.March 04, Postponed None)
                              TempStatusEntry(flukeDate 2020 Month.March 06, Dismissed) ]

                            { Task.Default with
                                  Name = "07"
                                  Scheduling = Recurrency(Offset(Days 4)) },
                            [ TempStatusEntry(flukeDate 2020 Month.March 08, Completed) ]

                            { Task.Default with
                                  Name = "08"
                                  Scheduling = Recurrency(Offset(Days 2)) },
                            [ TempStatusEntry(flukeDate 2020 Month.March 10, Completed) ]

                            { Task.Default with
                                  Name = "09"
                                  Scheduling = Recurrency(Offset(Days 2)) },
                            [ TempStatusEntry(flukeDate 2020 Month.March 10, Dismissed) ]

                            { Task.Default with
                                  Name = "10"
                                  Scheduling = Recurrency(Offset(Days 2)) },
                            [ TempStatusEntry(flukeDate 2020 Month.March 10, Postponed None) ]

                            { Task.Default with
                                  Name = "11"
                                  Scheduling = Recurrency(Offset(Days 1)) },
                            [ TempStatusEntry(flukeDate 2020 Month.March 10, Postponed(flukeTime 13 00 |> Some)) ]

                            { Task.Default with
                                  Name = "12"
                                  Scheduling = Manual WithoutSuggestion },
                            []

                            { Task.Default with
                                  Name = "13"
                                  Scheduling = Recurrency(Fixed [ Weekly DayOfWeek.Tuesday ]) },
                            []

                            { Task.Default with
                                  Name = "14"
                                  Scheduling = Recurrency(Fixed [ Weekly DayOfWeek.Wednesday ]) },
                            []

                            { Task.Default with
                                  Name = "15"
                                  Scheduling = Recurrency(Fixed [ Weekly DayOfWeek.Friday ]) },
                            [ TempStatusEntry(flukeDate 2020 Month.March 07, Postponed None)
                              TempStatusEntry(flukeDate 2020 Month.March 09, Dismissed) ]

                            { Task.Default with
                                  Name = "16"
                                  Scheduling = Recurrency(Offset(Days 1))
                                  MissedAfter = (flukeTime 13 00 |> Some) },
                            []

                            { Task.Default with
                                  Name = "17"
                                  Scheduling = Recurrency(Offset(Days 1)) },
                            [ TempStatusEntry(flukeDate 2020 Month.March 10, Postponed(flukeTime 15 00 |> Some)) ]

                            { Task.Default with
                                  Name = "18"
                                  Scheduling = Recurrency(Offset(Days 1)) },
                            [] ]
                      Expected =
                          [ "16"
                            "05"
                            "03"
                            "11"
                            "13"
                            "18"
                            "17"
                            "04"
                            "01"
                            "02"
                            "10"
                            "08"
                            "09"
                            "06"
                            "07"
                            "12"
                            "14"
                            "15" ] |}

                   |> createSortLanesTestData

                    |}
