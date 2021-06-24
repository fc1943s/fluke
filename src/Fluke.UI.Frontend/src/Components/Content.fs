namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Content =
    [<ReactComponent>]
    let Content () =
        Profiling.addTimestamp "mainComponent.render"

        let sessionRestored = Store.useValue Atoms.sessionRestored
        let initialPeerSkipped = Store.useValue Atoms.initialPeerSkipped
        let gunPeers = Store.useValue Atoms.gunPeers
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let username = Store.useValue Atoms.username

        Chakra.flex
            (fun x ->
                x.flex <- "1"
                x.minHeight <- "100vh"
                x.height <- if deviceInfo.IsExtension then "590px" else null
                x.width <- if deviceInfo.IsExtension then "790px" else null)
            [
                match sessionRestored with
                | false -> LoadingSpinner.LoadingSpinner ()
                | true ->
                    match username with
                    | None ->
                        match gunPeers, initialPeerSkipped with
                        | [||], false -> InitialPeers.InitialPeers ()
                        | _ -> LoginScreen.LoginScreen ()
                    | Some _ ->
                        React.suspense (
                            [
                                PositionUpdater.PositionUpdater ()

                                Chakra.stack
                                    (fun x ->
                                        x.spacing <- "0"
                                        x.flex <- "1"
                                        x.borderWidth <- "1px"
                                        x.borderColor <- "whiteAlpha.300"
                                        x.maxWidth <- "100vw")
                                    [

                                        TopBar.TopBar ()

                                        Chakra.flex
                                            (fun x -> x.flex <- "1")
                                            [
                                                React.suspense (
                                                    [
                                                        LeftDock.LeftDock ()
                                                    ],
                                                    LoadingSpinner.LoadingSpinner ()
                                                )
                                                React.suspense (
                                                    [
                                                        ViewTabs.ViewTabs ()
                                                    ],
                                                    LoadingSpinner.LoadingSpinner ()
                                                )
                                                React.suspense (
                                                    [
                                                        RightDock.RightDock ()
                                                    ],
                                                    LoadingSpinner.LoadingSpinner ()
                                                )
                                            ]

                                        StatusBar.StatusBar ()
                                    ]

                                React.suspense (
                                    [
                                        SoundPlayer.SoundPlayer ()
                                    ],
                                    nothing
                                )
                            ],
                            LoadingSpinner.LoadingSpinner ()
                        )
            ]
