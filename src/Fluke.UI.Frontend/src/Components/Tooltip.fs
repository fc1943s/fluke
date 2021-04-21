namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Tooltip =

    [<ReactComponent>]
    let Tooltip (input: {| Props: Chakra.IChakraProps |}) =
        Chakra.tooltip
            (fun x ->
                x <+ input.Props
                x.backgroundColor <- "gray.77"
                x.color <- "black"
                x.zIndex <- 20000)
            input.Props.children

    let inline wrap label children =
        Tooltip
            {|
                Props =
                    JS.newObj
                        (fun x ->
                            x.label <- label
                            x.children <- children)
            |}
