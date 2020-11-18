namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend


module TaskInformationName =
    let render =
        React.memo (fun (input: {| TaskId: Recoil.Atoms.Task.TaskId |}) ->
            let informationId = Recoil.useValue (Recoil.Atoms.Task.informationId input.TaskId)

            InformationName.render {| InformationId = informationId |})
