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


module State =
    open Model
    open Domain.UserInteraction
    open Domain.State
    open View

    type TextKey = TextKey of key: string

    and TextKey with
        static member inline Value (TextKey key) = key

    module Atoms =
        let rec isTesting = Recoil.atomWithProfiling ($"{nameof atom}/{nameof isTesting}", JS.isTesting)

        let rec debug =
            Recoil.atomWithProfiling (
                $"{nameof atom}/{nameof debug}",
                false,
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec gunPeers =
            Recoil.atomWithProfiling (
                $"{nameof atom}/{nameof gunPeers}",
                ([]: string list),
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec sessionRestored = Recoil.atomWithProfiling ($"{nameof atom}/{nameof sessionRestored}", false)

        let rec gunHash = Recoil.atomWithProfiling ($"{nameof atom}/{nameof gunHash}", "")

        let rec gunKeys =
            Recoil.atomWithProfiling (
                $"{nameof atom}/{nameof gunKeys}",
                {
                    Gun.pub = ""
                    Gun.epub = ""
                    Gun.priv = ""
                    Gun.epriv = ""
                }

            //                local_storage
            )

        let rec initialPeerSkipped = Recoil.atomWithProfiling ($"{nameof atom}/{nameof initialPeerSkipped}", false)
        let rec username = Recoil.atomWithProfiling ($"{nameof atom}/{nameof username}", None)
        let rec position = Recoil.atomWithProfiling ($"{nameof atom}/{nameof position}", None)
        let rec ctrlPressed = Recoil.atomWithProfiling ($"{nameof atom}/{nameof ctrlPressed}", false)
        let rec shiftPressed = Recoil.atomWithProfiling ($"{nameof atom}/{nameof shiftPressed}", false)


        //        module rec Events =
//            type EventId = EventId of position: float * guid: Guid
//
//            let newEventId () =
//                EventId (JS.Constructors.Date.now (), Guid.NewGuid ())
//
//            [<RequireQualifiedAccess>]
//            type Event =
//                | AddDatabase of id: EventId * name: DatabaseName * dayStart: FlukeTime
//                | AddTask of id: EventId * name: TaskName
//                | NoOp
//
//            let rec events =
//                Recoil.atomFamilyWithProfiling (
//                    $"{nameof atomFamily}/{nameof Events}/{nameof events}",
//                    (fun (_eventId: EventId) -> Event.NoOp)
//                )


        module rec User =
            let rec expandedDatabaseIdList =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof expandedDatabaseIdList}",
                    (fun (_username: Username) -> []: DatabaseId list),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (expandedDatabaseIdList, username)) []
                        ])
                )

            let rec selectedDatabaseIdList =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof selectedDatabaseIdList}",
                    (fun (_username: Username) -> []: DatabaseId list),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (selectedDatabaseIdList, username)) []
                        ])
                )

            let rec view =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof view}",
                    (fun (_username: Username) -> TempUI.defaultView),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (view, username)) []
                        ])
                )

            let rec language =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof language}",
                    (fun (_username: Username) -> Language.English),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (language, username)) []
                        ])
                )

            let rec color =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof color}",
                    (fun (_username: Username) -> UserColor.Black),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (color, username)) []
                        ])
                )

            let rec weekStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof weekStart}",
                    (fun (_username: Username) -> DayOfWeek.Sunday),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (weekStart, username)) []
                        ])
                )

            let rec dayStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof dayStart}",
                    (fun (_username: Username) -> FlukeTime.Create 0 0),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (dayStart, username)) []
                        ])
                )

            let rec sessionLength =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof sessionLength}",
                    (fun (_username: Username) -> Minute 25.),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (sessionLength, username)) []
                        ])
                )

            let rec sessionBreakLength =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof sessionBreakLength}",
                    (fun (_username: Username) -> Minute 5.),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (sessionBreakLength, username)) []
                        ])
                )

            let rec daysBefore =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof daysBefore}",
                    (fun (_username: Username) -> 7),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (daysBefore, username)) []
                        ])
                )

            let rec daysAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof daysAfter}",
                    (fun (_username: Username) -> 7),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (daysAfter, username)) []
                        ])
                )

            let rec cellMenuOpened =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof cellMenuOpened}",
                    (fun (_username: Username) -> None: (TaskId * DateId) option),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (cellMenuOpened, username)) []
                        ])
                )

            let rec cellSize =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof cellSize}",
                    (fun (_username: Username) -> 17),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (cellSize, username)) []
                        ])
                )

            let rec leftDock =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof leftDock}",
                    (fun (_username: Username) -> None: TempUI.DockType option),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (leftDock, username)) []
                        ])
                )

            let rec hideTemplates =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof hideTemplates}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (hideTemplates, username)) []
                        ])
                )

            let rec hideSchedulingOverlay =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof hideSchedulingOverlay}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) (Recoil.AtomFamily (hideSchedulingOverlay, username)) []
                        ])
                )

            let rec formIdFlag =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof formIdFlag}",
                    (fun (_username: Username, _key: TextKey) -> None: Guid option),
                    (fun (username: Username, key: TextKey) ->
                        [
                            Recoil.gunEffect
                                (Some username)
                                (Recoil.AtomFamily (formIdFlag, (username, key)))
                                (key |> TextKey.Value |> List.singleton)
                        ])
                )

            let rec formVisibleFlag =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof formVisibleFlag}",
                    (fun (_username: Username, _key: TextKey) -> false),
                    (fun (username: Username, key: TextKey) ->
                        [
                            Recoil.gunEffect
                                (Some username)
                                (Recoil.AtomFamily (formVisibleFlag, (username, key)))
                                (key |> TextKey.Value |> List.singleton)
                        ])
                )

            let rec accordionFlag =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof accordionFlag}",
                    (fun (_username: Username, _key: TextKey) -> [||]: string []),
                    (fun (username: Username, key: TextKey) ->
                        [
                            Recoil.gunEffect
                                (Some username)
                                (Recoil.AtomFamily (accordionFlag, (username, key)))
                                (key |> TextKey.Value |> List.singleton)
                        ])
                )


        module rec Database =
            let databaseIdIdentifier (databaseId: DatabaseId) =
                databaseId
                |> DatabaseId.Value
                |> string
                |> List.singleton

            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof name}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.Name),
                    (fun (databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                None
                                (Recoil.AtomFamily (name, databaseId))
                                (databaseIdIdentifier databaseId)
                        ])
                )

            let rec owner =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof owner}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.Owner),
                    (fun (databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                None
                                (Recoil.AtomFamily (owner, databaseId))
                                (databaseIdIdentifier databaseId)
                        ])
                )

            let rec sharedWith =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof sharedWith}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.SharedWith),
                    (fun (databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                None
                                (Recoil.AtomFamily (sharedWith, databaseId))
                                (databaseIdIdentifier databaseId)
                        ])
                )

            let rec dayStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof dayStart}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.DayStart),
                    (fun (databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                None
                                (Recoil.AtomFamily (dayStart, databaseId))
                                (databaseIdIdentifier databaseId)
                        ])
                )

            let rec position =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof position}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.Position),
                    (fun (databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                None
                                (Recoil.AtomFamily (position, databaseId))
                                (databaseIdIdentifier databaseId)
                        ])
                )


        module rec Information =
            let rec attachments =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Information}/{nameof attachments}",
                    (fun (_information: Information) -> []: Attachment list)
                )


        module rec Task =
            let taskIdIdentifier (taskId: TaskId) =
                taskId |> TaskId.Value |> string |> List.singleton

            let rec task =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof task}",
                    (fun (_taskId: TaskId) -> Task.Default)
                )

            let rec databaseId =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof databaseId}",
                    (fun (_taskId: TaskId) -> Database.Default.Id),
                    (fun (taskId: TaskId) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (databaseId, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec information =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof information}",
                    (fun (_taskId: TaskId) -> Task.Default.Information),
                    (fun (taskId: TaskId) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (information, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof name}",
                    (fun (_taskId: TaskId) -> Task.Default.Name),
                    (fun (taskId: TaskId) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (name, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec scheduling =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof scheduling}",
                    (fun (_taskId: TaskId) -> Task.Default.Scheduling),
                    (fun (taskId: TaskId) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (scheduling, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec pendingAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof pendingAfter}",
                    (fun (_taskId: TaskId) -> Task.Default.PendingAfter),
                    (fun (taskId: TaskId) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (pendingAfter, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec missedAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof missedAfter}",
                    (fun (_taskId: TaskId) -> Task.Default.MissedAfter),
                    (fun (taskId: TaskId) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (missedAfter, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec priority =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof priority}",
                    (fun (_taskId: TaskId) -> Task.Default.Priority),
                    (fun (taskId: TaskId) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (priority, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec duration =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof duration}",
                    (fun (_taskId: TaskId) -> Task.Default.Duration),
                    (fun (taskId: TaskId) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (duration, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec attachments =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof attachments}",
                    (fun (_taskId: TaskId) -> []: Attachment list) // TODO: move from here?
                )


        module rec Cell =
            let cellIdentifier (taskId: TaskId) (dateId: DateId) =
                [
                    taskId |> TaskId.Value |> string
                    dateId |> DateId.Value |> FlukeDate.Stringify
                ]

            let rec status =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof status}",
                    (fun (_taskId: TaskId, _dateId: DateId) -> Disabled),
                    (fun (taskId: TaskId, dateId: DateId) ->
                        [
                            Recoil.gunEffect
                                None
                                (Recoil.AtomFamily (status, (taskId, dateId)))
                                (cellIdentifier taskId dateId)
                        ])
                )

            let rec attachments =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof attachments}",
                    (fun (_taskId: TaskId, _dateId: DateId) -> []: Attachment list)
                )

            let rec sessions =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof sessions}",
                    (fun (_taskId: TaskId, _dateId: DateId) -> []: TaskSession list)
                )

            let rec selected =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof selected}",
                    (fun (_username: Username, _taskId: TaskId, _dateId: DateId) -> false),
                    (fun (username: Username, taskId: TaskId, dateId: DateId) ->
                        [
                            Recoil.gunEffect
                                (Some username)
                                (Recoil.AtomFamily (selected, (username, taskId, dateId)))
                                (cellIdentifier taskId dateId)
                        ])
                )



        module rec Session =
            let rec databaseIdSet =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Session}/{nameof databaseIdSet}",
                    (fun (_username: Username) -> Set.empty: Set<DatabaseId>),
                    (fun (_username: Username) ->
                        [
                            Recoil.gunKeyEffect
                                None
                                (Recoil.AtomFamily (Database.owner, Database.Default.Id))
                                (Recoil.filterEmptyGuid DatabaseId)
                        ])
                )

            let rec taskIdSet =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Session}/{nameof taskIdSet}",
                    (fun (_username: Username) -> Set.empty: Set<TaskId>),
                    (fun (_username: Username) ->
                        [
                            Recoil.gunKeyEffect
                                None
                                (Recoil.AtomFamily (Task.databaseId, Task.Default.Id))
                                (Recoil.filterEmptyGuid TaskId)
                        ])
                )


    module Selectors =
        let rec gunPeers =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof gunPeers}",
                (fun getter ->
                    let _gunHash = getter.get Atoms.gunHash
                    let gunPeers = getter.get Atoms.gunPeers

                    gunPeers
                    |> List.filter (String.IsNullOrWhiteSpace >> not))
            )

        let rec gun =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof gun}",
                (fun getter ->
                    let isTesting = getter.get Atoms.isTesting
                    let gunPeers = getter.get gunPeers

                    let gun =
                        Gun.gun
                            {
                                Gun.GunProps.peers = if isTesting then None else Some (gunPeers |> List.toArray)
                                Gun.GunProps.radisk = if isTesting then None else Some false
                                Gun.GunProps.localStorage = if isTesting then None else Some true
                            }

                    Browser.Dom.window?lastGun <- gun

                    printfn $"gun selector. peers={gunPeers}. returning gun..."

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

        let rec dateSequence =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof dateSequence}",
                (fun getter ->
                    let username = getter.get Atoms.username
                    let position = getter.get Atoms.position

                    match position, username with
                    | Some position, Some username ->
                        let daysBefore = getter.get (Atoms.User.daysBefore username)
                        let daysAfter = getter.get (Atoms.User.daysAfter username)
                        let dayStart = getter.get (Atoms.User.dayStart username)
                        let dateId = dateId dayStart position
                        let (DateId referenceDay) = dateId

                        referenceDay
                        |> List.singleton
                        |> Rendering.getDateSequence (daysBefore, daysAfter)
                    | _ -> [])
            )


        type DeviceInfo =
            {
                IsEdge: bool
                IsMobile: bool
                IsExtension: bool
                IsProduction: bool
            }

        let rec deviceInfo =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof deviceInfo}",
                (fun _getter ->
                    let userAgent =
                        if Browser.Dom.window?navigator = null then
                            ""
                        else
                            Browser.Dom.window?navigator?userAgent

                    let deviceInfo =
                        {
                            IsEdge = (JSe.RegExp @"Edg\/").Test userAgent
                            IsMobile =
                                JSe
                                    .RegExp("Android|BlackBerry|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop",
                                            JSe.RegExpFlag().i)
                                    .Test userAgent
                            IsExtension = Browser.Dom.window.location.protocol = "chrome-extension:"
                            IsProduction = JS.isProduction
                        }

                    printfn $"userAgent: {userAgent} deviceInfo: {deviceInfo}"
                    deviceInfo)
            )


        module rec FlukeDate =
            let isToday =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof FlukeDate}/{nameof isToday}",
                    (fun (date: FlukeDate) getter ->
                        let username = getter.get Atoms.username
                        let position = getter.get Atoms.position

                        match username, position with
                        | Some username, Some position ->
                            let dayStart = getter.get (Atoms.User.dayStart username)

                            Domain.UserInteraction.isToday dayStart position (DateId date)
                        | _ -> false)
                )



        module rec Database =
            let rec database =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Database}/{nameof database}",
                    (fun (databaseId: DatabaseId) getter ->
                        {
                            Id = databaseId
                            Name = getter.get (Atoms.Database.name databaseId)
                            Owner = getter.get (Atoms.Database.owner databaseId)
                            SharedWith = getter.get (Atoms.Database.sharedWith databaseId)
                            Position = getter.get (Atoms.Database.position databaseId)
                            DayStart = getter.get (Atoms.Database.dayStart databaseId)
                        })
                )

            let rec access =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Database}/{nameof access}",
                    (fun (databaseId: DatabaseId) getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let database = getter.get (database databaseId)

                            if database.Owner = Templates.templatesUser.Username then
                                None
                            else
                                getAccess database username
                        | None -> None)
                )


        module rec Task =
            let rec task =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof task}",
                    (fun (taskId: TaskId) getter ->
                        {
                            Id = taskId
                            Name = getter.get (Atoms.Task.name taskId)
                            Information = getter.get (Atoms.Task.information taskId)
                            PendingAfter = getter.get (Atoms.Task.pendingAfter taskId)
                            MissedAfter = getter.get (Atoms.Task.missedAfter taskId)
                            Scheduling = getter.get (Atoms.Task.scheduling taskId)
                            Priority = getter.get (Atoms.Task.priority taskId)
                            Duration = getter.get (Atoms.Task.duration taskId)
                        })
                )

            let rec lastSession =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof lastSession}",
                    (fun (taskId: TaskId) getter ->
                        let dateSequence = getter.get dateSequence

                        dateSequence
                        |> List.rev
                        |> List.tryPick
                            (fun date ->
                                let sessions = getter.get (Atoms.Cell.sessions (taskId, DateId date))

                                sessions
                                |> List.sortByDescending
                                    (fun (TaskSession (start, _, _)) -> start |> FlukeDateTime.DateTime)
                                |> List.tryHead))
                )

            let rec activeSession =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof activeSession}",
                    (fun (taskId: TaskId) getter ->
                        let position = getter.get Atoms.position
                        let lastSession = getter.get (lastSession taskId)

                        match position, lastSession with
                        | Some position, Some lastSession ->
                            let (TaskSession (start, Minute duration, Minute breakDuration)) = lastSession

                            let currentDuration =
                                ((position |> FlukeDateTime.DateTime)
                                 - (start |> FlukeDateTime.DateTime))
                                    .TotalMinutes

                            let active = currentDuration < duration + breakDuration

                            match active with
                            | true -> Some currentDuration
                            | false -> None

                        | _ -> None)
                )

            let rec showUser =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof showUser}",
                    (fun (taskId: TaskId) getter ->
                        //                            let username = getter.get Atoms.username
//                            match username with
//                            | Some username ->
                        let dateSequence = getter.get dateSequence
                        //                                let taskIdSet = getter.get (Atoms.Session.taskIdSet username)

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
                    (fun (taskId: TaskId) getter ->
                        let dateSequence = getter.get dateSequence
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            dateSequence
                            |> List.exists
                                (fun date -> getter.get (Atoms.Cell.selected (username, taskId, DateId date)))
                        | None -> false)
                )


        module rec Session =
            let rec informationSet =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof informationSet}",
                    (fun (username: Username) getter ->
                        let taskIdSet = getter.get (Atoms.Session.taskIdSet username)

                        taskIdSet
                        |> Set.map (fun taskId -> getter.get (Atoms.Task.information taskId))
                        |> Set.filter
                            (fun information ->
                                information
                                |> Information.Name
                                |> InformationName.Value
                                |> String.IsNullOrWhiteSpace
                                |> not))
                )

            let rec selectedTaskIdSet =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof selectedTaskIdSet}",
                    (fun (username: Username) getter ->
                        let taskIdSet = getter.get (Atoms.Session.taskIdSet username)
                        let selectedDatabaseIdList = getter.get (Atoms.User.selectedDatabaseIdList username)
                        let selectedDatabaseIdListSet = selectedDatabaseIdList |> Set.ofList

                        taskIdSet
                        |> Set.map (fun taskId -> taskId, getter.get (Atoms.Task.databaseId taskId))
                        |> Set.filter (fun (_, databaseId) -> selectedDatabaseIdListSet.Contains databaseId)
                        |> Set.map fst)
                )

            let rec activeSessions =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof activeSessions}",
                    (fun (username: Username) getter ->
                        let selectedTaskIdSet = getter.get (selectedTaskIdSet username)

                        let sessionLength = getter.get (Atoms.User.sessionLength username)
                        let sessionBreakLength = getter.get (Atoms.User.sessionBreakLength username)

                        selectedTaskIdSet
                        |> Set.toList
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

            let rec sessionData =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof sessionData}",
                    (fun (username: Username) getter ->
                        let dateSequence = getter.get dateSequence
                        let view = getter.get (Atoms.User.view username)
                        let position = getter.get Atoms.position
                        let dayStart = getter.get (Atoms.User.dayStart username)
                        let selectedTaskIdSet = getter.get (selectedTaskIdSet username)

                        let taskList =
                            selectedTaskIdSet
                            |> Seq.map (Task.task >> getter.get)
                            |> Seq.toList

                        let taskStateList =
                            taskList
                            |> List.map
                                (fun task ->
                                    { TaskState.Default with
                                        Task = task
                                        CellStateMap =
                                            dateSequence
                                            |> List.map
                                                (fun date ->
                                                    let dateId = DateId date

                                                    let cellState =
                                                        {
                                                            Status = getter.get (Atoms.Cell.status (task.Id, dateId))
                                                            Sessions =
                                                                getter.get (Atoms.Cell.sessions (task.Id, dateId))
                                                            Attachments =
                                                                getter.get (Atoms.Cell.attachments (task.Id, dateId))
                                                        }

                                                    dateId, cellState)
                                            |> Map.ofList
                                    })

                        getSessionData
                            {|
                                Username = username
                                DayStart = dayStart
                                DateSequence = dateSequence
                                View = view
                                Position = position
                                TaskStateList = taskStateList
                            |})
                )

            let rec filteredTaskIdList =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof filteredTaskIdList}",
                    (fun (username: Username) getter ->
                        let sessionData = getter.get (Session.sessionData username)

                        sessionData.TaskList
                        |> List.map (fun task -> task.Id))
                )

            let rec tasksByInformationKind =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof tasksByInformationKind}",
                    (fun (username: Username) getter ->
                        let filteredTaskIdList = getter.get (filteredTaskIdList username)

                        let informationMap =
                            filteredTaskIdList
                            |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.information taskId))
                            |> Map.ofList

                        filteredTaskIdList
                        |> List.groupBy (fun taskId -> informationMap.[taskId])
                        |> List.sortBy (fun (information, _) -> information |> Information.Name)
                        |> List.groupBy (fun (information, _) -> Information.toString information)
                        |> List.sortBy (snd >> List.head >> fst >> Information.toTag))
                )

            let rec cellSelectionMap =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof cellSelectionMap}",
                    (fun (username: Username) getter ->
                        let filteredTaskIdList = getter.get (filteredTaskIdList username)
                        let dateSequence = getter.get dateSequence

                        filteredTaskIdList
                        |> List.map
                            (fun taskId ->
                                let dates =
                                    dateSequence
                                    |> List.map
                                        (fun date ->
                                            date, getter.get (Atoms.Cell.selected (username, taskId, DateId date)))
                                    |> List.filter snd
                                    |> List.map fst
                                    |> Set.ofList

                                taskId, dates)
                        |> List.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                        |> Map.ofList),

                    (fun (_username: Username) setter (newSelection: Map<TaskId, Set<FlukeDate>>) ->
                        let username = setter.get Atoms.username

                        match username with
                        | Some username ->
                            let filteredTaskIdList = setter.get (filteredTaskIdList username)
                            let cellSelectionMap = setter.get (cellSelectionMap username)

                            let operations =
                                filteredTaskIdList
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
                                    setter.set (Atoms.Cell.selected (username, taskId, DateId date), selected))
                        | None -> ())
                )


            let rec hasCellSelection =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof FlukeDate}/{nameof hasCellSelection}",
                    (fun (date: FlukeDate) getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let filteredTaskIdList = getter.get (filteredTaskIdList username)

                            filteredTaskIdList
                            |> List.exists
                                (fun taskId -> getter.get (Atoms.Cell.selected (username, taskId, DateId date)))
                        | None -> false)
                )

        module rec Cell =
            let rec status =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Cell}/{nameof status}",
                    (fun (username: Username, taskId: TaskId, dateId: DateId) getter ->
                        let hideSchedulingOverlay = getter.get (Atoms.User.hideSchedulingOverlay username)

                        if hideSchedulingOverlay then
                            getter.get (Atoms.Cell.status (taskId, dateId))
                        else
                            let sessionData = getter.get (Session.sessionData username)

                            sessionData.TaskStateMap
                            |> Map.tryPick (fun task taskState -> if task.Id = taskId then Some taskState else None)
                            |> Option.map
                                (fun taskState ->
                                    taskState.CellStateMap
                                    |> Map.tryFind dateId
                                    |> Option.map (fun cellState -> cellState.Status)
                                    |> Option.defaultValue Disabled)
                            |> Option.defaultWith (fun () -> getter.get (Atoms.Cell.status (taskId, dateId)))),
                    (fun (_username: Username, taskId: TaskId, dateId: DateId) setter (newValue: CellStatus) ->
                        setter.set (Atoms.Cell.status (taskId, dateId), newValue))
                )

            let rec selected =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Cell}/{nameof selected}",
                    (fun (username: Username, taskId: TaskId, dateId: DateId) getter ->
                        getter.get (Atoms.Cell.selected (username, taskId, dateId))),
                    (fun (_username: Username, taskId: TaskId, dateId: DateId) setter (newValue: bool) ->
                        let username = setter.get Atoms.username

                        match username with
                        | Some username ->
                            let ctrlPressed = setter.get Atoms.ctrlPressed
                            let shiftPressed = setter.get Atoms.shiftPressed

                            let newCellSelectionMap =
                                match shiftPressed, ctrlPressed with
                                | false, false ->
                                    let newTaskSelection =
                                        if newValue then Set.singleton (dateId |> DateId.Value) else Set.empty

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

                                    let oldSelection = setter.get (Session.cellSelectionMap username)
                                    swapSelection oldSelection taskId (dateId |> DateId.Value)
                                | true, _ ->
                                    let filteredTaskIdList = setter.get (Session.filteredTaskIdList username)
                                    let oldCellSelectionMap = setter.get (Session.cellSelectionMap username)

                                    let initialTaskIdSet =
                                        oldCellSelectionMap
                                        |> Map.toSeq
                                        |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                                        |> Seq.map fst
                                        |> Set.ofSeq
                                        |> Set.add taskId

                                    let newTaskIdList =
                                        filteredTaskIdList
                                        |> List.skipWhile (initialTaskIdSet.Contains >> not)
                                        |> List.rev
                                        |> List.skipWhile (initialTaskIdSet.Contains >> not)
                                        |> List.rev

                                    let initialDateList =
                                        oldCellSelectionMap
                                        |> Map.values
                                        |> Set.unionMany
                                        |> Set.add (dateId |> DateId.Value)
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

                            setter.set (Session.cellSelectionMap username, newCellSelectionMap)
                        | None -> ())
                )

        module rec BulletJournalView =
            let rec weekCellsMap =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof BulletJournalView}/{nameof weekCellsMap}",
                    (fun (username: Username) getter ->
                        let position = getter.get Atoms.position
                        let filteredTaskIdList = getter.get (Session.filteredTaskIdList username)

                        match position with
                        | Some position ->
                            let dayStart = getter.get (Atoms.User.dayStart username)
                            let weekStart = getter.get (Atoms.User.weekStart username)

                            let weeks =
                                [
                                    -1 .. 1
                                ]
                                |> List.map
                                    (fun weekOffset ->
                                        let dateIdSequence =
                                            let rec getWeekStart (date: DateTime) =
                                                if date.DayOfWeek = weekStart then
                                                    date
                                                else
                                                    getWeekStart (date.AddDays -1)

                                            let startDate =
                                                dateId dayStart position
                                                |> fun (DateId referenceDay) ->
                                                    (referenceDay |> FlukeDate.DateTime)
                                                        .AddDays (7 * weekOffset)
                                                |> getWeekStart

                                            [
                                                0 .. 6
                                            ]
                                            |> List.map startDate.AddDays
                                            |> List.map FlukeDateTime.FromDateTime
                                            |> List.map (dateId dayStart)

                                        let taskMap =
                                            filteredTaskIdList
                                            |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.task taskId))
                                            |> Map.ofList

                                        let result =
                                            filteredTaskIdList
                                            |> List.collect
                                                (fun taskId ->
                                                    dateIdSequence
                                                    |> List.map
                                                        (fun dateId ->
                                                            match dateId with
                                                            | DateId referenceDay as dateId ->
                                                                //                                                    let taskId = getter.get task.Id
                                                                let status =
                                                                    getter.get (Cell.status (username, taskId, dateId))

                                                                let sessions =
                                                                    getter.get (Atoms.Cell.sessions (taskId, dateId))

                                                                let attachments =
                                                                    getter.get (Atoms.Cell.attachments (taskId, dateId))

                                                                let isToday =
                                                                    getter.get (FlukeDate.isToday referenceDay)

                                                                {|
                                                                    DateId = dateId
                                                                    TaskId = taskId
                                                                    Status = status
                                                                    Sessions = sessions
                                                                    IsToday = isToday
                                                                    Attachments = attachments
                                                                |}))
                                            |> List.groupBy (fun x -> x.DateId)
                                            |> List.map
                                                (fun (dateId, cellsMetadata) ->
                                                    match dateId with
                                                    | DateId referenceDay as dateId ->
                                                        //                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList
                                                        let taskSessions =
                                                            cellsMetadata
                                                            |> List.collect (fun x -> x.Sessions)

                                                        let sortedTasksMap =
                                                            cellsMetadata
                                                            |> List.map
                                                                (fun cellMetadata ->
                                                                    let taskState =
                                                                        { TaskState.Default with
                                                                            Task = taskMap.[cellMetadata.TaskId]
                                                                            Sessions = taskSessions
                                                                        }

                                                                    taskState,
                                                                    [
                                                                        {
                                                                            Task = taskState.Task
                                                                            DateId = dateId
                                                                        },
                                                                        cellMetadata.Status
                                                                    ])
                                                            |> Sorting.sortLanesByTimeOfDay
                                                                dayStart
                                                                { Date = referenceDay; Time = dayStart }
                                                            |> List.indexed
                                                            |> List.map
                                                                (fun (i, (taskState, _)) -> taskState.Task.Id, i)
                                                            |> Map.ofList

                                                        let newCells =
                                                            cellsMetadata
                                                            |> List.sortBy
                                                                (fun cell ->
                                                                    sortedTasksMap
                                                                    |> Map.tryFind cell.TaskId
                                                                    |> Option.defaultValue -1)

                                                        dateId, newCells)
                                            |> Map.ofList

                                        result)

                            weeks
                        | _ -> [])
                )
