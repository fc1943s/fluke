namespace Fluke.UI.Frontend.Bindings

open Fluke.Shared
open Fable.Core
open System


module Crypto =
    [<ImportAll "crypto-js">]
    let crypto : {| SHA3: string -> {| toString: obj -> string |}
                    enc: {| Hex: obj |} |} =
        jsNative

    let sha3 = crypto.SHA3

    let getTextGuidHash value =
        value
        |> sha3
        |> string
        |> String.take 16
        |> System.Text.Encoding.UTF8.GetBytes
        |> Guid
