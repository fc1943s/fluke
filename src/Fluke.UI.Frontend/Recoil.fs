namespace Fluke.UI.Frontend

open Feliz.Router


#nowarn "40"

open System
open System.Collections.Generic
open FSharpPlus
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.DateFunctions
open Suigetsu.Core


module Recoil =
    open Model
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    module Profiling =
        let private initialTicks = DateTime.Now.Ticks

        let private ticksDiff ticks = int64 (TimeSpan(ticks - initialTicks).TotalMilliseconds)

        let internal profilingState =
            {|
                CallCount = Dictionary ()
                Timestamps = List<string * int64> ()
            |}

        Ext.setDom (nameof profilingState) profilingState

        let internal addCount id =
            match profilingState.CallCount.ContainsKey id with
            | false -> profilingState.CallCount.[id] <- 1
            | true -> profilingState.CallCount.[id] <- profilingState.CallCount.[id] + 1

        let internal addTimestamp id = profilingState.Timestamps.Add (id, ticksDiff DateTime.Now.Ticks)

        addTimestamp "Init"



    module FakeBackend =


        let rec filterTaskStateList view dateRange (taskStateList: TaskState list) =
            printfn "DR %A" dateRange
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

        let getState (input: {| User: User
                                DateSequence: FlukeDate list
                                View: View
                                Position: FlukeDateTime
                                GetLivePosition: unit -> FlukeDateTime
                                TreeStateMap: Map<TreeId, TreeState>
                                TreeSelectionIds: Set<TreeId> |}) =
            //            let treeSelectionIds =
//                input.State.Session.TreeSelection
//                |> Set.map (fun treeState -> treeState.Id)
//
            let treeSelection =
                input.TreeSelectionIds
                |> Set.map (fun treeId -> input.TreeStateMap.[treeId])
                |> Set.toList

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
                            CellStateMap = TempData.mergeCellStateMap taskState.CellStateMap newCellStateMap
                        }))

            // TODO: this might be needed
            let informationStateMap, taskStateMap =
                ((Map.empty, Map.empty), treeSelection)
                ||> List.fold (fun (informationStateMap, taskStateMap) treeState ->
                        match treeState with
                        | treeState when hasAccess treeState input.User ->
                            let newInformationStateMap =
                                TempData.mergeInformationStateMap informationStateMap treeState.InformationStateMap

                            let newTaskStateMap = TempData.mergeTaskStateMap taskStateMap treeState.TaskStateMap
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

            printfn "getTree %A" (taskStateList.Length, filteredTaskStateList.Length)

            let filteredLanes =
                filteredTaskStateList
                |> List.map (fun taskState ->
                    Rendering.renderLane input.User.DayStart input.Position input.DateSequence taskState)

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

            let newSession =
                {
                    User = Some input.User
                    InformationStateMap = newInformationStateMap
                    TaskStateMap = newTaskStateMap
                    TaskList = newTaskList
                    GetLivePosition = input.GetLivePosition
                    TreeStateMap = input.TreeStateMap
                    TreeSelection = treeSelection |> Set.ofList
                }

            let newState = { Session = newSession }

            newState




    module Atoms =
        let atomFamilyFn (fn: 'a -> 'b) =
            atomFamily {
                key (string <| Guid.NewGuid ())
                def fn
            }

        module rec Information =
            type InformationId = InformationId of id: string

            let rec wrappedInformation =
                atomFamilyFn
                <| fun (_informationId: InformationId) ->
                    Profiling.addCount (nameof wrappedInformation)
                    Area (Area.Default, [])

            let rec attachments =
                atomFamilyFn
                <| fun (_informationId: InformationId) ->
                    Profiling.addCount (nameof attachments)
                    []


            let rec informationId (information: Information): InformationId =
                match information with
                | Project ({ Name = ProjectName name }, _) -> sprintf "%s/%s" information.KindName name
                | Area ({ Name = AreaName name }, _) -> sprintf "%s/%s" information.KindName name
                | Resource ({ Name = ResourceName name }, _) -> sprintf "%s/%s" information.KindName name
                | Archive x ->
                    let (InformationId archiveId) = informationId x
                    sprintf "%s/%s" information.KindName archiveId
                |> InformationId


        module rec Task =
            type TaskId = TaskId of informationName: InformationName * taskName: TaskName

            let rec informationId =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (nameof informationId)
                    Information.informationId Task.Default.Information


            let rec name =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (nameof name)
                    Task.Default.Name


            let rec scheduling =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (nameof scheduling)
                    Task.Default.Scheduling


            let rec pendingAfter =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (nameof pendingAfter)
                    Task.Default.PendingAfter


            let rec missedAfter =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (nameof missedAfter)
                    Task.Default.MissedAfter


            let rec priority =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (nameof priority)
                    Task.Default.Priority


            //            let rec sessionsFamily =
//                atomFamily {
//                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof sessionsFamily))
//                    def (fun (_taskId: TaskId) ->
//                            Profiling.addCount (nameof sessionsFamily)
//                            []) // TODO: move from here?
//                }
            //            let rec commentsFamily = atomFamily {
//                key (sprintf "%s/%s" (nameof RecoilTask) (nameof commentsFamily))
//                def (fun (_taskId: TaskId) -> Profiling.addCount  (nameof commentsFamily); []) // TODO: move from here?
//            }
            let rec attachments =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (nameof attachments)
                    [] // TODO: move from here?


            let rec duration =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (nameof duration)
                    Task.Default.Duration


            let taskId (task: Task) = TaskId (task.Information.Name, task.Name)

        module rec User =

            let rec color =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof color)
                    UserColor.Black


            let rec weekStart =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof weekStart)
                    DayOfWeek.Sunday


            let rec dayStart =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof dayStart)
                    FlukeTime.Create 04 00


            let rec sessionLength =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof sessionLength)
                    Minute 25.


            let rec sessionBreakLength =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof sessionBreakLength)
                    Minute 5.




        module rec Session =

            let rec user =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof user)
                    None


            let rec treeSelectionIds =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof treeSelectionIds)
                    (Set.empty: Set<TreeId>)


            let rec availableTreeIds =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof availableTreeIds)
                    []


            let rec taskIdList =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (nameof taskIdList)
                    []




        module rec Cell =
            type CellId = CellId of id: string

            let rec taskId =
                atomFamilyFn
                <| fun (_cellId: CellId) ->
                    Profiling.addCount (nameof taskId)
                    Task.TaskId (InformationName "", TaskName "")


            let rec date =
                atomFamilyFn
                <| fun (_cellId: CellId) ->
                    Profiling.addCount (nameof date)
                    FlukeDate.MinValue


            let rec status =
                atomFamilyFn
                <| fun (_cellId: CellId) ->
                    Profiling.addCount (nameof status)
                    Disabled


            let rec attachments =
                atomFamilyFn
                <| fun (_cellId: CellId) ->
                    Profiling.addCount (nameof attachments)
                    []: Attachment list


            let rec sessions =
                atomFamilyFn
                <| fun (_cellId: CellId) ->
                    Profiling.addCount (nameof sessions)
                    []: TaskSession list


            let rec selected =
                atomFamilyFn
                <| fun (_cellId: CellId) ->
                    Profiling.addCount (nameof selected)
                    false



            let cellId (Task.TaskId (InformationName informationName, TaskName taskName)) (DateId referenceDay) =
                CellId (sprintf "%s/%s/%s" informationName taskName (referenceDay.DateTime.Format "yyyy-MM-dd"))


        module rec Tree =
            //            type TreeId = TreeId of id: string

            //        type TreeState =
