namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings


module Content =

    [<ReactComponent>]
    let content () =
        Profiling.addTimestamp "mainComponent.render"
        let username = Recoil.useValue Recoil.Atoms.username
        let sessionRestored = Recoil.useValue Recoil.Atoms.sessionRestored

        Chakra.flex
            {| minHeight = "100vh" |}
            [
                match sessionRestored with
                | false -> LoadingScreen.loadingScreen ()
                | true ->
                    match username with
                    | Some username ->
                        React.suspense
                            ([
//                                SessionDataLoader.sessionDataLoader {| Username = username |}
                                SoundPlayer.soundPlayer {| Username = username |}

                                Chakra.stack
                                    {| spacing = 0; flex = 1 |}
                                    [
                                        TopBar.topBar ()
                                        HomeScreen.homeScreen {| Username = username; Props = {| flex = 1 |} |}
                                        StatusBar.statusBar {| Username = username |}
                                    ]
                             ],
                             LoadingScreen.loadingScreen ())

                    | None -> LoginScreen.loginScreen ()
            ]
