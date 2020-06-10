namespace Fluke.UI.Frontend

open Feliz
open Feliz.Recoil
open Browser.Dom


module Main =
    let appMain = React.memo (fun () ->
        Recoil.root [
            Components.MainComponent.render ()
        ]
    )

    ReactDOM.render (appMain (), document.getElementById "app")

