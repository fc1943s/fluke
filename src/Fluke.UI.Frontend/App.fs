namespace Fluke.UI.Frontend

open Fable.React
open Feliz
open Feliz.Recoil
open Browser.Dom


module App =
    let appMain = React.memo (fun () ->
        Recoil.root [
            root.localStorage (fun hydrater -> hydrater.setAtom Recoil.Atoms.view)

//            root.init Recoil.initState

            root.children [
//                ReactBindings.React.createElement
//                    (Ext.recoilLogger, (), [])

                Components.MainComponent.render ()
            ]
        ]
    )

    ReactDOM.render (appMain (), document.getElementById "app")

