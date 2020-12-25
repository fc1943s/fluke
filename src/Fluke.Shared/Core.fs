namespace Fluke.Shared

[<AutoOpen>]
module Operators =
    let inline (><) x (min, max) = (x > min) && (x < max)

    let inline (>=<) x (min, max) = (x >= min) && (x < max)

    let inline (>==<) x (min, max) = (x >= min) && (x <= max)


module Map =
    let singleton key value =
        [
            key, value
        ]
        |> Map.ofList
