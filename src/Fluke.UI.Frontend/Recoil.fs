namespace Fluke.UI.Frontend

open Feliz.Router


#nowarn "40"

open System
open FSharpPlus
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.DateFunctions


module Recoil =
    open Model
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State
    open View


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
                    Profiling.addCount (sprintf "%s/%s" (nameof Information) (nameof wrappedInformation))
                    Area (Area.Default, [])

            let rec attachments =
                atomFamilyFn
                <| fun (_informationId: InformationId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Information) (nameof attachments))
                    []: Attachment list


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
                    Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof informationId))
                    Information.informationId Task.Default.Information


            let rec name =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof name))
                    Task.Default.Name


            let rec scheduling =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof scheduling))
                    Task.Default.Scheduling


            let rec pendingAfter =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof pendingAfter))
                    Task.Default.PendingAfter


            let rec missedAfter =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof missedAfter))
                    Task.Default.MissedAfter


            let rec priority =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof priority))
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
                    Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof attachments))
                    []: Attachment list // TODO: move from here?


            let rec duration =
                atomFamilyFn
                <| fun (_taskId: TaskId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof duration))
                    Task.Default.Duration


            let taskId (task: Task) = TaskId (task.Information.Name, task.Name)

        module rec User =

            let rec color =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof color))
                    UserColor.Black


            let rec weekStart =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof weekStart))
                    DayOfWeek.Sunday


            let rec dayStart =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof dayStart))
                    FlukeTime.Create 04 00


            let rec sessionLength =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof sessionLength))
                    Minute 25.


            let rec sessionBreakLength =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof sessionBreakLength))
                    Minute 5.




        module rec Session =

            //            let rec sessionData =
//                atomFamilyFn
//                <| fun (_username: Username) ->
//                    Profiling.addCount (nameof sessionData)
//                    None: SessionData option
            let rec user =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof user))
                    None: User option

            //            let rec treeSelectionIds =
//                atomFamilyFn
//                <| fun (_username: Username) ->
//                    Profiling.addCount (nameof treeSelectionIds)
//                    Set.empty: Set<TreeId>

            let rec availableTreeIds =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof availableTreeIds))
                    []: TreeId list

            let rec taskIdList =
                atomFamilyFn
                <| fun (_username: Username) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof taskIdList))
                    []: Task.TaskId list




        module rec Cell =
            let rec taskId =
                atomFamilyFn
                <| fun (taskId: Task.TaskId, _dateId: DateId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof taskId))
                    taskId


            let rec dateId =
                atomFamilyFn
                <| fun (_taskId: Task.TaskId, dateId: DateId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof dateId))
                    dateId


            let rec status =
                atomFamilyFn
                <| fun (_taskId: Task.TaskId, _dateId: DateId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof status))
                    Disabled


            let rec attachments =
                atomFamilyFn
                <| fun (_taskId: Task.TaskId, _dateId: DateId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof attachments))
                    []: Attachment list


            let rec sessions =
                atomFamilyFn
                <| fun (_taskId: Task.TaskId, _dateId: DateId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof sessions))
                    []: TaskSession list


            let rec selected =
                atomFamilyFn
                <| fun (_taskId: Task.TaskId, _dateId: DateId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof selected))
                    false



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
                    Profiling.addCount (sprintf "%s/%s" (nameof Tree) (nameof name))
                    TreeName ""


            let rec owner =
                atomFamilyFn
                <| fun (_treeId: TreeId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Tree) (nameof owner))
                    None: User option


            let rec sharedWith =
                atomFamilyFn
                <| fun (_treeId: TreeId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Tree) (nameof sharedWith))
                    TreeAccess.Public


            let rec position =
                atomFamilyFn
                <| fun (_treeId: TreeId) ->
                    Profiling.addCount (sprintf "%s/%s" (nameof Tree) (nameof position))
                    None: FlukeDateTime option



        //            let treeId owner name =
