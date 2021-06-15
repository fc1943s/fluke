namespace Fluke.UI.Frontend.Bindings

open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fable.Core

[<AutoOpen>]
module StoreMagic =
    //    type StoreValue<'T>  =
//        class
//        end
//
//    [<Erase>]
//    type SelectorGetter =
//        [<Emit("$0.get($1)")>]
//        member _.get (_storeValue: StoreValue<'T>) : 'T = jsNative
//
//

    type atom = unit

    type atomFamily = unit
    type selector = unit
    type selectorFamily = unit

    type Store = Recoil


module Store =
    //    type InputAtom<'TValue1, 'TKey> =
//        | Atom of Username * StoreValue<'TValue1>
//        | AtomFamily of Username * ('TKey -> StoreValue<'TValue1>) * 'TKey
//        | AtomPath of Username * atomPath: string

    let inline useSetter () = Recoil.useCallbackRef id

    let inline useValueLoadableDefault atom def = Recoil.useValueLoadableDefault atom def
    let inline useStateLoadableDefault atom def = Recoil.useStateLoadableDefault atom def

    type InputAtom<'TValue1, 'TKey> = Recoil.InputAtom<'TValue1, 'TKey>
    type CallbackMethods = Feliz.Recoil.CallbackMethods
    type AtomEffect<'T, 'U> = Feliz.Recoil.AtomEffect<'T, 'U>

    let inline gunEffect<'TValue3, 'TKey> = Recoil.gunEffect<'TValue3, 'TKey>
//    let inline gunEffect<'TValue3, 'TKey> (atom: InputAtom<'TValue3, 'TKey>) (keyIdentifier: string list) =
//        Recoil.gunEffect atom keyIdentifier
