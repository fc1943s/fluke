namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module TaskCells =
    [<ReactComponent>]
    let TaskCells
        (input: {| Username: Username
                   TaskId: TaskId
                   Index: int |})
        =
        let dateSequence = Recoil.useValue Selectors.dateSequence

        Chakra.flex
            (fun _ -> ())
            [
                yield!
                    dateSequence
                    |> List.map
                        (fun date ->
                            Cell.Cell
                                {|
                                    Username = input.Username
                                    TaskId = input.TaskId
                                    DateId = DateId date
                                    SemiTransparent = input.Index % 2 <> 0
                                |})
            ]
