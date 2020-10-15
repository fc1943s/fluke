namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module CellStatusUserIndicatorComponent =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| User: User |}) ->
            Chakra.box
                {|
                    className =
                        [
                            Css.userIndicator
                            match input.User with
                            | { Color = UserColor.Blue } -> Css.bottomRightBlueIndicator
                            | { Color = UserColor.Pink } -> Css.bottomRightPinkIndicator
                            | _ -> ()
                        ]
                        |> String.concat " "
                |}
                [])
