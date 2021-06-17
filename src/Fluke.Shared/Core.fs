namespace Fluke.Shared

open System.IO
open System


module private ListIter =
    let inline length (target: ^X when ^X: (member length : int)) = (^X: (member length : int) target)

    let inline item (target: ^X when ^X: (member item : int -> ^Y)) (index: int) =
        (^X: (member item : int -> ^Y) target, index)


module Seq =
    let inline intersperse sep list =
        seq {
            let mutable notFirst = false

            for element in list do
                if notFirst then yield sep

                yield element
                notFirst <- true
        }


    let inline ofItems items =
        seq {
            for i = 0 to (ListIter.length items) - 1 do
                yield (ListIter.item items) i
        }


module Option =
    let inline ofObjUnbox<'T> (value: 'T) =
        Option.ofObj (unbox value)
        |> Option.map (fun x -> box x :?> 'T)


module List =
    let inline intersperse element source : list<'T> =
        source
        |> List.toSeq
        |> Seq.intersperse element
        |> Seq.toList

module Result =
    let inline defaultValue def result =
        match result with
        | Ok result -> result
        | Error _ -> def

module Map =
    let inline singleton key value =
        [
            key, value
        ]
        |> Map.ofList

    let inline keys (source: Map<'Key, 'T>) : seq<'Key> =
        source |> Seq.map (fun (KeyValue (k, _)) -> k)

    let inline values (source: Map<'Key, 'T>) : seq<'T> =
        source |> Seq.map (fun (KeyValue (_, v)) -> v)

    let inline unionWith combiner (source1: Map<'Key, 'Value>) (source2: Map<'Key, 'Value>) =
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

    let inline union (source: Map<'Key, 'T>) (altSource: Map<'Key, 'T>) =
        unionWith (fun x _ -> x) source altSource

    let inline mapValues f (x: Map<'Key, 'T>) = Map.map (fun _ -> f) x

module Set =
    let inline choose fn set =
        set
        |> Set.toSeq
        |> Seq.map fn
        |> Seq.filter Option.isSome
        |> Seq.map Option.get
        |> Set.ofSeq

    let inline toggle value (set: Set<'T>) =
        if set.Contains value then set.Remove value else set.Add value

    let inline addIf item condition set =
        if not condition then set else set |> Set.add item

    let inline collect fn set =
        set
        |> Set.toSeq
        |> Seq.map fn
        |> Seq.collect id
        |> Set.ofSeq


[<AutoOpen>]
module Operators =
    let inline (><) x (min, max) = (x > min) && (x < max)

    let inline (>=<) x (min, max) = (x >= min) && (x < max)

    let inline (>==<) x (min, max) = (x >= min) && (x <= max)

#if !FABLE_COMPILER
    let inline (</>) a b = Path.Combine (a, b)
#endif


module String =
    let inline split (separator: string) (str: string) = str.Split separator

    let inline trim (str: string) = str.Trim ()

    let inline take count (source: string) = source.[..count - 1]

    let inline toLower (source: string) =
        if isNull source then source else source.ToLowerInvariant ()

    let inline (|ValidString|WhitespaceString|NullString|) (str: string) =
        match str with
        | null -> NullString
        | str when String.IsNullOrWhiteSpace str -> WhitespaceString
        | str -> ValidString str

    let inline (|InvalidString|_|) (str: string) =
        match str with
        | WhitespaceString
        | NullString -> Some InvalidString
        | _ -> None

module Enum =
    let inline ToList<'T> () =
        (Enum.GetValues typeof<'T> :?> 'T [])
        |> Array.toList

    let inline name<'T> (value: 'T) = Enum.GetName (typeof<'T>, value)
