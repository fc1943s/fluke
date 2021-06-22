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
                   UIFlagType: Atoms.User.UIFlagType
                   Content: Atoms.User.UIFlag * (unit -> JS.Promise<unit>) * (Jotai.GetFn * Jotai.SetFn) -> ReactElement |})
        =
        let isTesting = Store.useValue Atoms.isTesting
        let formIdFlag, setFormIdFlag = Store.useState (Atoms.User.uiFlag (input.Username, input.UIFlagType))

        let formVisibleFlag, setFormVisibleFlag =
            Store.useState (Atoms.User.uiVisibleFlag (input.Username, input.UIFlagType))


        let onHide =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        setFormIdFlag Atoms.User.UIFlag.None
                        setFormVisibleFlag false
                    }),
                [||]
            )

        let callbacks = Store.useCallbacks ()
        let content, setContent = React.useState nothing

        React.useEffect (
            (fun () ->
                promise {
                    let! callbacks = callbacks ()
                    setContent (input.Content (formIdFlag, onHide, callbacks))
                }
                |> Promise.start),
            [|
                box onHide
                box formIdFlag
                box input
                box callbacks

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
                                        (fun x -> if isTesting then x?``data-testid`` <- input.UIFlagType)
                                        [
                                            content
                                        ]
                                ])
            |}
