namespace Fluke.UI.Frontend.Hooks

open System
open Fable.Core
open Feliz
open Fluke.UI.Frontend.Bindings


module Scheduling =
    type SchedulingType =
        | Timeout
        | Interval

    let private schedulingFn =
        function
        | Timeout -> JS.setTimeout, JS.clearTimeout
        | Interval -> JS.setInterval, JS.clearInterval

    let useScheduling schedulingType duration (fn: Store.CallbackMethods -> JS.Promise<unit>) =
        let savedCallback = React.useRef fn

        React.useEffect (
            (fun () -> savedCallback.current <- fn),
            [|
                box savedCallback
                box fn
            |]
        )

        let mounted, setMounted = React.useState true

        let fn = Store.useCallbackRef (fun setter _ -> promise { if mounted then do! savedCallback.current setter })

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
