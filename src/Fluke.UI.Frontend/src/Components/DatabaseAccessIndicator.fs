namespace Fluke.UI.Frontend.Components

open Feliz
open FsUi.Bindings


module DatabaseAccessIndicator =
    [<ReactComponent>]
    let DatabaseAccessIndicator () =
        Ui.stack
            (fun x ->
                x.direction <- "row"
                x.spacing <- "15px")
            [
                Ui.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "4px"
                        x.alignItems <- "center")
                    [
                        Ui.circle
                            (fun x ->
                                x.width <- "10px"
                                x.height <- "10px"
                                x.backgroundColor <- "#0f0")
                            []

                        Ui.str "Private"

                    ]
                Ui.iconButton
                    (fun x ->
                        x.icon <- Icons.bs.BsThreeDots |> Icons.render
                        x.disabled <- true
                        x.width <- "22px"
                        x.height <- "15px"
                        x.onClick <- fun _ -> promise { () })
                    []
            ]
