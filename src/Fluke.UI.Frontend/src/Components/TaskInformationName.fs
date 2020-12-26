namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fable.React
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module TaskInformationName =

    [<ReactComponent>]
    let TaskInformationName (taskId: Recoil.Atoms.Task.TaskId) =
        let informationId = Recoil.useValue (Recoil.Atoms.Task.informationId taskId)
//        InformationName.informationName {| InformationId = informationId |}
//        str "InformationName"
        nothing
