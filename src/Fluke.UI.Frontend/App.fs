namespace Fluke.UI.Frontend

open Feliz
open Feliz.Recoil
open Feliz.Router
open Browser.Dom
open Fluke.UI.Frontend
open Fable.Core.JsInterop
open Fluke.UI.Frontend.Bindings


module App =

    let router =
        React.memo (fun () ->
            React.router
                [
                    router.children
                        [
                            Chakra.provider
                                {| resetCSS = true; theme = Chakra.theme |}
                                [
                                    Chakra.darkMode
                                        ()
                                        [
                                            Components.MainComponent.render ()
                                        ]
                                ]
                        ]
                ])

    let appMain =
        React.memo (fun () ->

            Profiling.addTimestamp "appMain.render"
            React.strictMode
                [
                    Recoil.root [
                        root.init Recoil.initState
                        root.localStorage (fun hydrater ->
                            hydrater.setAtom Recoil.Atoms.debug
                            hydrater.setAtom Recoil.Atoms.treeSelectionIds
                            hydrater.setAtom Recoil.Atoms.selectedPosition)

                        root.children
                            [
                                router ()
                            ]
                    ]
                ])

    importAll "typeface-roboto-condensed"

    importAll "./public/index.scss"
    importAll "./public/index.tsx"
    importAll "./public/index.ts"
    importAll "./public/index.jsx"
    importAll "./public/index.js"

    React.render (document.getElementById "root") (appMain ())
