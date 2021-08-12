namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.State
open FsStore
open FsUi.Bindings
open FsUi.Components


module TaskCells =
    [<ReactComponent>]
    let TaskCells index taskIdAtom =
        let dateIdAtoms = Store.useValue Selectors.Selectors.dateIdAtoms

        Ui.flex
            (fun x -> x.backgroundColor <- "#212121")
            [
                React.suspense (
                    [
                        yield!
                            dateIdAtoms
                            |> Array.map
                                (fun dateIdAtom ->
                                    Cell.CellWrapper
                                        {|
                                            TaskIdAtom = taskIdAtom
                                            DateIdAtom = dateIdAtom
                                            SemiTransparent = index % 2 <> 0
                                        |})
                    ],
                    LoadingSpinner.LoadingSpinner ()
                )
            ]
