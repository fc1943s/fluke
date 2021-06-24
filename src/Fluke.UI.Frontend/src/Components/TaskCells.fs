namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module TaskCells =
    [<ReactComponent>]
    let TaskCells (input: {| TaskId: TaskId; Index: int |}) =
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
                                            TaskId = input.TaskId
                                            DateId = DateId date
                                            SemiTransparent = input.Index % 2 <> 0
                                        |}
                                ],
                                LoadingSpinner.InlineLoadingSpinner ()
                            ))
            ]
