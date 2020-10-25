namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Model
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
                    borderColor = Disabled.CellColor
                    border = "1px solid"
                |}
                [
                    yield! [
                               Missed.CellColor
                               Pending.CellColor
                               (Postponed None).CellColor
                               Completed.CellColor
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
