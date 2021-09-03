namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.State
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open FsUi.Components


module TaskCells =
    [<ReactComponent>]
    let TaskCells index taskIdAtom =
        let dateAtoms = Store.useValue Selectors.Selectors.dateAtoms

        Ui.flex
            (fun x -> x.backgroundColor <- "#212121")
            [
                React.suspense (
                    [
                        yield!
                            dateAtoms
                            |> Array.map
                                (fun dateAtom ->
                                    Cell.CellWrapper
                                        {|
                                            TaskIdAtom = taskIdAtom
                                            DateAtom = dateAtom
                                            SemiTransparent = index % 2 <> 0
                                        |})
                    ],
                    LoadingSpinner.LoadingSpinner ()
                )
            ]
