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


module State =
    open Model
    open Domain.UserInteraction
    open Domain.State
    open View

    type TextKey = TextKey of key: string

    and TextKey with
        static member inline Value (TextKey key) = key

    [<RequireQualifiedAccess>]
    type Join =
        | Database of DatabaseId
        | Task of DatabaseId * TaskId

    module Atoms =
        let rec isTesting = Recoil.atomWithProfiling ($"{nameof atom}/{nameof isTesting}", JS.deviceInfo.IsTesting)

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
            let rec joinSet =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof joinSet}",
                    (fun (_username: Username) -> Set.empty: Set<Join>),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, joinSet, username)) []
                        ])
                )

            let rec expandedDatabaseIdList =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof expandedDatabaseIdList}",
                    (fun (_username: Username) -> []: DatabaseId list),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, expandedDatabaseIdList, username)) []
                        ])
                )

            let rec selectedDatabaseIdList =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof selectedDatabaseIdList}",
                    (fun (_username: Username) -> []: DatabaseId list),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, selectedDatabaseIdList, username)) []
                        ])
                )

            let rec view =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof view}",
                    (fun (_username: Username) -> TempUI.defaultView),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, view, username)) []
                        ])
                )

            let rec language =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof language}",
                    (fun (_username: Username) -> Language.English),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, language, username)) []
                        ])
                )

            let rec color =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof color}",
                    (fun (_username: Username) -> UserColor.Black),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, color, username)) []
                        ])
                )

            let rec weekStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof weekStart}",
                    (fun (_username: Username) -> DayOfWeek.Sunday),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, weekStart, username)) []
                        ])
                )

            let rec dayStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof dayStart}",
                    (fun (_username: Username) -> FlukeTime.Create 0 0),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, dayStart, username)) []
                        ])
                )

            let rec sessionLength =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof sessionLength}",
                    (fun (_username: Username) -> Minute 25.),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, sessionLength, username)) []
                        ])
                )

            let rec sessionBreakLength =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof sessionBreakLength}",
                    (fun (_username: Username) -> Minute 5.),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, sessionBreakLength, username)) []
                        ])
                )

            let rec daysBefore =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof daysBefore}",
                    (fun (_username: Username) -> 7),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, daysBefore, username)) []
                        ])
                )

            let rec daysAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof daysAfter}",
                    (fun (_username: Username) -> 7),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, daysAfter, username)) []
                        ])
                )

            let rec taskSearch =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof taskSearch}",
                    (fun (_username: Username) -> ""),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, taskSearch, username)) []
                        ])
                )

            let rec cellMenuOpened =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof cellMenuOpened}",
                    (fun (_username: Username) -> None: (TaskId * DateId) option),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, cellMenuOpened, username)) []
                        ])
                )

            let rec cellSize =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof cellSize}",
                    (fun (_username: Username) -> 23),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, cellSize, username)) []
                        ])
                )

            let rec leftDock =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof leftDock}",
                    (fun (_username: Username) -> None: TempUI.DockType option),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, leftDock, username)) []
                        ])
                )

            let rec hideTemplates =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof hideTemplates}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, hideTemplates, username)) []
                        ])
                )

            let rec hideSchedulingOverlay =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof hideSchedulingOverlay}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, hideSchedulingOverlay, username)) []
                        ])
                )

            let rec showTaskSearch =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof showTaskSearch}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, showTaskSearch, username)) []
                        ])
                )

            let rec formIdFlag =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof formIdFlag}",
                    (fun (_username: Username, _key: TextKey) -> None: Guid option),
                    (fun (username: Username, key: TextKey) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, formIdFlag, (username, key)))
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
                                (Recoil.AtomFamily (username, formVisibleFlag, (username, key)))
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
                                (Recoil.AtomFamily (username, accordionFlag, (username, key)))
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
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.Name),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, name, (username, databaseId)))
                                (databaseIdIdentifier databaseId)
                        ])
                )

            let rec owner =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof owner}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.Owner),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, owner, (username, databaseId)))
                                (databaseIdIdentifier databaseId)
                        ])
                )

            let rec sharedWith =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof sharedWith}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.SharedWith),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, sharedWith, (username, databaseId)))
                                (databaseIdIdentifier databaseId)
                        ])
                )

            let rec dayStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof dayStart}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.DayStart),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, dayStart, (username, databaseId)))
                                (databaseIdIdentifier databaseId)
                        ])
                )

            let rec position =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof position}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.Position),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, position, (username, databaseId)))
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

            let rec information =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof information}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Information),
                    (fun (username: Username, taskId: TaskId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, information, (username, taskId)))
                                (taskIdIdentifier taskId)
                        ])
                )

            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof name}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Name),
                    (fun (username: Username, taskId: TaskId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, name, (username, taskId)))
                                (taskIdIdentifier taskId)
                        ])
                )

            let rec scheduling =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof scheduling}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Scheduling),
                    (fun (username: Username, taskId: TaskId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, scheduling, (username, taskId)))
                                (taskIdIdentifier taskId)
                        ])
                )

            let rec pendingAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof pendingAfter}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.PendingAfter),
                    (fun (username: Username, taskId: TaskId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, pendingAfter, (username, taskId)))
                                (taskIdIdentifier taskId)
                        ])
                )

            let rec missedAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof missedAfter}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.MissedAfter),
                    (fun (username: Username, taskId: TaskId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, missedAfter, (username, taskId)))
                                (taskIdIdentifier taskId)
                        ])
                )

            let rec priority =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof priority}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Priority),
                    (fun (username: Username, taskId: TaskId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, priority, (username, taskId)))
                                (taskIdIdentifier taskId)
                        ])
                )

            let rec duration =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof duration}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Duration),
                    (fun (username: Username, taskId: TaskId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, duration, (username, taskId)))
                                (taskIdIdentifier taskId)
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
                    (fun (_username: Username, _taskId: TaskId, _dateId: DateId) -> Disabled),
                    (fun (username: Username, taskId: TaskId, dateId: DateId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, status, (username, taskId, dateId)))
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
                                (Recoil.AtomFamily (username, selected, (username, taskId, dateId)))
                                (cellIdentifier taskId dateId)
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
                        if isTesting then
                            Gun.gunTest
                        else
                            Gun.gun
                                {
                                    Gun.GunProps.peers = Some (gunPeers |> List.toArray)
                                    Gun.GunProps.radisk = Some false
                                    Gun.GunProps.localStorage = Some true
                                    Gun.GunProps.multicast = None
                                }

                    match JS.window id with
                    | Some window -> window?lastGun <- gun
                    | None -> ()

                    printfn $"gun selector. peers={gunPeers}. returning gun..."

                    {| ``#`` = gun |})
            )

        let rec gunNamespace =
            Recoil.selectorWithProfiling (
                $"{nameof selector}/{nameof gunNamespace}",
                (fun getter ->
                    let gun = getter.get gun
                    //                    let username = getter.get Atoms.username
//                    let gunKeys = getter.get Atoms.gunKeys
                    let user = gun.``#``.user ()

                    match JS.window id with
                    | Some window -> window?gunNamespace <- gun
                    | None -> ()

                    printfn $"gunNamespace selector. user.is={JS.JSON.stringify user.is} keys={user.__.sea}..."

                    {| ``#`` = user |})
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

        let rec deviceInfo =
            Recoil.selectorWithProfiling ($"{nameof selector}/{nameof deviceInfo}", (fun _getter -> JS.deviceInfo))


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
                    (fun (username: Username, databaseId: DatabaseId) getter ->
                        {
                            Id = databaseId
                            Name = getter.get (Atoms.Database.name (username, databaseId))
                            Owner = getter.get (Atoms.Database.owner (username, databaseId))
                            SharedWith = getter.get (Atoms.Database.sharedWith (username, databaseId))
                            Position = getter.get (Atoms.Database.position (username, databaseId))
                            DayStart = getter.get (Atoms.Database.dayStart (username, databaseId))
                        })
                )

            let rec isReadWrite =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Database}/{nameof isReadWrite}",
                    (fun (databaseId: DatabaseId) getter ->
                        let username = getter.get Atoms.username

                        let access =
                            match username with
                            | Some username ->
                                let database = getter.get (database (username, databaseId))

                                if database.Owner = Templates.templatesUser.Username then
                                    None
                                else
                                    getAccess database username
                            | None -> None

                        access = Some Access.ReadWrite)
                )


        module rec Task =
            let rec task =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof task}",
                    (fun (username: Username, taskId: TaskId) getter ->
                        {
                            Id = taskId
                            Name = getter.get (Atoms.Task.name (username, taskId))
                            Information = getter.get (Atoms.Task.information (username, taskId))
                            PendingAfter = getter.get (Atoms.Task.pendingAfter (username, taskId))
                            MissedAfter = getter.get (Atoms.Task.missedAfter (username, taskId))
                            Scheduling = getter.get (Atoms.Task.scheduling (username, taskId))
                            Priority = getter.get (Atoms.Task.priority (username, taskId))
                            Duration = getter.get (Atoms.Task.duration (username, taskId))
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
                    (fun (username: Username, taskId: TaskId) getter ->
                        //                            let username = getter.get Atoms.username
//                            match username with
//                            | Some username ->
                        let dateSequence = getter.get dateSequence
                        //                                let taskIdSet = getter.get (Atoms.Session.taskIdSet username)

                        let statusList =
                            dateSequence
                            |> List.map (fun date -> Atoms.Cell.status (username, taskId, DateId date))
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
            let rec databaseIdSet =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof databaseIdSet}",
                    (fun (username: Username) getter ->
                        let joinSet = getter.get (Atoms.User.joinSet username)

                        joinSet
                        |> Set.choose
                            (function
                            | Join.Database databaseId -> Some databaseId
                            | _ -> None))
                )

            let rec taskMetadata =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof taskMetadata}",
                    (fun (username: Username) getter ->
                        let joinSet = getter.get (Atoms.User.joinSet username)

                        joinSet
                        |> Set.choose
                            (function
                            | Join.Task (databaseId, taskId) -> Some (taskId, {| DatabaseId = databaseId |})
                            | _ -> None)
                        |> Map.ofSeq)
                )

            let rec informationSet =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof informationSet}",
                    (fun (username: Username) getter ->
                        let taskMetadata = getter.get (taskMetadata username)

                        taskMetadata
                        |> Map.keys
                        |> Seq.map (fun taskId -> getter.get (Atoms.Task.information (username, taskId)))
                        |> Seq.filter
                            (fun information ->
                                information
                                |> Information.Name
                                |> InformationName.Value
                                |> String.IsNullOrWhiteSpace
                                |> not)
                        |> Set.ofSeq)
                )

            let rec selectedTaskIdSet =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof selectedTaskIdSet}",
                    (fun (username: Username) getter ->
                        let taskMetadata = getter.get (taskMetadata username)
                        let selectedDatabaseIdList = getter.get (Atoms.User.selectedDatabaseIdList username)
                        let selectedDatabaseIdListSet = selectedDatabaseIdList |> Set.ofList

                        taskMetadata
                        |> Map.filter (fun _ taskMetadata -> selectedDatabaseIdListSet.Contains taskMetadata.DatabaseId)
                        |> Map.keys
                        |> Set.ofSeq)
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
                                let (TaskName taskName) = getter.get (Atoms.Task.name (username, taskId))

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
                        let taskSearch = getter.get (Atoms.User.taskSearch username)
                        let selectedTaskIdSet = getter.get (selectedTaskIdSet username)

                        let taskList =
                            selectedTaskIdSet
                            |> Seq.map (fun taskId -> getter.get (Task.task (username, taskId)))
                            |> Seq.toList

                        let taskList =
                            if taskSearch = "" then
                                taskList
                            else
                                taskList
                                |> List.filter
                                    (fun task ->
                                        taskSearch = ""
                                        || (task.Name |> TaskName.Value).IndexOf taskSearch
                                           >= 0)

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
                                                            Status =
                                                                getter.get (
                                                                    Atoms.Cell.status (username, task.Id, dateId)
                                                                )
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
                            |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.information (username, taskId)))
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
                            getter.get (Atoms.Cell.status (username, taskId, dateId))
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
                            |> Option.defaultWith (fun () -> getter.get (Atoms.Cell.status (username, taskId, dateId)))),
                    (fun (username: Username, taskId: TaskId, dateId: DateId) setter (newValue: CellStatus) ->
                        setter.set (Atoms.Cell.status (username, taskId, dateId), newValue))
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
