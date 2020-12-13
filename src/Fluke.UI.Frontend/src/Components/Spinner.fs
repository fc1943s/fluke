namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Spinner =

    [<ReactComponent>]
    let Spinner () = Chakra.spinner {| size = "xl" |} []
