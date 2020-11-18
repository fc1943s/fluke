namespace Fluke.UI.Frontend

#nowarn "40"

open System
open FSharpPlus
open Feliz.Recoil
open Fluke.Shared
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
open Fable.Core.JsInterop
open Fable.Core


module Recoil =
    open Model
    open Domain.UserInteraction
    open Domain.State
    open View

    [<Emit "process.env.JEST_WORKER_ID">]
    let jestWorkerId: bool = jsNative

    let gunTmp =
        Gun.gun
            ({|
                 peers =
                     if jestWorkerId then
                         null
                     else
                         [|
                             "http://localhost:8765/gun"
                         |]
                 radisk = false
             |}
             |> toPlainJsObj
             |> unbox)

    Browser.Dom.window?gunTmp <- gunTmp

    module Atoms =
        module rec Information =
            type InformationId = InformationId of id: string

            let rec wrappedInformation =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Information) (nameof wrappedInformation))

                    def (fun (_informationId: InformationId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Information) (nameof wrappedInformation))
                            Area (Area.Default, []))
                }

            let rec attachments =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Information) (nameof attachments))

                    def (fun (_informationId: InformationId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Information) (nameof attachments))
                            []: Attachment list)
                }

            let rec informationId (information: Information): InformationId =
                match information with
                | Archive _ ->
                    let (InformationId archiveId) = informationId information
                    sprintf "%s/%s" (Information.toString information) archiveId
                | _ ->
                    let (InformationName informationName) = information.Name
                    sprintf "%s/%s" (Information.toString information) informationName
                |> InformationId


        module rec Task =
            type TaskId = TaskId of informationName: InformationName * taskName: TaskName

            let rec informationId =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof informationId))

                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof informationId))
                            Information.informationId Task.Default.Information)
                }

            let rec name =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof name))

                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof name))
                            Task.Default.Name)
                }

            let rec scheduling =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof scheduling))

                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof scheduling))
                            Task.Default.Scheduling)
                }

            let rec pendingAfter =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof pendingAfter))

                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof pendingAfter))
                            Task.Default.PendingAfter)
                }

            let rec missedAfter =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof missedAfter))

                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof missedAfter))
                            Task.Default.MissedAfter)
                }

            let rec priority =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof priority))

                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof priority))
                            Task.Default.Priority)
                }


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
                atomFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof attachments))

                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof attachments))
                            []: Attachment list) // TODO: move from here?
                }

            let rec duration =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof duration))

                    def (fun (_taskId: TaskId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Task) (nameof duration))
                            Task.Default.Duration)
                }

            let taskId (task: Task) = TaskId (task.Information.Name, task.Name)


        module rec User =
            let rec color =
                atomFamily {
                    key (sprintf "%s/%s" (nameof User) (nameof color))

                    def (fun (_username: Username) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof color))
                            UserColor.Black)
                }

            let rec weekStart =
                atomFamily {
                    key (sprintf "%s/%s" (nameof User) (nameof weekStart))

                    def (fun (_username: Username) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof weekStart))
                            DayOfWeek.Sunday)
                }

            let rec dayStart =
                atomFamily {
                    key (sprintf "%s/%s" (nameof User) (nameof dayStart))

                    def (fun (_username: Username) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof dayStart))
                            FlukeTime.Create 00 00)
                }

            let rec sessionLength =
                atomFamily {
                    key (sprintf "%s/%s" (nameof User) (nameof sessionLength))

                    def (fun (_username: Username) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof sessionLength))
                            Minute 25.)
                }

            let rec sessionBreakLength =
                atomFamily {
                    key (sprintf "%s/%s" (nameof User) (nameof sessionBreakLength))

                    def (fun (_username: Username) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof User) (nameof sessionBreakLength))
                            Minute 5.)
                }




        module rec Session =

            //            let rec sessionData =
//                atomFamilyFn
//                <| fun (_username: Username) ->
//                    Profiling.addCount (nameof sessionData)
//                    None: SessionData option
//            let rec user =
//                atomFamilyFn
//                <| fun (_username: Username) ->
//                    Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof user))
//                    None: User option

            //            let rec treeSelectionIds =
