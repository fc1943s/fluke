namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared.Model


module CellBorderComponent =
    let render =
        React.memo (fun (input: {| Date: FlukeDate |}) ->
            let user = Recoil.useValue Recoil.Selectors.user

            match user with
            | None -> str "No user found"
            | Some user ->
                match (user.WeekStart, input.Date) with
                | StartOfMonth -> Some Css.cellStartMonth
                | StartOfWeek -> Some Css.cellStartWeek
                | _ -> None
                |> Option.map (fun className ->
                    Html.div
                        [
                            prop.classes [
                                Css.cellSquare
                                className
                            ]
                        ])
                |> Option.defaultValue nothing)
