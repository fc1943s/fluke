namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener


module SpinnerComponent =
    let render = React.memo (fun () -> str "(S)")
