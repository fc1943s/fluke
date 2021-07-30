namespace FsStore

open FsCore
open FsCore.Model
open FsJs
open FsStore.Bindings

module FsStore =
    let collection = Collection (nameof FsStore)

module Model =
    type GetFn = Jotai.GetFn
    type SetFn = Jotai.SetFn
    type AtomReference<'T> = Jotai.AtomReference<'T>
    type Atom<'T> = Jotai.Atom<'T>


    type LogLevel =
        | Trace = 0
        | Debug = 1
        | Info = 2
        | Warning = 3
        | Error = 4
        | Critical = 5

    let defaultLogLevel = if Dom.isDebug () then LogLevel.Debug else LogLevel.Info

    type LogFn = (unit -> string) -> unit

    type Logger =
        {
            Trace: LogFn
            Debug: LogFn
            Info: LogFn
            Warning: LogFn
            Error: LogFn
        }
        static member inline Create currentLogLevel =
            let log logLevel (fn: unit -> string) =
                if currentLogLevel <= logLevel then
                    let result = fn ()

                    if result |> Option.ofObjUnbox |> Option.isSome then
                        Dom.log (fun () -> $"[{logLevel}] {result}")

            {
                Trace = log LogLevel.Trace
                Debug = log LogLevel.Debug
                Info = log LogLevel.Info
                Warning = log LogLevel.Warning
                Error = log LogLevel.Error
            }

        static member inline Default = Logger.Create defaultLogLevel

    [<RequireQualifiedAccess>]
    type InputScope<'TValue> =
        | Current
        | Temp of Gun.Serializer<'TValue>

    and InputScope<'TValue> with
        static member inline AtomScope<'TValue> (inputScope: InputScope<'TValue> option) =
            match inputScope with
            | Some (InputScope.Temp _) -> AtomScope.Temp
            | _ -> AtomScope.Current

    and [<RequireQualifiedAccess>] AtomScope =
        | Current
        | Temp

    type InputAtom<'T> = InputAtom of atomPath: AtomReference<'T>

    type AtomField<'TValue67> =
        {
            Current: Jotai.Atom<'TValue67> option
            Temp: Jotai.Atom<string> option
        }
