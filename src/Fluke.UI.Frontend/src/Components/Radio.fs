namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Radio =
    [<ReactComponent>]
    let Radio (props: Chakra.IChakraProps -> unit) children =
        let darkMode = Store.useValue Atoms.User.darkMode

        Chakra.stack
            (fun x ->
                x.spacing <- "4px"
                x.alignItems <- "center"
                x.direction <- "row")
            [
                Chakra.radio
                    (fun x ->
                        x.colorScheme <- "purple"
                        x.borderColor <- if darkMode then "#484848" else "#b7b7b7"
                        x.size <- "lg"
                        props x)
                    []
                Chakra.box
                    (fun _ -> ())
                    [
                        yield! children
                    ]
            ]