//                atomFamilyFn
//                <| fun (_username: Username) ->
//                    Profiling.addCount (nameof treeSelectionIds)
//                    Set.empty: Set<TreeId>

            let rec availableTreeIds =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Session) (nameof availableTreeIds))

                    def (fun (_username: Username) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof availableTreeIds))
                            []: TreeId list)
                }

            let rec taskIdList =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Session) (nameof taskIdList))

                    def (fun (_username: Username) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof taskIdList))
                            []: Task.TaskId list)
                }


        module rec Cell =
            let rec taskId =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Cell) (nameof taskId))

                    def (fun (taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof taskId))
                            taskId)
                }

            let rec dateId =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Cell) (nameof dateId))

                    def (fun (_taskId: Task.TaskId, dateId: DateId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof dateId))
                            dateId)
                }

            let rec status =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Cell) (nameof status))

                    def (fun (_taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof status))
                            Disabled)
                }

            let rec attachments =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Cell) (nameof attachments))

                    def (fun (_taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof attachments))
                            []: Attachment list)
                }

            let rec sessions =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Cell) (nameof sessions))

                    def (fun (_taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof sessions))
                            []: TaskSession list)
                }

            let rec selected =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Cell) (nameof selected))

                    def (fun (_taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Cell) (nameof selected))
                            false)

                    effects (fun (taskId: Task.TaskId, dateId: DateId) ->
                        [
                            (fun { node = node
                                   onSet = onSet
                                   setSelf = setSelf
                                   trigger = trigger } ->

                                let taskIdHash =
                                    Crypto
                                        .sha3(string taskId)
                                        .toString(Crypto.crypto.enc.Hex)

                                let dateIdHash =
                                    Crypto
                                        .sha3(string dateId)
                                        .toString(Crypto.crypto.enc.Hex)

                                let tasks = gunTmp.get "tasks"
                                let task = tasks.get taskIdHash
                                let cells = task.get ("cells")
                                let cell = cells.get dateIdHash
                                let selected = cell.get "selected"

                                match trigger with
                                | "get" ->
                                    selected.on (fun value ->
                                        printfn
                                            "GET@@ CELL SELECTED RENDER . taskid: %A dateId: %A. node: %A"
                                            taskId
                                            dateId
                                            node

                                        setSelf (value))
                                | _ -> ()

                                //                                        // Subscribe to storage updates
                                //                                        storage.subscribe(value => setSelf(value));




                                printfn
                                    "CELL SELECTED RENDER . taskid: %A dateId: %A. trigger: %A"
                                    taskId
                                    dateId
                                    trigger
                                //                            let storage = Browser.Dom.window.localStorage.getItem node.key
                                //                            let value: {| value: obj |} option = unbox JS.JSON.parse storage
                                //
                                //                            match value with
                                //                            | Some value -> setSelf (unbox value.value)
                                //                            | _ -> ()
                                //
                                onSet (fun value oldValue ->

                                    let tasks = gunTmp.get "tasks"

                                    let task =
                                        gunTmp
                                            .get(taskIdHash)
                                            .put({| id = taskIdHash; name = "taskName1" |})

                                    tasks.set task |> ignore

                                    let cells = task.get "cells"

                                    let cell =
                                        gunTmp
                                            .get(dateIdHash)
                                            .put({| dateId = dateIdHash; selected = value |})

                                    cells.set cell |> ignore
                                    //                                    let cell = cells.set ({| dateId = string _dateId |})
//                                    cell.put {| selected = value |} |> ignore


                                    //    const tasks = gun.get("tasks");
//    const task1 = gun.get("taskId1").put({id: 'taskId1', name: 'taskName1'});
//    tasks.set(task1);
//
//    const cells = task1.get("cells");
//    const cell1 = gun.get("dateId1").put({dateId: 'dateId1'});
//    const cell = cells.set(cell1);
//
//    cell.put({selected: true});

                                    printfn "oldValue: %A; newValue: %A" oldValue value
                                    //                                    Browser.Dom.window.localStorage.setItem
                                    //                                        (node.key, JS.JSON.stringify {| value = string value |}))
                                    //
                                    //                                        // Subscribe to storage updates
                                    //                                        storage.subscribe(value => setSelf(value));

                                    )


                                fun () ->
                                    printfn "> unsubscribe cell"
                                    selected.off ())
                        ])
                }



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
                atomFamily {
                    key (sprintf "%s/%s" (nameof Tree) (nameof name))

                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Tree) (nameof name))
                            TreeName "")
                }


            let rec owner =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Tree) (nameof owner))

                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Tree) (nameof owner))
                            None: User option)
                }


            let rec sharedWith =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Tree) (nameof sharedWith))

                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Tree) (nameof sharedWith))
                            TreeAccess.Public)
                }


            let rec position =
                atomFamily {
                    key (sprintf "%s/%s" (nameof Tree) (nameof position))

                    def (fun (_treeId: TreeId) ->
                            Profiling.addCount (sprintf "%s/%s" (nameof Tree) (nameof position))
                            None: FlukeDateTime option)
                }



        //            let treeId owner name =
