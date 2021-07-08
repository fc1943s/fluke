namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Radio =
    [<ReactComponent>]
    let Radio (props: UI.IChakraProps -> unit) children =
        let darkMode = Store.useValue Atoms.User.darkMode

        UI.stack
            (fun x ->
                x.spacing <- "4px"
                x.alignItems <- "center"
                x.direction <- "row")
            [
                UI.radio
                    (fun x ->
                        x.colorScheme <- "purple"
                        x.borderColor <- if darkMode then "#484848" else "#b7b7b7"
                        x.size <- "lg"
                        props x)
                    []
                UI.box
                    (fun _ -> ())
                    [
                        yield! children
                    ]
            ]
