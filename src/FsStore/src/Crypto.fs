namespace FsStore.Bindings

open System.Collections.Generic
open Fable.Core.JsInterop
open System


module Crypto =
    let private jssha: obj = importDefault "jssha"

    let private shake128 (str: string) : string =
        let hash = createNew jssha ("SHAKE128", "TEXT")
        hash?update str
        hash?getHash "HEX" {| outputLen = 128 |}

    let private hashCache = Dictionary<string, Guid> ()

    let getTextGuidHash value =
        if hashCache.ContainsKey value then
            hashCache.[value]
        else
            let hash = value |> shake128 |> Guid
            hashCache.Add (value, hash)
            hash
