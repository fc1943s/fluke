namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module CellBorderComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username; Date: FlukeDate |}) ->
            let weekStart = Recoil.useValue (Recoil.Atoms.User.weekStart input.Username)

            match (weekStart, input.Date) with
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
