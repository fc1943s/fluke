namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module InformationForm =
    [<ReactComponent>]
    let InformationForm (input: {| Username: Username |}) =
        let informationUIFlag =
            Store.useSetState (Atoms.User.uiFlag (input.Username, Atoms.User.UIFlagType.Information))

        Chakra.box
            (fun x -> x.padding <- "15px")
            [
                str "No information selected"
            ]
