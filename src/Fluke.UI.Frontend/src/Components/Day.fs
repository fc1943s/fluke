namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.TempUI
open Fluke.UI.Frontend.State
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.Shared

module Day =
    open Domain.UserInteraction


    [<ReactComponent>]
    let Day dayFormat dateAtom =
        let dateId = Store.useValue dateAtom
        let isToday = Store.useValue (Selectors.FlukeDate.isToday dateId)
        let hasCellSelection = Store.useValue (Selectors.FlukeDate.hasCellSelection dateId)
        let weekStart = Store.useValue Atoms.User.weekStart
        let cellHeight = Store.useValue Atoms.User.cellHeight
        let cellWidth = Store.useValue Atoms.User.cellWidth

        Ui.box
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
                x.height <- $"{cellHeight}px"
                x.width <- $"{cellWidth}px"
                x.lineHeight <- $"{cellHeight}px"
                x.textAlign <- "center")
            [
                dateId
                |> FlukeDate.Format dayFormat
                |> String.toLower
                |> str
            ]
