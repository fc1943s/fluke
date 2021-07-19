namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.TempUI
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared

module Day =
    open Domain.UserInteraction


    [<ReactComponent>]
    let Day dayFormat dateIdAtom =
        let dateId = Store.useValue dateIdAtom
        let isToday = Store.useValue (Selectors.DateId.isToday dateId)
        let hasCellSelection = Store.useValue (Selectors.DateId.hasCellSelection dateId)
        let weekStart = Store.useValue Atoms.User.weekStart
        let cellSize = Store.useValue Atoms.User.cellSize

        UI.box
            (fun x ->
                x.color <-
                    if hasCellSelection then "#ff5656"
                    elif isToday then "#777"
                    else null

                x.borderLeftWidth <-
                    match (weekStart, dateId) with
                    | StartOfMonth
                    | StartOfWeek -> "1px"
                    | _ -> null

                x.borderLeftColor <-
                    match (weekStart, dateId) with
                    | StartOfMonth -> "#ffffff3d"
                    | StartOfWeek -> "#222"
                    | _ -> null

                x.whiteSpace <- "nowrap"
                x.height <- $"{cellSize}px"
                x.width <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"
                x.textAlign <- "center")
            [
                dateId
                |> DateId.Format dayFormat
                |> String.toLower
                |> str
            ]
