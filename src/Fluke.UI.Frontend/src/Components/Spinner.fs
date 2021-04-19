namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Spinner =

    [<ReactComponent>]
    let Spinner () =
        Chakra.spinner (fun x -> x.size <- "xl") []
