namespace FsStore

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core
open System
open FsCore
open FsCore.Model
open FsStore.Bindings.Jotai
open FsStore.Model
open FsBeacon.Shared
open Microsoft.FSharp.Core.Operators
open FsJs
open FsStore.Bindings

#nowarn "40"



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


    module rec Gun =
        let collection = Collection (nameof Gun)

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
                            Bindings.Gun.gun
                                {
                                    Gun.GunProps.peers = None
                                    Gun.GunProps.radisk = Some false
                                    Gun.GunProps.localStorage = Some false
                                    Gun.GunProps.multicast = None
                                }
                        else
                            Bindings.Gun.gun
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



    module Hub =
        open Fable.SignalR

        let hubSubscriptionMap = Dictionary<string * string * string, string [] -> unit> ()


        let rec hubConnection =
            Store.readSelector
                FsStore.root
                (nameof hubConnection)
                (fun getter ->
                    let timeout = 2000

                    let hubUrl = Store.value getter Atoms.hubUrl

                    printfn $"hub connection selector. hubUrl={hubUrl}"

                    match hubUrl with
                    | Some (String.ValidString hubUrl) ->
                        let connection =
                            SignalR.connect<Sync.Request, Sync.Request, obj, Sync.Response, Sync.Response>
                                (fun hub ->
                                    hub
                                        .withUrl($"{hubUrl}{Sync.endpoint}")
                                        //                    .useMessagePack()
                                        .withAutomaticReconnect(
                                            {
                                                nextRetryDelayInMilliseconds =
                                                    fun _context ->
                                                        Dom.log (fun () -> "SignalR.connect(). withAutomaticReconnect")
                                                        Some timeout
                                            }
                                        )
                                        .onReconnecting(fun ex ->
                                            Dom.log (fun () -> $"SignalR.connect(). onReconnecting ex={ex}"))
                                        .onReconnected(fun ex ->
                                            Dom.log (fun () -> $"SignalR.connect(). onReconnected ex={ex}"))
                                        .onClose(fun ex -> Dom.log (fun () -> $"SignalR.connect(). onClose ex={ex}"))
                                        .configureLogging(LogLevel.Debug)
                                        .onMessage (fun msg ->
                                            match msg with
                                            | Sync.Response.ConnectResult ->
                                                Dom.log (fun () -> "Sync.Response.ConnectResult")
                                            | Sync.Response.SetResult result ->
                                                Dom.log (fun () -> $"Sync.Response.SetResult result={result}")
                                            | Sync.Response.GetResult value ->
                                                Dom.log (fun () -> $"Sync.Response.GetResult value={value}")
                                            | Sync.Response.GetStream (key, value) ->
                                                Dom.log (fun () -> $"Sync.Response.GetStream key={key} value={value}")
                                            | Sync.Response.FilterResult keys ->
                                                Dom.log (fun () -> $"Sync.Response.FilterResult keys={keys}")
                                            | Sync.Response.FilterStream (key, keys) ->
                                                Dom.log (fun () -> $"Sync.Response.FilterStream key={key} keys={keys}")

                                            match msg with
                                            | Sync.Response.FilterStream (key, keys) ->
                                                match hubSubscriptionMap.TryGetValue key with
                                                | true, fn ->
                                                    Dom.log (fun () -> $"Selectors.hub onMsg msg={msg}. triggering ")
                                                    fn keys
                                                | _ ->
                                                    Dom.log
                                                        (fun () ->
                                                            $"Selectors.hub onMsg msg={msg}. skipping. not in map ")
                                            | _ ->
                                                Dom.log
                                                    (fun () -> $"Selectors.hub onMsg msg={msg}. skipping. not handled ")))

                        connection.startNow ()
                        Some connection
                    | _ -> None)



        let rec hub =
            Store.readSelector
                FsStore.root
                (nameof hub)
                (fun getter ->
                    let hubTrigger = Store.value getter Atoms.hubTrigger
                    let hubConnection = Store.value getter hubConnection

                    match hubConnection with
                    | Some hubConnection ->
                        printfn $"hub selector. hubTrigger={hubTrigger} hubConnection.connectionId={hubConnection.connectionId}"
                        Some hubConnection
//                        match Store.value getter atomAccessors with
//                        | Some (getter, setter) ->
//                            Some hubConnection
//                        | None -> None
                    | _ -> None)
