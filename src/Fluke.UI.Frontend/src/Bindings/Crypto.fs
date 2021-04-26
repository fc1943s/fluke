namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open System


module Crypto =
    let jssha : obj = importDefault "jssha"

    let shake128 (str: string) : string =
        let hash = createNew jssha ("SHAKE128", "TEXT")
        hash?update str
        hash?getHash "HEX" {| outputLen = 128 |}

    let getTextGuidHash value = value |> shake128 |> Guid
