namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings


module Spinner =
    let render = React.memo (fun () -> Chakra.spinner {| size = "xl" |} [])
