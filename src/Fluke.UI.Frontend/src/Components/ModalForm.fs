namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.React
open Fable.Core.JsInterop


module ModalForm =
    [<ReactComponent>]
    let ModalForm
        (input: {| UIFlagType: Atoms.UIFlagType
                   Content: Atoms.UIFlag * (unit -> JS.Promise<unit>) * (Store.GetFn * Store.SetFn) -> ReactElement |})
        =
        let isTesting = Store.useValue Store.Atoms.isTesting
        let formIdFlag, setFormIdFlag = Store.useState (Atoms.uiFlag input.UIFlagType)
        let formVisibleFlag, setFormVisibleFlag = Store.useState (Atoms.uiVisibleFlag input.UIFlagType)

        let onHide =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        setFormIdFlag Atoms.UIFlag.None
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
                box input.Content
                box callbacks

            |]
        )

        Modal.Modal (
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
        )
