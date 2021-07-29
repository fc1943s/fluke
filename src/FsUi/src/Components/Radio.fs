namespace FsUi.Components

open FsUi.Bindings


module Radio =
    let inline Radio (props: UI.IChakraProps -> unit) children =
        UI.stack
            (fun x ->
                x.spacing <- "4px"
                x.alignItems <- "center"
                x.direction <- "row")
            [
                UI.radio
                    (fun x ->
                        x.colorScheme <- "purple"
                        x.borderColor <- "gray.30"
                        x.size <- "lg"
                        props x)
                    []
                UI.box
                    (fun _ -> ())
                    [
                        yield! children
                    ]
            ]
