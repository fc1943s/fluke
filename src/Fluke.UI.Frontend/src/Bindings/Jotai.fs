namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open Fable.Core
open Fable.React

module DeepEqual =
    let deepEqual<'T> (_a: 'T) (_b: 'T) : bool = importDefault "fast-deep-equal"

module Jotai =
    type AtomType<'TKey, 'TValue> =
        | Atom of value: 'TValue
        | AtomFamily of key: 'TKey * value: 'TValue

    type PrimitiveAtom<'TValue>  =
        class
        end

    type Atom<'TValue>  =
        class
        end

    type GetFn<'T> = Atom<'T> -> 'T
    type AtomInitFn<'TValue, 'T> = GetFn<'T> -> JS.Promise<'TValue>
    type SetFn<'T, 'U> = Atom<'T> -> 'U -> unit
    type SetterFn<'TValue, 'T, 'V, 'W> = GetFn<'T> -> SetFn<'V, 'W> -> 'TValue -> JS.Promise<unit>

    type IJotai =
        abstract atom : 'TValue -> Atom<'TValue>
        //        abstract atom : (unit -> unit) * JS.Promise<'Value> -> Atom<'TValue>

        abstract Provider : obj -> obj


        abstract atom : AtomInitFn<'TValue, _> * SetterFn<'TValue, _, _, _> -> Atom<'TValue>

        abstract useAtom : Atom<'TValue> -> 'TValue * ('TValue -> unit)

    let Jotai : IJotai = importAll "jotai"

    type IJotaiUtils =
        abstract atomWithReducer : 'TValue -> ('TValue -> 'TValue -> 'TValue) -> Atom<'TValue>
        abstract atomWithStorage : string -> 'TValue -> Atom<'TValue>

        //        [<Emit "$0.atomFamily($1, $2)">]
        abstract atomFamily : ('TKey -> Atom<'TValue>) -> ('TValue -> 'TValue -> bool) -> ('TKey -> Atom<'TValue>)

        abstract useAtomValue : Atom<'TValue> -> 'TValue


[<AutoOpen>]
module JotaiMagic =
    type Jotai.IJotai with
        member _.provider children =
            ReactBindings.React.createElement (Jotai.Jotai.Provider, (), children)

    let Jotai = Jotai.Jotai


[<AutoOpen>]
module JotaiUtilsMagic =
    let JotaiUtils : Jotai.IJotaiUtils = importAll "jotai/utils"

    type Jotai.IJotaiUtils with
        member inline this.atomFamilyWithProfiling (defaultValue, effects) =
            JotaiUtils.atomFamily
                (fun param ->
                    //                    Profiling.addCount atomKey
                    let internalAtom = Jotai.atom (defaultValue param)

                    Jotai.atom (
                        (fun get ->
                            promise {
                                printfn $"on INIT param={param}"
                                return get internalAtom
                            }),
                        (fun get set arg ->
                            promise {
                                printfn $"SET! param={param} arg={arg}"
                                let oldValue = get internalAtom
                                printfn $"SET! oldValue={oldValue}"
                                set internalAtom arg
                            })
                    ))
                DeepEqual.deepEqual


//            |> fun (fn: 'a -> Jotai.Atom<'b>) ->
//                let newFn : unit -> 'a -> Jotai.Atom<'b> = unbox fn
//                newFn ()




//    let atom<'TValue> (_initialValue: 'TValue) : PrimitiveAtom<'TValue> = importMember "atom" "jotai"
//    ()
//    // primitive atom
//function atom<Value>(initialValue: Value): PrimitiveAtom<Value>
//
//// read-only atom
//function atom<Value>(read: (get: Getter) => Value | Promise<Value>): Atom<Value>
//
//// writable derived atom
//function atom<Value, Update>(
//  read: (get: Getter) => Value | Promise<Value>,
//  write: (get: Getter, set: Setter, update: Update) => void | Promise<void>
//): WritableAtom<Value, Update>
//
//// write-only derived atom
//function atom<Value, Update>(
//  read: Value,
//  write: (get: Getter, set: Setter, update: Update) => void | Promise<void>
//): WritableAtom<Value, Update>