//            {
//                Id: TreeId
//                Name: TreeName
//                Owner: User
//                SharedWith: TreeAccess list
//                //                Position: FlukeDateTime option
//                InformationStateMap: Map<Information, InformationState>
//                TaskStateMap: Map<Task, TaskState>
//            }

            let rec name =
                atomFamilyFn
                <| fun (_treeId: TreeId) ->
                    Profiling.addCount (nameof name)
                    TreeName ""


            let rec owner =
                atomFamilyFn
                <| fun (_treeId: TreeId) ->
                    Profiling.addCount (nameof owner)
                    None


            let rec sharedWith =
                atomFamilyFn
                <| fun (_treeId: TreeId) ->
                    Profiling.addCount (nameof sharedWith)
                    TreeAccess.Public


            let rec position =
                atomFamilyFn
                <| fun (_treeId: TreeId) ->
                    Profiling.addCount (nameof position)
                    None



        //            let treeId owner name =
//                TreeId (sprintf "%s/%s" owner.Username name)
//

        let rec internal debug =
            atom {
                key ("atom/" + nameof debug)
                def false
                local_storage
            }

        let rec internal path =
            atom {
                key ("atom/" + nameof path)
                def (Router.currentPath ())
            }

        let rec internal getLivePosition =
            atom {
                key ("atom/" + nameof getLivePosition)
                def
                    ({|
                         Get = fun () -> FlukeDateTime.FromDateTime DateTime.MinValue
                     |})
            }

        let rec internal username =
            atom {
                key ("atom/" + nameof username)
                def None
            }

        let rec internal cellSize =
            atom {
                key ("atom/" + nameof cellSize)
                def 17
            }

        let rec internal state =
            atom {
                key ("atom/" + nameof state)
                def (None: State option)
            }

        let rec internal selection =
            atom {
                key ("atom/" + nameof selection)
                def (Map.empty: Map<Task.TaskId, Set<FlukeDate>>)
            }

        let rec internal ctrlPressed =
            atom {
                key ("atom/" + nameof ctrlPressed)
                def false
            }

        let rec internal shiftPressed =
            atom {
                key ("atom/" + nameof shiftPressed)
                def false
            }

        let rec internal positionTrigger =
            atom {
                key ("atom/" + nameof positionTrigger)
                def 0
            }


    module Selectors =

        let rec view =
            selector {
                key ("selector/" + nameof view)
                get (fun getter ->
                        let path = getter.get Atoms.path

                        let result =
                            match path with
                            | [ "view"; "Calendar" ] -> View.Calendar
                            | [ "view"; "Groups" ] -> View.Groups
                            | [ "view"; "Tasks" ] -> View.Tasks
                            | [ "view"; "Week" ] -> View.Week
                            | _ -> View.Calendar

                        Profiling.addCount (nameof view)
                        result)
            }

        //        let rec user =
//            selector {
//                key ("selector/" + nameof user)
//                get (fun getter ->
//                        let state = getter.get Atoms.state
//                        let username = getter.get Atoms.username
//
//                        let result =
//                            match state with
//                            | Some state ->
//                                match username, state.Session.User with
//                                | Some username, Some user when user.Username = username -> Some user
//                                | _ -> None
//                            | None -> None
//
//                        Profiling.addCount (nameof user)
//                        result)
//            }

        let rec position =
            selector {
                key ("selector/" + nameof position)
                get (fun getter ->
                        let _positionTrigger = getter.get Atoms.positionTrigger
                        let getLivePosition = getter.get Atoms.getLivePosition

                        let result = Some <| getLivePosition.Get ()

                        Profiling.addCount (nameof position)
                        result)
                set (fun setter _newValue ->
                        setter.set (Atoms.positionTrigger, (fun x -> x + 1))
                        Profiling.addCount (nameof position + " (SET)"))
            }

        let rec dateSequence =
            selector {
                key ("selector/" + nameof dateSequence)
                get (fun getter ->
                        let position = getter.get position

                        let result =
                            match position with
                            | None -> []
                            | Some position ->
                                [
                                    position.Date
                                ]
                                |> Rendering.getDateSequence (45, 20)

                        Profiling.addCount (nameof dateSequence)
                        result)
            }

        //        let rec state =
