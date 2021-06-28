namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fable.DateFunctions
open Fluke.Shared
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Empty =
    [<ReactComponent>]
    let Empty props =
        Chakra.box
            (fun x -> props x)
            [
                str ""
            ]
