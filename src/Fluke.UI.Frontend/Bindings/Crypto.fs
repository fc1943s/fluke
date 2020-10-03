namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module Crypto =
    [<ImportAll "crypto-js">]
    let private crypto: {| SHA3: string -> obj |} = jsNative

    let sha3 = crypto.SHA3
