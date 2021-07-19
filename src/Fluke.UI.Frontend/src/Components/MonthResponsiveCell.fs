namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared
open Fluke.UI.Frontend.TempUI
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module MonthResponsiveCell =
    open Domain.UserInteraction

    [<ReactComponent>]
    let MonthResponsiveCell (dateIdAtom: Store.Atom<DateId>) (props: UI.IChakraProps -> unit) =
        let dateId = Store.useValue dateIdAtom
        let weekStart = Store.useValue Atoms.User.weekStart
        let cellSize = Store.useValue Atoms.User.cellSize

        UI.box
            (fun x ->
                x.whiteSpace <- "nowrap"
                x.textAlign <- "center"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"

                x.borderLeftWidth <-
                    match (weekStart, dateId) with
                    | StartOfMonth -> "1px"
                    | _ -> null

                x.borderLeftColor <-
                    match (weekStart, dateId) with
                    | StartOfMonth -> "#ffffff3d"
                    | _ -> null

                props x)
            [

                dateId |> DateId.Format DateIdFormat.Month |> str
            ]