//            selector {
//                key ("selector/" + nameof state)
//                get (fun getter ->
//                        let state = getter.get Atoms.state
//                        Profiling.addCount (nameof state)
//                        state)
//                set (fun setter (newValue: State option) ->
//                        let dateSequence = setter.get dateSequence
//
//                        Profiling.addTimestamp "state.set[0]"
//
//                        printfn
//                            "dateSequence tree newValue==none=%A dateSequence.length=%A"
//                            newValue.IsNone
//                            dateSequence.Length
//
//                        match newValue with
//                        | Some state ->
//                            match state.Session with
//                            | { User = None } ->
//                                setter.set (Atoms.state, None)
//                                setter.reset Atoms.getLivePosition
//                                setter.set (Atoms.username, None)
//                            | { User = Some user } as session ->
//                                setter.set (Atoms.state, newValue)
//                                setter.set (Atoms.getLivePosition, {| Get = session.GetLivePosition |})
//                                setter.set (Atoms.username, Some user.Username)
//
//                                let recoilSession = setter.get (Atoms.RecoilSession.sessionFamily user.Username)
//
//                                let treeSelectionIds =
//                                    state.Session.TreeSelection
//                                    |> Set.map (fun treeState -> treeState.Id)
//
//                                let availableTreeIds =
//                                    state.Session.TreeStateMap
//                                    |> Map.values
//                                    |> Seq.map (fun treeState -> treeState.Id)
//                                    |> Seq.toList
//
//                                let taskIdList =
//                                    state.Session.TaskList
//                                    |> List.map (fun task -> Atoms.RecoilTask.taskId task)
//
//                                setter.set (recoilSession.User, Some user)
//                                setter.set (recoilSession.TreeSelectionIds, treeSelectionIds)
//                                setter.set (recoilSession.AvailableTreeIds, availableTreeIds)
//                                setter.set (recoilSession.TaskIdList, taskIdList)
//
//                            let recoilInformationMap =
//                                state.Session.TaskList
//                                |> Seq.map (fun task -> task.Information)
//                                |> Seq.distinct
//                                |> Seq.map (fun information -> state.Session.InformationStateMap.[information])
//                                |> Seq.map (fun informationState ->
//
//
//                                    let informationId =
//                                        Atoms.RecoilInformation.informationId informationState.Information
//
//                                    setter.set
//                                        (Atoms.RecoilInformation.wrappedInformation informationId,
//                                         informationState.Information)
//                                    setter.set
//                                        (Atoms.RecoilInformation.attachments informationId, informationState.Attachments)
//                                    informationState.Information, informationId)
//                                |> Map.ofSeq
//
//                            Profiling.addTimestamp "state.set[1]"
//
//                            state.Session.TaskList
//                            |> List.map (fun task -> state.Session.TaskStateMap.[task])
//                            |> List.iter (fun taskState ->
//                                let task = taskState.Task
//                                let taskId = Atoms.RecoilTask.taskId task
//
//                                let recoilTask = setter.get (Atoms.RecoilTask.taskFamily taskId)
//
//                                setter.set (recoilTask.Id, taskId)
//                                setter.set (recoilTask.Name, task.Name)
//                                setter.set (recoilTask.InformationId, recoilInformationMap.[task.Information])
//                                setter.set (recoilTask.PendingAfter, task.PendingAfter)
//                                setter.set (recoilTask.MissedAfter, task.MissedAfter)
//                                setter.set (recoilTask.Scheduling, task.Scheduling)
//                                setter.set (recoilTask.Priority, task.Priority)
//                                //                                setter.set (recoilTask.Sessions, taskState.Sessions) // TODO: move from here
//                                setter.set (recoilTask.Attachments, taskState.Attachments)
//                                setter.set (recoilTask.Duration, task.Duration)
//
//                                dateSequence
//                                |> List.iter (fun date ->
//                                    let cellId = Atoms.RecoilCell.cellId taskId (DateId date)
//
//                                    let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)
//
//                                    setter.set (recoilCell.Id, cellId)
//                                    setter.set (recoilCell.TaskId, taskId)
//                                    setter.set (recoilCell.Date, date))
//
//                                taskState.CellStateMap
//                                |> Map.filter (fun dateId cellState ->
//                                    (<>) cellState.Status Disabled
//                                    || not cellState.Attachments.IsEmpty
//                                    || not cellState.Sessions.IsEmpty)
//                                |> Map.iter (fun dateId cellState ->
//                                    let cellId = Atoms.RecoilCell.cellId taskId dateId
//
//                                    let recoilCell = setter.get (Atoms.RecoilCell.cellFamily cellId)
//
//                                    setter.set (recoilCell.Status, cellState.Status)
//                                    setter.set (recoilCell.Attachments, cellState.Attachments)
//                                    setter.set (recoilCell.Sessions, cellState.Sessions)
//                                    setter.set (recoilCell.Selected, false)))
//
//
//                            state.Session.TreeStateMap
//                            |> Map.values
//                            |> Seq.iter (fun treeState ->
//                                let recoilTree = setter.get (Atoms.RecoilTree.treeFamily treeState.Id)
//
//                                setter.set (recoilTree.Name, treeState.Name)
//                                setter.set (recoilTree.Owner, Some treeState.Owner)
//                                setter.set (recoilTree.SharedWith, treeState.SharedWith)
//                                setter.set (recoilTree.Position, treeState.Position))
//
//                        | _ -> ()
//
//                        Profiling.addTimestamp "state.set[2]"
//                        Profiling.addCount (nameof state + " (SET)"))
//            }

        /// [1]
        let rec selection =
            selector {
                key ("selector/" + nameof selection)
                get (fun getter ->
                        let selection = getter.get Atoms.selection
                        Profiling.addCount (nameof selection)
                        selection)
                set (fun setter (newSelection: Map<Atoms.Task.TaskId, Set<FlukeDate>>) ->
                        let selection = setter.get Atoms.selection

                        let operationsByTask =
                            let taskIdSet =
                                seq {
                                    yield! selection |> Map.keys
                                    yield! newSelection |> Map.keys
                                }
                                |> Set.ofSeq

                            taskIdSet
                            |> Seq.map (fun taskId ->
                                let taskSelection =
                                    selection
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let newTaskSelection =
                                    newSelection
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let datesToIgnore = Set.intersect taskSelection newTaskSelection

                                let datesToUnselect =
                                    datesToIgnore
                                    |> Set.difference taskSelection
                                    |> Seq.map (fun date -> date, false)

                                let datesToSelect =
                                    datesToIgnore
                                    |> Set.difference newTaskSelection
                                    |> Seq.map (fun date -> date, true)

                                taskId, Seq.append datesToSelect datesToUnselect)

                        operationsByTask
                        |> Seq.iter (fun (taskId, operations) ->
                            operations
                            |> Seq.iter (fun (date, selected) ->
                                let cellId = Atoms.Cell.cellId taskId (DateId date)
                                setter.set (Atoms.Cell.selected cellId, selected)))

                        setter.set (Atoms.selection, newSelection)
                        Profiling.addCount (nameof selection + " (SET)"))
            }
        /// [3]
//        let rec selectedCells =
//            selector {
//                key ("selector/" + nameof selectedCells)
//                get (fun getter ->
//                        let selection = Recoil.useValue selection
//
//                        let selectionCellIds =
//                            selection
//                            |> Seq.collect (fun (KeyValue (taskId, dates)) ->
//                                dates
//                                |> Seq.map DateId
//                                |> Seq.map (Atoms.RecoilCell.cellId taskId))
//
//                        let result =
//                            selectionCellIds
//                            |> Seq.map (Atoms.RecoilCell.cellFamily >> getter.get)
//                            |> Seq.toList
//
//                        Profiling.addCount (nameof selectedCells)
//                        result)
//            }
        /// [4]
