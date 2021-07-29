namespace FsStore

open Fable.Core.JsInterop
open Fable.Core
open System
open Fable.SignalR
open FsCore
open FsCore.Model
open FsStore.Bindings.Jotai
open FsStore.Shared
open Microsoft.FSharp.Core.Operators
open FsJs
open FsStore.Bindings


module Selectors =
    let rec deviceInfo = Store.readSelector ($"{nameof deviceInfo}", (fun _ -> Dom.deviceInfo))

    let rec log =
        Store.readSelector (
            $"{nameof log}",
            (fun getter ->
                let logLevel = Store.value getter Atoms.logLevel

                let log logLevel' (fn: unit -> string) =
                    if logLevel <= logLevel' then
                        let result = fn ()

                        if result |> Option.ofObjUnbox |> Option.isSome then
                            printfn $"[{logLevel}] {result}"

                {|
                    trace = log Model.LogLevel.Trace
                    debug = log Model.LogLevel.Debug
                    info = log Model.LogLevel.Information
                    warning = log Model.LogLevel.Warning
                    error = log Model.LogLevel.Error
                |})
        )

    let rec hub =
        Store.readSelector (
            $"{nameof hub}",
            (fun getter ->
                let apiUrl = Store.value getter Atoms.apiUrl

                match apiUrl with
                | Some (String.ValidString apiUrl) ->
                    let hub =
                        SignalR.connect<Sync.Request, Sync.Request, obj, Sync.Response, Sync.Response>
                            (fun hub ->
                                hub
                                    .withUrl($"{apiUrl}{Sync.endpoint}")
                                    //                                        .useMessagePack()
                                    .withAutomaticReconnect(
                                        {
                                            nextRetryDelayInMilliseconds =
                                                fun _context ->
                                                    Dom.log (fun () -> "SignalR.connect(). withAutomaticReconnect")
                                                    Some 1000
                                        }
                                    )
                                    .onReconnecting(fun ex ->
                                        Dom.log (fun () -> $"SignalR.connect(). onReconnecting ex={ex}"))
                                    .onReconnected(fun ex ->
                                        Dom.log (fun () -> $"SignalR.connect(). onReconnected ex={ex}"))
                                    .onClose(fun ex -> Dom.log (fun () -> $"SignalR.connect(). onClose ex={ex}"))
                                    .configureLogging(LogLevel.Debug)
                                    .onMessage (
                                        function
                                        | Sync.Response.ConnectResult ->
                                            Dom.log (fun () -> "Api.Response.ConnectResult")
                                        | Sync.Response.GetResult value ->
                                            Dom.log (fun () -> $"Api.Response.GetResult value={value}")
                                        | Sync.Response.SetResult result ->
                                            Dom.log (fun () -> $"Api.Response.SetResult result={result}")
                                        | Sync.Response.FilterResult keys ->
                                            Dom.log (fun () -> $"Api.Response.FilterResult keys={keys}")
                                    ))

                    hub.startNow ()
                    Some hub
                | _ -> None)
        )

    let rec gun =
        Store.readSelectorFamily (
            $"{nameof gun}",
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
        )

    let rec gunNamespace =
        Store.readSelector (
            $"{nameof gunNamespace}",
            fun getter ->
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

                user
        )

    let rec gunAtomNode =
        Store.selectAtomFamily (
            $"{nameof gunAtomNode}",
            gunNamespace,
            (fun (username, atomPath) gunNamespace ->
                match gunNamespace.is with
                | Some { alias = Some username' } when username' = (username |> Username.ValueOrDefault) ->
                    let nodes =
                        atomPath
                        |> AtomPath.Value
                        |> String.split "/"
                        |> Array.toList

                    (Some (gunNamespace.get nodes.Head), nodes.Tail)
                    ||> List.fold
                            (fun result node ->
                                result
                                |> Option.map (fun result -> result.get node))
                | _ ->
                    Dom.log
                        (fun () ->
                            $"gunAtomNode. Invalid username.
                                      atomPath={atomPath}
                                      user.is={JS.JSON.stringify gunNamespace.is}")

                    None)
        )
