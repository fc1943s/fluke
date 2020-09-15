namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared.Model


module CellStatusUserIndicatorComponent =
    let render =
        React.memo (fun (input: {| User: User |}) ->
            Html.div
                [
                    prop.classes [
                        Css.userIndicator
                        match input.User with
                        | { Color = UserColor.Blue } -> Css.bottomRightBlueIndicator
                        | { Color = UserColor.Pink } -> Css.bottomRightPinkIndicator
                        | _ -> ()
                    ]
                ])
