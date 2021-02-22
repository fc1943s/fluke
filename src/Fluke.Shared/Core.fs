namespace Fluke.Shared

open System.IO


[<AutoOpen>]
module Operators =
    let inline (><) x (min, max) = (x > min) && (x < max)

    let inline (>=<) x (min, max) = (x >= min) && (x < max)

    let inline (>==<) x (min, max) = (x >= min) && (x <= max)

#if !FABLE_COMPILER
    let (</>) a b = Path.Combine (a, b)
#endif


module Map =
    let singleton key value =
        [
            key, value
        ]
        |> Map.ofList
