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
        Profiling.addTimestamp (fun () -> "App().render")
        Profiling.addCount (fun () -> "App().render")

        (if wrap then
             RootWrapper.RootWrapper (Some (Selectors.User.theme |> unbox<AtomConfig<unit>>))
         else
             React.fragment)
            [
                GunObserver.GunObserver ()

                CtrlListener.CtrlListener ()
                ShiftListener.ShiftListener ()
                SelectionListener.SelectionListener ()

                Content.Content ()

                DebugPanel.DebugPanel DebugPanel.DebugPanelDisplay.Overlay
            ]
