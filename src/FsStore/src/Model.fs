namespace FsStore

open Fable.Core
open FsCore
open FsCore.Model
open FsJs
open FsStore.Bindings

module FsStore =
    let root = StoreRoot (nameof FsStore)

module Model =
    type Atom<'T> = Jotai.Atom<'T>
    type GetFn = Jotai.GetFn
    type SetFn = Jotai.SetFn

    type LogLevel =
        | Trace = 0
        | Debug = 1
        | Info = 2
        | Warning = 3
        | Error = 4
        | Critical = 5

    let DEFAULT_LOG_LEVEL = if Dom.isDebug () then LogLevel.Debug else LogLevel.Info

    type LogFn = (unit -> string) -> unit

    type Logger =
        {
            Trace: LogFn
            Debug: LogFn
            Info: LogFn
            Warning: LogFn
            Error: LogFn
        }

    [<RequireQualifiedAccess>]
    type InputScope<'TValue> =
        | Current
        | Temp of Gun.Serializer<'TValue>


    and [<RequireQualifiedAccess>] AtomScope =
        | Current
        | Temp

    [<RequireQualifiedAccess>]
    type AtomReference<'T> =
        | Atom of Atom<'T>
        | Path of string

    type InputAtom<'T> = InputAtom of atomPath: AtomReference<'T>

    type AtomField<'TValue67> =
        {
            Current: Atom<'TValue67> option
            Temp: Atom<string> option
        }

    [<RequireQualifiedAccess>]
    type GunOptions =
        | Minimal
        | Sync of string []


    [<Erase>]
    type AtomPath =
        | AtomPath of atomPath: string
        static member inline Value (AtomPath atomPath) = atomPath
        static member inline AtomKey atomPath =
            AtomPath (failwith "invalid")


//    let inline splitAtomPath (AtomPath atomPath) =
//        let matches =
//            (JSe.RegExp @"(.*?)\/([\w-]{36})\/\w+.*?")
//                .Match atomPath
//            |> Option.ofObj
//            |> Option.defaultValue Seq.empty
//            |> Seq.toList
//
//        match matches with
//        | _match :: root :: guid :: _key -> Some (root, guid)
//        | _ -> None

//        let tryTestKey table key =
//            let result = Regex.Match (key, $"^.*?/{table}/([a-fA-F0-9\\-]{{36}})")
//            if result.Groups.Count = 2 then Some result.Groups.[1].Value else None

//            [
//                yield atomKey.StoreRoot |> StoreRoot.Value
//                match atomKey.Collection with
//                | Some collection -> yield collection |> Collection.Value
//                | None -> ()
//                yield! atomKey.Keys
//                yield atomKey.Name
//            ]
//            |> String.concat "/"
//            |> AtomPath


    type AtomKey =
        {
            StoreRoot: StoreRoot
            Collection: Collection option
            Keys: string list
            Name: string
        }

    type Logger with
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

        static member inline Default = Logger.Create DEFAULT_LOG_LEVEL

    type InputScope<'TValue> with
        static member inline AtomScope<'TValue> (inputScope: InputScope<'TValue> option) =
            match inputScope with
            | Some (InputScope.Temp _) -> AtomScope.Temp
            | _ -> AtomScope.Current

    type AtomKey with
        static member inline AtomPath atomKey =
            [
                yield atomKey.StoreRoot |> StoreRoot.Value
                match atomKey.Collection with
                | Some collection -> yield collection |> Collection.Value
                | None -> ()
                yield! atomKey.Keys
                yield atomKey.Name
            ]
            |> String.concat "/"
            |> AtomPath
