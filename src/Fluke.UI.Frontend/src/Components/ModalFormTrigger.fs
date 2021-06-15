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
                   TextKeyValue: System.Guid option
                   TextKey: TextKey
                   Trigger: (unit -> JS.Promise<unit>) -> (unit -> Store.CallbackMethods) -> ReactElement |})
        =
        let onTrigger =
            Store.useCallbackRef
                (fun setter _ ->
                    promise {
                        setter.set (
                            Atoms.User.formIdFlag (input.Username, input.TextKey),
                            fun _ -> input.TextKeyValue
                        )

                        setter.set (Atoms.User.formVisibleFlag (input.Username, input.TextKey), (fun _ -> true))
                    })

        let setter = Store.useSetter ()

        let content =
            React.useMemo (
                (fun () -> input.Trigger onTrigger setter),
                [|
                    box input
                    box onTrigger
                    box setter
                |]
            )

        content
