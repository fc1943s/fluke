namespace Fluke.UI.Frontend

open FsCore
open Feliz
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.State
open FsJs
open FsStore.Model
open FsUi.Components
open FsUi.Bindings
open FsUi.State


module App =
    [<ReactComponent>]
    let App wrap =
        Profiling.addTimestamp (fun () -> $"{nameof Fluke} | App.render") getLocals

        (if wrap then
             RootWrapper.RootWrapper (Some (Selectors.User.theme |> unbox<AtomConfig<unit>>))
         else
             React.fragment)
            [
                GunObserver.GunObserver ()

                CtrlListener.CtrlListener ()
                ShiftListener.ShiftListener ()
                SelectionListener.SelectionListener ()
                MessagesListener.MessagesListener ()

                Content.Content ()

                DebugPanel.DebugPanel DebugPanel.DebugPanelDisplay.Overlay
            ]
