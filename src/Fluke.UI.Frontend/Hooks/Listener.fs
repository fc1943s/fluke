namespace Fluke.UI.Frontend.Hooks

open Browser.Types
open Feliz
open Feliz.UseListener


module Listener =
    let useElementHover (elemRef: IRefValue<#HTMLElement option>) =
        let isHovered, setIsHovered = React.useState false

        React.useElementListener.onMouseEnter (elemRef, (fun _ -> setIsHovered true))
        React.useElementListener.onMouseLeave (elemRef, (fun _ -> setIsHovered false))

        React.useMemo
            ((fun () -> isHovered),
             [|
                 isHovered :> obj
             |])
