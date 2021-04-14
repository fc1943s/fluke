namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Tooltip =
    type IProps =
        abstract label : ReactElement with get, set
        abstract hasArrow : bool with get, set
        abstract placement : string with get, set

    [<ReactComponent>]
    let Tooltip (input: IProps) children =
        Chakra.tooltip
            {|
                backgroundColor = "gray.10"
                color = "white"
                label = input.label
                hasArrow = input.hasArrow
                placement = input.placement
                zIndex = 20000
            |}
            children
