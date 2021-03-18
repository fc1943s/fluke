namespace Fluke.UI.Frontend

open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components


module App =

    [<ReactComponent>]
    let App wrap =
        Profiling.addTimestamp "appMain.render"

        (if wrap then
             RootWrapper.rootWrapper
         else
             React.fragment)
            [
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
