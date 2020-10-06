namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings


module DetailsComponent =
    let render =
        React.memo (fun () ->
            Chakra.box
                {| width = "100%"; padding = "5px" |}
                [
                    str "Details"
                ])
