namespace Fluke.UI.Frontend.Components

open FsStore
open FsCore
open Fable.React
open Feliz
open FsJs
open FsStore.Hooks
open FsStore.Model
open FsStore.State
open FsUi.Bindings
open FsUi.Hooks
open FsUi.State
open Fluke.UI.Frontend.State


module MessageConsumer =
    [<ReactComponent>]
    let MessageConsumer messageIdAtom =
        let logger = Store.useValue Selectors.Store.logger
        let deviceInfo = Store.useValue Selectors.Store.deviceInfo
        let appState = Store.useValue (Atoms.Device.appState deviceInfo.DeviceId)
        let consumeCommands = Store.useCallbackRef (Engine.consumeCommands Messaging.appUpdate appState)
        let messageId = Store.useValue messageIdAtom
        let appMessage = Store.useValue (Atoms.Message.appMessage messageId)
        let ack, setAck = Store.useState (Atoms.Message.ack messageId)

        let inline getLocals () =
            $"messageId={messageId} ack={ack} appMessage={appMessage} {getLocals ()}"

        Profiling.addTimestamp (fun () -> $"{nameof Fluke} | MessageConsumer [ render ]") getLocals

        let setHydratePending = Store.useSetState Atoms.Session.hydrateTemplatesPending

        React.useEffect (
            (fun () ->
                promise {
                    match ack with
                    | Some false ->
                        match appMessage with
                        | Message.Event event ->
                            match event with
                            | AppEvent.UserRegistered alias ->
                                let getLocals () = $"alias={alias} {getLocals ()}"

                                Profiling.addTimestamp
                                    (fun () -> $"{nameof Fluke} | MessageConsumer [ useEffect ] will hydrate template")
                                    getLocals

                                setHydratePending true
                                setAck (Some true)
                            | _ -> ()
                        | _ -> ()
                    | _ -> ()
                }
                |> Promise.start),
            [|
                box logger
                box messageId
                box consumeCommands
                box appMessage
                box ack
                box setAck
            |]
        )

        nothing
