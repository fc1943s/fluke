namespace Fluke.UI.Frontend

open Feliz
open Feliz.Recoil
open Feliz.Router
open Browser.Dom
open Fluke.UI.Frontend


module App =
    let router =
        React.memo (fun () ->
            React.router
                [
                    router.children [ Components.MainComponent.render () ]
                ])

    let appMain =
        React.memo (fun () ->

            Recoil.Profiling.addTimestamp "appMain.render"
            Recoil.root [
                root.localStorage (fun hydrater ->
                    hydrater.setAtom Recoil.Atoms.debug
                    hydrater.setAtom Recoil.Atoms.treeName)

                root.children [ router () ]
            ])

    ReactDOM.render (appMain (), document.getElementById "app")
