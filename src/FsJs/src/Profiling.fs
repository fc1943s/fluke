namespace FsJs

open System
open System.Collections.Generic
open FsCore


module Profiling =
    let private initialTicks = DateTime.Now.Ticks

    let profilingState =
        {|
            CallCount = Dictionary<string, int> ()
            Timestamps = List<string * float> ()
        |}

    Dom.set (nameof profilingState) profilingState

    let addCount id =
        if Dom.isDebug () then
            match profilingState.CallCount.ContainsKey id with
            | false -> profilingState.CallCount.[id] <- 1
            | true -> profilingState.CallCount.[id] <- profilingState.CallCount.[id] + 1

    let addTimestamp id =
        if Dom.isDebug () then
            profilingState.Timestamps.Add (id, DateTime.ticksDiff initialTicks)

    addTimestamp "Init"
