namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.React


module ModalFlagTrigger =
    [<ReactComponent>]
    let ModalFlagTrigger
        (input: {| UIFlagType: UIFlagType
                   UIFlagValue: UIFlag
                   Trigger: (unit -> JS.Promise<unit>) -> Store.GetFn * Store.SetFn -> ReactElement |})
        =
        let onTrigger =
            Store.useCallback (
                (fun _ setter _ ->
                    promise {
                        Store.set setter (Atoms.User.uiFlag input.UIFlagType) input.UIFlagValue
                        Store.set setter (Atoms.User.uiVisibleFlag input.UIFlagType) true
                    }),
                [|
                    box input.UIFlagType
                    box input.UIFlagValue
                |]
            )

        let callbacks = Store.useCallbacks ()
        let content, setContent = React.useState nothing

        React.useEffect (
            (fun () ->
                promise {
                    let! callbacks = callbacks ()
                    setContent (input.Trigger onTrigger callbacks)
                }
                |> Promise.start),
            [|
                box setContent
                box input
                box onTrigger
                box callbacks
            |]
        )

        content
