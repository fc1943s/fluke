namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open FsStore
open FsStore.Hooks
open FsUi.Bindings


module GridHeader =
    [<ReactComponent>]
    let GridHeader () =
        let cellSize = Store.useValue Atoms.User.cellSize
        let dateIdAtoms = Store.useValue Selectors.Selectors.dateIdAtoms
        let dateIdAtomsByMonth = Store.useValue Selectors.Selectors.dateIdAtomsByMonth

        Ui.box
            (fun _ -> ())
            [
                Ui.flex
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

                Ui.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateIdAtoms
                            |> Array.map (Day.Day TempUI.DateIdFormat.DayOfWeek)
                    ]

                Ui.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateIdAtoms
                            |> Array.map (Day.Day TempUI.DateIdFormat.Day)
                    ]
            ]
