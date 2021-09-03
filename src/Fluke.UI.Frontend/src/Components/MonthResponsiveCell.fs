namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared
open Fluke.UI.Frontend.TempUI
open Fluke.UI.Frontend.State
open FsStore
open FsStore.Hooks
open FsStore.Model
open FsUi.Bindings


module MonthResponsiveCell =
    open Domain.UserInteraction

    [<ReactComponent>]
    let MonthResponsiveCell (dateAtom: AtomConfig<FlukeDate>) (props: Ui.IChakraProps -> unit) =
        let date = Store.useValue dateAtom
        let weekStart = Store.useValue Atoms.User.weekStart
        let cellSize = Store.useValue Atoms.User.cellSize

        Ui.box
            (fun x ->
                x.whiteSpace <- "nowrap"
                x.textAlign <- "center"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"

                x.borderLeftWidth <-
                    match (weekStart, date) with
                    | StartOfMonth -> "1px"
                    | _ -> null

                x.borderLeftColor <-
                    match (weekStart, date) with
                    | StartOfMonth -> "#ffffff3d"
                    | _ -> null

                props x)
            [
                date |> FlukeDate.Format DateIdFormat.Month |> str
            ]
