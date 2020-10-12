namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings


module TopBarComponent =
    let render =
        React.memo (fun () ->
            Chakra.flex
                {|
                    height = "31px"
                    backgroundColor = "gray.10%"
                    align = "center"
                    padding = "7px"
                |}
                [
                    LogoComponent.render ()
                ])
