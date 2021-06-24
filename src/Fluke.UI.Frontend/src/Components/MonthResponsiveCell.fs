namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fable.DateFunctions
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module MonthResponsiveCell =
    open Domain.UserInteraction

    [<ReactComponent>]
    let MonthResponsiveCell
        (input: {| Date: FlukeDate
                   Props: Chakra.IChakraProps -> unit |})
        =
        let weekStart = Store.useValue Atoms.weekStart
        let cellSize = Store.useValue Atoms.cellSize

        let month = (input.Date |> FlukeDate.DateTime).Format "MMM"

        Chakra.box
            (fun x ->
                x.whiteSpace <- "nowrap"
                x.textAlign <- "center"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"

                x.borderLeftWidth <-
                    match (weekStart, input.Date) with
                    | StartOfMonth -> "1px"
                    | _ -> null

                x.borderLeftColor <-
                    match (weekStart, input.Date) with
                    | StartOfMonth -> "#ffffff3d"
                    | _ -> null

                input.Props x)
            [
                str month
            ]
