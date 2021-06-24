namespace Fluke.UI.Frontend.Bindings

open System
open System.Collections.Generic
open Fluke.Shared


module Profiling =
    let private initialTicks = DateTime.Now.Ticks

    let internal profilingState =
        {|
            CallCount = Dictionary<string, int> ()
            Timestamps = List<string * float> ()
        |}

    Dom.set (nameof profilingState) profilingState

    let addCount id =
        if JS.isDebug () then
            match profilingState.CallCount.ContainsKey id with
            | false -> profilingState.CallCount.[id] <- 1
            | true -> profilingState.CallCount.[id] <- profilingState.CallCount.[id] + 1

    let addTimestamp id =
        if JS.isDebug () then
            profilingState.Timestamps.Add (id, DateTime.ticksDiff initialTicks)

    addTimestamp "Init"