//        // TODO: Remove View and check performance
//        let rec stateAsync =
//            selectorFamily {
//                key ("selectorFamily/" + nameof stateAsync)
//                get (fun (view: View) getter ->
//                        async {
//                            Profiling.addTimestamp "stateAsync.get[0]"
//                            let state = getter.get Atoms.state
//                            let position = getter.get position
//
//                            let result =
//                                match state, position with
//                                | Some state, Some position ->
//                                    match state.Session.User with
//                                    | Some user ->
//                                        let dateSequence = getter.get dateSequence
//
//                                        let treeSelectionIds =
//                                            getter.get (Atoms.RecoilSession.treeSelectionIdsFamily user.Username)
//
//                                        let newTreeSelectionIds =
//                                            if treeSelectionIds.IsEmpty then
//                                                state.Session.TreeSelection
//                                                |> Set.map (fun treeState -> treeState.Id)
//                                            else
//                                                treeSelectionIds
//
//                                        Profiling.addTimestamp "stateAsync.get[1]"
//
//                                        let newState =
//                                            FakeBackend.getState
//                                                {|
//                                                    User = user
//                                                    DateSequence = dateSequence
//                                                    View = view
//                                                    Position = position
//                                                    TreeSelectionIds = newTreeSelectionIds
//                                                    TreeStateMap = state.Session.TreeStateMap
//                                                    GetLivePosition = state.Session.GetLivePosition
//                                                |}
//
//                                        Profiling.addTimestamp "stateAsync.get[2]"
//                                        Profiling.addCount (nameof stateAsync)
//
//                                        printfn
//                                            "B %A A %A"
//                                            state.Session.InformationStateMap.Count
//                                            newState.Session.InformationStateMap.Count
//
//                                        Some newState
//                                    | _ -> None
//                                | _ -> None
//
//                            return result
//                        })
//            }

        module rec RecoilFlukeDate =
            let isToday =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof isToday))
                    get (fun (date: FlukeDate) getter ->
                            let username = getter.get Atoms.username
                            let position = getter.get position

                            let result =
                                match username, position with
                                | Some username, Some position ->
                                    let dayStart = getter.get (Atoms.User.dayStart username)

                                    Domain.UserInteraction.isToday dayStart position (DateId date)
                                | _ -> false

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof isToday))
                            result)
                }

            let rec hasSelection =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof hasSelection))
                    get (fun (date: FlukeDate) getter ->
                            let selection = getter.get selection

                            let result =
                                selection
                                |> Map.values
                                |> Seq.exists (fun dateSequence -> dateSequence |> Set.contains date)

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilFlukeDate) (nameof hasSelection))
                            result

                        )
                }

        module rec RecoilSession =
            let rec state =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilSession) (nameof state))
                    get (fun (username: Username) getter ->
                            let state = getter.get Atoms.state
                            let user = getter.get (Atoms.Session.user username)
                            let dateSequence = getter.get dateSequence
                            let view = getter.get view
                            let position = getter.get position
                            let getLivePosition = (getter.get Atoms.getLivePosition).Get
                            let treeSelectionIds = getter.get (Atoms.Session.treeSelectionIds username)

                            let result =
                                match user, position, state with
                                | Some user, Some position, Some state ->
                                    let newTreeSelectionIds =
                                        if treeSelectionIds.IsEmpty then
                                            state.Session.TreeSelection
                                            |> Set.map (fun treeState -> treeState.Id)
                                        else
                                            treeSelectionIds

                                    let newState =
                                        FakeBackend.getState
                                            {|
                                                User = user
                                                DateSequence = dateSequence
                                                View = view
                                                Position = position
                                                TreeSelectionIds = newTreeSelectionIds
                                                TreeStateMap = state.Session.TreeStateMap
                                                GetLivePosition = getLivePosition
                                            |}

                                    Some newState
                                | _ -> None

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilSession) (nameof state))
                            result)
                    set (fun (username: Username) setter (newValue: State option) ->
                            let dateSequence = setter.get dateSequence

                            Profiling.addTimestamp "state.set[0]"

                            printfn
                                "dateSequence tree newValue==none=%A dateSequence.length=%A"
                                newValue.IsNone
                                dateSequence.Length

                            match newValue with
                            | Some state ->
                                match state.Session with
                                | { User = Some user } as session when user.Username = username ->
                                    //                                    setter.set (Atoms.state, newValue)
//                                    setter.set (Atoms.getLivePosition, {| Get = session.GetLivePosition |})
//                                    setter.set (Atoms.username, Some user.Username)

                                    let treeSelectionIds =
                                        state.Session.TreeSelection
                                        |> Set.map (fun treeState -> treeState.Id)

                                    let availableTreeIds =
                                        state.Session.TreeStateMap
                                        |> Map.values
                                        |> Seq.map (fun treeState -> treeState.Id)
                                        |> Seq.toList

                                    let taskIdList =
                                        state.Session.TaskList
                                        |> List.map (fun task -> Atoms.Task.taskId task)

                                    //                                    setter.set (recoilSession.User, Some user)
                                    setter.set (Atoms.Session.treeSelectionIds user.Username, treeSelectionIds)
                                    setter.set (Atoms.Session.availableTreeIds user.Username, availableTreeIds)
                                    setter.set (Atoms.Session.taskIdList user.Username, taskIdList)

                                    let recoilInformationMap =
                                        state.Session.TaskList
                                        |> Seq.map (fun task -> task.Information)
                                        |> Seq.distinct
                                        |> Seq.map (fun information -> state.Session.InformationStateMap.[information])
                                        |> Seq.map (fun informationState ->


                                            let informationId =
                                                Atoms.Information.informationId informationState.Information

                                            setter.set
                                                (Atoms.Information.wrappedInformation informationId,
                                                 informationState.Information)
                                            setter.set
                                                (Atoms.Information.attachments informationId,
                                                 informationState.Attachments)
                                            informationState.Information, informationId)
                                        |> Map.ofSeq

                                    Profiling.addTimestamp "state.set[1]"

                                    state.Session.TaskList
                                    |> List.map (fun task -> state.Session.TaskStateMap.[task])
                                    |> List.iter (fun taskState ->
                                        let task = taskState.Task
                                        let taskId = Atoms.Task.taskId task

                                        let recoilTask = setter.get (Atoms.Task.name taskId)

                                        setter.set (Atoms.Task.name taskId, task.Name)
                                        setter.set
                                            (Atoms.Task.informationId taskId, recoilInformationMap.[task.Information])
                                        setter.set (Atoms.Task.pendingAfter taskId, task.PendingAfter)
                                        setter.set (Atoms.Task.missedAfter taskId, task.MissedAfter)
                                        setter.set (Atoms.Task.scheduling taskId, task.Scheduling)
                                        setter.set (Atoms.Task.priority taskId, task.Priority)
                                        //                                setter.set (recoilTask.Sessions, taskState.Sessions) // TODO: move from here
                                        setter.set (Atoms.Task.attachments taskId, taskState.Attachments)
                                        setter.set (Atoms.Task.duration taskId, task.Duration)

                                        dateSequence
                                        |> List.iter (fun date ->
                                            let cellId = Atoms.Cell.cellId taskId (DateId date)

                                            setter.set (Atoms.Cell.taskId cellId, taskId)
                                            setter.set (Atoms.Cell.date cellId, date))

                                        taskState.CellStateMap
                                        |> Map.filter (fun dateId cellState ->
                                            (<>) cellState.Status Disabled
                                            || not cellState.Attachments.IsEmpty
                                            || not cellState.Sessions.IsEmpty)
                                        |> Map.iter (fun dateId cellState ->
                                            let cellId = Atoms.Cell.cellId taskId dateId

                                            setter.set (Atoms.Cell.status cellId, cellState.Status)
                                            setter.set (Atoms.Cell.attachments cellId, cellState.Attachments)
                                            setter.set (Atoms.Cell.sessions cellId, cellState.Sessions)
                                            setter.set (Atoms.Cell.selected cellId, false)))


                                    state.Session.TreeStateMap
                                    |> Map.values
                                    |> Seq.iter (fun treeState ->
                                        setter.set (Atoms.Tree.name treeState.Id, treeState.Name)
                                        setter.set (Atoms.Tree.owner treeState.Id, Some treeState.Owner)
                                        setter.set (Atoms.Tree.sharedWith treeState.Id, treeState.SharedWith)
                                        setter.set (Atoms.Tree.position treeState.Id, treeState.Position))

                                | _ ->
                                    setter.set (Atoms.state, None)
                                    setter.reset Atoms.getLivePosition
                                    setter.set (Atoms.username, None)
                            | _ -> ()

                            Profiling.addTimestamp "state.set[2]"
                            Profiling.addCount (nameof state + " (SET)"))
                }

            let rec currentSession =
                selectorFamily {
                    key ("selector/" + nameof currentSession)
                    get (fun (username: Username) getter ->
                            let state = getter.get (state username)

                            let result =
                                match state with
                                | Some state -> Some state.Session
                                | None -> None

                            Profiling.addCount (nameof currentSession)
                            result)
                }


        module rec RecoilInformation =
            ()

        module rec RecoilTask =
            let rec lastSession =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof lastSession))
                    get (fun (taskId: Atoms.Task.TaskId) getter ->
                            let username = getter.get Atoms.username
                            match username with
                            | Some username ->
                                let state = getter.get (RecoilSession.state username)

                                let sessions =
                                    match state with
                                    | Some state ->
                                        state.Session.TaskStateMap
                                        |> Map.tryPick (fun task taskState ->
                                            let taskId' = Atoms.Task.taskId task
                                            if taskId = taskId' then
                                                Some taskState
                                            else
                                                None)
                                        |> Option.map (fun taskState -> taskState.Sessions)
                                        |> Option.defaultValue []
                                    | None -> []

                                let result =
                                    sessions
                                    |> List.sortByDescending (fun (TaskSession (start, _, _)) -> start.DateTime)
                                    |> List.tryHead

                                Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof lastSession))
                                result
                            | None -> None)
                }

            let rec activeSession =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof activeSession))
                    get (fun (taskId: Atoms.Task.TaskId) getter ->
                            let position = getter.get position
                            let lastSession = getter.get (lastSession taskId)


                            let result =
                                match position, lastSession with
                                | Some position, Some lastSession ->
                                    let (TaskSession (start, (Minute duration), (Minute breakDuration))) = lastSession

                                    let currentDuration = (position.DateTime - start.DateTime).TotalMinutes

                                    let active = currentDuration < duration + breakDuration

                                    match active with
                                    | true -> Some currentDuration
                                    | false -> None

                                | _ -> None

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof activeSession))

                            result)
                }

            let rec showUser =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof showUser))
                    get (fun (taskId: Atoms.Task.TaskId) getter ->
                            let username = getter.get Atoms.username
                            match username with
                            | Some username ->
                                let currentSession = getter.get (RecoilSession.currentSession username)

                                let result =
                                    currentSession
                                    |> Option.map (fun session -> session.TaskStateMap)
                                    |> Option.defaultValue Map.empty
                                    |> Map.tryPick (fun task taskState ->
                                        if taskId = Atoms.Task.taskId task then
                                            Some taskState
                                        else
                                            None)
                                    |> Option.map (fun taskState ->
                                        let usersCount =
                                            taskState.CellStateMap
                                            |> Map.values
                                            |> Seq.map (fun cellState -> cellState.Status)
                                            |> Seq.choose (function
                                                | UserStatus (user, _) -> Some user
                                                | _ -> None)
                                            |> Seq.distinct
                                            |> Seq.length

                                        usersCount > 1)
                                    |> Option.defaultValue false

                                Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof showUser))
                                result
                            | None -> false)
                }

            let rec hasSelection =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilTask) (nameof hasSelection))
                    get (fun (taskId: Atoms.Task.TaskId) getter ->
                            let selection = getter.get selection

                            let result =
                                selection
                                |> Map.tryFind taskId
                                |> Option.defaultValue Set.empty
                                |> Set.isEmpty
                                |> not

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilTask) (nameof hasSelection))
                            result)
                }


        module rec RecoilTree =
            //            let rec taskListFamily =
