namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Router
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module RootWrapper =
    [<ReactComponent>]
    let ThemeLoader children =
        let theme = Theme.useTheme ()
        let darkMode = Store.useValue Atoms.User.darkMode

        UI.provider
            (fun x -> x.theme <- theme)
            [
                (if darkMode then UI.darkMode else UI.lightMode)
                    (fun _ -> ())
                    [
                        React.router [
                            router.children [ yield! children ]
                        ]
                    ]
            ]

    [<ReactComponent>]
    let RootWrapper children =
        React.strictMode [
            Store.provider [
//                React.ReactErrorBoundary.renderCatchFn
//                    (fun (error, info) -> printfn $"ReactErrorBoundary Error: {info.componentStack} {error}")
//                    (Html.div [
//                        prop.classes [ "static" ]
//                        prop.children [
//                            str "Unhandled Exception. Check the console log."
//                        ]
//                     ])
                    (ThemeLoader children)
            ]
        ]
