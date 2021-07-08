namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module CellSessionIndicator =
    open Domain.UserInteraction
    open Domain.State

    [<ReactComponent>]
    let CellSessionIndicator (status: CellStatus) (sessions: Session list) =
        UI.box
            (fun x ->
                x.fontSize <- "11px"

                x.color <-
                    match status with
                    | UserStatus (_, Completed) -> "#ccc"
                    | _ -> "#999"

                x.textShadow <- "0 0 2px #000")
            [
                match sessions.Length with
                | x when x > 0 -> str (string x)
                | _ -> nothing
            ]
