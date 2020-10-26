namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fable.React


module LoginScreen =
    let render =
        React.memo (fun () ->
            Chakra.stack
                {| spacing = 0 |}
                [
                    Chakra.box
                        {| flex = 1 |}
                        [
                            str "Login"
                        ]
                    Chakra.box
                        ()
                        [
                            str "Login"
                        ]
                ])
