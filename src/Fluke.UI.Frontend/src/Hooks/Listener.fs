namespace Fluke.UI.Frontend.Hooks

open Browser.Types
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open Fluke.UI.Frontend


module Listener =
    let useKeyPress fn =
        Profiling.addTimestamp "useKeyPress.render"

        let keyEvent =
            Recoil.useCallbackRef (fun setter (e: KeyboardEvent) -> async { do! fn setter e } |> Async.StartImmediate)

        React.useListener.onKeyDown keyEvent
        React.useListener.onKeyUp keyEvent


    let useElementHover (elemRef: IRefValue<#HTMLElement option>) =
        let isHovered, setIsHovered = React.useState false

        React.useElementListener.onMouseEnter (elemRef, (fun _ -> setIsHovered true))
        React.useElementListener.onMouseLeave (elemRef, (fun _ -> setIsHovered false))

        isHovered
