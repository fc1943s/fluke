namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.React
open Fable.Core.JsInterop


module ModalForm =
    [<ReactComponent>]
    let ModalForm
        (input: {| Username: Username
                   TextKey: TextKey
                   Content: System.Guid option * (unit -> JS.Promise<unit>) * (unit -> Store.CallbackMethods) -> ReactElement |})
        =
        let isTesting = Store.useValue Atoms.isTesting
        let formIdFlag, setFormIdFlag = Store.useState (Atoms.User.formIdFlag (input.Username, input.TextKey))

        let formVisibleFlag, setFormVisibleFlag =
            Store.useState (Atoms.User.formVisibleFlag (input.Username, input.TextKey))

        let setter = Store.useSetter ()

        let onHide =
            Store.useCallbackRef
                (fun _ _ ->
                    promise {
                        setFormIdFlag None
                        setFormVisibleFlag false
                    })

        let content =
            React.useMemo (
                (fun () -> input.Content (formIdFlag, onHide, setter)),
                [|
                    box onHide
                    box formIdFlag
                    box input
                    box setter
                |]
            )

        Modal.Modal
            {|
                Props =
                    JS.newObj
                        (fun x ->
                            x.isOpen <- formVisibleFlag
                            x.onClose <- onHide

                            x.children <-
                                [
                                    Chakra.box
                                        (fun x -> if isTesting then x?``data-testid`` <- input.TextKey)
                                        [
                                            content
                                        ]
                                ])
            |}
