namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Router
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module RootWrapper =
    [<ReactComponent>]
    let RootWrapper children =
        let theme = Theme.useTheme ()

        React.strictMode [
            Jotai.provider [
//                Recoil.root [
//                    root.children [
                        React.ReactErrorBoundary.renderCatchFn
                            (fun (error, info) -> printfn $"ReactErrorBoundary Error: {info.componentStack} {error}")
                            (Html.div [
                                prop.style [ style.color "white" ]
                                prop.children [ str "error" ]
                             ])
                            (Chakra.provider
                                (fun x -> x.theme <- theme)
                                [
                                    React.router [
                                        router.children [ yield! children ]
                                    ]
                                ])
//                    ]
//                ]
            ]
        ]
