namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend


module MainComponent =

    let render = React.functionComponent (fun () ->
        Recoil.root [
            HomePageComponent.``default`` ()
        ])

