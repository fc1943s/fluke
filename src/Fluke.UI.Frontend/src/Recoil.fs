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
open Fable.Extras


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

                Gun.GunProps.localStorage = if JS.isTesting then None else Some true
            }
            |> unbox
        )


    Browser.Dom.window?gunTmp <- gunTmp


    module Atoms =
        module rec Events =
            type EventId = EventId of position: float * guid: Guid

            [<RequireQualifiedAccess>]
            type Event =
                | AddDatabase of id: EventId * name: DatabaseName * dayStart: FlukeTime
                | AddTask of id: EventId * name: TaskName
                | NoOp

            let rec events =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Events}/{nameof events}",
                    (fun (_eventId: EventId) -> Event.NoOp)
                )


        module rec Information =
            type InformationId = InformationId of id: string

            let rec wrappedInformation =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Information}/{nameof wrappedInformation}",
                    (fun (_informationId: InformationId) -> Area (Area.Default, []))
                )

            let rec attachments =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Information}/{nameof attachments}",
                    (fun (_informationId: InformationId) -> []: Attachment list)
                )

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
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof informationId}",
                    (fun (_taskId: TaskId) -> Information.informationId Task.Default.Information)
                )

            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof name}",
                    (fun (_taskId: TaskId) -> Task.Default.Name)
                )

            let rec scheduling =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof scheduling}",
                    (fun (_taskId: TaskId) -> Task.Default.Scheduling)
                )

            let rec pendingAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof pendingAfter}",
                    (fun (_taskId: TaskId) -> Task.Default.PendingAfter)
                )

            let rec missedAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof missedAfter}",
                    (fun (_taskId: TaskId) -> Task.Default.MissedAfter)
                )

            let rec priority =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof priority}",
                    (fun (_taskId: TaskId) -> Task.Default.Priority)
                )

            let rec attachments =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof attachments}",
                    (fun (_taskId: TaskId) -> []: Attachment list) // TODO: move from here?
                )

            let rec duration =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof duration}",
                    (fun (_taskId: TaskId) -> Task.Default.Duration)
                )

            let taskId (task: Task) =
                TaskId (task.Information.Name, task.Name)


        module rec User =
            let rec color =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof color}",
                    (fun (_username: Username) -> UserColor.Black)
                )

            let rec weekStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof weekStart}",
                    (fun (_username: Username) -> DayOfWeek.Sunday)
                )

            let rec dayStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof dayStart}",
                    (fun (_username: Username) -> FlukeTime.Create 0 0)
                )

            let rec sessionLength =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof sessionLength}",
                    (fun (_username: Username) -> Minute 25.)
                )

            let rec sessionBreakLength =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof sessionBreakLength}",
                    (fun (_username: Username) -> Minute 5.)
                )


        module rec Session =
            let rec databaseStateMapCache =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Session}/{nameof databaseStateMapCache}",
                    (fun (_username: Username) -> Map.empty: Map<DatabaseId, DatabaseState>)
                )

            let rec availableDatabaseIds =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Session}/{nameof availableDatabaseIds}",
                    (fun (_username: Username) -> []: DatabaseId list)
                )

            let rec taskIdList =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Session}/{nameof taskIdList}",
                    (fun (_username: Username) -> []: Task.TaskId list)
                )


        module rec Cell =
            let rec taskId =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof taskId}",
                    (fun (taskId: Task.TaskId, _dateId: DateId) -> taskId)
                )

            let rec dateId =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof dateId}",
                    (fun (_taskId: Task.TaskId, dateId: DateId) -> dateId)
                )

            let rec status =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof status}",
                    (fun (_taskId: Task.TaskId, _dateId: DateId) -> Disabled)
                )

            let rec attachments =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof attachments}",
                    (fun (_taskId: Task.TaskId, _dateId: DateId) -> []: Attachment list)
                )

            let rec sessions =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof sessions}",
                    (fun (_taskId: Task.TaskId, _dateId: DateId) -> []: TaskSession list)
                )

            let rec selected =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof selected}",
                    (fun (_taskId: Task.TaskId, _dateId: DateId) -> false),
                    (fun (taskId: Task.TaskId, dateId: DateId) ->
                        [
                            (fun e ->
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
                                                                                   (string taskId) |> Crypto.getGuidHash
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
                                    let selected = cell.get<bool> "selected"

                                    match e.trigger with
                                    | "get" ->
                                        selected.on
                                            (fun value ->
                                                //                                        printfn
                                                //                                            "GET@@ CELL SELECTED RENDER . taskid: %A dateId: %A. node: %A"
                                                //                                            taskId
                                                //                                            dateId
                                                //                                            node

                                                e.setSelf value)
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
                                    e.onSet
                                        (fun value oldValue ->

                                            let tasks = gunTmp.get "tasks"

                                            let task =
                                                gunTmp
                                                    .get(taskIdHash)
                                                    .put {|
                                                             id = taskIdHash
                                                             name = "taskName1"
                                                         |}

                                            tasks.set task |> ignore

                                            let cells = task.get "cells"

                                            let cell =
                                                gunTmp
                                                    .get(dateIdHash)
                                                    .put {|
                                                             dateId = dateIdHash
                                                             selected = value
                                                         |}

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
                                                $"cell.selected. effects. onSet. oldValue: {oldValue}; newValue: {value}"
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
                )


        module rec Database =
            let newDatabaseId () = DatabaseId (Guid.NewGuid ())

            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof name}",
                    (fun (_databaseId: DatabaseId) -> DatabaseName "")
                )


            let rec owner =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof owner}",
                    (fun (_databaseId: DatabaseId) -> None: Username option)
                )


            let rec sharedWith =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof sharedWith}",
                    (fun (_databaseId: DatabaseId) -> DatabaseAccess.Public)
                )

            let rec dayStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof dayStart}",
                    (fun (_databaseId: DatabaseId) -> FlukeTime.Create 7 0)
                )


            let rec position =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof position}",
                    (fun (_databaseId: DatabaseId) -> None: FlukeDateTime option)
                )


        let rec debug =
            Recoil.atom (
                $"{nameof atom}/{nameof debug}",
                false,
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec isTesting = Recoil.atom ($"{nameof atom}/{nameof isTesting}", JS.isTesting)

        let rec view = Recoil.atom ($"{nameof atom}/{nameof view}", View.View.HabitTracker)

        let rec selectedDatabaseIds =
            Recoil.atom (
                $"{nameof atom}/{nameof selectedDatabaseIds}",
                ([||]: DatabaseId []),
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec selectedPosition =
            Recoil.atom (
                $"{nameof atom}/{nameof selectedPosition}",
                (None: FlukeDateTime option),
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec cellMenuOpened =
            Recoil.atom ($"{nameof atom}/{nameof cellMenuOpened}", (None: (Task.TaskId * DateId) option))

        let rec cellSize = Recoil.atom ($"{nameof atom}/{nameof cellSize}", 17)

        let rec daysBefore = Recoil.atom ($"{nameof atom}/{nameof daysBefore}", 7)

        let rec daysAfter = Recoil.atom ($"{nameof atom}/{nameof daysAfter}", 7)

        let rec leftDock =
            Recoil.atom (
                $"{nameof atom}/{nameof leftDock}",
                (None: TempUI.DockType option),
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec formDatabaseId = Recoil.atom ($"{nameof atom}/{nameof formDatabaseId}", (None: State.DatabaseId option))

        let rec formTaskId = Recoil.atom ($"{nameof atom}/{nameof formTaskId}", (None: Task.TaskId option))

        let rec apiBaseUrl =
            Recoil.atom (
                $"{nameof atom}/{nameof apiBaseUrl}",
                $"https://localhost:{Sync.serverPort}",
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec gunPeer1 =
            Recoil.atom (
                $"{nameof atom}/{nameof gunPeer1}",
                "",
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec gunPeer2 =
            Recoil.atom (
                $"{nameof atom}/{nameof gunPeer2}",
                "",
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec gunPeer3 =
            Recoil.atom (
                $"{nameof atom}/{nameof gunPeer3}",
                "",
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec gunKeys =
            Recoil.atom (
                $"{nameof atom}/{nameof gunKeys}",
                {
                    Gun.pub = ""
                    Gun.epub = ""
                    Gun.priv = ""
                    Gun.epriv = ""
                }

            //                local_storage
            )

        let rec api = Recoil.atom ($"{nameof atom}/{nameof api}", (None: Sync.Api option))

        let rec username = Recoil.atom ($"{nameof atom}/{nameof username}", None)

        let rec sessionRestored = Recoil.atom ($"{nameof atom}/{nameof sessionRestored}", false)

        let rec getLivePosition =
            Recoil.atom (
                $"{nameof atom}/{nameof getLivePosition}",
                {|
                    Get = fun () -> FlukeDateTime.FromDateTime DateTime.Now
                |}
            )

        let rec ctrlPressed = Recoil.atom ($"{nameof atom}/{nameof ctrlPressed}", false)

        let rec shiftPressed = Recoil.atom ($"{nameof atom}/{nameof shiftPressed}", false)

        let rec initialPeerSkipped = Recoil.atom ($"{nameof atom}/{nameof initialPeerSkipped}", false)

        let rec positionTrigger = Recoil.atom ($"{nameof atom}/{nameof positionTrigger}", 0)


    module Selectors =
        let rec gunPeers =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof gunPeers}",
                (fun getter ->
                    let gunPeer1 = getter.get Atoms.gunPeer1
                    let gunPeer2 = getter.get Atoms.gunPeer2
                    let gunPeer3 = getter.get Atoms.gunPeer3
                    //                        let gun = Gun.gun peers
                    [|
                        gunPeer1
                        gunPeer2
                        gunPeer3
                    |]
                    |> Array.filter (String.IsNullOrWhiteSpace >> not))
            )

        let rec gun =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof gun}",
                (fun getter ->
                    let gunPeers = getter.get gunPeers
                    let gun = gunTmp gunPeers

                    printfn $"gun selector. peers={gunPeers}. returning gun..."
                    gun.put null |> ignore

                    {| ref = gun |})
            )

        let rec gunNamespace =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof gunNamespace}",
                (fun getter ->
                    let gun = getter.get gun
                    let username = getter.get Atoms.username
                    let gunKeys = getter.get Atoms.gunKeys
                    let user = gun.ref.user ()
                    Browser.Dom.window?gunNamespace <- user

                    printfn
                        $"gun selector. username={username} gunKeys={JS.JSON.stringify gunKeys}. returning gun namespace..."

                    {| ref = user |})
            )


        let rec apiCurrentUserAsync =
            Recoil.asyncSelectorWithProfiling (
                $"{nameof selector}/{nameof apiCurrentUserAsync}",
                (fun getter ->
                    promise {
                        let api = getter.get Atoms.api

                        return!
                            api
                            |> Option.bind (fun api -> Some api.currentUser)
                            |> Sync.handleRequest
                    })
            )

        let rec position =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof position}",
                (fun getter ->
                    let _positionTrigger = getter.get Atoms.positionTrigger
                    let getLivePosition = getter.get Atoms.getLivePosition
                    let selectedPosition = getter.get Atoms.selectedPosition

                    selectedPosition
                    |> Option.defaultValue (getLivePosition.Get ())
                    |> Some

                    ),
                (fun setter _newValue -> setter.set (Atoms.positionTrigger, (fun x -> x + 1)))
            )

        let rec dateSequence =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof dateSequence}",
                (fun getter ->
                    let daysBefore = getter.get Atoms.daysBefore
                    let daysAfter = getter.get Atoms.daysAfter
                    let username = getter.get Atoms.username
                    let position = getter.get position

                    match position, username with
                    | Some position, Some username ->
                        let dayStart = getter.get (Atoms.User.dayStart username)
                        let dateId = dateId dayStart position
                        let (DateId referenceDay) = dateId

                        referenceDay
                        |> List.singleton
                        |> Rendering.getDateSequence (daysBefore, daysAfter)
                    | _ -> [])
            )

        let rec cellSelectionMap =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof cellSelectionMap}",
                (fun getter ->
                    let username = getter.get Atoms.username

                    match username with
                    | Some username ->
                        let taskIdList = getter.get (Atoms.Session.taskIdList username)
                        let dateSequence = getter.get dateSequence

                        taskIdList
                        |> List.map
                            (fun taskId ->
                                let dates =
                                    dateSequence
                                    |> List.map
                                        (fun date -> date, getter.get (Atoms.Cell.selected (taskId, DateId date)))
                                    |> List.filter snd
                                    |> List.map fst
                                    |> Set.ofList

                                taskId, dates)
                        |> List.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                        |> Map.ofList
                    | None -> Map.empty),

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
                    | None -> ())
            )

        type DeviceInfo = { IsEdge: bool; IsMobile: bool }

        let rec deviceInfo =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof deviceInfo}",
                (fun _getter ->
                    let userAgent =
                        if Browser.Dom.window?navigator = null then
                            ""
                        else
                            Browser.Dom.window?navigator?userAgent

                    {
                        IsEdge = (JSe.RegExp @"Edg\/").Test userAgent
                        IsMobile =
                            JSe
                                .RegExp("Android|BlackBerry|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop",
                                        JSe.RegExpFlag().i)
                                .Test userAgent
                    })
            )

        module rec FlukeDate =
            let isToday =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof FlukeDate}/{nameof isToday}",
                    (fun (date: FlukeDate) getter ->
                        let username = getter.get Atoms.username
                        let position = getter.get position

                        match username, position with
                        | Some username, Some position ->
                            let dayStart = getter.get (Atoms.User.dayStart username)

                            Domain.UserInteraction.isToday dayStart position (DateId date)
                        | _ -> false)
                )

            let rec hasSelection =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof FlukeDate}/{nameof hasSelection}",
                    (fun (date: FlukeDate) getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)

                            taskIdList
                            |> List.exists (fun taskId -> getter.get (Atoms.Cell.selected (taskId, DateId date)))
                        | None -> false)
                )

        module rec Information =
            ()

        module rec Task =
            let rec lastSession =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof lastSession}",
                    (fun (taskId: Atoms.Task.TaskId) getter ->
                        let dateSequence = getter.get dateSequence

                        dateSequence
                        |> List.rev
                        |> List.tryPick
                            (fun date ->
                                let sessions = getter.get (Atoms.Cell.sessions (taskId, DateId date))

                                sessions
                                |> List.sortByDescending (fun (TaskSession (start, _, _)) -> start.DateTime)
                                |> List.tryHead))
                )

            let rec activeSession =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof activeSession}",
                    (fun (taskId: Atoms.Task.TaskId) getter ->
                        let position = getter.get position
                        let lastSession = getter.get (lastSession taskId)

                        match position, lastSession with
                        | Some position, Some lastSession ->
                            let (TaskSession (start, Minute duration, Minute breakDuration)) = lastSession

                            let currentDuration = (position.DateTime - start.DateTime).TotalMinutes

                            let active = currentDuration < duration + breakDuration

                            match active with
                            | true -> Some currentDuration
                            | false -> None

                        | _ -> None)
                )

            let rec showUser =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof showUser}",
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

                        usersCount > 1
                        //                            | None -> false
                        )
                )

            let rec hasSelection =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof hasSelection}",
                    (fun (taskId: Atoms.Task.TaskId) getter ->
                        let dateSequence = getter.get dateSequence

                        dateSequence
                        |> List.exists (fun date -> getter.get (Atoms.Cell.selected (taskId, DateId date))))
                )

        module rec Session =
            let rec activeSessions =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof activeSessions}",
                    (fun (username: Username) getter ->
                        let taskIdList = getter.get (Atoms.Session.taskIdList username)

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
                        |> List.choose id)
                )

            let rec tasksByInformationKind =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof tasksByInformationKind}",
                    (fun (username: Username) getter ->
                        let taskIdList = getter.get (Atoms.Session.taskIdList username)

                        let informationMap =
                            taskIdList
                            |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.informationId taskId))
                            |> List.map
                                (fun (taskId, informationId) ->
                                    taskId, getter.get (Atoms.Information.wrappedInformation informationId))
                            |> Map.ofList

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

                                informationKindName, newGroups))
                )

            let rec weekCellsMap =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof weekCellsMap}",
                    (fun (username: Username) getter ->
                        let position = getter.get position
                        let taskIdList = getter.get (Atoms.Session.taskIdList username)
                        let sessionData : SessionData option = getter.get (sessionData username)

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
                                                                    getter.get (Atoms.Cell.status (taskId, dateId))

                                                                let sessions =
                                                                    getter.get (Atoms.Cell.sessions (taskId, dateId))

                                                                let attachments =
                                                                    getter.get (Atoms.Cell.attachments (taskId, dateId))

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
                                                        let taskSessions = cells |> List.collect (fun x -> x.Sessions)

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
                                                            |> List.sortBy (fun cell -> sortedTasksMap.[cell.TaskId])

                                                        dateId, newCells)
                                            |> Map.ofList

                                        result)

                            weeks
                        | _ -> [])
                )

            let rec sessionData =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof sessionData}",
                    (fun (username: Username) getter ->
                        let databaseStateMap = getter.get (Atoms.Session.databaseStateMapCache username)
                        let dateSequence = getter.get dateSequence
                        let view = getter.get Atoms.view
                        let position = getter.get position
                        let selectedDatabaseIds = getter.get Atoms.selectedDatabaseIds

                        let dayStart = getter.get (Atoms.User.dayStart username)

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
                        | _ -> None)
                )


        module rec Cell =
            let rec selected =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Cell}/{nameof selected}",
                    (fun (taskId: Atoms.Task.TaskId, dateId: DateId) getter ->
                        getter.get (Atoms.Cell.selected (taskId, dateId))),
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
                                            let fn = if newValue then Set.add else Set.remove

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
                        | None -> ())
                )
