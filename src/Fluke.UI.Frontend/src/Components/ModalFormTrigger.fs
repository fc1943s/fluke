namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.React


module ModalFormTrigger =
    [<ReactComponent>]
    let ModalFormTrigger
        (input: {| Username: Username
                   UIFlagType: Atoms.User.UIFlagType
                   UIFlagValue: Atoms.User.UIFlag
                   Trigger: (unit -> JS.Promise<unit>) -> Jotai.GetFn * Jotai.SetFn -> ReactElement |})
        =
        let onTrigger =
            Store.useCallback (
                (fun _get set _ ->
                    promise {
                        Atoms.setAtomValue
                            set
                            (Atoms.User.uiFlag (input.Username, input.UIFlagType))
                            (fun _ -> input.UIFlagValue)

                        Atoms.setAtomValue
                            set
                            (Atoms.User.uiVisibleFlag (input.Username, input.UIFlagType))
                            (fun _ -> true)
                    }),
                [||]
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
                box input
                box onTrigger
                box callbacks
            |]
        )

        content
