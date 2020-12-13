namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module LoadingScreen =

    [<ReactComponent>]
    let LoadingScreen () =
        Chakra.center
            {| flex = 1 |}
            [
                Chakra.stack
                    ()
                    [
                        Spinner.Spinner ()
                        Chakra.box
                            ()
                            [
                                str "Loading..."
                            ]
                    ]
            ]
