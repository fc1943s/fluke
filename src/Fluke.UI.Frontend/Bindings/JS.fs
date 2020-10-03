namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module JS =
    [<Emit("(w => $0 instanceof w[$1])(window)")>]
    let instanceof (_obj: obj, _typeName: string): bool = jsNative

    [<Emit "(() => { var audio = new Audio($0); audio.volume = 0.5; return audio; })().play();">]
    let playAudio (_file: string): unit = jsNative
