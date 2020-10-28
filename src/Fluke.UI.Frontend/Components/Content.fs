namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Feliz.Router


module Content =
    let render =
        React.memo (fun () ->
            Profiling.addTimestamp "mainComponent.render"
            let username = Recoil.useValue Recoil.Atoms.username
            let sessionRestored = Recoil.useValue Recoil.Atoms.sessionRestored

            React.router
                [
                    router.children
                        [
                            Chakra.flex
                                {| minHeight = "100vh" |}
                                [
                                    match sessionRestored with
                                    | false -> LoadingScreen.render ()
                                    | true ->
                                        match username with
                                        | Some username ->
                                            React.suspense
                                                ([
                                                    SessionDataLoader.render {| Username = username |}
                                                    SoundPlayer.render {| Username = username |}

                                                    Chakra.stack
                                                        {| spacing = 0; flex = 1 |}
                                                        [
                                                            TopBar.render ()
                                                            HomeScreen.render
                                                                {| Username = username; Props = {| flex = 1 |} |}
                                                            StatusBar.render {| Username = username |}
                                                        ]
                                                 ],
                                                 LoadingScreen.render ())

                                        | None -> LoginScreen.render ()
                                ]
                        ]
                ])
