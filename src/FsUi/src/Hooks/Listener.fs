namespace FsUi.Hooks

open Browser.Types
open Feliz
open FsStore
open FsUi.Bindings
open Fable.Core
open Feliz.UseListener
open FsJs


module Listener =
    let useKeyPress keys (fn: Store.GetFn -> Store.SetFn -> KeyboardEvent -> JS.Promise<unit>) =
        let fnCallback = React.useCallbackRef (fun (getter, setter, e) -> fn getter setter e)
        Profiling.addTimestamp "useKeyPress.render"

        let keyEvent =
            Store.useCallback (
                (fun getter setter e -> fnCallback (getter, setter, e)),
                [|
                    box fnCallback
                |]
            )

        Rooks.useKey
            keys
            (keyEvent >> Promise.start)
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
