namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.React
open Fluke.UI.Frontend.State.State


module ModalFlag =
    [<ReactComponent>]
    let ModalFlag
        (input: {| UIFlagType: UIFlagType
                   Content: UIFlag * (unit -> JS.Promise<unit>) -> Store.GetFn * Store.SetFn -> ReactElement |})
        =
        let uiFlag, setUIFlag= Store.useState (Atoms.User.uiFlag input.UIFlagType)
        let uiVisibleFlag, setUIVisibleFlag = Store.useState (Atoms.User.uiVisibleFlag input.UIFlagType)

        let onHide =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        setUIFlag UIFlag.None
                        setUIVisibleFlag false
                    }),
                [|
                    box setUIVisibleFlag
                    box setUIFlag
                |]
            )

        let callbacks = Store.useCallbacks ()
        let content, setContent = React.useState nothing

        React.useEffect (
            (fun () ->
                promise {
                    let! callbacks = callbacks ()
                    setContent (input.Content (uiFlag, onHide) callbacks)
                }
                |> Promise.start),
            [|
                box setContent
                box onHide
                box uiFlag
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
                        x.isOpen <- uiVisibleFlag
                        x.onClose <- onHide

                        x.children <-
                            [
                                UI.box
                                    (fun x ->
                                        UI.setTestId x input.UIFlagType
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
