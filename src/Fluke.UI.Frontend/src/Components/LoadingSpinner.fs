namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module LoadingSpinner =

    [<ReactComponent>]
    let LoadingSpinner () =
        Chakra.center
            (fun x -> x.flex <- "1")
            [
                Chakra.stack
                    (fun x -> x.alignItems <- "center")
                    [
                        Spinner.Spinner (fun _ -> ())
                        Chakra.box
                            (fun _ -> ())
                            [
                                str "Loading..."
                            ]
                    ]
            ]

    [<ReactComponent>]
    let InlineLoadingSpinner () =
        Chakra.flex
            (fun _ -> ())
            [
                Spinner.Spinner
                    (fun x ->
                        x.width <- "10px"
                        x.height <- "10px")
            ]
