namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module PositionUpdater =
    let render =
        React.memo (fun () ->
            let resetPosition = Recoil.useResetState Recoil.Selectors.position
            Scheduling.useScheduling Scheduling.Interval (60 * 1000) resetPosition

            nothing)
