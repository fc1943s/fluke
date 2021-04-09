namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.Core.JsInterop


module JS =
    [<Emit "(w => $0 instanceof w[$1])(window)">]
    let instanceof (_obj: obj, _typeName: string) : bool = jsNative

    [<Emit "(() => { var audio = new Audio($0); audio.volume = 0.5; return audio; })().play();">]
    let playAudio (_file: string) : unit = jsNative

    [<Emit "process.env.JEST_WORKER_ID">]
    let jestWorkerId : bool = jsNative
    let isTesting = jestWorkerId || Browser.Dom.window?Cypress <> null
