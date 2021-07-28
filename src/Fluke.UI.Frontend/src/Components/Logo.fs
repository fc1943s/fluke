namespace Fluke.UI.Frontend.Components

open FsCore.Model
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State.State


module Logo =
    let inline Logo () =
        UI.simpleGrid
            (fun x ->
                x.columns <- 2
                x.borderWidth <- "1px"

                x.borderColor <-
                    Color.Value UserState.Default.CellColorDisabled
                    |> Option.get)
            [
                yield!
                    [
                        Color.Value UserState.Default.CellColorMissed
                        Color.Value UserState.Default.CellColorPending
                        Color.Value UserState.Default.CellColorPostponed
                        Color.Value UserState.Default.CellColorCompleted
                    ]
                    |> List.map Option.get
                    |> List.map
                        (fun color ->
                            UI.box
                                (fun x ->
                                    x.height <- "7px"
                                    x.width <- "7px"
                                    x.backgroundColor <- color)
                                [])
            ]
