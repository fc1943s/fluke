namespace Fluke.UI.Frontend

open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components


module App =

    [<ReactComponent>]
    let App () =
        Profiling.addTimestamp "appMain.render"

        RootWrapper.rootWrapper [
            DebugOverlay.DebugOverlay ()

            CtrlListener.CtrlListener ()
            ShiftListener.ShiftListener ()
            RouterObserver.RouterObserver ()
            ApiSubscriber.ApiSubscriber ()
            SelectionListener.SelectionListener ()
            //                PositionUpdater.render ()
            GunObserver.GunObserver ()
            UserLoader.UserLoader ()

            Content.Content ()
        ]
