namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fable.DateFunctions
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module StatusBarComponent =
    let render = React.memo (fun () -> Chakra.flex {| height = "26px" |} [])
