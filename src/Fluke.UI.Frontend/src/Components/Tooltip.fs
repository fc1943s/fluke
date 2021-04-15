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
                label = input.label
                hasArrow = input.hasArrow
                placement = input.placement
                backgroundColor = "gray.45"
                color = "black"
                closeDelay = 200
                zIndex = 20000
            |}
            children