//                TreeId (sprintf "%s/%s" owner.Username name)
//

        let rec debug =
            atom {
                key ("atom/" + nameof debug)
                def false
                local_storage
            }

        let rec treeSelectionIds =
            atom {
                key ("atom/" + nameof treeSelectionIds)
                def ([||]: TreeId [])
                local_storage
            }

        let rec selectedPosition =
            atom {
                key ("atom/" + nameof selectedPosition)
                def (None: FlukeDateTime option)
                local_storage
            }

        let rec path =
            atom {
                key ("atom/" + nameof path)
                def (Router.currentPath ())
            }

        let rec getLivePosition =
            atom {
                key ("atom/" + nameof getLivePosition)
                def
                    ({|
                         Get = fun () -> FlukeDateTime.FromDateTime DateTime.Now
                     |})
            }

        let rec username =
            atom {
                key ("atom/" + nameof username)
                def None
            }

        let rec cellSize =
            atom {
                key ("atom/" + nameof cellSize)
                def 17
            }

        let rec lanePaddingLeft =
            atom {
                key ("atom/" + nameof lanePaddingLeft)
                def 45
            }

        let rec lanePaddingRight =
            atom {
                key ("atom/" + nameof lanePaddingRight)
                def 20
            }

        let rec treeStateMap =
            atom {
                key ("atom/" + nameof treeStateMap)
                def (Map.empty: Map<TreeId, TreeState>)
            }

        let rec cellSelectionMap =
            atom {
                key ("atom/" + nameof cellSelectionMap)
                def (Map.empty: Map<Task.TaskId, Set<FlukeDate>>)
            }

        let rec ctrlPressed =
            atom {
                key ("atom/" + nameof ctrlPressed)
                def false
            }

        let rec shiftPressed =
            atom {
                key ("atom/" + nameof shiftPressed)
                def false
            }

        let rec positionTrigger =
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
                            | [ "view"; "Calendar" ] -> View.View.Calendar
                            | [ "view"; "Groups" ] -> View.View.Groups
                            | [ "view"; "Tasks" ] -> View.View.Tasks
                            | [ "view"; "Week" ] -> View.View.Week
                            | _ -> View.View.Calendar

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
                        let selectedPosition = getter.get Atoms.selectedPosition
                        //
                        let result =
                            selectedPosition
                            |> Option.defaultValue (getLivePosition.Get ())
                            |> Some

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
                        let lanePaddingLeft = getter.get Atoms.lanePaddingLeft
                        let lanePaddingRight = getter.get Atoms.lanePaddingRight
                        let position = getter.get position

                        let result =
                            match position with
                            | None -> []
                            | Some position ->
                                [
                                    position.Date
                                ]
                                |> Rendering.getDateSequence (lanePaddingLeft, lanePaddingRight)

                        Profiling.addCount (nameof dateSequence)
                        result)
            }

        let rec treeStateMap =
            selector {
                key ("selector/" + nameof treeStateMap)
                get (fun getter ->
                        let position = getter.get position

                        let result =
                            match position with
                            | Some position ->
                                let user, treeStateList = RootPrivateData.State.getTreeStateList position

                                let treeStateMap =
                                    treeStateList
                                    |> List.map (fun ({ Name = TreeName name } as treeState) ->
                                        let id =
                                            name
                                            |> Bindings.Crypto.sha3
                                            |> string
                                            |> String.take 16
                                            |> System.Text.Encoding.UTF8.GetBytes
                                            |> Guid
                                            |> TreeId

                                        id, treeState)
                                    |> Map.ofList

                                Some (user, treeStateMap)
                            | None -> None

                        Profiling.addCount (nameof treeStateMap)

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
        let rec cellSelectionMap =
            selector {
                key ("selector/" + nameof cellSelectionMap)
                get (fun getter ->
                        let selection = getter.get Atoms.cellSelectionMap
                        Profiling.addCount (nameof selection)
                        selection)
                set (fun setter (newSelection: Map<Atoms.Task.TaskId, Set<FlukeDate>>) ->
                        let cellSelectionMap = setter.get Atoms.cellSelectionMap

                        let operationsByTask =
                            let taskIdSet =
                                seq {
                                    yield! cellSelectionMap |> Map.keys
                                    yield! newSelection |> Map.keys
                                }
                                |> Set.ofSeq

                            taskIdSet
                            |> Seq.map (fun taskId ->
                                let taskSelection =
                                    cellSelectionMap
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
                                setter.set (Atoms.Cell.selected (taskId, DateId date), selected)))

                        setter.set (Atoms.cellSelectionMap, newSelection)
                        Profiling.addCount (nameof cellSelectionMap + " (SET)"))
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

        module rec FlukeDate =
            let isToday =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof FlukeDate) (nameof isToday))
                    get (fun (date: FlukeDate) getter ->
                            let username = getter.get Atoms.username
                            let position = getter.get position

                            let result =
                                match username, position with
                                | Some username, Some position ->
                                    let dayStart = getter.get (Atoms.User.dayStart username)

                                    Domain.UserInteraction.isToday dayStart position (DateId date)
                                | _ -> false

                            Profiling.addCount (sprintf "%s/%s" (nameof FlukeDate) (nameof isToday))
                            result)
                }

            let rec hasSelection =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof FlukeDate) (nameof hasSelection))
                    get (fun (date: FlukeDate) getter ->
                            let cellSelectionMap = getter.get cellSelectionMap

                            let result =
                                cellSelectionMap
                                |> Map.values
                                |> Seq.exists (fun dateSequence -> dateSequence |> Set.contains date)

                            Profiling.addCount (sprintf "%s/%s" (nameof FlukeDate) (nameof hasSelection))
                            result

                        )
                }

        module rec Information =
            ()

        module rec Task =
            let rec lastSession =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof lastSession))
                    get (fun (taskId: Atoms.Task.TaskId) getter ->
                            let username = getter.get Atoms.username
                            match username with
                            | Some username ->
                                let dateSequence = getter.get dateSequence
                                let taskIdList = getter.get (Atoms.Session.taskIdList username)

                                let result =
                                    dateSequence
                                    |> List.rev
                                    |> List.tryPick (fun date ->
                                        let sessions = getter.get (Atoms.Cell.sessions (taskId, DateId date))
                                        sessions
                                        |> List.sortByDescending (fun (TaskSession (start, _, _)) -> start.DateTime)
                                        |> List.tryHead)

                                Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof lastSession))
                                result
                            | None -> None)
                }

            let rec activeSession =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof activeSession))
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

                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof activeSession))

                            result)
                }

            let rec showUser =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof showUser))
                    get (fun (taskId: Atoms.Task.TaskId) getter ->
                            let username = getter.get Atoms.username
                            match username with
                            | Some username ->
                                let dateSequence = getter.get dateSequence
                                let taskIdList = getter.get (Atoms.Session.taskIdList username)

                                let statusList =
                                    dateSequence
                                    |> List.map (fun date -> Atoms.Cell.status (taskId, DateId date))
                                    |> List.map getter.get

                                let usersCount =
                                    statusList
                                    |> List.choose (function
                                        | UserStatus (user, _) -> Some user
                                        | _ -> None)
                                    |> Seq.distinct
                                    |> Seq.length

                                let result = usersCount > 1

                                Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof showUser))
                                result
                            | None -> false)
                }

            let rec hasSelection =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof hasSelection))
                    get (fun (taskId: Atoms.Task.TaskId) getter ->
                            let cellSelectionMap = getter.get cellSelectionMap

                            let result =
                                cellSelectionMap
                                |> Map.tryFind taskId
                                |> Option.defaultValue Set.empty
                                |> Set.isEmpty
                                |> not

                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof hasSelection))
                            result)
                }

        module rec Session =
            //            let rec currentTaskList =
