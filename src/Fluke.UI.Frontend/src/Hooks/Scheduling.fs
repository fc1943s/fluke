namespace Fluke.UI.Frontend.Hooks

open System
open Fable.Core
open Feliz
open Feliz.Recoil

module Scheduling =
    type SchedulingType =
        | Timeout
        | Interval

    let private schedulingFn =
        function
        | Timeout -> JS.setTimeout, JS.clearTimeout
        | Interval -> JS.setInterval, JS.clearInterval

    let useScheduling schedulingType duration (fn: unit -> JS.Promise<unit>) =
        let savedCallback = React.useRef fn

        React.useEffect (
            (fun () -> savedCallback.current <- fn),
            [|
                box savedCallback
                box fn
            |]
        )

        let fn = Recoil.useCallbackRef (fun _ -> promise { do! savedCallback.current () })

        React.useEffect (
            (fun () ->
                let set, clear = schedulingFn schedulingType

                let id = set (fn >> Promise.start) duration

                { new IDisposable with
                    member _.Dispose () = clear id
                }),
            [|
                box fn
                box schedulingType
                box savedCallback
                box duration
            |]
        )
