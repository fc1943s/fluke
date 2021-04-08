namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared

module Day =

    open Domain.UserInteraction

    [<ReactComponent>]
    let Day
        (input: {| Username: Username
                   Date: FlukeDate
                   Label: string |})
        =
        let isToday = Recoil.useValue (Recoil.Selectors.FlukeDate.isToday input.Date)
        let hasSelection = Recoil.useValue (Recoil.Selectors.FlukeDate.hasSelection input.Date)
        let weekStart = Recoil.useValue (Recoil.Atoms.User.weekStart input.Username)

        Chakra.box
            {|
                color =
                    if hasSelection then "#ff5656"
                    elif isToday then "#777"
                    else ""
                borderLeft =
                    match (weekStart, input.Date) with
                    | StartOfMonth -> "1px solid #ffffff3d"
                    | StartOfWeek -> "1px solid #222"
                    | _ -> ""
                height = "17px"
                width = "17px"
                lineHeight = "17px"
                textAlign = "center"
            |}
            [
                str <| String.toLower input.Label
            ]
