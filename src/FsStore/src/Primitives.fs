namespace FsStore

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core
open FsStore.Model
open Microsoft.FSharp.Core.Operators
open FsCore
open FsJs
open FsStore.Bindings.Jotai


module Internal =
    let private atomPathMap = Dictionary<string, string> ()
    let private atomIdMap = Dictionary<string, string> ()

    match Dom.window () with
    | Some window ->
        window?atomPathMap <- atomPathMap
        window?atomIdMap <- atomIdMap
    | None -> ()

    let registerAtom (AtomPath atomPath) (atom: Atom<_>) =
        Dom.log (fun () -> $"registerAtom {atomPath}")
        Profiling.addCount $"registerAtom {atomPath}"
        atomPathMap.[atomPath] <- atom.toString ()
        atomIdMap.[atom.toString ()] <- atomPath
        atom, Some (AtomPath atomPath)

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


module Primitives =
    let inline atom<'TValue> atomKey (defaultValue: 'TValue) =
        let atomPath = atomKey |> AtomKey.AtomPath

        jotai.atom (
            (fun () ->
                Profiling.addCount $"atom atomPath={atomPath}"

                defaultValue)
                ()
        )
        |> Internal.registerAtom atomPath
        |> fst

    let inline selector<'TValue> atomKey (getFn: GetFn -> 'TValue) (setFn: GetFn -> SetFn -> 'TValue -> unit) =
        let atomPath = atomKey |> AtomKey.AtomPath

        jotai.atom (
            (fun getter ->
                Profiling.addCount $"selector atomPath={atomPath}"

                getFn getter),
            Some
                (fun getter setter value ->
                    Profiling.addCount $"selector set atomPath={atomPath}"

                    let newValue = value
                    //                        match jsTypeof value with
                    //                         | "function" -> (unbox value) () |> unbox
                    //                         | _ -> value
                    setFn getter setter newValue)
        )
        |> Internal.registerAtom atomPath
        |> fst

    let inline selectAtom atomKey atom selector =
        //        readSelector (
        //            atomPath,
        //            fun getter ->
        //                let value = value getter atom
        //                Profiling.addCount $"{atomPath} :selectAtom"
        //                selector value
        //        )

        let atomPath = atomKey |> AtomKey.AtomPath

        jotaiUtils.selectAtom
            atom
            (fun value ->
                Profiling.addCount $"selectAtom atomPath={atomPath}"
                selector value)
            JS.undefined
        |> Internal.registerAtom atomPath
        |> fst

    let inline asyncSelector<'TValue>
        atomKey
        (getFn: GetFn -> JS.Promise<'TValue>)
        (setFn: GetFn -> SetFn -> 'TValue -> JS.Promise<unit>)
        =
        let atomPath = atomKey |> AtomKey.AtomPath

        jotai.atom (
            (fun getter ->
                promise {
                    Profiling.addCount $"asyncSelector atomPath={atomPath}"
                    return! getFn getter
                }),
            Some
                (fun getter setter newValue ->
                    promise {
                        Profiling.addCount $"asyncSelector set atomPath={atomPath}"
                        do! setFn getter setter newValue
                    })
        )
        |> Internal.registerAtom atomPath
        |> fst


[<AutoOpen>]
module PrimitivesMagic =
    module Store =
        let inline atom<'TValue> storeRoot name (defaultValue: 'TValue) =
            Primitives.atom
                {
                    StoreRoot = storeRoot
                    Collection = None
                    Keys = []
                    Name = name
                }
                defaultValue

        let inline atomFamily<'TKey, 'TValue>
            storeRoot
            collection
            name
            keyIdentifier
            (defaultValueFn: 'TKey -> 'TValue)
            =
            jotaiUtils.atomFamily
                (fun param ->
                    Primitives.atom
                        {
                            StoreRoot = storeRoot
                            Collection = Some collection
                            Keys = keyIdentifier param
                            Name = name
                        }
                        (defaultValueFn param))
                Object.compare

        let inline selector<'TValue>
            storeRoot
            name
            (getFn: GetFn -> 'TValue)
            (setFn: GetFn -> SetFn -> 'TValue -> unit)
            =
            Primitives.selector
                {
                    StoreRoot = storeRoot
                    Collection = None
                    Keys = []
                    Name = name
                }
                getFn
                setFn

        let inline readSelector<'TValue> storeRoot name (getFn: GetFn -> 'TValue) =
            selector storeRoot name getFn (fun _ _ _ -> failwith $"readSelector {storeRoot}/{name} is read only.")

        let inline selectorFamily<'TKey, 'TValue>
            storeRoot
            collection
            name
            (getFn: 'TKey -> GetFn -> 'TValue)
            (setFn: 'TKey -> GetFn -> SetFn -> 'TValue -> unit)
            =
            jotaiUtils.atomFamily
                (fun param ->
                    Primitives.selector
                        {
                            StoreRoot = storeRoot
                            Collection = Some collection
                            Keys = []
                            Name = name
                        }
                        (getFn param)
                        (setFn param))
                Object.compare


        let inline readSelectorFamily<'TKey, 'TValue>
            storeRoot
            collection
            name
            (getFn: 'TKey -> GetFn -> 'TValue)
            : ('TKey -> Atom<'TValue>) =
            selectorFamily
                storeRoot
                collection
                name
                getFn
                (fun _ _ _ -> failwith $"readSelectorFamily {storeRoot}/{collection}/{name} is read only.")

        let inline value<'TValue> (getter: GetFn) (atom: Atom<'TValue>) : 'TValue = (getter (unbox atom)) :?> 'TValue

        let inline set<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue) =
            setter (atom |> box |> unbox) value

        let inline change<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue -> 'TValue) =
            setter (atom |> box |> unbox) value

        let inline selectAtom storeRoot name atom selector =
            Primitives.selectAtom
                {
                    StoreRoot = storeRoot
                    Collection = None
                    Keys = []
                    Name = name
                }
                atom
                selector

        let inline selectAtomFamily storeRoot collection name atom selector =
            jotaiUtils.atomFamily
                (fun param ->
                    Primitives.selectAtom
                        {
                            StoreRoot = storeRoot
                            Collection = Some collection
                            Keys = []
                            Name = name
                        }
                        atom
                        (selector param))
                Object.compare

        let inline atomWithStorage storeRoot name defaultValue =
            let internalAtom =
                jotaiUtils.atomWithStorage
                    ({
                         StoreRoot = storeRoot
                         Collection = None
                         Keys = []
                         Name = name
                     }
                     |> AtomKey.AtomPath
                     |> AtomPath.Value)
                    defaultValue

            selector
                storeRoot
                name
                (fun getter -> value getter internalAtom)
                (fun _ setter argFn ->
                    let arg =
                        match jsTypeof argFn with
                        | "function" -> (argFn |> box |> unbox) () |> unbox
                        | _ -> argFn

                    set setter internalAtom arg)


        let inline asyncSelector<'TValue>
            storeRoot
            name
            (getFn: GetFn -> JS.Promise<'TValue>)
            (setFn: GetFn -> SetFn -> 'TValue -> JS.Promise<unit>)
            =
            Primitives.asyncSelector
                {
                    StoreRoot = storeRoot
                    Collection = None
                    Keys = []
                    Name = name
                }
                getFn
                setFn

        let inline asyncReadSelector<'TValue> storeRoot name (getFn: GetFn -> JS.Promise<'TValue>) =
            asyncSelector
                storeRoot
                name
                getFn
                (fun _ _ _newValue -> promise { failwith $"asyncReadSelector {storeRoot}/{name} is read only." })


        let inline asyncSelectorFamily<'TKey, 'TValue>
            storeRoot
            collection
            name
            keyIdentifier
            (getFn: 'TKey -> GetFn -> JS.Promise<'TValue>)
            (setFn: 'TKey -> GetFn -> SetFn -> 'TValue -> JS.Promise<unit>)
            =
            jotaiUtils.atomFamily
                (fun param ->
                    Primitives.asyncSelector
                        {
                            StoreRoot = storeRoot
                            Collection = Some collection
                            Keys = keyIdentifier param
                            Name = name
                        }
                        (getFn param)
                        (fun getter setter newValue -> promise { do! setFn param getter setter newValue }))
                Object.compare

        let inline asyncReadSelectorFamily<'TKey, 'TValue>
            storeRoot
            collection
            name
            keyIdentifier
            (getFn: 'TKey -> GetFn -> JS.Promise<'TValue>)
            =
            asyncSelectorFamily
                storeRoot
                collection
                name
                keyIdentifier
                getFn
                (fun _key _ _ _newValue ->
                    promise { failwith $"asyncReadSelectorFamily {storeRoot}/{collection}/{name} is read only." })