//                selectorFamily {
//                    key ("selector/" + nameof currentTaskList)
//                    get (fun (username: Username) getter ->
//                            let taskIdList = getter.get (Atoms.Session.taskIdList username)
//
//                            printfn "Selectors.currentTaskList -> taskIdList.Length = %A" taskIdList.Length
//
//                            let result =
//                                taskIdList
//                                |> List.map (fun taskId ->
//                                    let informationId = getter.get (Atoms.Task.informationId taskId)
//
//                                    {|
//                                        Id = taskId
//                                        Name = getter.get (Atoms.Task.name taskId)
//                                        Information = getter.get (Atoms.Information.wrappedInformation informationId)
//                                        InformationAttachments =
//                                            getter.get (Atoms.Information.attachments informationId)
//                                        Scheduling = getter.get (Atoms.Task.scheduling taskId)
//                                        PendingAfter = getter.get (Atoms.Task.pendingAfter taskId)
//                                        MissedAfter = getter.get (Atoms.Task.missedAfter taskId)
//                                        Priority = getter.get (Atoms.Task.priority taskId)
//                                        Attachments = getter.get (Atoms.Task.attachments taskId)
//                                        Duration = getter.get (Atoms.Task.duration taskId)
//                                    |})
//
//                            //
//                            //                            |> List.map (fun task ->
//                            //                                getter.get (Atoms.RecoilTask.taskFamily (Atoms.RecoilTask.taskId task)))
//
//                            Profiling.addCount (nameof currentTaskList)
//                            result)
//                }

            let rec activeSessions =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Session) (nameof activeSessions))
                    get (fun (username: Username) getter ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)

                            let result =
                                let sessionLength = getter.get (Atoms.User.sessionLength username)
                                let sessionBreakLength = getter.get (Atoms.User.sessionBreakLength username)
                                taskIdList
                                |> List.map (fun taskId ->
                                    let (TaskName taskName) = getter.get (Atoms.Task.name taskId)

                                    let duration = getter.get (Task.activeSession taskId)

                                    duration
                                    |> Option.map (fun duration ->
                                        ActiveSession (taskName, Minute duration, sessionLength, sessionBreakLength)))
                                |> List.choose id

                            Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof activeSessions))
                            result)
                }

            let rec tasksByInformationKind =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Session) (nameof tasksByInformationKind))
                    get (fun (username: Username) getter ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)

                            let informationMap =
                                taskIdList
                                |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.informationId taskId))
                                |> List.map (fun (taskId, informationId) ->
                                    taskId, getter.get (Atoms.Information.wrappedInformation informationId))
                                |> Map.ofList

                            //                        let sessionData = Recoil.useValue (Session.sessionData username)

                            //                        let taskList =
