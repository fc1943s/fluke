namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fable.React
open Fluke.UI.Frontend
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.Bindings


module TaskInformationName =

    [<ReactComponent>]
    let TaskInformationName (taskId: TaskId) =
        let informationId = Recoil.useValue (Recoil.Atoms.Task.informationId (Some taskId))
        //        InformationName.informationName {| InformationId = informationId |}
//        str "InformationName"
        nothing
