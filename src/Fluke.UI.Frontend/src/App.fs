namespace Fluke.UI.Frontend

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components


module App =

    [<ReactComponent>]
    let App wrap =
        Profiling.addTimestamp "appMain.render"

        (if wrap then RootWrapper.RootWrapper else React.fragment)
            [
                RouterObserver.RouterObserver ()
                //                GunObserver.GunObserver ()

                Content.Content ()

                CtrlListener.CtrlListener ()
                ShiftListener.ShiftListener ()
                SelectionListener.SelectionListener ()

                DebugOverlay.DebugOverlay ()
            ]
