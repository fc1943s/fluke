namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Feliz.Recoil
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
                   Trigger: (unit -> unit) -> (unit -> CallbackMethods) -> ReactElement |})
        =
        let onTrigger =
            Recoil.useCallbackRef
                (fun setter ->
                    setter.set (Atoms.User.formIdFlag (input.Username, input.TextKey), input.TextKeyValue)
                    setter.set (Atoms.User.formVisibleFlag (input.Username, input.TextKey), true))

        let setter = Recoil.useCallbackRef id

        let content =
            React.useMemo (
                (fun () -> input.Trigger (fun () -> JS.setTimeout onTrigger 0 |> ignore) setter),
                [|
                    box input
                    box onTrigger
                    box setter
                |]
            )

        content
