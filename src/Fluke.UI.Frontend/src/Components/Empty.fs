namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fable.DateFunctions
open Fluke.Shared
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Feliz.Recoil


module Empty =
    [<ReactComponent>]
    let Empty (input: {| Props: Chakra.IChakraProps |}) =
        Chakra.box
            (fun _ -> ())
            [
                str ""
            ]
