namespace FsStore.Bindings

open Fable.Core.JsInterop
open Fable.Core
open Fable.React
open FsJs


module Jotai =
    [<RequireQualifiedAccess>]
    type AtomScope =
        | Current
        | Temp

    type Atom<'TValue> =
        abstract member toString : unit -> string
        abstract member onMount : (('TValue -> unit) -> unit -> unit) with get, set

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
        abstract useHydrateAtoms : (Atom<'TValue> * 'TValue) [] -> AtomScope -> unit
        abstract useUpdateAtom : Atom<'TValue> -> ('TValue -> unit)
        abstract useAtomCallback : (GetFn * SetFn * 'TArg -> JS.Promise<'TValue>) -> ('TArg -> JS.Promise<'TValue>)
        abstract waitForAll : Atom<'T> [] -> Atom<'T []>

    let jotaiUtils: IJotaiUtils = importAll "jotai/utils"


[<AutoOpen>]
module JotaiMagic =

    type Jotai.IJotai with
        member inline _.provider children =
            ReactBindings.React.createElement (Jotai.jotai.Provider, (), children)
