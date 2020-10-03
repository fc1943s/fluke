namespace Fluke.Shared

[<AutoOpen>]
module Operators =
    let inline (><) x (min, max) =
        (x > min) && (x < max)

    let inline (>=<) x (min, max) =
        (x >= min) && (x < max)

    let inline (>==<) x (min, max) =
        (x >= min) && (x <= max)