//                            match sessionData with
//                            | Some sesionData -> sessionData.Value.TaskList
//                            | None -> []

                            //                        let groupMap =
//                            taskList
//                            |> List.map (fun x -> x.Information, x)
//                            |> Map.ofList

                            let informationKindGroups =
                                taskIdList
                                |> List.groupBy (fun taskId -> informationMap.[taskId])
                                |> List.sortBy (fun (information, _) -> information.Name)
                                |> List.groupBy (fun (information, _) -> information.KindName)
                                |> List.sortBy
                                    (snd
                                     >> List.head
                                     >> fst
                                     >> fun information -> information.Order)
                                |> List.map (fun (informationKindName, groups) ->
                                    let newGroups =
                                        groups
                                        |> List.map (fun (information, taskIdList) ->
                                            let informationId = Atoms.Information.informationId information

                                            informationId, taskIdList)

                                    informationKindName, newGroups)

                            Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof tasksByInformationKind))
                            informationKindGroups)
                }

            let rec weekCellsMap =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Session) (nameof weekCellsMap))
                    get (fun (username: Username) getter ->
                            let position = getter.get position
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)
                            let sessionData: SessionData option = getter.get (sessionData username)

                            let result =
                                match position, sessionData with
                                | Some position, Some sessionData ->
                                    let dayStart = getter.get (Atoms.User.dayStart username)
                                    let weekStart = getter.get (Atoms.User.weekStart username)

                                    let taskMap =
                                        sessionData.TaskList
                                        |> List.map (fun task -> Atoms.Task.taskId task, task)
                                        |> Map.ofList

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
                                                taskIdList
                                                |> List.collect (fun taskId ->
                                                    dateIdSequence
                                                    |> List.map (fun ((DateId referenceDay) as dateId) ->
                                                        //                                                    let taskId = getter.get task.Id
                                                        let status = getter.get (Atoms.Cell.status (taskId, dateId))
                                                        let sessions = getter.get (Atoms.Cell.sessions (taskId, dateId))

                                                        let attachments =
                                                            getter.get (Atoms.Cell.attachments (taskId, dateId))

                                                        let isToday = getter.get (FlukeDate.isToday referenceDay)

                                                        match status, sessions, attachments with
                                                        | (Disabled
                                                          | Suggested),
                                                          [],
                                                          [] -> None
                                                        | _ ->
                                                            {|
                                                                DateId = dateId
                                                                TaskId = taskId
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

                                                                let task = taskMap.[cell.TaskId]

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
                                                        |> List.sortBy (fun cell -> sortedTasksMap.[cell.TaskId])

                                                    dateId, newCells)
                                                |> Map.ofList

                                            result)

                                    weeks
                                | _ -> []

                            Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof weekCellsMap))
                            result)
                }

            let rec sessionData =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Session) (nameof sessionData))
                    get (fun (username: Username) getter ->
                            async {
                                let treeStateMap = getter.get Atoms.treeStateMap
                                let user = getter.get (Atoms.Session.user username)
                                let dateSequence = getter.get dateSequence
                                let view = getter.get view
                                let position = getter.get position
                                //                            let getLivePosition = (getter.get Atoms.getLivePosition).Get
//                                let treeSelectionIds = getter.get (Atoms.Session.treeSelectionIds username)
                                let treeSelectionIds = getter.get Atoms.treeSelectionIds

                                let result =
                                    match user, position, treeStateMap.Count with
                                    | Some user, Some position, treeCount when treeCount > 0 ->
                                        //                                    let newTreeSelectionIds =
//                                        if treeSelectionIds.IsEmpty then
//                                            state.Session.TreeSelection
//                                            |> Set.map (fun treeState -> treeState.Id)
//                                        else
//                                            treeSelectionIds

                                        let newSession =
                                            getSessionData
                                                {|
                                                    User = user
                                                    DateSequence = dateSequence
                                                    View = view
                                                    Position = position
                                                    TreeSelectionIds = treeSelectionIds |> Set.ofArray
                                                    TreeStateMap = treeStateMap
                                                |}
                                        //                                                GetLivePosition = getLivePosition

                                        Some newSession
                                    | _ -> None

                                Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof sessionData))

                                return result
                            })
                }

        //            let rec currentSession =
