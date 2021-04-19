namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.Core.JsInterop


[<AutoOpen>]
module Operators =
    [<Emit("Object.assign({}, $0, $1)")>]
    let (++) _o1 _o2 : obj = jsNative

    [<Emit("Object.assign($0, $1)")>]
    let (<+) _o1 _o2 : unit = jsNative

module JS =
    [<Emit "(w => $0 instanceof w[$1])(window)">]
    let instanceof (_obj: obj, _typeName: string) : bool = jsNative

    [<Emit "(() => { var audio = new Audio($0); audio.volume = 0.5; return audio; })().play();">]
    let playAudio (_file: string) : unit = jsNative

    [<Emit "process.env.JEST_WORKER_ID">]
    let jestWorkerId : bool = jsNative

    let isTesting = jestWorkerId || Browser.Dom.window?Cypress <> null
    let newObj fn = jsOptions<_> fn
    let cloneDeep<'T> (_: 'T) : 'T = importDefault "lodash.clonedeep"
    let cloneObj<'T> (obj: 'T) (fn: 'T -> 'T) = fn (cloneDeep obj)
