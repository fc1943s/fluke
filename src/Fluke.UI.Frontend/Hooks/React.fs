namespace Fluke.UI.Frontend.Hooks

open System
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings


module React =
    let useDisposableEffect (effect, dependencies) =
        let disposed = React.useRef false

        React.useEffect
            ((fun () ->
                effect disposed

                { new IDisposable with
                    member _.Dispose () = disposed.current <- true
                }),
             dependencies)
