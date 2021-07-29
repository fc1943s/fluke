namespace FsCore

open System


module Function =
    let inline memoizeLazy fn =
        let result = lazy (fn ())
        fun () -> result.Value

module Object =
    let inline compare a b = (unbox a) = (unbox b)

    let inline newDisposable fn =
        { new IDisposable with
            member _.Dispose () = fn ()
        }


module Option =
    let inline ofObjUnbox<'T> (value: 'T) =
        Option.ofObj (unbox value)
        |> Option.map (fun x -> box x :?> 'T)


module String =
    let inline split (separator: string) (str: string) = str.Split separator

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


module DateTime =
    let inline ticksDiff ticks =
        (TimeSpan (DateTime.Now.Ticks - ticks))
            .TotalMilliseconds


module Enum =
    let inline ToList<'T> () =
        (Enum.GetValues typeof<'T> :?> 'T [])
        |> Array.toList

    let inline name<'T> (value: 'T) = Enum.GetName (typeof<'T>, value)