//                selectorFamily {
//                    key ("selector/" + nameof currentSession)
//                    get (fun (username: Username) getter ->
//                            let state = getter.get (Atoms.Session.sessionData username)
//
//                            let result =
//                                match state with
//                                | Some state -> Some state.Session
//                                | None -> None
//
//                            Profiling.addCount (nameof currentSession)
//                            result)
//                }
//            ()



        module rec Tree =
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




        module rec Cell =
            let rec selected =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Cell) (nameof selected))
                    get (fun (taskId: Atoms.Task.TaskId, dateId: DateId) getter ->
                            let selected = getter.get (Atoms.Cell.selected (taskId, dateId))

                            Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof selected))
                            selected)
                    set (fun (taskId: Atoms.Task.TaskId, (DateId referenceDay)) setter (newValue: bool) ->
                            let username = setter.get Atoms.username

                            match username with
                            | Some username ->
                                let ctrlPressed = setter.get Atoms.ctrlPressed
                                let shiftPressed = setter.get Atoms.shiftPressed

                                let newCellSelectionMap =
                                    match shiftPressed, ctrlPressed with
                                    | false, false ->
                                        let newTaskSelection =
                                            if newValue then
                                                Set.singleton referenceDay
                                            else
                                                Set.empty

                                        [
                                            taskId, newTaskSelection
                                        ]
                                        |> Map.ofList
                                    | false, true ->
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

                                        let oldSelection = setter.get Atoms.cellSelectionMap
                                        swapSelection oldSelection taskId referenceDay
                                    | true, _ ->
                                        let taskIdList = setter.get (Atoms.Session.taskIdList username)
                                        let oldCellSelectionMap = setter.get Atoms.cellSelectionMap

                                        let initialTaskIdSet =
                                            oldCellSelectionMap
                                            |> Map.keys
                                            |> Set.ofSeq
                                            |> Set.add taskId

                                        let newTaskIdList =
                                            taskIdList
                                            |> List.skipWhile (initialTaskIdSet.Contains >> not)
                                            |> List.rev
                                            |> List.skipWhile (initialTaskIdSet.Contains >> not)
                                            |> List.rev

                                        let initialDateList =
                                            oldCellSelectionMap
                                            |> Map.values
                                            |> Set.unionMany
                                            |> Set.add referenceDay
                                            |> Set.toList
                                            |> List.sort

                                        let dateSeq =
                                            match initialDateList with
                                            | [] -> []
                                            | dateList ->
                                                [
                                                    dateList.Head
                                                    dateList |> List.last
                                                ]
                                                |> Rendering.getDateSequence (0, 0)
                                            |> Set.ofList

                                        let newMap =
                                            newTaskIdList
                                            |> List.map (fun taskId -> taskId, dateSeq)
                                            |> Map.ofList

                                        newMap

                                setter.set (cellSelectionMap, newCellSelectionMap)

                            | None -> ()

                            Profiling.addCount (sprintf "%s/%s (SET)" (nameof Cell) (nameof selected)))
                }

    /// [1]

    let initState (initializer: MutableSnapshot) = ()
    //        let baseState = RootPrivateData.State.getBaseState ()

    //        let state2 = {| User =state.User; TreeStateMap = state.TreeStateMap |}
//
//        let simpleJson = Fable.SimpleJson.SimpleJson.stringify state2
//        let thothJson = Thoth.Json.Encode.Auto.toString(4, state2)
//
//        Ext.setDom (nameof baseState) baseState
    //        Browser.Dom.window?flukeStateSimple <- simpleJson
//        Browser.Dom.window?flukeStateThoth <- thothJson

    //        match baseState.Session.User with
//        | Some user ->
//            initializer.set (Atoms.state, Some baseState)
//            initializer.set (Atoms.getLivePosition, {| Get = baseState.Session.GetLivePosition |})
//            initializer.set (Atoms.username, Some user.Username)
//            initializer.set (Atoms.Session.user user.Username, Some user)
//        | None -> ()

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
