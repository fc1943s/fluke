namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module CellSessionIndicatorComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Sessions: TaskSession list |}) ->
            Html.div [
                prop.classes [
                    Css.cellSquare
                    Css.sessionLengthIndicator
                ]
                prop.children
                    [
                        match input.Sessions.Length with
                        | x when x > 0 -> str (string x)
                        | _ -> ()
                    ]
            ])