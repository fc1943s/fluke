namespace Fluke.UI.Frontend

open Fable.Core
open FsJs


module TempAudio =
    let inline playDing () =
        [
            0
            1000
        ]
        |> List.map (JS.setTimeout (fun () -> Js.playAudio "../sounds/ding.wav"))
        |> ignore

    let inline playTick () =
        Js.playAudioVolume 1. "../sounds/tick.wav"
