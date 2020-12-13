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

        Chakra.flex
            {| minHeight = "100vh" |}
            [
                match sessionRestored with
                | false -> LoadingScreen.LoadingScreen ()
                | true ->
                    match username with
                    | Some username ->
                        React.suspense
                            ([
                                SessionDataLoader.SessionDataLoader {| Username = username |}
                                SoundPlayer.SoundPlayer {| Username = username |}

                                Chakra.stack
                                    {| spacing = 0; flex = 1 |}
                                    [
                                        TopBar.TopBar ()
                                        HomeScreen.HomeScreen {| Username = username; Props = {| flex = 1 |} |}
                                        StatusBar.StatusBar {| Username = username |}
                                    ]
                             ],
                             LoadingScreen.LoadingScreen ())

                    | None -> LoginScreen.LoginScreen ()
            ]