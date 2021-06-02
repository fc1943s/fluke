namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.React
open Fable.Core.JsInterop


module ModalForm =

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

        input.Trigger (fun () -> JS.setTimeout onTrigger 0 |> ignore) setter

    [<ReactComponent>]
    let ModalForm
        (input: {| Username: Username
                   TextKey: TextKey
                   Content: System.Guid option * (unit -> unit) * (unit -> CallbackMethods) -> ReactElement |})
        =
        let isTesting = Recoil.useValue Atoms.isTesting
        let formIdFlag, setFormIdFlag = Recoil.useState (Atoms.User.formIdFlag (input.Username, input.TextKey))

        let formVisibleFlag, setFormVisibleFlag =
            Recoil.useState (Atoms.User.formVisibleFlag (input.Username, input.TextKey))

        let setter = Recoil.useCallbackRef id

        Modal.Modal
            {|
                Props =
                    JS.newObj
                        (fun x ->
                            x.isOpen <- formVisibleFlag

                            x.onClose <-
                                fun () ->
                                    promise {
                                        setFormIdFlag None
                                        setFormVisibleFlag false
                                    }

                            x.children <-
                                [
                                    Chakra.box
                                        (fun x -> if isTesting then x?``data-testid`` <- input.TextKey)
                                        [
                                            input.Content (formIdFlag, (fun () -> setFormVisibleFlag false), setter)
                                        ]
                                ])
            |}
