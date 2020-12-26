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
    open Domain.UserInteraction

    [<ReactComponent>]
    let Empty (username: Username) =
        Chakra.box
            {|  |}
            [
                str ""
            ]
