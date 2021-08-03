namespace FsStore

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core
open System
open FsCore
open FsCore.Model
open FsStore.Bindings.Jotai
open FsStore.Model
open FsStore.Shared
open Microsoft.FSharp.Core.Operators
open FsJs
open FsStore.Bindings

#nowarn "40"


module SignalR =
    open Fable.SignalR

    let connect hubUrl _getter setter fn =
        let timeout = 1000

        SignalR.connect<Sync.Request, Sync.Request, obj, Sync.Response, Sync.Response>
            (fun hub ->
                hub
                    .withUrl($"{hubUrl}{Sync.endpoint}")
                    //                                        .useMessagePack()
                    .withAutomaticReconnect(
                        {
                            nextRetryDelayInMilliseconds =
                                fun _context ->
                                    Dom.log (fun () -> "SignalR.connect(). withAutomaticReconnect")
                                    Some timeout
                        }
                    )
                    .onReconnecting(fun ex -> Dom.log (fun () -> $"SignalR.connect(). onReconnecting ex={ex}"))
                    .onReconnected(fun ex -> Dom.log (fun () -> $"SignalR.connect(). onReconnected ex={ex}"))
                    .onClose(fun ex ->
                        Dom.log (fun () -> $"SignalR.connect(). onClose ex={ex}")

                        JS.setTimeout (fun () -> Store.change setter Atoms.hubTrigger ((+) 1)) (timeout / 2)
                        |> ignore)
                    .configureLogging(LogLevel.Debug)
                    .onMessage (fun msg ->
                        match msg with
                        | Sync.Response.ConnectResult -> Dom.log (fun () -> "Sync.Response.ConnectResult")
                        | Sync.Response.GetResult (key, value) ->
                            Dom.log (fun () -> $"Sync.Response.GetResult key={key} value={value}")
                        | Sync.Response.SetResult result ->
                            Dom.log (fun () -> $"Sync.Response.SetResult result={result}")
                        | Sync.Response.FilterResult (key, keys) ->
                            Dom.log (fun () -> $"Sync.Response.FilterResult key={key} keys={keys}")

                        fn msg))

module Selectors =
    let rec deviceInfo = Store.readSelector FsStore.root (nameof deviceInfo) (fun _ -> Dom.deviceInfo)

    let rec logger =
        Store.readSelector
            FsStore.root
            (nameof logger)
            (fun getter ->
                let logLevel = Store.value getter Atoms.logLevel
                Logger.Create logLevel)

    let rec atomAccessors =
        let mutable lastValue = 0
        let valueAtom = jotai.atom lastValue
        let accessorsAtom = jotai.atom (None: (GetFn * SetFn) option)

        let getBaseInfo () =
            $"
