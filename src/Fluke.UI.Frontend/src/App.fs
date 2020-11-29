namespace Fluke.UI.Frontend

open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components


module App =

    [<ReactComponent>]
    let app () =
        Profiling.addTimestamp "appMain.render"

        RootWrapper.rootWrapper [
            DebugOverlay.debugOverlay ()

            CtrlListener.ctrlListener ()
            ShiftListener.shiftListener ()
            SelectionListener.selectionListener ()
            RouterObserver.routerObserver ()
            //                PositionUpdater.render ()
            GunObserver.gunObserver ()
            UserLoader.userLoader ()

            Content.content ()
        ]
