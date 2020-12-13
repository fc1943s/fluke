namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fable.React
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module TaskInformationName =

    [<ReactComponent>]
    let TaskInformationName (input: {| TaskId: Recoil.Atoms.Task.TaskId |}) =
        let informationId = Recoil.useValue (Recoil.Atoms.Task.informationId input.TaskId)
//        InformationName.informationName {| InformationId = informationId |}
//        str "InformationName"
        nothing
