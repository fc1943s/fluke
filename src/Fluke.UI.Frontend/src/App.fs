namespace Fluke.UI.Frontend

open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings


module App =
    let render =
        React.memo (fun () ->
            Profiling.addTimestamp "appMain.render"

            RootWrapper.render [
                DebugOverlay.render ()

                CtrlListener.render ()
                ShiftListener.render ()
                SelectionListener.render ()
                RouterObserver.render ()
                //                PositionUpdater.render ()
                GunObserver.render ()
                UserLoader.render ()

                Content.render ()
            ])
