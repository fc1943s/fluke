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

        Profiling.addCount "ThemeLoader().render"

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
    let StateInitializer children =
        Profiling.addCount "StateInitializer().render"
        ThemeLoader children

    [<ReactComponent>]
    let RootWrapper children =
        Profiling.addCount "RootWrapper().render"

        React.strictMode [
            Store.provider [
                React.suspense (
                    [
                        React.ErrorBoundary [
                            StateInitializer [ yield! children ]
                        ]
                    ],
                    Html.div [
                        prop.className "static"
                        prop.children [
                            str "Loading database..."
                        ]
                    ]
                )
            ]
        ]
