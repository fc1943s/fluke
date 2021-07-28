namespace FsUi.Components

open Feliz
open FsUi.Bindings


module LoadingSpinner =

    [<ReactComponent>]
    let LoadingSpinner () =
        UI.center
            (fun x -> x.flex <- "1")
            [
                UI.stack
                    (fun x -> x.alignItems <- "center")
                    [
                        Spinner.Spinner (fun _ -> ())
                        UI.str "Loading..."
                    ]
            ]

    [<ReactComponent>]
    let InlineLoadingSpinner () =
        UI.flex
            (fun x -> x.alignItems <- "center")
            [
                Spinner.Spinner
                    (fun x ->
                        x.width <- "10px"
                        x.height <- "10px")
            ]
