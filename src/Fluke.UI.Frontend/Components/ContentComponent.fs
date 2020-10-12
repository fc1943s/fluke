namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Hooks
open Fluke.Shared


module ContentComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun () ->
            let username = Recoil.useValue Recoil.Atoms.username

            match username with
            | Some username ->
                React.suspense
                    ([
                        SessionDataLoader.hook {| Username = username |}
                        SoundPlayer.hook {| Username = username |}

                        TopBarComponent.render ()
                        NavBarComponent.render {| Username = username |}
                        PanelsComponent.render ()
                     ],
                     PageLoaderComponent.render ())

            | None -> UserLoader.hook ())
