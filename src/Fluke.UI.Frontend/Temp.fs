namespace Fluke.UI.Frontend

open Browser.Types
open Feliz
open Feliz.UseListener
open Fable.Core
open FSharpPlus
open Suigetsu.Core
open Fluke.UI.Frontend

module Temp =
    module UseListener =
        let onElementHover (elemRef: IRefValue<#HTMLElement option>) =
            let isHovered, setIsHovered = React.useState false

            React.useElementListener.onMouseEnter (elemRef, (fun _ -> setIsHovered true))
            React.useElementListener.onMouseLeave (elemRef, (fun _ -> setIsHovered false))

            React.useMemo
                ((fun () -> isHovered),
                 [|
                     isHovered :> obj
                 |])

    module Sound =
        let playDing () =
            [
                0
                1400
            ]
            |> List.map (JS.setTimeout (fun () -> Ext.playSound "../sounds/ding.wav"))
            |> ignore

        let playTick () = Ext.playSound "../sounds/tick.wav"