//                TreeId (sprintf "%s/%s" owner.Username name)
//

        let rec debug =
            atom {
                key ("atom/" + nameof debug)
                def false
                local_storage
            }

        let rec view =
            atom {
                key ("atom/" + nameof view)
                def View.View.HabitTracker
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

        let rec cellSize =
            atom {
                key ("atom/" + nameof cellSize)
                def 17
            }

        let rec daysBefore =
            atom {
                key ("atom/" + nameof daysBefore)
                def 7
                log

                local_storage
            }

        let rec daysAfter =
            atom {
                key ("atom/" + nameof daysAfter)
                def 7
                local_storage
            }

        let rec leftDock =
            atom {
                key ("atom/" + nameof leftDock)
                def (None: TempUI.DockType option)
                local_storage
            }

        let rec api =
            atom {
                key ("atom/" + nameof api)
                def Sync.api
            }

        let rec peers =
            atom {
                key ("atom/" + nameof peers)
                def [| "http://localhost:8765/gun" |]
            }

        let rec username =
            atom {
                key ("atom/" + nameof username)
                def None
            }

        let rec sessionRestored =
            atom {
                key ("atom/" + nameof sessionRestored)
                def false
            }

        let rec getLivePosition =
            atom {
                key ("atom/" + nameof getLivePosition)

                def
                    ({|
                         Get = fun () -> FlukeDateTime.FromDateTime DateTime.Now
                     |})
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
        let rec gun =
            selector {
                key ("selector/" + nameof gun)

                get (fun getter ->
                        let _peers = getter.get Atoms.peers
                        //                        let gun = Gun.gun peers
                        let gun = gunTmp

                        Profiling.addCount (nameof gun)
                        {| root = gun |})
            }


        let rec apiCurrentUser =
            selector {
                key ("selector/" + nameof apiCurrentUser)

                get (fun getter ->
                        async {
                            let api = getter.get Atoms.api

                            let! result = api.currentUser |> Sync.handleRequest

                            Profiling.addCount (nameof apiCurrentUser)

                            return result
                        })
            }

        let rec position =
            selector {
                key ("selector/" + nameof position)

                get (fun getter ->
                        let _positionTrigger = getter.get Atoms.positionTrigger
                        let getLivePosition = getter.get Atoms.getLivePosition
                        let selectedPosition = getter.get Atoms.selectedPosition

                        //                        let selectedPosition = Some (FlukeDateTime.Create 2020 Month.October 19 07 00)

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
                        let daysBefore = getter.get Atoms.daysBefore
                        let daysAfter = getter.get Atoms.daysAfter
                        let username = getter.get Atoms.username
                        let position = getter.get position

                        let result =
                            match position, username with
                            | Some position, Some username ->
                                let dayStart = getter.get (Atoms.User.dayStart username)
                                let dateId = dateId dayStart position
                                let (DateId referenceDay) = dateId

                                referenceDay
                                |> List.singleton
                                |> Rendering.getDateSequence (daysBefore, daysAfter)
                            | _ -> []

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

        let rec cellSelectionMap =
            selector {
                key ("selector/" + nameof cellSelectionMap)

                get (fun getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)
                            let dateSequence = getter.get dateSequence

                            let result =
                                taskIdList
                                |> List.map (fun taskId ->
                                    let dates =
                                        dateSequence
                                        |> List.map (fun date ->
                                            date, getter.get (Atoms.Cell.selected (taskId, DateId date)))
                                        |> List.filter snd
                                        |> List.map fst
                                        |> Set.ofList

                                    taskId, dates)
                                |> Map.ofList

                            Profiling.addCount (nameof cellSelectionMap)
                            result
                        | None -> Map.empty)

                set (fun setter (newSelection: Map<Atoms.Task.TaskId, Set<FlukeDate>>) ->
                        let cellSelectionMap = setter.get cellSelectionMap

                        let operations =
                            cellSelectionMap
                            |> Map.toList
                            |> List.collect (fun (taskId, dates) ->
                                let newDates =
                                    newSelection
                                    |> Map.tryFind taskId
                                    |> Option.defaultValue Set.empty

                                let deselect =
                                    newDates
                                    |> Set.difference dates
                                    |> Set.toList
                                    |> List.map (fun date -> taskId, date, false)

                                let select =
                                    dates
                                    |> Set.difference newDates
                                    |> Set.toList
                                    |> List.map (fun date -> taskId, date, true)

                                deselect @ select)

                        operations
                        |> List.iter (fun (taskId, date, selected) ->
                            setter.set (Atoms.Cell.selected (taskId, DateId date), selected))

                        Profiling.addCount (nameof cellSelectionMap + " (SET)"))
            }

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

                                    //                                    printfn
//                                        "isToday dayStart=%A; position=%A; date=%A"
//                                        (dayStart.Stringify ())
//                                        (position.Stringify ())
//                                        (date.Stringify ())
                                    Domain.UserInteraction.isToday dayStart position (DateId date)
                                | _ -> false

                            Profiling.addCount (sprintf "%s/%s" (nameof FlukeDate) (nameof isToday))
                            result)
                }

            let rec hasSelection =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof FlukeDate) (nameof hasSelection))

                    get (fun (date: FlukeDate) getter ->
                            let username = getter.get Atoms.username

                            match username with
                            | Some username ->
                                let taskIdList = getter.get (Atoms.Session.taskIdList username)

                                let result =
                                    taskIdList
                                    |> List.exists (fun taskId ->
                                        getter.get (Atoms.Cell.selected (taskId, DateId date)))

                                Profiling.addCount (sprintf "%s/%s" (nameof FlukeDate) (nameof hasSelection))
                                result
                            | None -> false)
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
                                let _taskIdList = getter.get (Atoms.Session.taskIdList username)

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
                            //                            let username = getter.get Atoms.username
