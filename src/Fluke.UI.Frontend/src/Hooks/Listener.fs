namespace Fluke.UI.Frontend.Hooks

open Browser.Types
open Feliz.Recoil
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fable.Core


module Listener =
    let useKeyPress (fn: CallbackMethods -> KeyboardEvent -> Async<unit>) =
        Profiling.addTimestamp "useKeyPress.render"

        let keyEvent =
            Recoil.useCallbackRef
                (fun setter (e: KeyboardEvent) ->
                    fn setter e
                    |> Async.StartAsPromise
                    |> Promise.start)

        React.useListener.onKeyDown keyEvent
        React.useListener.onKeyUp keyEvent


    let useElementHover (elemRef: IRefValue<#HTMLElement option>) =
        let isHovered, setIsHovered = React.useState false

        let setIsHoveredTrue = Recoil.useCallbackRef (fun _ _ -> setIsHovered true)
        let setIsHoveredFalse = Recoil.useCallbackRef (fun _ _ -> setIsHovered false)

        React.useElementListener.onMouseEnter (elemRef, setIsHoveredTrue)
        React.useElementListener.onMouseLeave (elemRef, setIsHoveredFalse)
        isHovered
