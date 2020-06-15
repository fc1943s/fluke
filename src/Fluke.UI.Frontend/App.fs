namespace Fluke.UI.Frontend

open Fable.React
open Feliz
open Feliz.Recoil
open Browser.Dom


module App =
    let appMain = React.memo (fun () ->
        Recoil.root [
            ReactBindings.React.createElement
                (Ext.recoilLogger, (), [])

            Components.MainComponent.render ()
        ]
    )

    ReactDOM.render (appMain (), document.getElementById "app")