| atomAccessors baseInfo:
lastValue={lastValue}
"

        Dom.log (fun () -> $"atomAccessors.constructor {getBaseInfo ()}")

        let rec valueWrapper =
            Store.selector
                FsStore.root
                (nameof valueWrapper)
                (fun getter ->
                    let result = Store.value getter valueAtom
                    Dom.log (fun () -> $"atomAccessors.valueWrapper.get() result={result} {getBaseInfo ()}")
                    result)
                (fun getter setter newValue ->
                    Dom.log (fun () -> $"atomAccessors.valueWrapper.set() newValue={newValue} {getBaseInfo ()}")
                    Store.set setter accessorsAtom (Some (getter, setter))
                    Store.set setter valueAtom newValue)

        valueWrapper.onMount <-
            fun setAtom ->
                Dom.log (fun () -> $"atomAccessors.valueWrapper.onMount() lastValue={lastValue} {getBaseInfo ()}")
                lastValue <- lastValue + 1
                setAtom lastValue

                fun () ->
                    Dom.log (fun () -> $"atomAccessors.valueWrapper.onUnmount() lastValue={lastValue} {getBaseInfo ()}")
                    ()

        Store.readSelector
            FsStore.root
            (nameof atomAccessors)
            (fun getter ->
                let value = Store.value getter valueWrapper
                let accessors = Store.value getter accessorsAtom

                Dom.log
                    (fun () ->
                        $"atomAccessors.selfWrapper.get() value={value} accessors={accessors.IsSome} {getBaseInfo ()}")

                accessors)

    module Hub =
        let hubSubscriptionMap = Dictionary<string * string * string, string [] -> unit> ()

    let rec hub =
        Store.readSelector
            FsStore.root
            (nameof hub)
            (fun getter ->
                let _hubTrigger = Store.value getter Atoms.hubTrigger
                let hubUrl = Store.value getter Atoms.hubUrl

                match hubUrl with
                | Some (String.ValidString hubUrl) ->
                    match Store.value getter atomAccessors with
                    | Some (getter, setter) ->
                        let hub =
                            SignalR.connect
                                hubUrl
                                getter
                                setter
                                (fun msg ->
                                    match msg with
                                    | Sync.Response.FilterResult (key, keys) ->
                                        match Hub.hubSubscriptionMap.TryGetValue key with
                                        | true, fn -> fn keys
                                        | _ -> ()
                                    | _ -> ())

                        hub.startNow ()
                        Some hub
                    | None -> None
                | _ -> None)

    let rec gunPeers =
        Store.readSelector
            FsStore.root
            (nameof gunPeers)
            (fun getter ->
                let gunOptions = Store.value getter Atoms.gunOptions

                match gunOptions with
                | GunOptions.Minimal -> [||]
                | GunOptions.Sync gunPeers ->
                    gunPeers
                    |> Array.filter (String.IsNullOrWhiteSpace >> not))

    let rec gun =
        Store.readSelector
            FsStore.root
            (nameof gun)
            (fun getter ->
                let isTesting = Store.value getter Atoms.isTesting
                let gunPeers = Store.value getter gunPeers

                let gun =
                    if isTesting then
                        Gun.gun
                            {
                                Gun.GunProps.peers = None
                                Gun.GunProps.radisk = Some false
                                Gun.GunProps.localStorage = Some false
                                Gun.GunProps.multicast = None
                            }
                    else
                        Gun.gun
                            {
                                Gun.GunProps.peers = Some gunPeers
                                Gun.GunProps.radisk = Some true
                                Gun.GunProps.localStorage = Some false
                                Gun.GunProps.multicast = None
                            }

                printfn $"Gun selector. gunPeers={gunPeers}. gun={gun} returning..."

                gun)


    let rec gunNamespace =
        Store.readSelector
            FsStore.root
            (nameof gunNamespace)
            (fun getter ->
                let _gunTrigger = Store.value getter Atoms.gunTrigger
                let gun = Store.value getter gun
                let user = gun.user ()

                printfn
                    $"gunNamespace selector.
                        gunPeers={gunPeers}
                        user.is.keys={JS.Constructors.Object.keys (
                                          user.is
                                          |> Option.defaultValue (createObj [] |> unbox)
                                      )}
                        keys={user.__.sea}..."

                user)

    module rec Gun =
        let collection = Collection (nameof Gun)

        let rec gunAtomNode =
            Store.readSelectorFamily
                FsStore.root
                collection
                (nameof gunAtomNode)
                (fun (username, AtomPath atomPath) getter ->
                    let gunNamespace = Store.value getter gunNamespace

                    match gunNamespace.is with
                    | Some { alias = Some username' } when username' = (username |> Username.ValueOrDefault) ->
                        let nodes = atomPath |> String.split "/" |> Array.toList

                        match nodes with
                        | [] -> None
                        | [ root ] -> Some (gunNamespace.get root)
                        | nodes ->
                            let lastNode = nodes |> List.last
                            let parentAtomPath = AtomPath (nodes.[0..nodes.Length - 2] |> String.concat "/")
                            let node = Store.value getter (gunAtomNode (username, parentAtomPath))

                            node
                            |> Option.map (fun (node: Gun.Types.IGunChainReference) -> node.get lastNode)

                    //                        (Some (gunNamespace.get nodes.Head), nodes.Tail)
//                        ||> List.fold
//                                (fun result node ->
//                                    result
//                                    |> Option.map (fun result -> result.get node))
                    | _ ->
                        Dom.log
                            (fun () ->
                                $"gunAtomNode. Invalid username.
                                          atomPath={atomPath}
                                          user.is={JS.JSON.stringify gunNamespace.is}")

                        None)
