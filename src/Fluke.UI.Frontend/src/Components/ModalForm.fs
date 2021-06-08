namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.React


module ModalForm =
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

        let hide =
            Recoil.useCallbackRef
                (fun _ ->
                    setFormIdFlag None
                    setFormVisibleFlag false)

        let content =
            React.useMemo (
                (fun () -> input.Content (formIdFlag, hide, setter)),
                [|
                    box formIdFlag
                    box input
                    box setFormVisibleFlag
                    box setter
                |]
            )

        Modal.Modal
            {|
                Props =
                    JS.newObj
                        (fun x ->
                            x.isOpen <- formVisibleFlag

                            x.onClose <- fun () -> promise { hide () }

                            x.children <-
                                [
                                    Chakra.box
                                        (fun x ->
                                            if isTesting then
                                                (JsInterop.op_Dynamic x "data-testid") <- input.TextKey)
                                        [
                                            content
                                        ]
                                ])
            |}
