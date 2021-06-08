namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module DatabaseAccessIndicator =
    [<ReactComponent>]
    let DatabaseAccessIndicator () =
        Chakra.stack
            (fun x ->
                x.direction <- "row"
                x.spacing <- "15px")
            [
                Chakra.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "4px"
                        x.alignItems <- "center")
                    [
                        Chakra.circle
                            (fun x ->
                                x.width <- "10px"
                                x.height <- "10px"
                                x.backgroundColor <- "#0f0")
                            []

                        Chakra.box
                            (fun _ -> ())
                            [
                                str "Private"
                            ]

                    ]
                Chakra.iconButton
                    (fun x ->
                        x.icon <- Icons.bs.BsThreeDots |> Icons.render
                        x.disabled <- true
                        x.width <- "22px"
                        x.height <- "15px"
                        x.onClick <- fun _ -> promise { () })
                    []
            ]
