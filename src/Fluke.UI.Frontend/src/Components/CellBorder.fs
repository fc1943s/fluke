namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module CellBorder =
    open Domain.UserInteraction

    [<ReactComponent>]
    let CellBorder (input: {| Username: Username; Date: FlukeDate |}) =
        let weekStart = Recoil.useValue (Recoil.Atoms.User.weekStart input.Username)

        match (weekStart, input.Date) with
        | StartOfMonth -> Some "1px solid #ffffff3d"
        | StartOfWeek -> Some "1px solid #222"
        | _ -> None
        |> Option.map (fun borderLeft ->
            Chakra.box
                {|
                    position = "absolute"
                    top = 0
                    left = 0
                    bottom = 0
                    borderLeft = borderLeft
                |}
                [])
        |> Option.defaultValue nothing
