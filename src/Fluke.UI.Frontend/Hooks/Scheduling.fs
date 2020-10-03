namespace Fluke.UI.Frontend.Hooks

open System
open Fable.Core
open Feliz


module Scheduling =
    type SchedulingType =
        | Timeout
        | Interval

    let private schedulingFn =
        function
        | Timeout -> JS.setTimeout, JS.clearTimeout
        | Interval -> JS.setInterval, JS.clearInterval

    let useScheduling schedulingType duration (fn: unit -> unit) =
        let savedCallback = React.useRef fn

        React.useEffect
            ((fun () -> savedCallback.current <- fn),
             [|
                 fn :> obj
             |])

        React.useEffect
            ((fun () ->
                let set, clear = schedulingFn schedulingType
                let onExecute () = savedCallback.current ()

                let id = set onExecute duration

                { new IDisposable with
                    member _.Dispose () = clear id
                }),
             [|
                 duration :> obj
             |])
