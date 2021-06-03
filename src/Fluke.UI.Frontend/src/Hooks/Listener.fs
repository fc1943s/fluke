namespace Fluke.UI.Frontend.Hooks

open Browser.Types
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Listener =
    let useKeyPress fn =
        Profiling.addTimestamp "useKeyPress.render"

        let isTesting = Recoil.useValue Atoms.isTesting

        let keyEvent =
            Recoil.useCallbackRef (fun setter (e: KeyboardEvent) -> async { do! fn setter e } |> Async.StartImmediate)

        if not isTesting then
            React.useListener.onKeyDown keyEvent
            React.useListener.onKeyUp keyEvent


    let useElementHover (elemRef: IRefValue<#HTMLElement option>) =
        let isHovered, setIsHovered = React.useState false
        let isTesting = Recoil.useValue Atoms.isTesting

        let setIsHoveredTrue = Recoil.useCallbackRef (fun _ _ -> setIsHovered true)
        let setIsHoveredFalse = Recoil.useCallbackRef (fun _ _ -> setIsHovered false)

        if not isTesting then
            React.useElementListener.onMouseEnter (elemRef, setIsHoveredTrue)
            React.useElementListener.onMouseLeave (elemRef, setIsHoveredFalse)

        isHovered
