namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Model
open Fluke.Shared


module TaskInformationName =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| TaskId: Recoil.Atoms.Task.TaskId |}) ->
            let informationId = Recoil.useValue (Recoil.Atoms.Task.informationId input.TaskId)

            InformationName.render {| InformationId = informationId |})
