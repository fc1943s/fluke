namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Hint =
    type IProps =
        abstract hint : ReactElement option with get, set
        abstract hintTitle : ReactElement option with get, set

    [<ReactComponent>]
    let Hint (input: IProps) =
        match input.hint with
        | Some hint ->
            Chakra.popover
                (fun _ -> ())
                [
                    Chakra.popoverTrigger
                        (fun _ -> ())
                        [
                            Chakra.iconButton
                                (fun x ->
                                    x.icon <- Icons.bsQuestionCircle ()
                                    x.border <- "0"
                                    x.color <- "heliotrope"
                                    x.marginLeft <- "3px"
                                    x.marginTop <- "-2px"
                                    x.padding <- "2px"
                                    x.minWidth <- "0"
                                    x.width <- "auto"
                                    x.height <- "auto")
                                []
                        ]
                    Chakra.popoverContent
                        (fun _ -> ())
                        [
                            Chakra.popoverArrow (fun _ -> ()) []
                            Chakra.popoverCloseButton (fun _ -> ()) []
                            Chakra.popoverBody
                                (fun x ->
                                    x.padding <- "10px"
                                    x.backgroundColor <- "gray.13")
                                [
                                    match input.hintTitle with
                                    | Some hintTitle ->
                                        Chakra.box
                                            (fun x ->
                                                x.paddingBottom <- "12px"
                                                x.fontSize <- "15px")
                                            [
                                                Chakra.icon
                                                    (fun x ->
                                                        x.marginTop <- "-3px"
                                                        x.marginRight <- "5px"
                                                        x.color <- "heliotrope"
                                                        x.icon <- Icons.bsQuestionCircle ())
                                                    []

                                                hintTitle
                                            ]
                                    | None -> ()
                                    hint
                                ]
                        ]
                ]
        | None -> nothing
