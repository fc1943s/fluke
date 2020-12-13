namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module Logo =
    open Domain.State

    [<ReactComponent>]
    let Logo () =
        Chakra.simpleGrid
            {|
                columns = 2
                borderWidth = "1px"
                borderColor = TempUI.cellStatusColor Disabled
            |}
            [
                yield!
                    [
                        TempUI.cellStatusColor Missed
                        TempUI.cellStatusColor Pending
                        TempUI.manualCellStatusColor (Postponed None)
                        TempUI.manualCellStatusColor Completed
                    ]
                    |> List.map (fun color ->
                        Chakra.box
                            {|
                                height = "7px"
                                width = "7px"
                                backgroundColor = color
                            |}
                            [])
            ]