//                selectorFamily {
//                    key (sprintf "%s/%s" (nameof RecoilTree) (nameof taskListFamily))
//                    get (fun (treeId: TreeId) getter ->
//                            let taskIdList =
//                                getter.get (Atoms.RecoilTree.taskIdListFamily treeId)
//
//                            let taskList =
//                                match state with
//                                | Some state ->
//                                    taskIdList
//                                    |> List.map (fun taskId ->
//                                        let task =
//                                            getter.get (Atoms.RecoilTask.taskFamily taskId)
//
//                                        let informationId = getter.get task.InformationId
//
//                                        let information =
//                                            getter.get (Atoms.RecoilInformation.informationFamily informationId)
//
//                                        {|
//                                            Id = taskId
//                                            Name = getter.get task.Name
//                                            Information = getter.get information.WrappedInformation
//                                            InformationAttachments = getter.get information.Attachments
//                                            Scheduling = getter.get task.Scheduling
//                                            PendingAfter = getter.get task.PendingAfter
//                                            MissedAfter = getter.get task.MissedAfter
//                                            Priority = getter.get task.Priority
//    //                                        Sessions = getter.get task.Sessions
//                                            Attachments = getter.get task.Attachments
//                                            Duration = getter.get task.Duration
//                                        |})
//                                | None -> []
//
//                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilTree) (nameof taskListFamily))
//                            taskList)
//                }
            ()
        //

        let rec currentTaskList =
            selector {
                key ("selector/" + nameof currentTaskList)
                get (fun getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)

                            let result =
                                taskIdList
                                |> List.map (fun taskId ->
                                    //                                    state.TaskStateMap.[task]
                                    let informationId = getter.get (Atoms.Task.informationId taskId)
                                    let information = Atoms.Information.wrappedInformation informationId
                                    let attachments = Atoms.Information.attachments informationId

                                    {|
                                        Id = taskId
                                        Name = getter.get (Atoms.Task.name taskId)
                                        Information = getter.get information
                                        InformationAttachments = getter.get attachments
                                        Scheduling = getter.get (Atoms.Task.scheduling taskId)
                                        PendingAfter = getter.get (Atoms.Task.pendingAfter taskId)
                                        MissedAfter = getter.get (Atoms.Task.missedAfter taskId)
                                        Priority = getter.get (Atoms.Task.priority taskId)
                                        Attachments = getter.get (Atoms.Task.attachments taskId)
                                        Duration = getter.get (Atoms.Task.duration taskId)
                                    |})

                            //
                            //                            |> List.map (fun task ->
                            //                                getter.get (Atoms.RecoilTask.taskFamily (Atoms.RecoilTask.taskId task)))

                            Profiling.addCount (nameof currentTaskList)
                            result
                        | None -> [])
            }

        let rec activeSessions =
            selector {
                key ("selector/" + nameof activeSessions)
                get (fun getter ->
                        let taskList = getter.get currentTaskList
                        let username = getter.get Atoms.username

                        let result =
                            match username with
                            | Some username ->
                                let sessionLength = getter.get (Atoms.User.sessionLength username)
                                let sessionBreakLength = getter.get (Atoms.User.sessionBreakLength username)
                                taskList
                                |> List.map (fun task ->
                                    let (TaskName taskName) = task.Name

                                    let duration = getter.get (RecoilTask.activeSession task.Id)

                                    duration
                                    |> Option.map (fun duration ->
                                        ActiveSession (taskName, Minute duration, sessionLength, sessionBreakLength)))
                                |> List.choose id
                            | _ -> []

                        Profiling.addCount (nameof activeSessions)
                        result)
            }

        let rec weekCellsMap =
            selector {
                key ("selector/" + nameof weekCellsMap)
                get (fun getter ->
                        let username = getter.get Atoms.username
                        let position = getter.get position
                        let taskList = getter.get currentTaskList

                        let result =
                            match username, position with
                            | Some username, Some position ->
                                let dayStart = getter.get (Atoms.User.dayStart username)
                                let weekStart = getter.get (Atoms.User.weekStart username)

                                let weeks =
                                    [
                                        -1 .. 1
                                    ]
                                    |> List.map (fun weekOffset ->
                                        let dateIdSequence =
                                            let rec getStartDate (date: DateTime) =
                                                if date.DayOfWeek = weekStart then
                                                    date
                                                else
                                                    getStartDate (date.AddDays -1)

                                            let startDate =
                                                dateId dayStart position
                                                |> fun (DateId referenceDay) ->
                                                    referenceDay.DateTime.AddDays (7 * weekOffset)
                                                |> getStartDate

                                            [
                                                0 .. 6
                                            ]
                                            |> List.map startDate.AddDays
                                            |> List.map FlukeDateTime.FromDateTime
                                            |> List.map (dateId dayStart)

                                        let result =
                                            taskList
                                            |> List.collect (fun task ->
                                                dateIdSequence
                                                |> List.map (fun ((DateId referenceDay) as dateId) ->
                                                    //                                                    let taskId = getter.get task.Id
                                                    let cellId = Atoms.Cell.cellId task.Id dateId

                                                    let status = getter.get (Atoms.Cell.status cellId)
                                                    let sessions = getter.get (Atoms.Cell.sessions cellId)
                                                    let attachments = getter.get (Atoms.Cell.attachments cellId)

                                                    let isToday = getter.get (RecoilFlukeDate.isToday referenceDay)

                                                    match status, sessions, attachments with
                                                    | (Disabled
                                                      | Suggested),
                                                      [],
                                                      [] -> None
                                                    | _ ->
                                                        {|
                                                            DateId = dateId
                                                            Task = task
                                                            Status = status
                                                            Sessions = sessions
                                                            IsToday = isToday
                                                            Attachments = attachments
                                                        |}
                                                        |> Some)
                                                |> List.choose id)
                                            |> List.groupBy (fun x -> x.DateId)
                                            |> List.map (fun (((DateId referenceDay) as dateId), cells) ->

                                                //                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList
                                                let taskSessions = cells |> List.collect (fun x -> x.Sessions)

                                                let sortedTasksMap =
                                                    cells
                                                    |> List.map (fun cell ->
                                                        let taskState =
                                                            //                                                            let informationId = getter.get cell.Task.InformationId
//
//                                                            let information =
//                                                                getter.get
//                                                                    (Atoms.RecoilInformation.informationFamily
//                                                                        informationId)

                                                            let task =
                                                                { Task.Default with
                                                                    Name = cell.Task.Name
                                                                    Information = cell.Task.Information
                                                                    Scheduling = cell.Task.Scheduling
                                                                    PendingAfter = cell.Task.PendingAfter
                                                                    MissedAfter = cell.Task.MissedAfter
                                                                    Priority = cell.Task.Priority
                                                                    Duration = cell.Task.Duration
                                                                }

                                                            {
                                                                Task = task
                                                                Sessions = taskSessions
                                                                Attachments = []
                                                                SortList = []
                                                                //                                                        UserInteractions = cell.Task.UserInteractions
                                                                InformationMap = Map.empty
                                                                CellStateMap = Map.empty
                                                            }

                                                        taskState,
                                                        [
                                                            { Task = taskState.Task; DateId = dateId }, cell.Status
                                                        ])
                                                    |> Sorting.sortLanesByTimeOfDay
                                                        dayStart
                                                           { Date = referenceDay; Time = dayStart }
                                                    |> List.indexed
                                                    |> List.map (fun (i, (taskState, _)) ->
                                                        Atoms.Task.taskId taskState.Task, i)
                                                    |> Map.ofList

                                                let newCells =
                                                    cells
                                                    |> List.sortBy (fun cell -> sortedTasksMap.[cell.Task.Id])

                                                dateId, newCells)
                                            |> Map.ofList

                                        result)

                                weeks
                            | _ -> []

                        Profiling.addCount (nameof weekCellsMap)
                        result)
            }


        module rec RecoilCell =
            let rec selected =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof RecoilCell) (nameof selected))
                    get (fun (cellId: Atoms.Cell.CellId) getter ->
                            let selected = getter.get (Atoms.Cell.selected cellId)

                            Profiling.addCount (sprintf "%s/%s" (nameof RecoilCell) (nameof selected))
                            selected)
                    set (fun (cellId: Atoms.Cell.CellId) setter (newValue: bool) ->
                            let ctrlPressed = setter.get Atoms.ctrlPressed
                            let shiftPressed = setter.get Atoms.shiftPressed

                            let date = setter.get (Atoms.Cell.date cellId)
                            let taskId = setter.get (Atoms.Cell.taskId cellId)

                            let newSelection =
                                let swapSelection oldSelection taskId date =
                                    let oldSet =
                                        oldSelection
                                        |> Map.tryFind taskId
                                        |> Option.defaultValue Set.empty

                                    let newSet =
                                        let fn =
                                            if newValue then
                                                Set.add
                                            else
                                                Set.remove

                                        fn date oldSet

                                    oldSelection |> Map.add taskId newSet

                                let taskList = setter.get currentTaskList
                                match shiftPressed, ctrlPressed with
                                | true, _ ->
                                    let oldSelection = setter.get Atoms.selection

                                    let selectionTaskList =
                                        taskList
                                        |> List.mapi (fun i task ->
                                            match oldSelection |> Map.tryFind taskId with
                                            | Some oldSelectionDates ->
                                                let selectionDates =
                                                    match oldSelectionDates.IsEmpty, task.Id = taskId with
                                                    | _, true -> oldSelectionDates |> Set.add date
                                                    | true, _ -> Set.empty
                                                    | false, _ -> oldSelectionDates

                                                {|
                                                    Index = i
                                                    TaskId = task.Id
                                                    Range = Set.minElement selectionDates, Set.maxElement selectionDates
                                                |}
                                                |> Some
                                            | _ -> None)
                                        |> List.choose id

                                    let minDates, maxDates =
                                        selectionTaskList
                                        |> List.map (fun selectionTask -> selectionTask.Range)
                                        |> List.unzip

                                    let minDate =
                                        match minDates with
                                        | [] -> None
                                        | x -> Some (List.min x)

                                    let maxDate =
                                        match maxDates with
                                        | [] -> None
                                        | x -> Some (List.max x)

                                    match minDate, maxDate with
                                    | Some minDate, Some maxDate ->
                                        let newSet =
                                            [
                                                minDate
                                                maxDate
                                            ]
                                            |> Rendering.getDateSequence (0, 0)
                                            |> Set.ofList

                                        taskList
                                        |> List.indexed
                                        |> List.skipWhile (fun (i, task) ->
                                            i < (selectionTaskList
                                                 |> List.tryHead
                                                 |> Option.map (fun x -> x.Index)
                                                 |> Option.defaultValue 0))
                                        |> List.takeWhile (fun (i, task) ->
                                            i
                                            <= (selectionTaskList
                                                |> List.tryLast
                                                |> Option.map (fun x -> x.Index)
                                                |> Option.defaultValue 0))
                                        |> List.map (fun (i, task) -> task.Id, newSet)
                                        |> Map.ofList
                                    | _ -> Map.empty

                                | false, false ->
                                    let newTaskSelection =
                                        if newValue then
                                            Set.singleton date
                                        else
                                            Set.empty

                                    [
                                        taskId, newTaskSelection
                                    ]
                                    |> Map.ofList
                                | false, true ->
                                    let oldSelection = setter.get Atoms.selection
                                    swapSelection oldSelection taskId date

                            setter.set (selection, newSelection)
                            Profiling.addCount (sprintf "%s/%s (SET)" (nameof RecoilCell) (nameof selected)))
                }

    /// [1]

    let initState (initializer: MutableSnapshot) =
        let baseState = RootPrivateData.State.getBaseState ()

        //        let state2 = {| User =state.User; TreeStateMap = state.TreeStateMap |}
