namespace Fluke.UI.Frontend

open Feliz
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.State
open FsJs
open FsStore.Model
open FsUi.Components


module App =

    [<ReactComponent>]
    let App wrap =
        Profiling.addTimestamp "App().render"
        Profiling.addCount "App().render"

        (if wrap then
             RootWrapper.RootWrapper (Some (Selectors.User.theme :?> Atom<obj>))
         else
             React.fragment)
            [
                RouterObserver.RouterObserver ()
                GunObserver.GunObserver ()

                CtrlListener.CtrlListener ()
                ShiftListener.ShiftListener ()
                SelectionListener.SelectionListener ()

                Content.Content ()

                DebugPanel.DebugPanel DebugPanel.DebugPanelDisplay.Overlay
            ]
