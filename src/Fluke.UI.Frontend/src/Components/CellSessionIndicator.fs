namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module CellSessionIndicator =
    open Domain.UserInteraction
    open Domain.State

    [<ReactComponent>]
    let CellSessionIndicator
        (input: {| Status: CellStatus
                   Sessions: TaskSession list |})
        =
        Chakra.box
            (fun x ->
                x.fontSize <- "11px"

                x.color <-
                    match input.Status with
                    | UserStatus (_, Completed) -> "#ccc"
                    | _ -> "#999"

                x.textShadow <- "0 0 2px #000")
            [
                match input.Sessions.Length with
                | x when x > 0 -> str (string x)
                | _ -> ()
            ]
