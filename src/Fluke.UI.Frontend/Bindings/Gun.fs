namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop
open Feliz
open Feliz.Recoil


module Gun =
    type AppState = {
        a: {| b: string; c: int; |}
    }

    type IGunChainReference<'T> =
        abstract get: string -> IGunChainReference<'U>
        abstract put: 'V -> IGunChainReference<'U>

    let gun: unit -> IGunChainReference<AppState> = importDefault "gun"

