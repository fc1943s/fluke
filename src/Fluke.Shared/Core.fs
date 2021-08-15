namespace Fluke.Shared

open System.IO
open System


module Async =
    let inline lift (value: 'T) = async { return value }


module Result =
    let inline defaultValue def result =
        match result with
        | Ok result -> result
        | Error _ -> def


module Set =
    let inline choose fn set =
        set
        |> Set.fold
            (fun result item ->
                match fn item with
                | Some item -> result |> Set.add item
                | None -> result)
            Set.empty

    let inline toggle value (set: Set<'T>) =
        if set.Contains value then set.Remove value else set.Add value

    let inline addIf item condition set =
        if not condition then set else set |> Set.add item

//    let inline seqAdd item seq =
//        seq |> Set.ofSeq |> Set.add item |> Set.toSeq

//    let inline collect fn set =
//        set
//        |> Set.toSeq
//        |> Seq.collect id
//        |> Seq.map fn
//        |> Set.ofSeq


[<AutoOpen>]
module Operators =
    let inline (><) x (min, max) = (x > min) && (x < max)

    let inline (>=<) x (min, max) = (x >= min) && (x < max)

    let inline (>==<) x (min, max) = (x >= min) && (x <= max)

#if !FABLE_COMPILER
    let inline (</>) a b = Path.Combine (a, b)
#endif


module String =
    let inline trim (str: string) = str.Trim ()

    let inline take count (source: string) = source.[..count - 1]

    let inline toLower (source: string) =
        if isNull source then source else source.ToLowerInvariant ()

    let inline parseInt (text: string) =
        match Int32.TryParse text with
        | true, value -> Some value
        | _ -> None

    let inline parseIntMin min (text: string) =
        parseInt text
        |> Option.bind (fun n -> if n >= min then Some n else None)

    let parseUInt = parseIntMin 0

    let inline parseIntMax max (text: string) =
        parseInt text
        |> Option.bind (fun n -> if n <= max then Some n else None)
