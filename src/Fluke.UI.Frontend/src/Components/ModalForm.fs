namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State

module ModalForm =

    [<ReactComponent>]
    let ModalFormTrigger
        (input: {| Username: Username
                   TextKeyValue: System.Guid option
                   TextKey: TextKey
                   Trigger: (unit -> unit) -> ReactElement |})
        =
        let setFormIdFlag = Recoil.useSetState (Atoms.User.formIdFlag (input.Username, input.TextKey))

        let setFormVisibleFlag = Recoil.useSetState (Atoms.User.formVisibleFlag (input.Username, input.TextKey))

        input.Trigger
            (fun () ->
                setFormIdFlag input.TextKeyValue
                setFormVisibleFlag true)

    [<ReactComponent>]
    let ModalForm
        (input: {| Username: Username
                   TextKey: TextKey
                   Content: System.Guid option * (unit -> unit) * (unit -> CallbackMethods) -> ReactElement |})
        =
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
                                    input.Content (formIdFlag, (fun () -> setFormVisibleFlag false), setter)
                                ])
            |}
