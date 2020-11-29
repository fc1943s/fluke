namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module LoadingScreen =

    [<ReactComponent>]
    let loadingScreen () =
        Chakra.center
            {| flex = 1 |}
            [
                Chakra.stack
                    ()
                    [
                        Spinner.spinner ()
                        Chakra.box
                            ()
                            [
                                str "Loading..."
                            ]
                    ]
            ]
