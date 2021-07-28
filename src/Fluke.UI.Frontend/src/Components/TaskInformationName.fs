namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings; open FsStore; open FsUi.Bindings


module TaskInformationName =

    [<ReactComponent>]
    let TaskInformationName taskIdAtom =
        let taskId = Store.useValue taskIdAtom
        let information = Store.useValue (Atoms.Task.information taskId)

        InformationName.InformationName information