//
//        let simpleJson = Fable.SimpleJson.SimpleJson.stringify state2
//        let thothJson = Thoth.Json.Encode.Auto.toString(4, state2)
//
        Ext.setDom (nameof baseState) baseState
        //        Browser.Dom.window?flukeStateSimple <- simpleJson
//        Browser.Dom.window?flukeStateThoth <- thothJson

        match baseState.Session.User with
        | Some user ->
            initializer.set (Atoms.state, Some baseState)
            initializer.set (Atoms.getLivePosition, {| Get = baseState.Session.GetLivePosition |})
            initializer.set (Atoms.username, Some user.Username)
            initializer.set (Atoms.Session.user user.Username, Some user)
        | None -> ()

    (************************* END *************************)





    //                    {|
//                        Name = "task1"
//                        Information = Area { Name = "area1" }
//                        Scheduling = Recurrency (Offset (Days 1))
//                        PendingAfter = None
//                        MissedAfter = None
//                        Priority = Critical10
//                        Duration = Some 30
//                        Sessions = [
//                            flukeDateTime 2020 Month.May 20 02 05
//                            flukeDateTime 2020 Month.May 31 00 09
//                            flukeDateTime 2020 Month.May 31 23 21
//                            flukeDateTime 2020 Month.June 04 01 16
//                            flukeDateTime 2020 Month.June 20 15 53
//                        ]
//                        Lane = [
//                            (FlukeDate.Create 2020 Month.June 13), Completed
//                        ]
//                        Comments = [
//                            Comment (TempData.Users.fc1943s, "fc1943s: task1 comment")
//                            Comment (TempData.Users.liryanne, "liryanne: task1 comment")
//                        ]
//                    |}
//
//                    {|
//                        Name = "task2"
//                        Information = Area { Name = "area2" }
//                        Scheduling = Recurrency (Offset (Days 1))
//                        PendingAfter = None
//                        MissedAfter = FlukeTime.Create 09 00 |> Some
//                        Priority = Critical10
//                        Duration = Some 5
//                        Sessions = []
//                        Lane = []
//                        Comments = []
//                    |}
//
//                {|
//                    Id = TreeId "liryanne/shared"
//                    Access = [ TreeAccess.Owner TempData.Users.liryanne
//                               TreeAccess.Admin TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//
//                {|
//                    Id = TreeId "fluke/samples/laneSorting/frequency"
//                    Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
//                               TreeAccess.ReadOnly TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//
//                {|
//                    Id = TreeId "fluke/samples/laneSorting/timeOfDay"
//                    Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
//                               TreeAccess.ReadOnly TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//            treeList
//            |> List.tryFind (fun tree -> tree.Id = treeId && hasAccess tree user)


    //        let state = {|
