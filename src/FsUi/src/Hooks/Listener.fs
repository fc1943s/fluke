namespace FsUi.Hooks

open Browser.Types
open Feliz
open FsStore
open FsStore.Model
open FsUi.Bindings
open Fable.Core
open Feliz.UseListener
open FsJs


module Listener =
    let inline useKeyPress keys (fn: GetFn -> SetFn -> KeyboardEvent -> JS.Promise<unit>) =
        Profiling.addTimestamp "useKeyPress.render"

        let keyEvent = Store.useCallbackRef fn

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


    let inline useElementHover (elemRef: IRefValue<#HTMLElement option>) =
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
