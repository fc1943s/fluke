namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.State
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
        let isToday = Recoil.useValue (Selectors.FlukeDate.isToday input.Date)
        let hasSelection = Recoil.useValue (Selectors.FlukeDate.hasSelection input.Date)
        let weekStart = Recoil.useValue (Atoms.User.weekStart input.Username)

        Chakra.box
            (fun x ->
                x.color <-
                    if hasSelection then "#ff5656"
                    elif isToday then "#777"
                    else null

                x.borderLeftWidth <-
                    match (weekStart, input.Date) with
                    | StartOfMonth
                    | StartOfWeek -> "1px"
                    | _ -> null

                x.borderLeftColor <-
                    match (weekStart, input.Date) with
                    | StartOfMonth -> "#ffffff3d"
                    | StartOfWeek -> "#222"
                    | _ -> null

                x.height <- "17px"
                x.width <- "17px"
                x.lineHeight <- "17px"
                x.textAlign <- "center")
            [
                str <| String.toLower input.Label
            ]
