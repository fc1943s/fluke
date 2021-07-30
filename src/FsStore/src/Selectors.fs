namespace FsStore

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

    let connect hubUrl =
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
                                    Some 1000
                        }
                    )
                    .onReconnecting(fun ex -> Dom.log (fun () -> $"SignalR.connect(). onReconnecting ex={ex}"))
                    .onReconnected(fun ex -> Dom.log (fun () -> $"SignalR.connect(). onReconnected ex={ex}"))
                    .onClose(fun ex -> Dom.log (fun () -> $"SignalR.connect(). onClose ex={ex}"))
                    .configureLogging(LogLevel.Debug)
                    .onMessage (
                        function
                        | Sync.Response.ConnectResult -> Dom.log (fun () -> "Sync.Response.ConnectResult")
                        | Sync.Response.GetResult value -> Dom.log (fun () -> $"Sync.Response.GetResult value={value}")
                        | Sync.Response.SetResult result ->
                            Dom.log (fun () -> $"Sync.Response.SetResult result={result}")
                        | Sync.Response.FilterResult keys ->
                            Dom.log (fun () -> $"Sync.Response.FilterResult keys={keys}")
                    ))

module Selectors =
    let rec deviceInfo = Store.readSelector $"{nameof deviceInfo}" (fun _ -> Dom.deviceInfo)

    let rec logger =
        Store.readSelector
            $"{nameof logger}"
            (fun getter ->
                let logLevel = Store.value getter Atoms.logLevel
                Logger.Create logLevel)

    let rec hub =
        Store.readSelector
            $"{nameof hub}"
            (fun getter ->
                let _hubTrigger = Store.value getter Atoms.hubTrigger
                let hubUrl = Store.value getter Atoms.hubUrl

                match hubUrl with
                | Some (String.ValidString hubUrl) ->
                    let hub = SignalR.connect hubUrl
                    hub.startNow ()
                    Some hub
                | _ -> None)


    let rec gun =
        Store.readSelectorFamily
            $"{nameof gun}"
            (fun gunPeers getter ->
                let isTesting = Store.value getter Atoms.isTesting

                let peers =
                    gunPeers
                    |> Array.filter (String.IsNullOrWhiteSpace >> not)

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
                                Gun.GunProps.peers = Some peers
                                Gun.GunProps.radisk = Some true
                                Gun.GunProps.localStorage = Some false
                                Gun.GunProps.multicast = None
                            }

                printfn $"Gun selector. peers={peers}. gun={gun} returning..."

                gun)


    let rec gunNamespace =
        Store.readSelector
            $"{nameof gunNamespace}"
            (fun getter ->
                let gunPeers = Store.value getter Atoms.gunPeers
                let _gunTrigger = Store.value getter Atoms.gunTrigger
                let gun = Store.value getter (gun (gunPeers |> Option.defaultValue [||]))
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
            $"{nameof gunAtomNode}"
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
                | _ ->
                    Dom.log
                        (fun () ->
                            $"gunAtomNode. Invalid username.
                                      atomPath={atomPath}
                                      user.is={JS.JSON.stringify gunNamespace.is}")

                    None)

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
                $"{nameof valueWrapper}"
                None
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
            $"{nameof atomAccessors}"
            (fun getter ->
                let value = Store.value getter valueWrapper
                let accessors = Store.value getter accessorsAtom

                Dom.log
                    (fun () ->
                        $"atomAccessors.selfWrapper.get() value={value} accessors={accessors.IsSome} {getBaseInfo ()}")

                accessors)
