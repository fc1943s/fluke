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
            let databaseIdIdentifier (databaseId: DatabaseId option) =
                databaseId
                |> Option.map DatabaseId.Value
                |> Option.defaultValue Guid.Empty
                |> string
                |> List.singleton

            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Database}/{nameof name}",
                    (fun (_databaseId: DatabaseId option) -> Database.Default.Name),
                    (fun (databaseId: DatabaseId option) ->
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
                    (fun (_databaseId: DatabaseId option) -> Database.Default.Owner),
                    (fun (databaseId: DatabaseId option) ->
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
                    (fun (_databaseId: DatabaseId option) -> Database.Default.SharedWith),
                    (fun (databaseId: DatabaseId option) ->
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
                    (fun (_databaseId: DatabaseId option) -> Database.Default.DayStart),
                    (fun (databaseId: DatabaseId option) ->
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
                    (fun (_databaseId: DatabaseId option) -> Database.Default.Position),
                    (fun (databaseId: DatabaseId option) ->
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
            let taskIdIdentifier (taskId: TaskId option) =
                taskId
                |> Option.map TaskId.Value
                |> Option.defaultValue Guid.Empty
                |> string
                |> List.singleton

            let rec task =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof task}",
                    (fun (_taskId: TaskId option) -> Task.Default)
                )

            let rec databaseId =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof databaseId}",
                    (fun (_taskId: TaskId option) -> Database.Default.Id),
                    (fun (taskId: TaskId option) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (databaseId, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec information =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof information}",
                    (fun (_taskId: TaskId option) -> Task.Default.Information),
                    (fun (taskId: TaskId option) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (information, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec name =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof name}",
                    (fun (_taskId: TaskId option) -> Task.Default.Name),
                    (fun (taskId: TaskId option) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (name, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec scheduling =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof scheduling}",
                    (fun (_taskId: TaskId option) -> Task.Default.Scheduling),
                    (fun (taskId: TaskId option) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (scheduling, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec pendingAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof pendingAfter}",
                    (fun (_taskId: TaskId option) -> Task.Default.PendingAfter),
                    (fun (taskId: TaskId option) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (pendingAfter, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec missedAfter =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof missedAfter}",
                    (fun (_taskId: TaskId option) -> Task.Default.MissedAfter),
                    (fun (taskId: TaskId option) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (missedAfter, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec priority =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof priority}",
                    (fun (_taskId: TaskId option) -> Task.Default.Priority),
                    (fun (taskId: TaskId option) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (priority, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec duration =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof duration}",
                    (fun (_taskId: TaskId option) -> Task.Default.Duration),
                    (fun (taskId: TaskId option) ->
                        [
                            Recoil.gunEffect None (Recoil.AtomFamily (duration, taskId)) (taskIdIdentifier taskId)
                        ])
                )

            let rec attachments =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Task}/{nameof attachments}",
                    (fun (_taskId: TaskId option) -> []: Attachment list) // TODO: move from here?
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
                                (Recoil.AtomFamily (selected, (username, taskId, dateId)))
                                [
                                    taskId.KeyFormat ()
                                    dateId.KeyFormat ()
                                ]
                        ])
                )


        module rec Session =
            let rec databaseStateMapCache =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Session}/{nameof databaseStateMapCache}",
                    (fun (_username: Username) -> Map.empty: Map<DatabaseId, DatabaseState>)
                )

            //            let rec availableDatabaseIds =
//                Recoil.atomFamilyWithProfiling (
//                    $"{nameof atomFamily}/{nameof Session}/{nameof availableDatabaseIds}",
//                    (fun (_username: Username) -> []: DatabaseId list)
//                )

            let rec databaseIdList =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Session}/{nameof databaseIdList}",
                    (fun (_username: Username) -> []: DatabaseId list),
                    (fun (username: Username) ->
                        let _atomFamily = databaseIdList
                        let _atomKey = username
                        let _username = Some username
                        let _keySuffix = ""

                        [
                            (fun (e: RecoilEffectProps<_>) ->

                                let atomFamilyDatabase = Recoil.getGunAtomKey None (Database.owner None).key []

                                printfn
                                    $"@@@@--- atomFamilyDatabase={atomFamilyDatabase} (Database.owner None).key={
                                                                                                                     (Database.owner
                                                                                                                         None)
                                                                                                                         .key
                                    }"

                                let path = "GunRecoil/atomFamily/Database"

                                match e.trigger with
                                | "get" ->
                                    (async {
                                        let! gun = Recoil.getGun ()
                                        let gunAtomNode = Gun.getGunAtomNode (Some gun) path

                                        match gunAtomNode with
                                        | Some gunAtomNode ->
                                            gunAtomNode
                                                .map()
                                                .on (fun _v k ->
                                                    if string Recoil.atomFormGuid <> k then
                                                        e.setSelf
                                                            (fun oldValue ->
                                                                oldValue
                                                                @ [
                                                                    k |> Guid |> DatabaseId
                                                                ]))
                                        | None ->
                                            Browser.Dom.console.error
                                                $"[databaseIdList.effect] Gun node not found: path={path}"
                                     })
                                    |> Async.StartAsPromise
                                    |> Promise.start
                                | _ -> ()

                                e.onSet (fun _ _ -> failwith "[databaseIdList.effect] read only atom")

                                fun () ->
                                    (promise {
                                        let! gun = Recoil.getGun ()
                                        let gunAtomNode = Gun.getGunAtomNode (Some gun) path

                                        match gunAtomNode with
                                        | Some gunAtomNode ->

                                            if not JS.isProduction && not JS.isTesting then
                                                printfn
                                                    "[databaseIdList.effect] unsubscribe atom. calling selected.off ()"

                                            gunAtomNode.map().off () |> ignore
                                        | None ->
                                            Browser.Dom.console.error
                                                $"[databaseIdList.effect.off] Gun node not found: path={path}"
                                     })
                                    |> Promise.start)
                        ])
                )

            let rec taskIdList =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Session}/{nameof taskIdList}",
                    (fun (_username: Username) -> []: TaskId list),
                    (fun (username: Username) ->
                        let _atomFamily = taskIdList
                        let _atomKey = username
                        let _username = Some username
                        let _keySuffix = ""

                        [
                            (fun (e: RecoilEffectProps<_>) ->
                                let path = "GunRecoil/atomFamily/Task"

                                match e.trigger with
                                | "get" ->
                                    (async {
                                        let! gun = Recoil.getGun ()
                                        let gunAtomNode = Gun.getGunAtomNode (Some gun) path

                                        match gunAtomNode with
                                        | Some gunAtomNode ->
                                            gunAtomNode
                                                .map()
                                                .on (fun _v k ->
                                                    if string Recoil.atomFormGuid <> k then
                                                        e.setSelf
                                                            (fun oldValue ->
                                                                oldValue
                                                                @ [
                                                                    k |> Guid |> TaskId
                                                                ]))
                                        | None ->
                                            Browser.Dom.console.error
                                                $"[taskIdList.effect] Gun node not found: path={path}"
                                     })
                                    |> Async.StartAsPromise
                                    |> Promise.start
                                | _ -> ()

                                e.onSet (fun _ _ -> failwith "[taskIdList.effect] read only atom")

                                fun () ->
                                    (promise {
                                        let! gun = Recoil.getGun ()
                                        let gunAtomNode = Gun.getGunAtomNode (Some gun) path

                                        match gunAtomNode with
                                        | Some gunAtomNode ->
                                            if not JS.isProduction && not JS.isTesting then
                                                printfn "[taskIdList.effect] unsubscribe atom. calling selected.off ()"

                                            gunAtomNode.map().off () |> ignore
                                        | None ->
                                            Browser.Dom.console.error
                                                $"[taskIdList.effect.off] Gun node not found: path={path}"

                                     })
                                    |> Promise.start)
                        ])
                )

    //            let rec taskMap =
