namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Content =
    [<ReactComponent>]
    let Content () =
        Profiling.addTimestamp "mainComponent.render"
        let username = Recoil.useValue Atoms.username
        let sessionRestored = Recoil.useValue Atoms.sessionRestored
        let initialPeerSkipped = Recoil.useValue Atoms.initialPeerSkipped
        let gunPeers = Recoil.useValue Selectors.gunPeers
        let deviceInfo = Recoil.useValue Selectors.deviceInfo

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
                        | [], false -> InitialPeers.InitialPeers ()
                        | _ -> LoginScreen.LoginScreen ()
                    | Some username ->
                        React.suspense (
                            [
                                PositionUpdater.PositionUpdater {| Username = username |}

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
                                                LeftDock.LeftDock {| Username = username |}
                                                ViewTabs.ViewTabs {| Username = username |}
                                            ]

                                        StatusBar.StatusBar {| Username = username |}
                                    ]

                                SoundPlayer.SoundPlayer {| Username = username |}
                            ],
                            LoadingSpinner.LoadingSpinner ()
                        )
            ]
