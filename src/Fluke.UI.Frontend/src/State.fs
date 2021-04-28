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

        let rec sessionRestored = Recoil.atomWithProfiling ($"{nameof atom}/{nameof sessionRestored}", false)

        let rec gunPeers =
            Recoil.atomWithProfiling (
                $"{nameof atom}/{nameof gunPeers}",
                ([]: string list),
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

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

        let rec apiBaseUrl =
            Recoil.atomWithProfiling (
                $"{nameof atom}/{nameof apiBaseUrl}",
                $"https://localhost:{Sync.serverPort}",
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec api = Recoil.atomWithProfiling ($"{nameof atom}/{nameof api}", (None: Sync.Api option))

        let rec selectedDatabaseIds =
            Recoil.atomWithProfiling (
                $"{nameof atom}/{nameof selectedDatabaseIds}",
                ([||]: DatabaseId []),
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec expandedDatabaseIds =
            Recoil.atomWithProfiling (
                $"{nameof atom}/{nameof expandedDatabaseIds}",
                ([||]: DatabaseId []),
                effects =
                    [
                        AtomEffect Storage.local
                    ]
            )

        let rec ctrlPressed = Recoil.atomWithProfiling ($"{nameof atom}/{nameof ctrlPressed}", false)

        let rec shiftPressed = Recoil.atomWithProfiling ($"{nameof atom}/{nameof shiftPressed}", false)


        module rec Events =
            type EventId = EventId of position: float * guid: Guid

            let newEventId () =
                EventId (JS.Constructors.Date.now (), Guid.NewGuid ())

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


        module rec User =
            let rec view =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof view}",
                    (fun (_username: Username) -> View.View.HabitTracker),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) view username ""
                        ])
                )

            let rec color =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof color}",
                    (fun (_username: Username) -> UserColor.Black),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) color username ""
                        ])
                )

            let rec weekStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof weekStart}",
                    (fun (_username: Username) -> DayOfWeek.Sunday),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) weekStart username ""
                        ])
                )

            let rec dayStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof dayStart}",
                    (fun (_username: Username) -> FlukeTime.Create 0 0),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) dayStart username ""
                        ])
                )

            let rec sessionLength =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof sessionLength}",
                    (fun (_username: Username) -> Minute 25.),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) sessionLength username ""
                        ])
                )

            let rec sessionBreakLength =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof sessionBreakLength}",
                    (fun (_username: Username) -> Minute 5.),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) sessionBreakLength username ""
                        ])
                )

            let rec daysBefore =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof daysBefore}",
                    (fun (_username: Username) -> 7),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) daysBefore username ""
                        ])
                )

            let rec daysAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof daysAfter}",
                    (fun (_username: Username) -> 7),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) daysAfter username ""
                        ])
                )

            let rec cellMenuOpened =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof cellMenuOpened}",
                    (fun (_username: Username) -> None: (TaskId * DateId) option),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) cellMenuOpened username ""
                        ])
                )

            let rec cellSize =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof cellSize}",
                    (fun (_username: Username) -> 17),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) cellSize username ""
                        ])
                )

            let rec leftDock =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof leftDock}",
                    (fun (_username: Username) -> None: TempUI.DockType option),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) leftDock username ""
                        ])
                )

            let rec hideTemplates =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof hideTemplates}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Some username) hideTemplates username ""
                        ])
                )

            let rec formIdFlag =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof formIdFlag}",
                    (fun (_username: Username, _key: TextKey) -> None: Guid option),
                    (fun (username: Username, key: TextKey) ->
                        [
                            Recoil.gunEffect (Some username) formIdFlag (username, key) $"/{key |> TextKey.Value}"
                        ])
                )

            let rec formVisibleFlag =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof formVisibleFlag}",
                    (fun (_username: Username, _key: TextKey) -> false),
                    (fun (username: Username, key: TextKey) ->
                        [
                            Recoil.gunEffect (Some username) formVisibleFlag (username, key) $"/{key |> TextKey.Value}"
                        ])
                )

            let rec accordionFlag =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof accordionFlag}",
                    (fun (_username: Username, _key: TextKey) -> [||]: string []),
                    (fun (username: Username, key: TextKey) ->
                        [
                            Recoil.gunEffect (Some username) accordionFlag (username, key) $"/{key |> TextKey.Value}"
                        ])
                )


        module rec Database =
            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof name}",
                    (fun (_databaseId: DatabaseId option) -> DatabaseName "")
                )

            let rec owner =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof owner}",
                    (fun (_databaseId: DatabaseId option) -> None: Username option)
                )

            let rec sharedWith =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof sharedWith}",
                    (fun (_databaseId: DatabaseId option) -> DatabaseAccess.Public)
                )

            let rec dayStart =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof dayStart}",
                    (fun (_databaseId: DatabaseId option) -> FlukeTime.Create 7 0)
                )

            let rec position =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof position}",
                    (fun (_databaseId: DatabaseId option) -> None: FlukeDateTime option)
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
            let rec task =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof task}",
                    (fun (_taskId: TaskId option) -> Task.Default)
                )

            let rec informationId =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof informationId}",
                    (fun (_taskId: TaskId option) -> Information.informationId Task.Default.Information)
                )

            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof name}",
                    (fun (_taskId: TaskId option) -> Task.Default.Name)
                )

            let rec databaseId =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof databaseId}",
                    (fun (_taskId: TaskId option) -> None: DatabaseId option)
                )

            let rec scheduling =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof scheduling}",
                    (fun (_taskId: TaskId option) -> Task.Default.Scheduling)
                )

            let rec pendingAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof pendingAfter}",
                    (fun (_taskId: TaskId option) -> Task.Default.PendingAfter)
                )

            let rec missedAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof missedAfter}",
                    (fun (_taskId: TaskId option) -> Task.Default.MissedAfter)
                )

            let rec priority =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof priority}",
                    (fun (_taskId: TaskId option) -> Task.Default.Priority)
                )

            let rec attachments =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof attachments}",
                    (fun (_taskId: TaskId option) -> []: Attachment list) // TODO: move from here?
                )

            let rec duration =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof duration}",
                    (fun (_taskId: TaskId option) -> Task.Default.Duration)
                )


        module rec Cell =
            let rec taskId =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof taskId}",
                    (fun (taskId: TaskId, _dateId: DateId) -> taskId)
                )

            let rec dateId =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof dateId}",
                    (fun (_taskId: TaskId, dateId: DateId) -> dateId)
                )

            let rec status =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof status}",
                    (fun (_taskId: TaskId, _dateId: DateId) -> Disabled)
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

            type TaskId with
                member this.KeyFormat () =
                    let (TaskId taskId) = this
                    $"TaskId/{taskId}"

            type DateId with
                member this.KeyFormat () =
                    let (DateId referenceDay) = this
                    $"DateId/{referenceDay.Stringify ()}"

            let rec selected =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Cell}/{nameof selected}",
                    (fun (_username: Username, _taskId: TaskId, _dateId: DateId) -> false),
                    (fun (username: Username, taskId: TaskId, dateId: DateId) ->
                        [
                            Recoil.gunEffect
                                (Some username)
                                selected
                                (username, taskId, dateId)
                                $"/{taskId.KeyFormat ()}/{dateId.KeyFormat ()}"
                        ])
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
                    (fun (_username: Username) -> []: TaskId list)
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
                    let gunPeers = getter.get gunPeers

                    let gun =
                        Gun.gun (
                            {
                                Gun.GunProps.peers = if JS.isTesting then None else Some (gunPeers |> List.toArray)
                                Gun.GunProps.radisk = if JS.isTesting then None else Some false
                                Gun.GunProps.localStorage = if JS.isTesting then None else Some true
                            }
                            |> unbox
                        )

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
                                        (fun date ->
                                            date, getter.get (Atoms.Cell.selected (username, taskId, DateId date)))
                                    |> List.filter snd
                                    |> List.map fst
                                    |> Set.ofList

                                taskId, dates)
                        |> List.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                        |> Map.ofList
                    | None -> Map.empty),

                (fun setter (newSelection: Map<TaskId, Set<FlukeDate>>) ->
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
                                setter.set (Atoms.Cell.selected (username, taskId, DateId date), selected))
                    | None -> ())
            )

        type DeviceInfo =
            {
                IsEdge: bool
                IsMobile: bool
                IsExtension: bool
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

            let rec hasSelection =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof FlukeDate}/{nameof hasSelection}",
                    (fun (date: FlukeDate) getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = getter.get (Atoms.Session.taskIdList username)

                            taskIdList
                            |> List.exists
                                (fun taskId -> getter.get (Atoms.Cell.selected (username, taskId, DateId date)))
                        | None -> false)
                )


        module rec Database =
            let rec database =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Database}/{nameof database}",
                    (fun (databaseId: DatabaseId) getter ->
                        {
                            Id = databaseId
                            Name = getter.get (Atoms.Database.name (Some databaseId))
                            Owner =
                                getter.get (Atoms.Database.owner (Some databaseId))
                                |> Option.defaultValue TempData.testUser.Username
                            SharedWith = getter.get (Atoms.Database.sharedWith (Some databaseId))
                            Position = getter.get (Atoms.Database.position (Some databaseId))
                            DayStart = getter.get (Atoms.Database.dayStart (Some databaseId))
                        })
                )


        module rec Information =
            ()


        module rec Task =
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
                                |> List.sortByDescending (fun (TaskSession (start, _, _)) -> start.DateTime)
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
                    (fun (taskId: TaskId) getter ->
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


        module rec Cell =
            let rec selected =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Cell}/{nameof selected}",
                    (fun (username: Username, taskId: TaskId, dateId: DateId) getter ->
                        getter.get (Atoms.Cell.selected (username, taskId, dateId))),
                    (fun (_username: Username, taskId: TaskId, (DateId referenceDay)) setter (newValue: bool) ->
                        let username = setter.get Atoms.username

                        match username with
                        | Some username ->
                            let ctrlPressed = setter.get Atoms.ctrlPressed
                            let shiftPressed = setter.get Atoms.shiftPressed

                            let newCellSelectionMap =
                                match shiftPressed, ctrlPressed with
                                | false, false ->
                                    let newTaskSelection = if newValue then Set.singleton referenceDay else Set.empty

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
                                let (TaskName taskName) = getter.get (Atoms.Task.name (Some taskId))

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
                            |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.informationId (Some taskId)))
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
                        let position = getter.get Atoms.position
                        let taskIdList = getter.get (Atoms.Session.taskIdList username)

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

                                        let taskMap =
                                            taskIdList
                                            |> List.map
                                                (fun taskId -> taskId, getter.get (Atoms.Task.task (Some taskId)))
                                            |> Map.ofList

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

                                                                        let task = taskMap.[cellMetadata.TaskId]

                                                                        {
                                                                            TaskId = cellMetadata.TaskId
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
                                                                        cellMetadata.Status
                                                                    ])
                                                            |> Sorting.sortLanesByTimeOfDay
                                                                dayStart
                                                                { Date = referenceDay; Time = dayStart }
                                                            |> List.indexed
                                                            |> List.map (fun (i, (taskState, _)) -> taskState.TaskId, i)
                                                            |> Map.ofList

                                                        let newCells =
                                                            cellsMetadata
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
                        let databaseStateMapCache = getter.get (Atoms.Session.databaseStateMapCache username)
                        let dateSequence = getter.get dateSequence
                        let view = getter.get (Atoms.User.view username)
                        let position = getter.get Atoms.position
                        let selectedDatabaseIds = getter.get Atoms.selectedDatabaseIds
                        let dayStart = getter.get (Atoms.User.dayStart username)

                        getSessionData
                            {|
                                Username = username
                                DayStart = dayStart
                                DateSequence = dateSequence
                                View = view
                                Position = position
                                SelectedDatabaseIds = selectedDatabaseIds |> Set.ofArray
                                DatabaseStateMap = databaseStateMapCache
                            |}

                        )
                )
