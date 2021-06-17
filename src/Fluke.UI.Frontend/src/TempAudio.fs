namespace Fluke.UI.Frontend

open Fable.Core


module TempAudio =
    let inline playDing () =
        [
            0
            1400
        ]
        |> List.map (JS.setTimeout (fun () -> Bindings.JS.playAudio "../sounds/ding.wav"))
        |> ignore

    let inline playTick () =
        Bindings.JS.playAudio "../sounds/tick.wav"
