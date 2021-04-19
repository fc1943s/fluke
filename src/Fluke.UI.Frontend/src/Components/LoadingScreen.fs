namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module LoadingScreen =

    [<ReactComponent>]
    let LoadingScreen () =
        Chakra.center
            (fun x -> x.flex <- 1)
            [
                Chakra.stack
                    (fun _ -> ())
                    [
                        Spinner.Spinner ()
                        Chakra.box
                            (fun _ -> ())
                            [
                                str "Loading..."
                            ]
                    ]
            ]
