namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open Fable.Core


module Jotai =
    type PrimitiveAtom<'TValue>  =
        class
        end

    type Atom<'TValue>  =
        class
        end

    type IJotai =
        abstract atom : 'TValue -> PrimitiveAtom<'TValue>
        abstract atom : (unit -> unit) * JS.Promise<'Value> -> Atom<'TValue>

        abstract atom :
            (unit -> unit) * (unit -> unit * unit -> unit * unit -> unit) * JS.Promise<'Value> ->
            Atom<'TValue>

    let jotai : IJotai = importAll "jotai"


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
