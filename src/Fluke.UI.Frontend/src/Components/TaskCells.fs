namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module TaskCells =
    [<ReactComponent>]
    let TaskCells index taskIdAtom =
        let dateIdAtoms = Store.useValue Selectors.dateIdAtoms

        UI.flex
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
