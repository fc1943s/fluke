namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings


module Content =

    [<ReactComponent>]
    let Content () =
        Profiling.addTimestamp "mainComponent.render"
        let username = Recoil.useValue Recoil.Atoms.username
        let sessionRestored = Recoil.useValue Recoil.Atoms.sessionRestored
        let initialPeerSkipped = Recoil.useValue Recoil.Atoms.initialPeerSkipped
        let gunPeers = Recoil.useValue Recoil.Selectors.gunPeers

        Chakra.flex
            {| minHeight = "100vh" |}
            [
                match sessionRestored with
                | false -> LoadingScreen.LoadingScreen ()
                | true ->
                    match username with
                    | Some username ->
                        React.suspense (
                            [
                                SessionDataLoader.SessionDataLoader username
                                SoundPlayer.SoundPlayer username

                                GunBind.GunBind ()

                                Chakra.stack
                                    {| spacing = 0; flex = 1 |}
                                    [
                                        TopBar.TopBar ()
                                        HomeScreen.HomeScreen {| Username = username; Props = {| flex = 1 |} |}
                                        StatusBar.StatusBar username
                                    ]
                            ],
                            LoadingScreen.LoadingScreen ()
                        )

                    | None ->
                        match gunPeers, initialPeerSkipped with
                        | [||], false -> InitialPeers.InitialPeers ()
                        | _ -> LoginScreen.LoginScreen ()
            ]
