namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.React


module ModalFlag =
    [<ReactComponent>]
    let ModalFlag
        (input: {| UIFlagType: UIFlagType
                   Content: UIFlag * (unit -> JS.Promise<unit>) -> Store.GetFn * Store.SetFn -> ReactElement |})
        =
        let formIdFlag, setFormIdFlag = Store.useState (Atoms.User.uiFlag input.UIFlagType)
        let formVisibleFlag, setFormVisibleFlag = Store.useState (Atoms.User.uiVisibleFlag input.UIFlagType)

        let onHide =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        setFormIdFlag UIFlag.None
                        setFormVisibleFlag false
                    }),
                [|
                    box setFormVisibleFlag
                    box setFormIdFlag
                |]
            )

        let callbacks = Store.useCallbacks ()
        let content, setContent = React.useState nothing

        React.useEffect (
            (fun () ->
                promise {
                    let! callbacks = callbacks ()
                    setContent (input.Content (formIdFlag, onHide) callbacks)
                }
                |> Promise.start),
            [|
                box setContent
                box onHide
                box formIdFlag
                box input
                box callbacks

            |]
        )

        if content = nothing then
            nothing
        else
            Modal.Modal (
                JS.newObj
                    (fun x ->
                        x.isOpen <- formVisibleFlag
                        x.onClose <- onHide

                        x.children <-
                            [
                                Chakra.box
                                    (fun x ->
                                        Chakra.setTestId x input.UIFlagType
                                        x.minWidth <- "max-content")
                                    [
                                        content
                                    ]
                            ])
            )

    [<ReactComponent>]
    let ModalFlagBundle
        (input: {| UIFlagType: UIFlagType
                   UIFlagValue: UIFlag
                   Trigger: (unit -> JS.Promise<unit>) -> Store.GetFn * Store.SetFn -> ReactElement
                   Content: (unit -> JS.Promise<unit>) -> Store.GetFn * Store.SetFn -> ReactElement |})
        =
        React.fragment [
            ModalFlagTrigger.ModalFlagTrigger
                {|
                    UIFlagType = input.UIFlagType
                    UIFlagValue = input.UIFlagValue
                    Trigger = input.Trigger
                |}

            ModalFlag
                {|
                    UIFlagType = input.UIFlagType
                    Content =
                        fun (uiFlag, onHide) (getter, setter) ->
                            if uiFlag <> input.UIFlagValue then
                                nothing
                            else
                                input.Content onHide (getter, setter)
                |}
        ]
