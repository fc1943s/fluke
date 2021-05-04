namespace Fluke.UI.Frontend

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components


module App =

    [<ReactComponent>]
    let App wrap =
        Profiling.addTimestamp "appMain.render"

        (if wrap then RootWrapper.rootWrapper else React.fragment)
            [
                CtrlListener.CtrlListener ()
                ShiftListener.ShiftListener ()
                RouterObserver.RouterObserver ()
                SelectionListener.SelectionListener ()
                //                GunObserver.GunObserver ()

                Content.Content ()

                DebugOverlay.DebugOverlay ()
            ]
