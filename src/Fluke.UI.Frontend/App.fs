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
                    router.children
                        [
                            Chakra.provider
                                {|
                                    resetCSS = true
                                    theme = Chakra.theme
                                |}
                                [
                                    Components.MainComponent.render ()
                                ]
                        ]
                ])

    let appMain =
        React.memo (fun () ->

            Recoil.Profiling.addTimestamp "appMain.render"
            Recoil.root [
                root.localStorage (fun hydrater -> hydrater.setAtom Recoil.Atoms.debug)
                root.init Recoil.initState


                root.children
                    [
                        router ()
                    ]
            ])

    ReactDOM.render (appMain (), document.getElementById "app")
