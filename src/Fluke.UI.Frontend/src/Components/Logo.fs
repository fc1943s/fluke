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
            (fun x ->
                x.columns <- 2
                x.borderWidth <- "1px"
                x.borderColor <- TempUI.cellStatusColor Disabled)
            [
                yield!
                    [
                        TempUI.cellStatusColor Missed
                        TempUI.cellStatusColor Pending
                        TempUI.manualCellStatusColor (Postponed None)
                        TempUI.manualCellStatusColor Completed
                    ]
                    |> List.map
                        (fun color ->
                            Chakra.box
                                (fun x ->
                                    x.height <- "7px"
                                    x.width <- "7px"
                                    x.backgroundColor <- color)
                                [])
            ]
