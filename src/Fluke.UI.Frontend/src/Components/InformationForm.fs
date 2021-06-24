namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module InformationForm =
    [<ReactComponent>]
    let InformationForm () =
        let informationUIFlag =
            Store.useSetState (Atoms.uiFlag Atoms.UIFlagType.Information)

        Chakra.box
            (fun x -> x.padding <- "15px")
            [
                str "No information selected"
            ]
