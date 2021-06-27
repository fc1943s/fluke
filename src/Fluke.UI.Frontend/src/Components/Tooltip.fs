namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks


module Tooltip =

    [<ReactComponent>]
    let Tooltip
        (input: {| Children: seq<ReactElement>
                   WrapperProps: Chakra.IChakraProps -> unit
                   Props: Chakra.IChakraProps -> unit |})
        =
        let ref = React.useElementRef ()
        let hovered = Listener.useElementHover ref

        Chakra.box
            (fun x ->
                x.ref <- ref
                x.display <- "inline"
                input.WrapperProps x)
            [
                Chakra.tooltip
                    (fun x ->
                        x.isLazy <- true
                        x.isOpen <- hovered
                        x.paddingTop <- "3px"
                        x.backgroundColor <- "gray.77"
                        x.color <- "black"
                        x.zIndex <- 20000
                        x.closeOnMouseDown <- true
                        x.portalProps <- {| appendToParentPortal = true |}
//                        x.shouldWrapChildren <- true
                        input.Props x)
                    input.Children
            ]


    let inline wrap label children =
        if label = nothing then
            React.fragment children
        else
            Tooltip
                {|
                    Children = children
                    WrapperProps = (fun _ -> ())
                    Props = (fun x -> x.label <- label)
                |}
