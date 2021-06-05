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

    let useScheduling schedulingType duration (fn: CallbackMethods -> JS.Promise<unit>) =
        let savedCallback = React.useRef fn

        React.useEffect (
            (fun () -> savedCallback.current <- fn),
            [|
                box savedCallback
                box fn
            |]
        )

        let mounted, setMounted = React.useState true

        let fn = Recoil.useCallbackRef (fun setter -> promise { if mounted then do! savedCallback.current setter })

        React.useEffect (
            (fun () ->
                let setFn, clearFn = schedulingFn schedulingType

                let id = setFn (fn >> Promise.start) duration

                setMounted true

                { new IDisposable with
                    member _.Dispose () =
                        setMounted false
                        clearFn id
                }),
            [|
                box fn
                box setMounted
                box schedulingType
                box savedCallback
                box duration
            |]
        )
