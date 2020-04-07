// https://github.com/fable-compiler/fable-react/issues/190

namespace Suigetsu.UI.Frontend.React

open System
open Fable.Core
open Fable.React

module CustomHooks =
    let useInterval fn interval =
        let savedCallback = Hooks.useRef fn
        
        Hooks.useEffect (fun () ->
            savedCallback.current <- fn
        , [| fn |])
        
        Hooks.useEffectDisposable (fun () ->
            let id =
                JS.setInterval (fun () ->
                    savedCallback.current ()
                ) interval
            
            { new IDisposable with
                member _.Dispose () =
                    JS.clearInterval id }
        , [| interval |])
