namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.State
open FsStore
open FsStore.Hooks


module TaskInformationName =

    [<ReactComponent>]
    let TaskInformationName taskIdAtom =
        let taskId = Store.useValue taskIdAtom
        let information = Store.useValue (Atoms.Task.information taskId)

        InformationName.InformationName information
