namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fable.React
open Fluke.UI.Frontend.Bindings


module LoadingScreen =
    let render =
        React.memo (fun () ->
            Chakra.center
                {| flex = 1 |}
                [
                    Chakra.stack
                        ()
                        [
                            Spinner.render ()
                            Chakra.box
                                ()
                                [
                                    str "Loading..."
                                ]
                        ]
                ])
