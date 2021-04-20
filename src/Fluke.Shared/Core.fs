namespace Fluke.Shared

open System.IO
open System

module Seq =
    let intersperse sep list =
        seq {
            let mutable notFirst = false

            for element in list do
                if notFirst then yield sep

                yield element
                notFirst <- true
        }

module List =
    let intersperse element source : list<'T> =
        source
        |> List.toSeq
        |> Seq.intersperse element
        |> Seq.toList

module Result =
    let defaultValue def result =
        match result with
        | Ok result -> result
        | Error _ -> def

module Map =
    let singleton key value =
        [
            key, value
        ]
        |> Map.ofList

    let keys (source: Map<'Key, 'T>) : seq<'Key> =
        source |> Seq.map (fun (KeyValue (k, _)) -> k)

    let values (source: Map<'Key, 'T>) : seq<'T> =
        source |> Seq.map (fun (KeyValue (_, v)) -> v)

    let unionWith combiner (source1: Map<'Key, 'Value>) (source2: Map<'Key, 'Value>) =
        Map.fold
            (fun m k v' ->
                Map.add
                    k
                    (match Map.tryFind k m with
                     | Some v -> combiner v v'
                     | None -> v')
                    m)
            source1
            source2

    let union (source: Map<'Key, 'T>) (altSource: Map<'Key, 'T>) =
        unionWith (fun x _ -> x) source altSource

    let mapValues f (x: Map<'Key, 'T>) = Map.map (fun _ -> f) x




[<AutoOpen>]
module Operators =
    let inline (><) x (min, max) = (x > min) && (x < max)

    let inline (>=<) x (min, max) = (x >= min) && (x < max)

    let inline (>==<) x (min, max) = (x >= min) && (x <= max)

#if !FABLE_COMPILER
    let (</>) a b = Path.Combine (a, b)
#endif


module String =
    let take count (source: string) = source.[..count - 1]

    let toLower (source: string) =
        if isNull source then source else source.ToLowerInvariant ()

    let (|ValidString|WhitespaceStr|NullString|) (str: string) =
        match str with
        | null -> NullString
        | str when String.IsNullOrWhiteSpace str -> WhitespaceStr
        | _ -> ValidString
