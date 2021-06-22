namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Dropdown =
    [<ReactComponent>]
    let Dropdown
        (input: {| Tooltip: string
                   Left: bool
                   Trigger: bool -> (bool -> unit) -> ReactElement
                   Body: (unit -> unit) -> ReactElement list |})
        =
        let visible, setVisible = React.useState false

        Chakra.flex
            (fun x -> x.direction <- "column")
            [
                Tooltip.wrap
                    (str input.Tooltip)
                    [
                        input.Trigger visible setVisible
                    ]
                if not visible then
                    nothing
                else
                    Chakra.box
                        (fun x ->
                            x.flex <- "1"
                            x.flexDirection <- "column"
                            x.marginTop <- "-1px"

                            if input.Left then
                                x.borderLeftWidth <- "1px"
                            else
                                x.borderRightWidth <- "1px"

                            x.borderBottomWidth <- "1px"
                            x.borderColor <- "whiteAlpha.200"

                            x.background <-
                                "linear-gradient(180deg, rgba(255,255,255,0) 0%, rgba(255,255,255,0.01) 20%, rgba(255,255,255,0.02) 100%);"

                            x.padding <- "17px")
                        [
                            yield! input.Body (fun () -> setVisible false)
                        ]
            ]
