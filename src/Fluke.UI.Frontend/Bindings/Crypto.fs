namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module Crypto =
    [<ImportAll "crypto-js">]
    let crypto: {| SHA3: string -> {| toString: obj -> string |}
                   enc: {| Hex: obj |} |} = jsNative

    let sha3 = crypto.SHA3
