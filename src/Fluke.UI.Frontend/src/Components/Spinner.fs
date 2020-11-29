namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Spinner =

    [<ReactComponent>]
    let spinner () = Chakra.spinner {| size = "xl" |} []