//                            match username with
//                            | Some username ->
                            let dateSequence = getter.get dateSequence
                            //                                let taskIdList = getter.get (Atoms.Session.taskIdList username)

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
                            //                            | None -> false
                            )
                }

            let rec hasSelection =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Task) (nameof hasSelection))

                    get (fun (taskId: Atoms.Task.TaskId) getter ->
                            let dateSequence = getter.get dateSequence

                            let result =
                                dateSequence
                                |> List.exists (fun date -> getter.get (Atoms.Cell.selected (taskId, DateId date)))

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
                                        TempUI.ActiveSession
                                            (taskName, Minute duration, sessionLength, sessionBreakLength)))
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
                                |> List.groupBy (fun (information, _) -> Information.toString information)
                                |> List.sortBy (snd >> List.head >> fst >> Information.toTag)
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

            let rec treeStateMap =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Session) (nameof treeStateMap))

                    get (fun (username: Username) getter ->
                            async {
                                let position = getter.get position

                                let! result =
                                    match position with
                                    | Some position ->
                                        async {
                                            let api = getter.get Atoms.api

                                            let! treeStateList =
                                                api.treeStateList username position
                                                |> Sync.handleRequest

                                            let treeStateMap =
                                                treeStateList
                                                |> Option.defaultValue []
                                                |> List.map (fun ({ Name = TreeName name } as treeState) ->
                                                    let id =
                                                        name
                                                        |> Crypto.sha3
                                                        |> string
                                                        |> String.take 16
                                                        |> System.Text.Encoding.UTF8.GetBytes
                                                        |> Guid
                                                        |> TreeId

                                                    id, treeState)
                                                |> Map.ofList

                                            return treeStateMap
                                        }
                                    | _ -> async { return Map.empty }

                                Profiling.addCount (sprintf "%s/%s" (nameof Session) (nameof treeStateMap))

                                return result
                            })
                }

            let rec sessionData =
                selectorFamily {
                    key (sprintf "%s/%s" (nameof Session) (nameof sessionData))

                    get (fun (username: Username) getter ->
                            async {
                                let treeStateMap = getter.get (treeStateMap username)
                                let dateSequence = getter.get dateSequence
                                let view = getter.get Atoms.view
                                let position = getter.get position
                                //                            let getLivePosition = (getter.get Atoms.getLivePosition).Get
//                                let treeSelectionIds = getter.get (Atoms.Session.treeSelectionIds username)
                                let treeSelectionIds = getter.get Atoms.treeSelectionIds

                                let dayStart = getter.get (Atoms.User.dayStart username)

                                let result =
                                    match position, treeStateMap.Count with
                                    | Some position, treeCount when treeCount > 0 ->
                                        //                                    let newTreeSelectionIds =
//                                        if treeSelectionIds.IsEmpty then
//                                            state.Session.TreeSelection
//                                            |> Set.map (fun treeState -> treeState.Id)
//                                        else
//                                            treeSelectionIds

                                        let newSession =
                                            getSessionData
                                                {|
                                                    Username = username
                                                    DayStart = dayStart
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

                                        let oldSelection = setter.get cellSelectionMap
                                        swapSelection oldSelection taskId referenceDay
                                    | true, _ ->
                                        let taskIdList = setter.get (Atoms.Session.taskIdList username)
                                        let oldCellSelectionMap = setter.get cellSelectionMap

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

    let initState (_initializer: MutableSnapshot) = ()
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
