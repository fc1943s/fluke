namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module GridHeader =
    [<ReactComponent>]
    let GridHeader () =
        let cellSize = Store.useValue Atoms.User.cellSize
        let dateIdAtoms = Store.useValue Selectors.dateIdAtoms
        let dateIdAtomsByMonth = Store.useValue Selectors.dateIdAtomsByMonth

        UI.box
            (fun _ -> ())
            [
                UI.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateIdAtomsByMonth
                            |> Array.map
                                (fun dateIdAtoms ->
                                    if dateIdAtoms.Length = 0 then
                                        nothing
                                    else
                                        let cellWidth = cellSize * dateIdAtoms.Length

                                        MonthResponsiveCell.MonthResponsiveCell
                                            dateIdAtoms.[0]
                                            (fun x -> x.width <- $"{cellWidth}px"))
                    ]

                UI.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateIdAtoms
                            |> Array.map (Day.Day TempUI.DateIdFormat.DayOfWeek)
                    ]

                UI.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateIdAtoms
                            |> Array.map (Day.Day TempUI.DateIdFormat.Day)
                    ]
            ]
