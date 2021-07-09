namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Dropdown =
    [<ReactComponent>]
    let Dropdown
        (input: {| Tooltip: string
                   Left: bool
                   Trigger: bool -> (bool -> unit) -> ReactElement
                   Body: (unit -> unit) -> ReactElement list |})
        =
        let visible, setVisible = React.useState false
        let darkMode = Store.useValue Atoms.User.darkMode

        UI.flex
            (fun x ->
                x.direction <- "column"
                x.overflow <- "auto")
            [
                Tooltip.wrap
                    (str input.Tooltip)
                    [
                        input.Trigger visible setVisible
                    ]
                if not visible then
                    nothing
                else
                    UI.box
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

                            let n = if darkMode then "255" else "0"

                            x.background <-
                                $"""linear-gradient(
                                    180deg,
                                    rgba({n},{n},{n},0) 0%%,
                                    rgba({n},{n},{n},0.01) 20%%,
                                    rgba({n},{n},{n},0.02) 100%%);"""

                            x.padding <- "17px")
                        [
                            yield! input.Body (fun () -> setVisible false)
                        ]
            ]
