namespace FsStore.Bindings

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core
open Fable.React
open FsJs


module Jotai =
    type Atom<'TValue> =
        abstract member toString : unit -> string
        abstract member onMount : ((int -> int) -> unit) -> unit -> unit

    let private atomPathMap = Dictionary<string, string> ()
    let private atomIdMap = Dictionary<string, string> ()

    match Dom.window () with
    | Some window ->
        window?atomPathMap <- atomPathMap
        window?atomIdMap <- atomIdMap
    | None -> ()

    let registerAtom atomPath keyIdentifier atom =
        let registerAtomPathById atomPath (atom: Atom<_>) =
            atomIdMap.[atom.toString ()] <- atomPath
            atom

        let registerAtomIdByPath (atom: Atom<_>) atomPath =
            atomPathMap.[atomPath] <- atom.toString ()
            atomPath

        Dom.log (fun () -> $"registerAtom atomPath={atomPath} keyIdentifier={keyIdentifier} atom={atom}")

        match keyIdentifier with
        | Some (collection, keyIdentifier) ->
            let gunNodePath = Gun.getGunNodePath collection atomPath keyIdentifier
            registerAtomIdByPath atom gunNodePath |> ignore
            let atom = registerAtomPathById gunNodePath atom
            atom, Some gunNodePath
        | _ -> atom, None

    [<RequireQualifiedAccess>]
    type AtomReference<'T> =
        | Atom of Atom<'T>
        | Path of string

    [<Erase>]
    type AtomPath =
        | AtomPath of atomPath: string
        static member inline Value (AtomPath atomPath) = atomPath

    let queryAtomPath atomReference =
        match atomReference with
        | AtomReference.Atom atom ->
            match atomIdMap.TryGetValue (atom.toString ()) with
            | true, value -> Some (AtomPath value)
            | _ -> None
        | AtomReference.Path path ->
            match atomPathMap.TryGetValue path with
            | true, value -> Some (AtomPath value)
            | _ -> None

    type GetFn = Atom<obj> -> obj
    type SetFn = Atom<obj> -> obj -> unit
    type CompareFn<'TValue> = 'TValue -> 'TValue -> bool

    type IJotai =
        abstract Provider : obj -> obj

        abstract atom : 'TValue -> Atom<'TValue>

        abstract atom :
            (GetFn -> JS.Promise<'TValue>) * (GetFn -> SetFn -> 'TValue -> JS.Promise<unit>) option -> Atom<'TValue>

        abstract atom : (GetFn -> 'TValue) * (GetFn -> SetFn -> 'TValue -> unit) option -> Atom<'TValue>
        abstract useAtom : Atom<'TValue> -> 'TValue * ('TValue -> unit)

    let jotai: IJotai = importAll "jotai"


    type IJotaiUtils =
        abstract atomFamily : ('TKey -> Atom<'TValue>) -> CompareFn<'TValue> -> ('TKey -> Atom<'TValue>)
        abstract atomWithDefault : (GetFn -> 'TValue) -> Atom<'TValue>
        abstract atomWithReducer : 'TValue -> ('TValue -> 'TValue -> 'TValue) -> Atom<'TValue>
        abstract atomWithStorage : string -> 'TValue -> Atom<'TValue>
        abstract selectAtom : Atom<'TValue> -> ('TValue -> 'U) -> CompareFn<'TValue> -> Atom<'U>
        abstract splitAtom : Atom<'TValue []> -> Atom<Atom<'TValue> []>
        abstract useAtomValue : Atom<'TValue> -> 'TValue
        abstract useUpdateAtom : Atom<'TValue> -> ('TValue -> unit)
        abstract useAtomCallback : (GetFn * SetFn * 'TArg -> JS.Promise<'TValue>) -> ('TArg -> JS.Promise<'TValue>)
        abstract waitForAll : Atom<'T> [] -> Atom<'T []>

    let jotaiUtils: IJotaiUtils = importAll "jotai/utils"


[<AutoOpen>]
module JotaiMagic =

    type Jotai.IJotai with
        member inline _.provider children =
            ReactBindings.React.createElement (Jotai.jotai.Provider, (), children)
