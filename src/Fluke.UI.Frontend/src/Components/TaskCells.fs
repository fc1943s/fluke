namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module TaskCells =
    [<ReactComponent>]
    let TaskCells index taskId =
        let dateSequence = Store.useValue Selectors.dateSequence

        Chakra.flex
            (fun _ -> ())
            [
                yield!
                    dateSequence
                    |> List.map
                        (fun date ->
                            React.suspense (
                                [
                                    Cell.Cell
                                        {|
                                            TaskId = taskId
                                            DateId = DateId date
                                            SemiTransparent = index % 2 <> 0
                                        |}
                                ],
                                LoadingSpinner.InlineLoadingSpinner ()
                            ))
            ]
