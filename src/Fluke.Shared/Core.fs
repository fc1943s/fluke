namespace Fluke.Shared

open FSharpPlus
open System

module List =
    let prepend a b = List.append b a

// TODO: move to suigetsu
module Core =
    type ResultBuilder () =
        member _.Return x = Ok x

        member _.ReturnFrom (m: Result<_, _>) = m

        member _.Bind (m, f) = Result.bind f m

        member _.Bind ((m, error), f) =
            m
            |> function
                | Some s -> Ok s
                | None -> Error error
            |> Result.bind f

        member _.Zero () = None

        member _.Combine (m, f) = Result.bind f m

        member _.Delay (f: unit -> _) = f

        member _.Run f = f ()

        member this.TryWith (m, h) =
            try
                this.ReturnFrom m
            with ex -> h ex

        member this.TryFinally (m, compensation) =
            try
                this.ReturnFrom m
            finally
                compensation ()

        member this.Using (res: #IDisposable, body) =
            let fn () =
                match res with
                | null -> ()
                | disp -> disp.Dispose ()
            this.TryFinally (body res, fn)

        member this.While (guard, f) =
            if not (guard ())
            then Ok ()
            else
                do f () |> ignore
                this.While (guard, f)

        member this.For (sequence: seq<_>, body) =
            this.Using (sequence.GetEnumerator (), (fun enum -> this.While (enum.MoveNext, this.Delay (fun () -> body enum.Current))))

    let result = ResultBuilder ()

