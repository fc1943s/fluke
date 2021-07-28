namespace Fluke.UI.Frontend

open Feliz
open Fluke.UI.Frontend.Components
open FsJs
open FsUi.Components


module App =

    [<ReactComponent>]
    let App wrap =
        Profiling.addTimestamp "App().render"
        Profiling.addCount "App().render"

        (if wrap then RootWrapper.RootWrapper else React.fragment)
            [
                RouterObserver.RouterObserver ()
                //                GunObserver.GunObserver ()

                CtrlListener.CtrlListener ()
                ShiftListener.ShiftListener ()
                SelectionListener.SelectionListener ()

                Content.Content ()

                DebugOverlay.DebugOverlay ()
            ]
