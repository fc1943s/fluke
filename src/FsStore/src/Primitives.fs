namespace FsStore

open Fable.Core.JsInterop
open Fable.Core
open Microsoft.FSharp.Core.Operators
open FsCore
open FsJs
open FsStore.Bindings.Jotai

[<AutoOpen>]
module PrimitivesMagic =
    module Store =
        let inline atom<'TValue> (atomPath, defaultValue: 'TValue) =
            jotai.atom (
                (fun () ->
                    Profiling.addCount atomPath
                    defaultValue)
                    ()
            )
            |> registerAtom atomPath None
            |> fst

        let inline atomFamily<'TKey, 'TValue> (atomPath, defaultValueFn: 'TKey -> 'TValue) =
            jotaiUtils.atomFamily (fun param -> atom (atomPath, defaultValueFn param)) Object.compare

        let inline selector<'TValue>
            (
                atomPath,
                keyIdentifier,
                getFn: GetFn -> 'TValue,
                setFn: GetFn -> SetFn -> 'TValue -> unit
            ) =
            jotai.atom (
                (fun getter ->
                    Profiling.addCount atomPath
                    getFn getter),
                Some
                    (fun getter setter value ->
                        Profiling.addCount $"{atomPath} set"
                        let newValue = value
                        //                        match jsTypeof value with
                        //                         | "function" -> (unbox value) () |> unbox
                        //                         | _ -> value
                        setFn getter setter newValue)
            )
            |> registerAtom atomPath keyIdentifier
            |> fst


        let inline readSelector<'TValue> (atomPath, getFn: GetFn -> 'TValue) =
            selector (
                atomPath,
                None,
                getFn,
                (fun _ _ _ ->
                    Profiling.addCount $"{atomPath} set"
                    failwith "readonly selector")
            )

        let inline readSelectorFamily<'TKey, 'TValue>
            (
                atomPath,
                getFn: 'TKey -> GetFn -> 'TValue
            ) : ('TKey -> Atom<'TValue>) =
            jotaiUtils.atomFamily (fun param -> readSelector (atomPath, getFn param)) Object.compare

        let inline value<'TValue> (getter: GetFn) (atom: Atom<'TValue>) : 'TValue = (getter (unbox atom)) :?> 'TValue

        let inline set<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue) =
            setter (atom |> box |> unbox) value

        let inline change<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue -> 'TValue) =
            setter (atom |> box |> unbox) value

        let inline selectAtom (atomPath: string, atom, selector) =
            //        readSelector (
            //            atomPath,
            //            fun getter ->
            //                let value = value getter atom
            //                Profiling.addCount $"{atomPath} :selectAtom"
            //                selector value
            //        )

            jotaiUtils.selectAtom
                atom
                (fun value ->
                    Profiling.addCount $"{atomPath} :selectAtom"
                    selector value)
                JS.undefined

        let inline selectAtomFamily (atomPath, atom, selector) =
            jotaiUtils.atomFamily (fun param -> selectAtom (atomPath, atom, selector param)) Object.compare

        let inline atomWithStorage (collection, atomPath, defaultValue) =
            let internalAtom = jotaiUtils.atomWithStorage atomPath defaultValue

            let wrapper =
                jotai.atom (
                    (fun getter -> value getter internalAtom),
                    Some
                        (fun _ setter argFn ->
                            let arg =
                                match jsTypeof argFn with
                                | "function" -> (argFn |> box |> unbox) () |> unbox
                                | _ -> argFn

                            set setter internalAtom arg)
                )

            wrapper
            |> registerAtom atomPath (Some (collection, []))
            |> fst


        let inline asyncSelector<'TValue>
            (
                atomPath,
                keyIdentifier,
                getFn: GetFn -> JS.Promise<'TValue>,
                setFn: GetFn -> SetFn -> 'TValue -> JS.Promise<unit>
            ) =
            jotai.atom (
                (fun getter ->
                    promise {
                        Profiling.addCount $"{atomPath}"
                        let a = getFn getter
                        return! a
                    }),
                Some
                    (fun getter setter newValue ->
                        promise {
                            Profiling.addCount $"{atomPath} set"
                            do! setFn getter setter newValue
                        })
            )
            |> registerAtom atomPath keyIdentifier
            |> fst

        let inline asyncReadSelector<'TValue> (atomPath, getFn: GetFn -> JS.Promise<'TValue>) =
            asyncSelector (
                atomPath,
                None,
                getFn,
                (fun _ _ _newValue -> promise { failwith $"readonly selector {atomPath}" })
            )

        let inline selectorFamily<'TKey, 'TValue>
            (
                atomPath,
                getFn: 'TKey -> GetFn -> 'TValue,
                setFn: 'TKey -> GetFn -> SetFn -> 'TValue -> unit
            ) =
            jotaiUtils.atomFamily (fun param -> selector (atomPath, None, getFn param, setFn param)) Object.compare


        let inline asyncSelectorFamily<'TKey, 'TValue>
            (
                atomPath,
                getFn: 'TKey -> GetFn -> JS.Promise<'TValue>,
                setFn: 'TKey -> GetFn -> SetFn -> 'TValue -> JS.Promise<unit>
            ) =
            jotaiUtils.atomFamily
                (fun param ->
                    asyncSelector (
                        (atomPath,
                         None,
                         (getFn param),
                         (fun getter setter newValue -> promise { do! setFn param getter setter newValue }))
                    ))
                Object.compare

        let inline asyncReadSelectorFamily<'TKey, 'TValue> (atomPath, getFn: 'TKey -> GetFn -> JS.Promise<'TValue>) =
            asyncSelectorFamily (
                atomPath,
                getFn,
                (fun _key _ _ _newValue -> promise { failwith $"readonly selector family {atomPath}" })
            )
