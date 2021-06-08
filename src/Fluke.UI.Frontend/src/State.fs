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
                JS.isDebug,
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
            let rec databaseIdSet =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof databaseIdSet}",
                    (fun (_username: Username) -> Set.empty: Set<DatabaseId>),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, databaseIdSet, username)) []
                        ])
                )

            let rec joinSet =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof joinSet}",
                    (fun (_username: Username) -> Set.empty: Set<Join>),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, joinSet, username)) []
                        ])
                )

            let rec expandedDatabaseIdSet =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof expandedDatabaseIdSet}",
                    (fun (_username: Username) -> Set.empty: Set<DatabaseId>),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, expandedDatabaseIdSet, username)) []
                        ])
                )

            let rec selectedDatabaseIdSet =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof selectedDatabaseIdSet}",
                    (fun (_username: Username) -> Set.empty: Set<DatabaseId>),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, selectedDatabaseIdSet, username)) []
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

            let rec searchText =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof searchText}",
                    (fun (_username: Username) -> ""),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, searchText, username)) []
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

            let rec showViewOptions =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof showViewOptions}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, showViewOptions, username)) []
                        ])
                )

            let rec filterTasksByView =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof User}/{nameof filterTasksByView}",
                    (fun (_username: Username) -> true),
                    (fun (username: Username) ->
                        [
                            Recoil.gunEffect (Recoil.AtomFamily (username, filterTasksByView, username)) []
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

            let rec taskIdSet =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof taskIdSet}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Set.empty: Set<TaskId>),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        [
                            Recoil.gunEffect
                                (Recoil.AtomFamily (username, taskIdSet, (username, databaseId)))
                                (databaseIdIdentifier databaseId)
                        ])
                )

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
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof FlukeDate}/{nameof isToday}",
                    (fun (date: FlukeDate) getter ->
                        promise {
                            let username = getter.get Atoms.username
                            let position = getter.get Atoms.position

                            return
                                match username, position with
                                | Some username, Some position ->
                                    let dayStart = getter.get (Atoms.User.dayStart username)

                                    Domain.UserInteraction.isToday dayStart position (DateId date)
                                | _ -> false
                        })
                )



        module rec Database =
            let rec database =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Database}/{nameof database}",
                    (fun (username: Username, databaseId: DatabaseId) getter ->
                        promise {
                            return
                                {
                                    Id = databaseId
                                    Name = getter.get (Atoms.Database.name (username, databaseId))
                                    Owner = getter.get (Atoms.Database.owner (username, databaseId))
                                    SharedWith = getter.get (Atoms.Database.sharedWith (username, databaseId))
                                    Position = getter.get (Atoms.Database.position (username, databaseId))
                                    DayStart = getter.get (Atoms.Database.dayStart (username, databaseId))
                                }
                        })
                )

            let rec isReadWrite =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Database}/{nameof isReadWrite}",
                    (fun (databaseId: DatabaseId) getter ->
                        promise {
                            let username = getter.get Atoms.username

                            let access =
                                match username with
                                | Some username ->
                                    let database = getter.get (database (username, databaseId))

                                    if username <> Templates.templatesUser.Username
                                       && database.Owner = Templates.templatesUser.Username then
                                        None
                                    else
                                        getAccess database username
                                | None -> None

                            return access = Some Access.ReadWrite
                        })
                )

        module rec Information =
            let rec informationState =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Information}/{nameof informationState}",
                    (fun (information: Information) getter ->
                        promise {
                            return
                                {
                                    Information = information
                                    Attachments = getter.get (Atoms.Information.attachments information)
                                    SortList = []
                                }
                        })
                )


        module rec Task =
            let rec task =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof task}",
                    (fun (username: Username, taskId: TaskId) getter ->
                        promise {
                            return
                                {
                                    Id = taskId
                                    Name = getter.get (Atoms.Task.name (username, taskId))
                                    Information = getter.get (Atoms.Task.information (username, taskId))
                                    PendingAfter = getter.get (Atoms.Task.pendingAfter (username, taskId))
                                    MissedAfter = getter.get (Atoms.Task.missedAfter (username, taskId))
                                    Scheduling = getter.get (Atoms.Task.scheduling (username, taskId))
                                    Priority = getter.get (Atoms.Task.priority (username, taskId))
                                    Duration = getter.get (Atoms.Task.duration (username, taskId))
                                }
                        })
                )

            let rec lastSession =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof lastSession}",
                    (fun (taskId: TaskId) getter ->
                        promise {
                            let dateSequence = getter.get dateSequence

                            return
                                dateSequence
                                |> List.rev
                                |> List.tryPick
                                    (fun date ->
                                        let sessions = getter.get (Atoms.Cell.sessions (taskId, DateId date))

                                        sessions
                                        |> List.sortByDescending
                                            (fun (TaskSession (start, _, _)) -> start |> FlukeDateTime.DateTime)
                                        |> List.tryHead)
                        })
                )


            let rec taskDateSequence =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof taskDateSequence}",
                    (fun (username: Username, taskId: TaskId) getter ->
                        promise {
                            let position = getter.get Atoms.position
                            let dateSequence = getter.get dateSequence
                            let scheduling = getter.get (Atoms.Task.scheduling (username, taskId))

                            return
                                dateSequence
                                |> Rendering.stretchDateSequence position scheduling
                        })
                )

            let rec taskState =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof taskState}",
                    (fun (username: Username, taskId: TaskId) getter ->
                        promise {
                            let task = getter.get (task (username, taskId))
                            let taskDateSequence = getter.get (taskDateSequence (username, taskId))

                            return
                                {
                                    Task = task
                                    Sessions = []
                                    Attachments = []
                                    SortList = []
                                    CellStateMap =
                                        taskDateSequence
                                        |> List.map DateId
                                        |> List.map
                                            (fun dateId ->
                                                let cellState =
                                                    {
                                                        Status =
                                                            getter.get (Atoms.Cell.status (username, taskId, dateId))
                                                        Sessions = getter.get (Atoms.Cell.sessions (taskId, dateId))
                                                        Attachments =
                                                            getter.get (Atoms.Cell.attachments (taskId, dateId))
                                                    }

                                                dateId, cellState)
                                        |> List.filter
                                            (fun (_, cellState) ->
                                                cellState.Status <> Disabled
                                                || not cellState.Sessions.IsEmpty
                                                || not cellState.Attachments.IsEmpty)
                                        |> Map.ofList
                                }
                        })
                )

            let rec statusMap =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof statusMap}",
                    (fun (username: Username, taskId: TaskId) getter ->
                        promise {
                            let dayStart = getter.get (Atoms.User.dayStart username)
                            let position = getter.get Atoms.position
                            let taskState = getter.get (taskState (username, taskId))
                            let taskDateSequence = getter.get (taskDateSequence (username, taskId))

                            return
                                match position with
                                | Some position when not taskDateSequence.IsEmpty ->
                                    Rendering.renderTaskStatusMap dayStart position taskDateSequence taskState
                                | _ -> Map.empty
                        })
                )

            let rec databaseId =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof databaseId}",
                    (fun (username: Username, taskId: TaskId) getter ->
                        promise {
                            let databaseIdSet = getter.get (Atoms.User.databaseIdSet username)

                            let databaseIdSet =
                                databaseIdSet
                                |> Set.choose
                                    (fun databaseId ->
                                        let taskIdSet = getter.get (Atoms.Database.taskIdSet (username, databaseId))
                                        if taskIdSet.Contains taskId then Some databaseId else None)

                            return
                                match databaseIdSet |> Set.toList with
                                | [] -> Database.Default.Id
                                | [ databaseId ] -> databaseId
                                | _ -> failwith $"Error: task {taskId} exists in two databases ({databaseIdSet})"
                        })
                )

            let rec isReadWrite =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof isReadWrite}",
                    (fun (username: Username, taskId: TaskId) getter ->
                        promise {
                            let databaseId = getter.get (Task.databaseId (username, taskId))
                            return getter.get (Database.isReadWrite databaseId)
                        })
                )

            let rec activeSession =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof activeSession}",
                    (fun (taskId: TaskId) getter ->
                        promise {
                            let position = getter.get Atoms.position
                            let lastSession = getter.get (lastSession taskId)

                            return
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
                                | _ -> None
                        })
                )

            let rec showUser =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof showUser}",
                    (fun (username: Username, taskId: TaskId) getter ->
                        promise {
                            let taskState = getter.get (taskState (username, taskId))

                            let usersCount =
                                taskState.CellStateMap
                                |> Map.values
                                |> Seq.choose
                                    (function
                                    | { Status = UserStatus (user, _) } -> Some user
                                    | _ -> None)
                                |> Seq.distinct
                                |> Seq.length

                            return usersCount > 1
                        })
                )

            let rec hasSelection =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof hasSelection}",
                    (fun (taskId: TaskId) getter ->
                        promise {
                            let dateSequence = getter.get dateSequence
                            let username = getter.get Atoms.username

                            return
                                match username with
                                | Some username ->
                                    dateSequence
                                    |> List.exists
                                        (fun date -> getter.get (Atoms.Cell.selected (username, taskId, DateId date)))
                                | None -> false
                        })
                )

        module rec Cell =
            let rec sessionStatus =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Cell}/{nameof sessionStatus}",
                    (fun (username: Username, taskId: TaskId, dateId: DateId) getter ->
                        promise {
                            let hideSchedulingOverlay = getter.get (Atoms.User.hideSchedulingOverlay username)

                            if hideSchedulingOverlay then
                                return getter.get (Atoms.Cell.status (username, taskId, dateId))
                            else
                                let statusMap = getter.get (Task.statusMap (username, taskId))

                                return
                                    statusMap
                                    |> Map.tryFind dateId
                                    |> Option.defaultValue Disabled
                        })
                )


        module rec Session =
            let rec taskIdSet =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof taskIdSet}",
                    (fun (username: Username) getter ->
                        promise {
                            let databaseIdSet = getter.get (Atoms.User.databaseIdSet username)

                            return
                                databaseIdSet
                                |> Set.collect
                                    (fun databaseId -> getter.get (Atoms.Database.taskIdSet (username, databaseId)))
                        })
                )

            let rec informationSet =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof informationSet}",
                    (fun (username: Username) getter ->
                        promise {
                            let taskIdSet = getter.get (taskIdSet username)

                            return
                                taskIdSet
                                |> Set.toList
                                |> List.map (fun taskId -> getter.get (Atoms.Task.information (username, taskId)))
                                |> List.filter
                                    (fun information ->
                                        information
                                        |> Information.Name
                                        |> InformationName.Value
                                        |> String.IsNullOrWhiteSpace
                                        |> not)
                                |> Set.ofList
                        })
                )

            let rec selectedTaskIdSet =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof selectedTaskIdSet}",
                    (fun (username: Username) getter ->
                        promise {
                            let selectedDatabaseIdSet = getter.get (Atoms.User.selectedDatabaseIdSet username)
                            let taskIdSet = getter.get (taskIdSet username)

                            return
                                taskIdSet
                                |> Set.toList
                                |> List.map (fun taskId -> taskId, getter.get (Task.databaseId (username, taskId)))
                                |> List.filter (fun (_, databaseId) -> selectedDatabaseIdSet |> Set.contains databaseId)
                                |> List.map fst
                                |> Set.ofList
                        })
                )

            let rec informationStateList =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Information}/{nameof informationStateList}",
                    (fun (username: Username) getter ->
                        promise {
                            let informationSet = getter.get (informationSet username)

                            return
                                informationSet
                                |> Set.toList
                                |> List.map (fun information -> getter.get (Information.informationState information))
                        })
                )

            let rec activeSessions =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof activeSessions}",
                    (fun (username: Username) getter ->
                        promise {
                            let selectedTaskIdSet = getter.get (selectedTaskIdSet username)

                            let sessionLength = getter.get (Atoms.User.sessionLength username)
                            let sessionBreakLength = getter.get (Atoms.User.sessionBreakLength username)

                            return
                                selectedTaskIdSet
                                |> Set.toList
                                |> List.choose
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
                        })
                )

            let rec taskStateList =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof taskStateList}",
                    (fun (username: Username) getter ->
                        promise {
                            let selectedTaskIdSet = getter.get (selectedTaskIdSet username)

                            return
                                selectedTaskIdSet
                                |> Set.toList
                                |> List.map (fun taskId -> getter.get (Task.taskState (username, taskId)))
                        })
                )

            let rec filteredTaskIdSet =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof filteredTaskIdSet}",
                    (fun (username: Username) getter ->
                        promise {
                            let filterTasksByView = getter.get (Atoms.User.filterTasksByView username)
                            let searchText = getter.get (Atoms.User.searchText username)
                            let view = getter.get (Atoms.User.view username)
                            let dateSequence = getter.get dateSequence
                            let taskStateList = getter.get (taskStateList username)

                            let taskStateSearch =
                                match searchText with
                                | "" -> taskStateList
                                | _ ->
                                    taskStateList
                                    |> List.filter
                                        (fun taskState ->
                                            let check (text: string) = text.IndexOf searchText >= 0

                                            (taskState.Task.Name |> TaskName.Value |> check)
                                            || (taskState.Task.Information
                                                |> Information.Name
                                                |> InformationName.Value
                                                |> check))

                            let filteredTaskStateList =
                                if filterTasksByView then
                                    filterTaskStateSeq view dateSequence taskStateSearch
                                    |> Seq.toList
                                else
                                    taskStateSearch

                            printfn $"filteredTaskStateList.Length={filteredTaskStateList.Length} taskStateSearch.Length={taskStateSearch.Length}"

                            return
                                filteredTaskStateList
                                |> List.map (fun taskState -> taskState.Task.Id)
                                |> Set.ofList
                        })
                )


            let rec sortedTaskIdList =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof sortedTaskIdList}",
                    (fun (username: Username) getter ->
                        promise {
                            let position = getter.get Atoms.position

                            return
                                match position with
                                | Some position ->
                                    let view = getter.get (Atoms.User.view username)
                                    let dayStart = getter.get (Atoms.User.dayStart username)
                                    let filteredTaskIdSet = getter.get (filteredTaskIdSet username)

                                    let informationStateList = getter.get (Session.informationStateList username)

                                    let lanes =
                                        filteredTaskIdSet
                                        |> Set.toList
                                        |> List.map
                                            (fun taskId ->
                                                let taskState = getter.get (Task.taskState (username, taskId))
                                                let statusMap = getter.get (Task.statusMap (username, taskId))
                                                taskState, statusMap)

                                    let result =
                                        sortLanes
                                            {|
                                                View = view
                                                DayStart = dayStart
                                                Position = position
                                                InformationStateList = informationStateList
                                                Lanes = lanes
                                            |}

                                    result
                                    |> List.map (fun (taskState, _) -> taskState.Task.Id)
                                | _ -> []
                        })
                )

            let rec tasksByInformationKind =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof tasksByInformationKind}",
                    (fun (username: Username) getter ->
                        promise {
                            let sortedTaskIdList = getter.get (sortedTaskIdList username)

                            return
                                sortedTaskIdList
                                |> List.groupBy (fun taskId -> getter.get (Atoms.Task.information (username, taskId)))
                                |> List.sortBy (fun (information, _) -> information |> Information.Name)
                                |> List.groupBy (fun (information, _) -> Information.toString information)
                                |> List.sortBy (snd >> List.head >> fst >> Information.toTag)
                        })
                )

            let rec cellSelectionMap =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof cellSelectionMap}",
                    (fun (username: Username) getter ->
                        promise {
                            let sortedTaskIdList = getter.get (sortedTaskIdList username)
                            let dateSequence = getter.get dateSequence

                            return
                                sortedTaskIdList
                                |> List.map
                                    (fun taskId ->
                                        let dates =
                                            dateSequence
                                            |> List.map
                                                (fun date ->
                                                    date,
                                                    getter.get (Atoms.Cell.selected (username, taskId, DateId date)))
                                            |> List.filter snd
                                            |> List.map fst
                                            |> Set.ofList

                                        taskId, dates)
                                |> List.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                                |> Map.ofList
                        })
                )


            let rec hasCellSelection =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof FlukeDate}/{nameof hasCellSelection}",
                    (fun (date: FlukeDate) getter ->
                        promise {
                            let username = getter.get Atoms.username

                            return
                                match username with
                                | Some username ->
                                    let cellSelectionMap = getter.get (cellSelectionMap username)

                                    cellSelectionMap
                                    |> Map.values
                                    |> Seq.exists (Set.contains date)
                                | None -> false
                        })
                )



        module rec BulletJournalView =
            let rec weekCellsMap =
                Recoil.asyncSelectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof BulletJournalView}/{nameof weekCellsMap}",
                    (fun (username: Username) getter ->
                        promise {
                            let position = getter.get Atoms.position
                            let sortedTaskIdList = getter.get (Session.sortedTaskIdList username)

                            return
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

                                                let taskStateMap =
                                                    sortedTaskIdList
                                                    |> List.map
                                                        (fun taskId ->
                                                            taskId, getter.get (Task.taskState (username, taskId)))
                                                    |> Map.ofList

                                                let result =
                                                    sortedTaskIdList
                                                    |> List.collect
                                                        (fun taskId ->
                                                            dateIdSequence
                                                            |> List.map
                                                                (fun dateId ->
                                                                    match dateId with
                                                                    | DateId referenceDay as dateId ->
                                                                        let isToday =
                                                                            getter.get (FlukeDate.isToday referenceDay)

                                                                        let cellState =
                                                                            taskStateMap
                                                                            |> Map.tryFind taskId
                                                                            |> Option.bind
                                                                                (fun taskState ->
                                                                                    taskState.CellStateMap
                                                                                    |> Map.tryFind dateId)
                                                                            |> Option.defaultValue
                                                                                {
                                                                                    Status = CellStatus.Disabled
                                                                                    Attachments = []
                                                                                    Sessions = []
                                                                                }

                                                                        {|
                                                                            DateId = dateId
                                                                            TaskId = taskId
                                                                            Status = cellState.Status
                                                                            Sessions = cellState.Sessions
                                                                            IsToday = isToday
                                                                            Attachments = cellState.Attachments
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
                                                                                    Task =
                                                                                        taskStateMap.[cellMetadata.TaskId]
                                                                                            .Task
                                                                                    Sessions = taskSessions
                                                                                }

                                                                            taskState,
                                                                            [
                                                                                dateId, cellMetadata.Status
                                                                            ]
                                                                            |> Map.ofList)
                                                                    |> Sorting.sortLanesByTimeOfDay
                                                                        dayStart
                                                                        (FlukeDateTime.Create (referenceDay, dayStart))
                                                                    |> List.indexed
                                                                    |> List.map
                                                                        (fun (i, (taskState, _)) ->
                                                                            taskState.Task.Id, i)
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
                                | _ -> []
                        })
                )
