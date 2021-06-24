namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Tooltip =

    let inline Tooltip
        (input: {| Children: seq<ReactElement>
                   Props: Chakra.IChakraProps -> unit |})
        =
        Chakra.tooltip
            (fun x ->
                x.paddingTop <- "3px"
                x.backgroundColor <- "gray.77"
                x.color <- "black"
                x.zIndex <- 20000
                x.closeOnMouseDown <- true
                x.shouldWrapChildren <- true
                input.Props x)
            input.Children

    let inline wrap label children =
        if label = nothing then
            React.fragment children
        else
            Tooltip
                {|
                    Children = children
                    Props = (fun x -> x.label <- label)
                |}
