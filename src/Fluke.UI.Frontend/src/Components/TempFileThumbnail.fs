namespace Fluke.UI.Frontend.Components

open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Feliz


module TempFileThumbnail =
    [<ReactComponent>]
    let TempFileThumbnail onDelete onAdd fileId =

        UI.box
            (fun x -> x.position <- "relative")
            [
                FileThumbnail.FileThumbnail fileId

                UI.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "2px"
                        x.position <- "absolute"
                        x.bottom <- "1px"
                        x.right <- "1px")
                    [
                        InputLabelIconButton.InputLabelIconButton
                            (fun x ->
                                x.icon <- Icons.bs.BsTrash |> Icons.render
                                x.margin <- "0"
                                x.fontSize <- "11px"
                                x.height <- "15px"
                                x.color <- "whiteAlpha.700"
                                x.onClick <- fun _ -> onDelete ())

                        InputLabelIconButton.InputLabelIconButton
                            (fun x ->
                                x.icon <- Icons.fi.FiSave |> Icons.render
                                x.margin <- "0"
                                x.fontSize <- "11px"
                                x.height <- "15px"
                                x.color <- "whiteAlpha.700"
                                x.onClick <- fun _ -> onAdd ())
                    ]
            ]
