namespace Fluke.UI.Frontend

open Fable.Core
open FSharpPlus

module Temp =

    module Sound =
        let playDing () =
            [
                0
                1400
            ]
            |> List.map (JS.setTimeout (fun () -> Bindings.JS.playAudio "../sounds/ding.wav"))
            |> ignore

        let playTick () = Bindings.JS.playAudio "../sounds/tick.wav"
