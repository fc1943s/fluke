namespace Fluke.UI.Frontend.Components

open Browser
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks


module MainComponent =

    module PositionUpdater =
        let hook =
            React.memo (fun () ->
                let resetPosition = Recoil.useResetState Recoil.Selectors.position

                Scheduling.useScheduling Scheduling.Interval (60 * 1000) resetPosition
                //        Scheduling.useScheduling Scheduling.Interval (10 * 1000) resetPosition

                nothing)


    module AutoReload =
        let hook_TEMP =
            React.memo (fun () ->
                let reload = React.useCallback (fun () -> Dom.window.location.reload true)

                Scheduling.useScheduling Scheduling.Timeout (60 * 60 * 1000) reload

                nothing)


    let render =
        React.memo (fun () ->
            let username = Recoil.useValue Recoil.Atoms.username

            React.fragment [
                GlobalShortcutHandler.hook ()
                //                PositionUpdater.hook ()
//                AutoReload.hook_TEMP ()
                DebugOverlayComponent.render ()

                match username with
                | Some username ->
                    React.suspense
                        ([
                            SessionDataLoader.hook {| Username = username |}
                            SoundPlayer.hook {| Username = username |}

                            TopBarComponent.render ()
                            ContentComponent.render {| Username = username |}
                            StatusBarComponent.render ()
                         ],
                         PageLoaderComponent.render ())

                | None -> UserLoader.hook ()
            ])
