namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module TaskInformationName =

    [<ReactComponent>]
    let TaskInformationName (input: {| TaskId: TaskId |}) =
        let information = Store.useValue (Atoms.Task.information input.TaskId)

        InformationName.InformationName {| Information = information |}
