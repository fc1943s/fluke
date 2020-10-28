namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module Logo =
    open Domain.State

    let render =
        React.memo (fun () ->
            Chakra.simpleGrid
                {|
                    columns = 2
                    height = "16px"
                    width = "16px"
                    borderColor = TempUI.cellStatusColor Disabled
                    border = "1px solid"
                |}
                [
                    yield! [
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
                ])