//                Recoil.atomFamilyWithProfiling (
//                    $"{nameof atomFamily}/{nameof Session}/{nameof taskMap}",
//                    (fun (_username: Username) -> Map.empty: Map<DatabaseId, Set<TaskId>>),
//                    (fun (username: Username) ->
//                        let _atomFamily = databaseIdList
//                        let _atomKey = username
//                        let _username = Some username
//                        let _keySuffix = ""
//
//                        [
//                            (fun (e: Recoil.EffectProps<_>) ->
//                                let path = "Fluke/atomFamily/Task"
//
//                                match e.trigger with
//                                | "get" ->
//                                    (async {
//                                        let! gun = Recoil.getGun ()
//                                        let gunAtomNode = Gun.getGunAtomNode gun path
//
//                                        gunAtomNode
//                                            .map()
//                                            .on (fun _v k ->
//                                                printfn $"!!!@ on effect. k={k} v={_v}"
//                                                let taskId = k |> Guid |> TaskId
//
//                                                let databaseId =
//                                                    Thoth.Json.Decode.Auto.fromString<DatabaseId option> v
//                                                    |> function
//                                                    | Ok value -> value
//                                                    | Error error -> failwith error
//                                                //                                                if not JS.isProduction && not JS.isTesting then
////                                                    printfn
////                                                        $"@@@gunEffect. gunAtomNode.on() effect. data={
////                                                                                                           JS.JSON.stringify
////                                                                                                               {|
////                                                                                                                   v = v
////                                                                                                                   k = k
////                                                                                                               |}
////                                                        }"
//                                                printfn $"!!! on effect. taskId={taskId} databaseId={databaseId}"
//
//                                                match taskId, databaseId with
//                                                | TaskId taskIdGuid, Some databaseId when taskIdGuid <> Guid.Empty ->
//                                                    e.setSelf
//                                                        (fun oldValue ->
//
//
//                                                            //                                                        printfn $"@@@oldValue={JS.JSON.stringify oldValue};newValue={k}"
//                                                            let currentMap = oldValue |> JS.ofObjDefault Map.empty
//
//                                                            printfn $"!!! on effect. currentMap={currentMap}"
//
//                                                            let newTaskIdSet =
//                                                                currentMap
//                                                                |> Map.tryFind databaseId
//                                                                |> Option.defaultValue Set.empty
//                                                                |> Set.add taskId
//
//                                                            printfn
//                                                                $"!!! on effect. oldValue={JS.JSON.stringify oldValue} newTaskIdSet={
//                                                                                                                                         newTaskIdSet
//                                                                }"
//
//                                                            currentMap |> Map.add databaseId newTaskIdSet)
//                                                | _ -> ()
//                                                //                                                match Gun.deserializeGunAtomNode v with
////                                                | Some gunAtomNodeValue ->
////                                                    e.setSelf (fun oldValue -> printfn $"oldValue={oldValue};newValue={gunAtomNodeValue}"; gunAtomNodeValue)
////                                                | None -> ()
//                                                )
//
//                                     //                                        let atom = atomFamily atomKey
//
//                                     //                                        let! gunAtomNode, id = Recoil.getGunAtomNode username atom keySuffix
//
//                                     //                                        gunAtomNode.on
////                                            (fun data ->
////                                                if not JS.isProduction && not JS.isTesting then
////                                                    printfn
////                                                        $"gunEffect. gunAtomNode.on() effect. id={id} data={
////                                                                                                                JS.JSON.stringify
////                                                                                                                    data
////                                                        }"
////
////                                                //                                                match Gun.deserializeGunAtomNode data with
//////                                                | Some gunAtomNodeValue -> e.setSelf gunAtomNodeValue
//////                                                | None -> ()
////                                                )
//                                     })
//                                    |> Async.StartAsPromise
//                                    |> Promise.start
//                                | _ -> ()
//
//                                e.onSet
//                                    (fun value oldValue ->
//                                        (promise {
//                                            if oldValue <> value then
//                                                //                                            let! gunAtomNode, _ = Recoil.getGunAtomNode username atom keySuffix
//                                                //                                            Gun.putGunAtomNode gunAtomNode value
//
//                                                if not JS.isProduction && not JS.isTesting then
//                                                    printfn
//                                                        $"@@@gunEffect. onSet. oldValue: {JS.JSON.stringify oldValue}; newValue: {
//                                                                                                                                      JS.JSON.stringify
//                                                                                                                                          value
//                                                        }"
//                                            else
//                                                printfn
//                                                    $"gunEffect. onSet. value=oldValue. skipping. newValue: {
//                                                                                                                 JS.JSON.stringify
//                                                                                                                     value
//                                                    }"
//                                         })
//                                        |> Promise.start)
//
//                                fun () ->
//                                    (promise {
//                                        //                                        let! gunAtomNode, _ = Recoil.getGunAtomNode username atom keySuffix
//
//                                        if not JS.isProduction && not JS.isTesting then
//                                            printfn "@@@gunEffect. unsubscribe atom. calling selected.off ()"
//
//                                     //                                        gunAtomNode.off () |> ignore
//                                     })
//                                    |> Promise.start)
//                        ])
//                )


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
                        Gun.gun
                            {
                                Gun.GunProps.peers = if JS.isTesting then None else Some (gunPeers |> List.toArray)
                                Gun.GunProps.radisk = if JS.isTesting then None else Some false
                                Gun.GunProps.localStorage = if JS.isTesting then None else Some true
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


        //        let rec apiCurrentUserAsync =
//            Recoil.asyncSelectorWithProfiling (
//                $"{nameof selector}/{nameof apiCurrentUserAsync}",
//                (fun getter ->
//                    promise {
//                        let api = getter.get Atoms.api
//
//                        return!
//                            api
//                            |> Option.bind (fun api -> Some api.currentUser)
//                            |> Sync.handleRequest
//                    })
//            )

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
                            Name = getter.get (Atoms.Database.name (Some databaseId))
                            Owner = getter.get (Atoms.Database.owner (Some databaseId))
                            SharedWith = getter.get (Atoms.Database.sharedWith (Some databaseId))
                            Position = getter.get (Atoms.Database.position (Some databaseId))
                            DayStart = getter.get (Atoms.Database.dayStart (Some databaseId))
                        })
                )


        module rec Task =
            let rec task =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Task}/{nameof task}",
                    (fun (taskId: TaskId) getter ->
                        {
                            Id = taskId
                            Name = getter.get (Atoms.Task.name (Some taskId))
                            Information = getter.get (Atoms.Task.information (Some taskId))
                            PendingAfter = getter.get (Atoms.Task.pendingAfter (Some taskId))
                            MissedAfter = getter.get (Atoms.Task.missedAfter (Some taskId))
                            Scheduling = getter.get (Atoms.Task.scheduling (Some taskId))
                            Priority = getter.get (Atoms.Task.priority (Some taskId))
                            Duration = getter.get (Atoms.Task.duration (Some taskId))
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


        module rec Session =
            let rec informationList =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof informationList}",
                    (fun (username: Username) getter ->
                        let taskIdList = getter.get (Atoms.Session.taskIdList username)

                        taskIdList
                        |> Seq.map (fun taskId -> getter.get (Atoms.Task.information (Some taskId)))
                        |> Seq.distinct
                        |> Seq.filter
                            (fun information ->
                                information
                                |> Information.Name
                                |> InformationName.Value
                                |> String.IsNullOrWhiteSpace
                                |> not)
                        |> Seq.toList)
                )

            let rec taskIdList =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof taskIdList}",
                    (fun (username: Username) getter ->
                        let taskIdList = getter.get (Atoms.Session.taskIdList username)
                        let selectedDatabaseIdList = getter.get (Atoms.User.selectedDatabaseIdList username)
                        let selectedDatabaseIdListSet = selectedDatabaseIdList |> Set.ofList

                        taskIdList
                        |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.databaseId (Some taskId)))
                        |> List.filter (fun (_, databaseId) -> selectedDatabaseIdListSet.Contains databaseId)
                        |> List.map fst)
                )

            let rec visibleTaskIdList =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof visibleTaskIdList}",
                    (fun (username: Username) getter ->
                        let taskIdList = getter.get (taskIdList username)
                        taskIdList)
                )

            let rec cellSelectionMap =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof cellSelectionMap}",
                    (fun (username: Username) getter ->
                        let taskIdList = getter.get (taskIdList username)
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
                        |> Map.ofList),

                    (fun (_username: Username) setter (newSelection: Map<TaskId, Set<FlukeDate>>) ->
                        let username = setter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = setter.get (taskIdList username)
                            let cellSelectionMap = setter.get (cellSelectionMap username)

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

            let rec activeSessions =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof activeSessions}",
                    (fun (username: Username) getter ->
                        let taskIdList = getter.get (taskIdList username)

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
                        let taskIdList = getter.get (taskIdList username)

                        let informationMap =
                            taskIdList
                            |> List.map (fun taskId -> taskId, getter.get (Atoms.Task.information (Some taskId)))
                            |> Map.ofList

                        taskIdList
                        |> List.groupBy (fun taskId -> informationMap.[taskId])
                        |> List.sortBy (fun (information, _) -> information |> Information.Name)
                        |> List.groupBy (fun (information, _) -> Information.toString information)
                        |> List.sortBy (snd >> List.head >> fst >> Information.toTag))
                )

            let rec weekCellsMap =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Session}/{nameof weekCellsMap}",
                    (fun (username: Username) getter ->
                        let position = getter.get Atoms.position
                        let taskIdList = getter.get (taskIdList username)

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
                                                            |> List.map
                                                                (fun (i, (taskState, _)) -> taskState.Task.Id, i)
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
                        let selectedDatabaseIdList = getter.get (Atoms.User.selectedDatabaseIdList username)
                        let dayStart = getter.get (Atoms.User.dayStart username)

                        let _ =
                            getSessionData
                                {|
                                    Username = username
                                    DayStart = dayStart
                                    DateSequence = dateSequence
                                    View = view
                                    Position = position
                                    SelectedDatabaseIdList = selectedDatabaseIdList |> Set.ofList
                                    DatabaseStateMap = databaseStateMapCache
                                |}

                        ()

                        )
                )

            let rec hasSelection =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof FlukeDate}/{nameof hasSelection}",
                    (fun (date: FlukeDate) getter ->
                        let username = getter.get Atoms.username

                        match username with
                        | Some username ->
                            let taskIdList = getter.get (taskIdList username)

                            taskIdList
                            |> List.exists
                                (fun taskId -> getter.get (Atoms.Cell.selected (username, taskId, DateId date)))
                        | None -> false)
                )

        module rec Cell =
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
                                    let taskIdList = setter.get (Session.taskIdList username)
                                    let oldCellSelectionMap = setter.get (Session.cellSelectionMap username)

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
