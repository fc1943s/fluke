namespace FsUi.Components

open Feliz
open Fable.Core
open FsStore
open FsStore.Hooks
open FsStore.Model
open FsUi.Bindings
open Fluke.UI.Frontend.State
open Fable.React
open Fluke.UI.Frontend.State.State


module ModalFlagTrigger =
    [<ReactComponent>]
    let ModalFlagTrigger
        (input: {| UIFlagType: UIFlagType
                   UIFlagValue: UIFlag
                   Trigger: (unit -> JS.Promise<unit>) -> Getter<_> * Setter<_> -> ReactElement |})
        =
        let onTrigger =
            Store.useCallbackRef
                (fun _ setter _ ->
                    promise {
                        Atom.set setter (Atoms.User.uiFlag input.UIFlagType) input.UIFlagValue
                        Atom.set setter (Atoms.User.uiVisibleFlag input.UIFlagType) true
                    })

        let store = Store.useStore ()
        let content, setContent = React.useState nothing

        React.useEffect (
            (fun () ->
                promise {
                    let! store = store ()
                    setContent (input.Trigger onTrigger store)
                }
                |> Promise.start),
            [|
                box setContent
                box input
                box onTrigger
                box store
            |]
        )

        content
