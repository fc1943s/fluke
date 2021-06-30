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
    let MonthResponsiveCell (date: FlukeDate) (props: Chakra.IChakraProps -> unit) =
        let weekStart = Store.useValue Atoms.User.weekStart
        let cellSize = Store.useValue Atoms.User.cellSize

        let month = (date |> FlukeDate.DateTime).Format "MMM"

        Chakra.box
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
                str month
            ]
