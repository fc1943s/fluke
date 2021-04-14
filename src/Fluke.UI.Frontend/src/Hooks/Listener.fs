namespace Fluke.UI.Frontend.Hooks

open Browser.Types
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Listener =
    let useKeyPress fn =
        Profiling.addTimestamp "useKeyPress.render"

        let keyEvent =
            Recoil.useCallbackRef (fun setter (e: KeyboardEvent) -> async { do! fn setter e } |> Async.StartImmediate)

        React.useListener.onKeyDown keyEvent
        React.useListener.onKeyUp keyEvent


    let useElementHover (elemRef: IRefValue<#HTMLElement option>) =
        let isHovered, setIsHovered = React.useState false

        let setIsHoveredTrue = Recoil.useCallbackRef (fun _ _ -> setIsHovered true)
        let setIsHoveredFalse = Recoil.useCallbackRef (fun _ _ -> setIsHovered false)

        React.useElementListener.onMouseEnter (elemRef, setIsHoveredTrue)
        React.useElementListener.onMouseLeave (elemRef, setIsHoveredFalse)

        isHovered