//            Input_User = TempData.Users.fc1943s
//            Input_View = View.Calendar
//            Input_Position = FlukeDate.Create 2020 Month.June 28
//            TreeList = [
//                {|
//                    Id = TreeId "fc1943s/tree/default"
//                    Access = [ TreeAccess.Owner TempData.Users.fc1943s ]
//                    InformationList = [
//                        {|
//                            Information = Area { Name = "area1" }
//                            Comments = [
//                                Comment (TempData.Users.fc1943s, "fc1943s: area1 area/information comment")
//                                Comment (TempData.Users.liryanne, "liryanne: area1 area/information comment")
//                            ]
//                        |}
//
//                        {|
//                            Information = Area { Name = "area2" }
//                            Comments = []
//                        |}
//                    ]
//                    Tasks = [
//                        {|
//                            Name = "task1"
//                            Information = Area { Name = "area1" }
//                            Scheduling = Recurrency (Offset (Days 1))
//                            PendingAfter = None
//                            MissedAfter = None
//                            Priority = Critical10
//                            Duration = Some 30
//                            Sessions = [
//                                flukeDateTime 2020 Month.May 20 02 05
//                                flukeDateTime 2020 Month.May 31 00 09
//                                flukeDateTime 2020 Month.May 31 23 21
//                                flukeDateTime 2020 Month.June 04 01 16
//                                flukeDateTime 2020 Month.June 20 15 53
//                            ]
//                            Lane = [
//                                (FlukeDate.Create 2020 Month.June 13), Completed
//                            ]
//                            Comments = [
//                                Comment (TempData.Users.fc1943s, "fc1943s: task1 comment")
//                                Comment (TempData.Users.liryanne, "liryanne: task1 comment")
//                            ]
//                        |}
//
//                        {|
//                            Name = "task2"
//                            Information = Area { Name = "area2" }
//                            Scheduling = Recurrency (Offset (Days 1))
//                            PendingAfter = None
//                            MissedAfter = FlukeTime.Create 09 00 |> Some
//                            Priority = Critical10
//                            Duration = Some 5
//                            Sessions = []
//                            Lane = []
//                            Comments = []
//                        |}
//                    ]
//                |}
//
//                {|
//                    Id = TreeId "liryanne/shared"
//                    Access = [ TreeAccess.Owner TempData.Users.liryanne
//                               TreeAccess.Admin TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//
//                {|
//                    Id = TreeId "fluke/samples/laneSorting/frequency"
//                    Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
//                               TreeAccess.ReadOnly TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//
//                {|
//                    Id = TreeId "fluke/samples/laneSorting/timeOfDay"
//                    Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
//                               TreeAccess.ReadOnly TempData.Users.fc1943s ]
//                    InformationList = []
//                    Tasks = []
//                |}
//            ]
//        |}

    module private OldData =
        //        type TempDataType =
