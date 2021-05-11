namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module TaskInformationName =

    [<ReactComponent>]
    let TaskInformationName (input: {| TaskId: TaskId |}) =
        let information = Recoil.useValue (Atoms.Task.information (Some input.TaskId))
        //        InformationName.informationName {| InformationId = informationId |}
//        str "InformationName"
        nothing
