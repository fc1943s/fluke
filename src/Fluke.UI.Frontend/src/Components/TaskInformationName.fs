namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module TaskInformationName =

    [<ReactComponent>]
    let TaskInformationName (input: {| Username: Username; TaskId: TaskId |}) =
        let information = Recoil.useValue (Atoms.Task.information (input.Username, input.TaskId))

        InformationName.InformationName
            {|
                Username = input.Username
                Information = information
            |}
