namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
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
        let isToday = Store.useValueLoadableDefault (Selectors.FlukeDate.isToday input.Date) false
        let hasCellSelection = Store.useValueLoadableDefault (Selectors.FlukeDate.hasCellSelection input.Date) false
        let weekStart = Store.useValue (Atoms.User.weekStart input.Username)
        let cellSize = Store.useValue (Atoms.User.cellSize input.Username)

        Chakra.box
            (fun x ->
                x.color <-
                    if hasCellSelection then "#ff5656"
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

                x.whiteSpace <- "nowrap"
                x.height <- $"{cellSize}px"
                x.width <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"
                x.textAlign <- "center")
            [
                str (String.toLower input.Label)
            ]
