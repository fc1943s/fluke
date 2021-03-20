namespace Fluke.UI.Frontend

#nowarn "40"

open System
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

    //    let peersArray =
//        let peer1 = Browser.Dom.window.localStorage.getItem "peer1"
//        let peer2 = Browser.Dom.window.localStorage.getItem "peer2"
//        // "http://localhost:8765/gun"
//        // "https://???.brazilsouth.azurecontainer.io:8765/gun"
//        match peer1, peer2 with
//        | (""
//          | null),
//          (""
//          | null) -> [||]
//        | peer1,
//          (""
//          | null) ->
//            [|
//                peer1
//            |]
//        | peer1, peer2 ->
//            [|
//                peer1
//                peer2
//            |]
//
//    printfn $"peersArray {peersArray}"

    let gunTmp (peers: string []) =
        Gun.gun (
            {
                Gun.GunProps.peers =
                    if JS.isTesting then
                        None
                    else
                        Some peers

                Gun.GunProps.radisk =
                    if JS.isTesting then
                        None
                    else
                        Some false

                Gun.GunProps.localStorage =
                    if JS.isTesting then
                        None
                    else
                        Some true
            }
            |> toPlainJsObj
            |> unbox
        )


    Browser.Dom.window?gunTmp <- gunTmp

    module Atoms =
        module rec Events =
            type EventId = EventId of position: float * guid: Guid

            [<RequireQualifiedAccess>]
            type Event =
                | AddDatabase of id: EventId * name: DatabaseName * dayStart: FlukeTime
                | NoOp

            let rec events =
                atomFamily {
                    key $"{nameof Events}/{nameof events}"

                    def
                        (fun (_eventId: EventId) ->
                            Profiling.addCount $"{nameof Events}/{nameof events}"
                            Event.NoOp)
                }

        module rec Information =
            type InformationId = InformationId of id: string

            let rec wrappedInformation =
                atomFamily {
                    key $"{nameof Information}/{nameof wrappedInformation}"

                    def
                        (fun (_informationId: InformationId) ->
                            Profiling.addCount $"{nameof Information}/{nameof wrappedInformation}"
                            Area (Area.Default, []))
                }

            let rec attachments =
                atomFamily {
                    key $"{nameof Information}/{nameof attachments}"

                    def
                        (fun (_informationId: InformationId) ->
                            Profiling.addCount $"{nameof Information}/{nameof attachments}"
                            []: Attachment list)
                }

            let rec informationId (information: Information) : InformationId =
                match information with
                | Archive _ ->
                    let (InformationId archiveId) = informationId information
                    $"{Information.toString information}/{archiveId}"
                | _ ->
                    let (InformationName informationName) = information.Name
                    $"{Information.toString information}/{informationName}"
                |> InformationId


        module rec Task =
            type TaskId = TaskId of informationName: InformationName * taskName: TaskName

            let newTaskId () =
                TaskId (InformationName (Guid.NewGuid().ToString ()), TaskName (Guid.NewGuid().ToString ()))

            let rec informationId =
                atomFamily {
                    key $"{nameof Task}/{nameof informationId}"

                    def
                        (fun (_taskId: TaskId) ->
                            Profiling.addCount $"{nameof Task}/{nameof informationId}"
                            Information.informationId Task.Default.Information)
                }

            let rec name =
                atomFamily {
                    key $"{nameof Task}/{nameof name}"

                    def
                        (fun (_taskId: TaskId) ->
                            Profiling.addCount $"{nameof Task}/{nameof name}"
                            Task.Default.Name)
                }

            let rec scheduling =
                atomFamily {
                    key $"{nameof Task}/{nameof scheduling}"

                    def
                        (fun (_taskId: TaskId) ->
                            Profiling.addCount $"{nameof Task}/{nameof scheduling}"
                            Task.Default.Scheduling)
                }

            let rec pendingAfter =
                atomFamily {
                    key $"{nameof Task}/{nameof pendingAfter}"

                    def
                        (fun (_taskId: TaskId) ->
                            Profiling.addCount $"{nameof Task}/{nameof pendingAfter}"
                            Task.Default.PendingAfter)
                }

            let rec missedAfter =
                atomFamily {
                    key $"{nameof Task}/{nameof missedAfter}"

                    def
                        (fun (_taskId: TaskId) ->
                            Profiling.addCount $"{nameof Task}/{nameof missedAfter}"
                            Task.Default.MissedAfter)
                }

            let rec priority =
                atomFamily {
                    key $"{nameof Task}/{nameof priority}"

                    def
                        (fun (_taskId: TaskId) ->
                            Profiling.addCount $"{nameof Task}/{nameof priority}"
                            Task.Default.Priority)
                }

            let rec attachments =
                atomFamily {
                    key $"{nameof Task}/{nameof attachments}"

                    def
                        (fun (_taskId: TaskId) ->
                            Profiling.addCount $"{nameof Task}/{nameof attachments}"
                            []: Attachment list) // TODO: move from here?
                }

            let rec duration =
                atomFamily {
                    key $"{nameof Task}/{nameof duration}"

                    def
                        (fun (_taskId: TaskId) ->
                            Profiling.addCount $"{nameof Task}/{nameof duration}"
                            Task.Default.Duration)
                }

            let taskId (task: Task) = TaskId (task.Information.Name, task.Name)


        module rec User =
            let rec color =
                atomFamily {
                    key $"{nameof User}/{nameof color}"

                    def
                        (fun (_username: Username) ->
                            Profiling.addCount $"{nameof User}/{nameof color}"
                            UserColor.Black)
                }

            let rec weekStart =
                atomFamily {
                    key $"{nameof User}/{nameof weekStart}"

                    def
                        (fun (_username: Username) ->
                            Profiling.addCount $"{nameof User}/{nameof weekStart}"
                            DayOfWeek.Sunday)
                }

            let rec dayStart =
                atomFamily {
                    key $"{nameof User}/{nameof dayStart}"

                    def
                        (fun (_username: Username) ->
                            Profiling.addCount $"{nameof User}/{nameof dayStart}"
                            FlukeTime.Create 0 0)
                }

            let rec sessionLength =
                atomFamily {
                    key $"{nameof User}/{nameof sessionLength}"

                    def
                        (fun (_username: Username) ->
                            Profiling.addCount $"{nameof User}/{nameof sessionLength}"
                            Minute 25.)
                }

            let rec sessionBreakLength =
                atomFamily {
                    key $"{nameof User}/{nameof sessionBreakLength}"

                    def
                        (fun (_username: Username) ->
                            Profiling.addCount $"{nameof User}/{nameof sessionBreakLength}"
                            Minute 5.)
                }




        module rec Session =
            let rec databaseStateMapCache =
                atomFamily {
                    key $"{nameof Session}/{nameof databaseStateMapCache}"

                    def
                        (fun (_username: Username) ->
                            Profiling.addCount $"{nameof Session}/{nameof databaseStateMapCache}"
                            Map.empty: Map<DatabaseId, DatabaseState>)
                }

            let rec availableDatabaseIds =
                atomFamily {
                    key $"{nameof Session}/{nameof availableDatabaseIds}"

                    def
                        (fun (_username: Username) ->
                            Profiling.addCount $"{nameof Session}/{nameof availableDatabaseIds}"
                            []: DatabaseId list)
                }

            let rec taskIdList =
                atomFamily {
                    key $"{nameof Session}/{nameof taskIdList}"

                    def
                        (fun (_username: Username) ->
                            Profiling.addCount $"{nameof Session}/{nameof taskIdList}"
                            []: Task.TaskId list)
                }


        module rec Cell =
            let rec taskId =
                atomFamily {
                    key $"{nameof Cell}/{nameof taskId}"

                    def
                        (fun (taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount $"{nameof Cell}/{nameof taskId}"
                            taskId)
                }

            let rec dateId =
                atomFamily {
                    key $"{nameof Cell}/{nameof dateId}"

                    def
                        (fun (_taskId: Task.TaskId, dateId: DateId) ->
                            Profiling.addCount $"{nameof Cell}/{nameof dateId}"
                            dateId)
                }

            let rec status =
                atomFamily {
                    key $"{nameof Cell}/{nameof status}"

                    def
                        (fun (_taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount $"{nameof Cell}/{nameof status}"
                            Disabled)
                }

            let rec attachments =
                atomFamily {
                    key $"{nameof Cell}/{nameof attachments}"

                    def
                        (fun (_taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount $"{nameof Cell}/{nameof attachments}"
                            []: Attachment list)
                }

            let rec sessions =
                atomFamily {
                    key $"{nameof Cell}/{nameof sessions}"

                    def
                        (fun (_taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount $"{nameof Cell}/{nameof sessions}"
                            []: TaskSession list)
                }

            let rec selected =
                atomFamily {
                    key $"{nameof Cell}/{nameof selected}"

                    def
                        (fun (_taskId: Task.TaskId, _dateId: DateId) ->
                            Profiling.addCount $"{nameof Cell}/{nameof selected}"
                            false)

                    effects
                        (fun (taskId: Task.TaskId, dateId: DateId) ->
                            [
                                (fun {
                                         onSet = onSet
                                         setSelf = setSelf
                                         trigger = trigger
                                     } ->

                                    match false with
                                    | false -> id
                                    | true ->

                                        let gunTmp = gunTmp [||]

                                        let taskIdHash =
                                            Crypto
                                                .sha3(string taskId)
                                                .toString Crypto.crypto.enc.Hex

                                        printfn
                                            $"cell.selected .effects. {
                                                                           JS.JSON.stringify
                                                                               {|
                                                                                   taskIdHash = taskIdHash
                                                                                   taskIdGuidHash =
                                                                                       (string taskId)
                                                                                       |> Crypto.getGuidHash
                                                                               |}
                                            }"

                                        let dateIdHash =
                                            Crypto
                                                .sha3(string dateId)
                                                .toString Crypto.crypto.enc.Hex

                                        let tasks = gunTmp.get "tasks"
                                        let task = tasks.get taskIdHash
                                        let cells = task.get "cells"
                                        let cell = cells.get dateIdHash
                                        let selected = cell.get "selected"

                                        match trigger with
                                        | "get" ->
                                            selected.on
                                                (fun value ->
                                                    //                                        printfn
                                                    //                                            "GET@@ CELL SELECTED RENDER . taskid: %A dateId: %A. node: %A"
                                                    //                                            taskId
                                                    //                                            dateId
                                                    //                                            node

                                                    setSelf value)
                                        | _ -> ()

                                        //                                        // Subscribe to storage updates
                                        //                                        storage.subscribe(value => setSelf(value));




                                        //                                printfn
                                        //                                    "CELL SELECTED RENDER . taskid: %A dateId: %A. trigger: %A"
                                        //                                    taskId
                                        //                                    dateId
                                        //                                    trigger
                                        //                            let storage = Browser.Dom.window.localStorage.getItem node.key
                                        //                            let value: {| value: obj |} option = unbox JS.JSON.parse storage
                                        //
                                        //                            match value with
                                        //                            | Some value -> setSelf (unbox value.value)
                                        //                            | _ -> ()
                                        //
                                        onSet
                                            (fun value oldValue ->

                                                let tasks = gunTmp.get "tasks"

                                                let task =
                                                    gunTmp
                                                        .get(taskIdHash)
                                                        .put {| id = taskIdHash; name = "taskName1" |}

                                                tasks.set task |> ignore

                                                let cells = task.get "cells"

                                                let cell =
                                                    gunTmp
                                                        .get(dateIdHash)
                                                        .put {| dateId = dateIdHash; selected = value |}

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

                                                printfn
                                                    $"cell.selected. effects. onSet. oldValue: {oldValue}; newValue: {
                                                                                                                          value
                                                    }"
                                                //                                    Browser.Dom.window.localStorage.setItem
                                                //                                        (node.key, JS.JSON.stringify {| value = string value |}))
                                                //
                                                //                                        // Subscribe to storage updates
                                                //                                        storage.subscribe(value => setSelf(value));

                                                )


                                        fun () ->
                                            printfn "> unsubscribe cell. calling selected.off ()"
                                            selected.off ())
                            ])
                }



        module rec Database =
            //            type DatabaseId = DatabaseId of id: string

            //        type DatabaseState =
//            {
//                Id: DatabaseId
//                Name: DatabaseName
//                Owner: User
//                SharedWith: DatabaseAccess list
//                //                Position: FlukeDateTime option
//                InformationStateMap: Map<Information, InformationState>
//                TaskStateMap: Map<Task, TaskState>
//            }
            let newDatabaseId () = DatabaseId (Guid.NewGuid ())

            let rec name =
                atomFamily {
                    key $"{nameof Database}/{nameof name}"

                    def
                        (fun (_databaseId: DatabaseId) ->
                            Profiling.addCount $"{nameof Database}/{nameof name}"
                            DatabaseName "")
                }


            let rec owner =
                atomFamily {
                    key $"{nameof Database}/{nameof owner}"

                    def
                        (fun (_databaseId: DatabaseId) ->
                            Profiling.addCount $"{nameof Database}/{nameof owner}"
                            None: User option)
                }


            let rec sharedWith =
                atomFamily {
                    key $"{nameof Database}/{nameof sharedWith}"

                    def
                        (fun (_databaseId: DatabaseId) ->
                            Profiling.addCount $"{nameof Database}/{nameof sharedWith}"
                            DatabaseAccess.Public)
                }

            let rec dayStart =
                atomFamily {
                    key $"{nameof Database}/{nameof dayStart}"

                    def
                        (fun (_databaseId: DatabaseId) ->
                            Profiling.addCount $"{nameof Database}/{nameof dayStart}"
                            FlukeTime.Create 7 0)
                }


            let rec position =
                atomFamily {
                    key $"{nameof Database}/{nameof position}"

                    def
                        (fun (_databaseId: DatabaseId) ->
                            Profiling.addCount $"{nameof Database}/{nameof position}"
                            None: FlukeDateTime option)
                }


        let rec debug =
            atom {
                key $"atom/{nameof debug}"
                def false
                local_storage
            }

        let rec isTesting =
            atom {
                key $"atom/{nameof isTesting}"
                def JS.isTesting
            }

        let rec view =
            atom {
                key $"atom/{nameof view}"
                def View.View.HabitTracker
            }

        let rec selectedDatabaseIds =
            atom {
                key $"atom/{nameof selectedDatabaseIds}"
                def ([||]: DatabaseId [])
                local_storage
            }

        let rec selectedPosition =
            atom {
                key $"atom/{nameof selectedPosition}"
                def (None: FlukeDateTime option)
                local_storage
            }

        let rec cellMenuOpened =
            atom {
                key $"atom/{nameof cellMenuOpened}"
                def (None: (Task.TaskId * DateId) option)
            }

        let rec cellSize =
            atom {
                key $"atom/{nameof cellSize}"
                def 17
            }

        let rec daysBefore =
            atom {
                key $"atom/{nameof daysBefore}"
                def 7
            }

        let rec daysAfter =
            atom {
                key $"atom/{nameof daysAfter}"
                def 7
                local_storage
            }

        let rec leftDock =
            atom {
                key $"atom/{nameof leftDock}"
                def (None: TempUI.DockType option)
                local_storage
            }

        let rec formDatabaseId =
            atom {
                key $"atom/{nameof formDatabaseId}"
                def (None: State.DatabaseId option)
            }

        let rec taskIdForm =
            atom {
                key $"atom/{nameof taskIdForm}"
                def (None: Task.TaskId option)
            }

        let rec apiBaseUrl =
            atom {
                key $"atom/{nameof apiBaseUrl}"
                def $"https://localhost:{Sync.serverPort}"
                local_storage
            }

        let rec gunPeer1 =
            atom {
                key $"atom/{nameof gunPeer1}"
                def ""
                local_storage
            }

        let rec gunPeer2 =
            atom {
                key $"atom/{nameof gunPeer2}"
                def ""
                local_storage
            }

        let rec gunPeer3 =
            atom {
                key $"atom/{nameof gunPeer3}"
                def ""
                local_storage
            }

        let rec gunKeys =
            atom {
                key $"atom/{nameof gunKeys}"

                def
                    {
                        Gun.pub = ""
                        Gun.epub = ""
                        Gun.priv = ""
                        Gun.epriv = ""
                    }

//                local_storage
            }

        let rec api =
            atom {
                key $"atom/{nameof api}"
                def (None: Sync.Api option)
            }

        let rec username =
            atom {
                key $"atom/{nameof username}"
                def None
            }

        let rec sessionRestored =
            atom {
                key $"atom/{nameof sessionRestored}"
                def false
            }

        let rec getLivePosition =
            atom {
                key $"atom/{nameof getLivePosition}"

                def
                    {|
                        Get = fun () -> FlukeDateTime.FromDateTime DateTime.Now
                    |}

            }

        let rec ctrlPressed =
            atom {
                key $"atom/{nameof ctrlPressed}"
                def false
            }

        let rec shiftPressed =
            atom {
                key $"atom/{nameof shiftPressed}"
                def false
            }

        let rec positionTrigger =
            atom {
                key $"atom/{nameof positionTrigger}"
                def 0
            }


    module Selectors =
        let rec gun =
            selector {
                key $"selector/{nameof gun}"

                get
                    (fun getter ->
                        let gunPeer1 = getter.get Atoms.gunPeer1
                        let gunPeer2 = getter.get Atoms.gunPeer2
                        let gunPeer3 = getter.get Atoms.gunPeer3
                        //                        let gun = Gun.gun peers
                        let peers =
                            [|
                                gunPeer1
                                gunPeer2
                                gunPeer3
                            |]
                            |> Array.filter (String.IsNullOrWhiteSpace >> not)

                        let gun = gunTmp peers

                        printfn $"gun selector. peers={peers}. returning gun..."

                        Profiling.addCount (nameof gun)
                        {| ref = gun |})
            }

        let rec gunNamespace =
            selector {
                key $"selector/{nameof gunNamespace}"

                get
                    (fun getter ->
                        let gun = getter.get gun
                        let username = getter.get Atoms.username
                        let gunKeys = getter.get Atoms.gunKeys
                        let user = gun.ref.user ()
                        Browser.Dom.window?gunNamespace <- user

                        printfn $"gun selector. username={username} gunKeys={JS.JSON.stringify gunKeys}. returning gun namespace..."

                        Profiling.addCount (nameof gunNamespace)
                        {| ref = user |})
            }


        let rec apiCurrentUserAsync =
            selector {
                key $"selector/{nameof apiCurrentUserAsync}"

                get
                    (fun getter ->
                        async {
                            let api = getter.get Atoms.api

                            let! result =
                                api
                                |> Option.bind (fun api -> Some api.currentUser)
                                |> Sync.handleRequest

                            Profiling.addCount (nameof apiCurrentUserAsync)

                            return result
                        })
            }

        let rec position =
            selector {
                key $"selector/{nameof position}"

                get
                    (fun getter ->
                        let _positionTrigger = getter.get Atoms.positionTrigger
                        let getLivePosition = getter.get Atoms.getLivePosition
                        let selectedPosition = getter.get Atoms.selectedPosition

                        let result =
                            selectedPosition
                            |> Option.defaultValue (getLivePosition.Get ())
                            |> Some

                        Profiling.addCount (nameof position)
                        result)

                set
                    (fun setter _newValue ->
                        setter.set (Atoms.positionTrigger, (fun x -> x + 1))
                        Profiling.addCount (nameof position + " (SET)"))
            }

        let rec dateSequence =
            selector {
                key $"selector/{nameof dateSequence}"

                get
                    (fun getter ->
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

        let rec cellSelectionMap =
            selector {
                key $"selector/{nameof cellSelectionMap}"

                get
                    (fun getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)
                            let dateSequence = getter.get dateSequence

                            let result =
                                taskIdList
                                |> List.map
                                    (fun taskId ->
                                        let dates =
                                            dateSequence
                                            |> List.map
                                                (fun date ->
                                                    date, getter.get (Atoms.Cell.selected (taskId, DateId date)))
                                            |> List.filter snd
                                            |> List.map fst
                                            |> Set.ofList

                                        taskId, dates)
                                |> List.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                                |> Map.ofList

                            Profiling.addCount (nameof cellSelectionMap)
                            result
                        | None -> Map.empty)

                set
                    (fun setter (newSelection: Map<Atoms.Task.TaskId, Set<FlukeDate>>) ->
                        let username = setter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = setter.get (Atoms.Session.taskIdList username)
                            let cellSelectionMap = setter.get cellSelectionMap

                            let operations =
                                taskIdList
                                |> List.collect
                                    (fun taskId ->
                                        let dates =
                                            cellSelectionMap
                                            |> Map.tryFind taskId
                                            |> Option.defaultValue Set.empty

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
                            |> List.iter
                                (fun (taskId, date, selected) ->
                                    setter.set (Atoms.Cell.selected (taskId, DateId date), selected))

                            Profiling.addCount (nameof cellSelectionMap + " (SET)")
                        | None -> ())
            }

        module rec FlukeDate =
            let isToday =
                selectorFamily {
                    key $"{nameof FlukeDate}/{nameof isToday}"

                    get
                        (fun (date: FlukeDate) getter ->
                            let username = getter.get Atoms.username
                            let position = getter.get position

                            let result =
                                match username, position with
                                | Some username, Some position ->
                                    let dayStart = getter.get (Atoms.User.dayStart username)

                                    Domain.UserInteraction.isToday dayStart position (DateId date)
                                | _ -> false

                            Profiling.addCount $"{nameof FlukeDate}/{nameof isToday}"
                            result)
                }

            let rec hasSelection =
                selectorFamily {
                    key $"{nameof FlukeDate}/{nameof hasSelection}"

                    get
                        (fun (date: FlukeDate) getter ->
                            let username = getter.get Atoms.username

                            match username with
                            | Some username ->
                                let taskIdList = getter.get (Atoms.Session.taskIdList username)

                                let result =
                                    taskIdList
                                    |> List.exists
                                        (fun taskId -> getter.get (Atoms.Cell.selected (taskId, DateId date)))

                                Profiling.addCount $"{nameof FlukeDate}/{nameof hasSelection}"
                                result
                            | None -> false)
                }

        module rec Information =
            ()

        module rec Task =
            let rec lastSession =
                selectorFamily {
                    key $"{nameof Task}/{nameof lastSession}"

                    get
                        (fun (taskId: Atoms.Task.TaskId) getter ->
                            let dateSequence = getter.get dateSequence

                            let result =
                                dateSequence
                                |> List.rev
                                |> List.tryPick
                                    (fun date ->
                                        let sessions = getter.get (Atoms.Cell.sessions (taskId, DateId date))

                                        sessions
                                        |> List.sortByDescending (fun (TaskSession (start, _, _)) -> start.DateTime)
                                        |> List.tryHead)

                            Profiling.addCount $"{nameof Task}/{nameof lastSession}"
                            result)
                }

            let rec activeSession =
                selectorFamily {
                    key $"{nameof Task}/{nameof activeSession}"

                    get
                        (fun (taskId: Atoms.Task.TaskId) getter ->
                            let position = getter.get position
                            let lastSession = getter.get (lastSession taskId)

                            let result =
                                match position, lastSession with
                                | Some position, Some lastSession ->
                                    let (TaskSession (start, Minute duration, Minute breakDuration)) = lastSession

                                    let currentDuration = (position.DateTime - start.DateTime).TotalMinutes

                                    let active = currentDuration < duration + breakDuration

                                    match active with
                                    | true -> Some currentDuration
                                    | false -> None

                                | _ -> None

                            Profiling.addCount $"{nameof Task}/{nameof activeSession}"

                            result)
                }

            let rec showUser =
                selectorFamily {
                    key $"{nameof Task}/{nameof showUser}"

                    get
                        (fun (taskId: Atoms.Task.TaskId) getter ->
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
                                |> List.choose
                                    (function
                                    | UserStatus (user, _) -> Some user
                                    | _ -> None)
                                |> Seq.distinct
                                |> Seq.length

                            let result = usersCount > 1

                            Profiling.addCount $"{nameof Task}/{nameof showUser}"
                            result
                            //                            | None -> false
                            )
                }

            let rec hasSelection =
                selectorFamily {
                    key $"{nameof Task}/{nameof hasSelection}"

                    get
                        (fun (taskId: Atoms.Task.TaskId) getter ->
                            let dateSequence = getter.get dateSequence

                            let result =
                                dateSequence
                                |> List.exists (fun date -> getter.get (Atoms.Cell.selected (taskId, DateId date)))

                            Profiling.addCount $"{nameof Task}/{nameof hasSelection}"
                            result)
                }

        module rec Session =
            let rec activeSessions =
                selectorFamily {
                    key $"{nameof Session}/{nameof activeSessions}"

                    get
                        (fun (username: Username) getter ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)

                            let result =
                                let sessionLength = getter.get (Atoms.User.sessionLength username)
                                let sessionBreakLength = getter.get (Atoms.User.sessionBreakLength username)

                                taskIdList
                                |> List.map
                                    (fun taskId ->
                                        let (TaskName taskName) = getter.get (Atoms.Task.name taskId)

                                        let duration = getter.get (Task.activeSession taskId)

                                        duration
                                        |> Option.map
                                            (fun duration ->
                                                TempUI.ActiveSession (
                                                    taskName,
                                                    Minute duration,
                                                    sessionLength,
                                                    sessionBreakLength
                                                )))
                                |> List.choose id

                            Profiling.addCount $"{nameof Session}/{nameof activeSessions}"
                            result)
                }

            let rec tasksByInformationKind =
                selectorFamily {
                    key $"{nameof Session}/{nameof tasksByInformationKind}"

                    get
                        (fun (username: Username) getter ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)

                            let informationMap =
                                taskIdList
                                |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.informationId taskId))
                                |> List.map
                                    (fun (taskId, informationId) ->
                                        taskId, getter.get (Atoms.Information.wrappedInformation informationId))
                                |> Map.ofList

                            let informationKindGroups =
                                taskIdList
                                |> List.groupBy (fun taskId -> informationMap.[taskId])
                                |> List.sortBy (fun (information, _) -> information.Name)
                                |> List.groupBy (fun (information, _) -> Information.toString information)
                                |> List.sortBy (snd >> List.head >> fst >> Information.toTag)
                                |> List.map
                                    (fun (informationKindName, groups) ->
                                        let newGroups =
                                            groups
                                            |> List.map
                                                (fun (information, taskIdList) ->
                                                    let informationId = Atoms.Information.informationId information

                                                    informationId, taskIdList)

                                        informationKindName, newGroups)

                            Profiling.addCount $"{nameof Session}/{nameof tasksByInformationKind}"
                            informationKindGroups)
                }

            let rec weekCellsMap =
                selectorFamily {
                    key $"{nameof Session}/{nameof weekCellsMap}"

                    get
                        (fun (username: Username) getter ->
                            let position = getter.get position
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)
                            let sessionData : SessionData option = getter.get (sessionData username)

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
                                        |> List.map
                                            (fun weekOffset ->
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
                                                    |> List.collect
                                                        (fun taskId ->
                                                            dateIdSequence
                                                            |> List.map
                                                                (fun dateId ->
                                                                    match dateId with
                                                                    | DateId referenceDay as dateId ->
                                                                        //                                                    let taskId = getter.get task.Id
                                                                        let status =
                                                                            getter.get (
                                                                                Atoms.Cell.status (taskId, dateId)
                                                                            )

                                                                        let sessions =
                                                                            getter.get (
                                                                                Atoms.Cell.sessions (taskId, dateId)
                                                                            )

                                                                        let attachments =
                                                                            getter.get (
                                                                                Atoms.Cell.attachments (taskId, dateId)
                                                                            )

                                                                        let isToday =
                                                                            getter.get (FlukeDate.isToday referenceDay)

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
                                                    |> List.map
                                                        (fun (dateId, cells) ->
                                                            match dateId with
                                                            | DateId referenceDay as dateId ->

                                                                //                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList
                                                                let taskSessions =
                                                                    cells |> List.collect (fun x -> x.Sessions)

                                                                let sortedTasksMap =
                                                                    cells
                                                                    |> List.map
                                                                        (fun cell ->
                                                                            let taskState =

                                                                                let task = taskMap.[cell.TaskId]

                                                                                {
                                                                                    Task = task
                                                                                    Sessions = taskSessions
                                                                                    Attachments = []
                                                                                    SortList = []
                                                                                    InformationMap = Map.empty
                                                                                    CellStateMap = Map.empty
                                                                                }

                                                                            taskState,
                                                                            [
                                                                                {
                                                                                    Task = taskState.Task
                                                                                    DateId = dateId
                                                                                },
                                                                                cell.Status
                                                                            ])
                                                                    |> Sorting.sortLanesByTimeOfDay
                                                                        dayStart
                                                                        { Date = referenceDay; Time = dayStart }
                                                                    |> List.indexed
                                                                    |> List.map
                                                                        (fun (i, (taskState, _)) ->
                                                                            Atoms.Task.taskId taskState.Task, i)
                                                                    |> Map.ofList

                                                                let newCells =
                                                                    cells
                                                                    |> List.sortBy
                                                                        (fun cell -> sortedTasksMap.[cell.TaskId])

                                                                dateId, newCells)
                                                    |> Map.ofList

                                                result)

                                    weeks
                                | _ -> []

                            Profiling.addCount $"{nameof Session}/{nameof weekCellsMap}"
                            result)
                }

            let rec sessionData =
                selectorFamily {
                    key $"{nameof Session}/{nameof sessionData}"

                    get
                        (fun (username: Username) getter ->
                            let databaseStateMap = getter.get (Atoms.Session.databaseStateMapCache username)
                            let dateSequence = getter.get dateSequence
                            let view = getter.get Atoms.view
                            let position = getter.get position
                            let selectedDatabaseIds = getter.get Atoms.selectedDatabaseIds

                            let dayStart = getter.get (Atoms.User.dayStart username)

                            let result =
                                match position, databaseStateMap.Count, dateSequence.Length with
                                | Some position, databaseCount, dateSequenceLength when
                                    databaseCount > 0 && dateSequenceLength > 0 ->

                                    let newSession =
                                        getSessionData
                                            {|
                                                Username = username
                                                DayStart = dayStart
                                                DateSequence = dateSequence
                                                View = view
                                                Position = position
                                                SelectedDatabaseIds = selectedDatabaseIds |> Set.ofArray
                                                DatabaseStateMap = databaseStateMap
                                            |}

                                    Some newSession
                                | _ -> None

                            Profiling.addCount $"{nameof Session}/{nameof sessionData}"

                            result)
                }


        module rec Cell =
            let rec selected =
                selectorFamily {
                    key $"{nameof Cell}/{nameof selected}"

                    get
                        (fun (taskId: Atoms.Task.TaskId, dateId: DateId) getter ->
                            let selected = getter.get (Atoms.Cell.selected (taskId, dateId))

                            Profiling.addCount $"{nameof Cell}/{nameof selected}"
                            selected)

                    set
                        (fun (taskId: Atoms.Task.TaskId, (DateId referenceDay)) setter (newValue: bool) ->
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
                                            |> Map.toSeq
                                            |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                                            |> Seq.map fst
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

                                        let dateSet =
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
                                            |> List.map (fun taskId -> taskId, dateSet)
                                            |> Map.ofList

                                        newMap

                                setter.set (cellSelectionMap, newCellSelectionMap)

                            | None -> ()

                            Profiling.addCount $"{nameof Cell}/{nameof selected} (SET)")
                }
