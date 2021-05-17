namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fable.DateFunctions
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Feliz.Recoil


module MonthResponsiveCell =
    open Domain.UserInteraction

    [<ReactComponent>]
    let MonthResponsiveCell
        (input: {| Username: Username
                   Date: FlukeDate
                   Props: Chakra.IChakraProps -> unit |})
        =
        let weekStart = Recoil.useValue (Atoms.User.weekStart input.Username)
        let month = (input.Date |> FlukeDate.DateTime).Format "MMM"

        Chakra.box
            (fun x ->
                x.textAlign <- "center"
                x.height <- "17px"
                x.lineHeight <- "17px"

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