//            | TempPrivate
//            | TempPublic
//            | Test
//        let tempDataType = TempPrivate
//        let tempDataType = Test
//        let tempDataType = TempPublic



        //        let tempState =
//            let testData = TempData.tempData.RenderLaneTests
//            let testData = TempData.tempData.SortLanesTests

        //            let getNow =
//                match tempDataType with
//                | TempPrivate -> TempData.getNow
//                | TempPublic  -> TempData.getNow
//                | Test        -> testData.GetNow

        //            let dayStart =
//                match tempDataType with
//                | TempPrivate -> TempData.dayStart
//                | TempPublic  -> TempData.dayStart
//                | Test        -> TempData.testDayStart

        //            let informationList =
//                match tempDataType with
//                | TempPrivate -> RootPrivateData.manualTasks.InformationList
//                | TempPublic  -> TempData.tempData.ManualTasks.InformationList
//                | Test        -> []

        //            let taskOrderList =
//                match tempDataType with
//                | TempPrivate -> RootPrivateData.manualTasks.TaskOrderList @ RootPrivateData.taskOrderList
//                | TempPublic  -> TempData.tempData.ManualTasks.TaskOrderList
//                | Test        -> testData.TaskOrderList

        //            let informationCommentsMap =
//                match tempDataType with
//                | TempPrivate ->
//                    RootPrivateData.informationComments
//                    |> List.append RootPrivateData.Shared.informationComments
//                    |> List.groupBy (fun x -> x.Information)
//                    |> Map.ofList
//                    |> Map.mapValues (List.map (fun x -> x.Comment))
//                | TempPublic  -> Map.empty
//                | Test        -> Map.empty

        //            let taskStateList =
//                match tempDataType with
//                | TempPrivate ->
//                    let taskData = RootPrivateData.manualTasks
//                    let sharedTaskData = RootPrivateData.Shared.manualTasks
//
//                    let cellComments =
//                        RootPrivateData.cellComments
//                        |> List.append RootPrivateData.Shared.cellComments
//
//                    let applyState statusEntries comments (taskState: TaskState) =
//                        { taskState with
//                            StatusEntries =
//                                statusEntries
//                                |> createTaskStatusEntries taskState.Task
//                                |> List.prepend taskState.StatusEntries
//                            Comments =
//                                comments
//                                |> List.filter (fun (TaskComment (task, _)) -> task = taskState.Task)
//                                |> List.map (ofTaskComment >> snd)
//                                |> List.prepend taskState.Comments
//                            CellCommentsMap =
//                                cellComments
//                                |> List.filter (fun (CellComment (address, _)) -> address.Task = taskState.Task)
//                                |> List.map (fun (CellComment (address, comment)) -> address.Date, comment)
//                                |> List.groupBy fst
//                                |> Map.ofList
//                                |> Map.mapValues (List.map snd)
//                                |> Map.union taskState.CellCommentsMap }
//
//                    let taskStateList =
//                        taskData.TaskStateList
//                        |> List.map (applyState
//                                         RootPrivateData.cellStatusEntries
//                                         RootPrivateData.taskComments)
//
//                    let sharedTaskStateList =
//                        sharedTaskData.TaskStateList
//                        |> List.map (applyState
//                                         RootPrivateData.Shared.cellStatusEntries
//                                         RootPrivateData.Shared.taskComments)
//
//                    taskStateList |> List.append sharedTaskStateList
//                | TempPublic  -> TempData.tempData.ManualTasks.TaskStateList
//                | Test        -> testData.TaskStateList

        //            let taskStateList =
//                taskStateList
//                |> List.sortByDescending (fun x -> x.StatusEntries.Length)
//                |> List.take 10

        //            let lastSessions =
//                taskStateList
//                |> Seq.filter (fun taskState -> not taskState.Sessions.IsEmpty)
//                |> Seq.map (fun taskState -> taskState.Task, taskState.Sessions)
//                |> Seq.map (Tuple2.mapItem2 (fun sessions ->
//                    sessions
//                    |> Seq.sortByDescending (fun (TaskSession start) -> start.DateTime)
//                    |> Seq.head
//                ))
//                |> Seq.toList

        //            printfn "RETURNING TEMPSTATE."
//            {| GetNow = getNow
//               DayStart = dayStart
//               InformationCommentsMap = informationCommentsMap
//               InformationList = informationList
//               TaskOrderList = taskOrderList
//               TaskStateList = taskStateList
//               LastSessions = lastSessions |}




        //        type TestTemplate =
//            | LaneSorting of string
//            | LaneRendering of string
//
//        [<RequireQualifiedAccess>]
//        type Database =
//            | Test of TestTemplate
//            | Public
//            | Private

        //        let a = [
//            "test", [
//                "laneSorting", [
//
//                ]
//                "laneRendering", [
//
//                ]
//            ]
//            "public", [
//                "default", []
//            ]
//            "private", [
//                "default", []
//            ]
//        ]


        //        let getLivePosition () = RootPrivateData.getLivePosition ()
//
//        let getCurrentUser () = RootPrivateData.currentUser
//
//        let getDayStart () = RootPrivateData.dayStart
//
//        let getWeekStart () = RootPrivateData.weekStart
        ()
