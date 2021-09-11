namespace Fluke.UI.Frontend.Components

open FsStore
open FsCore
open Feliz
open FsJs
open FsStore.Hooks
open FsUi.Bindings
open FsUi.Hooks


module MessagesListener =
    [<ReactComponent>]
    let MessagesListener () =
        Profiling.addTimestamp (fun () -> $"{nameof Fluke} | MessagesListener [ render ] ") getLocals
        let messageIdAtoms = Store.useValue Subscriptions.messageIdAtoms

        React.fragment [
            yield!
                messageIdAtoms
                |> Array.map MessageConsumer.MessageConsumer
        ]
