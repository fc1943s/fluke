namespace Fluke.UI.Frontend.Hooks

open Browser.Types
open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Feliz.UseListener


module Listener =
    let useKeyPress keys (fn: Store.GetFn -> Store.SetFn -> KeyboardEvent -> JS.Promise<unit>) =
        let fnCallback = React.useCallbackRef (fun (get, set, e) -> fn get set e)
        Profiling.addTimestamp "useKeyPress.render"

        let keyEvent =
            Store.useCallback (
                (fun get set e -> fnCallback (get, set, e)),
                [|
                    box fnCallback
                |]
            )

        Rooks.useKey
            keys
            (fun e -> keyEvent e |> Promise.start)
            {|
                eventTypes =
                    [|
                        "keydown"
                        "keyup"
                    |]
            |}


    let useElementHover (elemRef: IRefValue<#HTMLElement option>) =
        let isHovered, setIsHovered = React.useState false

        let setIsHoveredTrue =
            React.useCallback (
                (fun _ -> setIsHovered true),
                [|
                    box setIsHovered
                |]
            )

        let setIsHoveredFalse =
            React.useCallback (
                (fun _ -> setIsHovered false),
                [|
                    box setIsHovered
                |]
            )

        React.useElementListener.onMouseEnter (elemRef, setIsHoveredTrue)
        React.useElementListener.onMouseLeave (elemRef, setIsHoveredFalse)

        isHovered
