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
        let dateAtoms = Store.useValue Selectors.Selectors.dateAtoms
        let dateAtomsByMonth = Store.useValue Selectors.Selectors.dateAtomsByMonth

        Ui.box
            (fun _ -> ())
            [
                Ui.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateAtomsByMonth
                            |> Array.map
                                (fun dateAtoms ->
                                    if dateAtoms.Length = 0 then
                                        nothing
                                    else
                                        let cellWidth = cellSize * dateAtoms.Length

                                        MonthResponsiveCell.MonthResponsiveCell
                                            dateAtoms.[0]
                                            (fun x -> x.width <- $"{cellWidth}px"))
                    ]

                Ui.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateAtoms
                            |> Array.map (Day.Day TempUI.DateIdFormat.DayOfWeek)
                    ]

                Ui.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateAtoms
                            |> Array.map (Day.Day TempUI.DateIdFormat.Day)
                    ]
            ]
