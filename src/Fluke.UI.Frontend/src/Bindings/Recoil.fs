namespace Fluke.UI.Frontend.Bindings

open System
open Microsoft.FSharp.Core.Operators
open Fluke.Shared.Domain
open Fluke.Shared
open Feliz.Recoil
open Fluke.UI.Frontend
open Fable.Core.JsInterop
open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.Core


[<AutoOpen>]
module RecoilExtensions =
    type RecoilEffectProps<'T> =
        {
            node: {| key: string |}
            onSet: ('T -> 'T -> unit) -> unit
            trigger: string
            setSelf: ('T -> 'T) -> unit
        }

    type AtomStateWithEffects<'T, 'U, 'V> =
        {
            State: AtomState.ReadWrite<'T, 'U, 'V>
            Effects: (RecoilEffectProps<'T> -> unit -> unit) list
        }

    type AtomCE.AtomBuilder with
        [<CustomOperation("effects")>]
        member inline _.Effects
            (
                state: AtomState.ReadWrite<'T, 'U, 'V>,
                effects: (RecoilEffectProps<'T> -> unit -> unit) list
            ) : AtomStateWithEffects<'T, 'U, 'V> =
            { State = state; Effects = effects }

        member inline _.Run<'T, 'V> ({ Effects = effects; State = state }: AtomStateWithEffects<'T, 'T, 'V>) =
            Bindings.Recoil.atom<'T> (
                [
                    "key" ==> state.Key
                    "default" ==> state.Def
                    "effects_UNSTABLE" ==> effects
                    match state.Persist with
                    | Some persist ->
                        "persistence_UNSTABLE"
                        ==> PersistenceSettings.CreateObj persist
                    | None -> ()
                    match state.DangerouslyAllowMutability with
                    | Some dangerouslyAllowMutability ->
                        "dangerouslyAllowMutability"
                        ==> dangerouslyAllowMutability
                    | None -> ()
                ]
                |> createObj
            )

    type AtomFamilyStateWithEffects<'T, 'U, 'V, 'P> =
        {
            State: AtomFamilyState.ReadWrite<'P -> 'U, 'U, 'V, 'P>
            Effects: 'P -> (RecoilEffectProps<'T> -> unit -> unit) list
        }

    type AtomFamilyCE.AtomFamilyBuilder with
        [<CustomOperation("effects")>]
        member inline _.Effects
            (
                state: AtomFamilyState.ReadWrite<'P -> 'U, 'U, 'V, 'P>,
                effects: 'P -> (RecoilEffectProps<'T> -> unit -> unit) list
            ) : AtomFamilyStateWithEffects<'T, 'U, 'V, 'P> =
            { State = state; Effects = effects }


        member inline _.Run<'U, 'V, 'P>
            ({ Effects = effects; State = state }: AtomFamilyStateWithEffects<'U, 'U, 'V, 'P>)
            : 'P -> RecoilValue<'U, ReadWrite> =
            Bindings.Recoil.atomFamily<'U, 'P> (
                [
                    "key" ==> state.Key
                    "default" ==> state.Def
                    "effects_UNSTABLE" ==> effects
                    match state.Persist with
                    | Some persist ->
                        "persistence_UNSTABLE"
                        ==> PersistenceSettings.CreateObj persist
                    | None -> ()
                    match state.DangerouslyAllowMutability with
                    | Some dangerouslyAllowMutability ->
                        "dangerouslyAllowMutability"
                        ==> dangerouslyAllowMutability
                    | None -> ()
                ]
                |> createObj
            )

    type Recoil with
        static member inline atomWithProfiling
            (
                atomKey: string,
                defaultValue,
                ?effects,
                ?_persistence,
                ?_dangerouslyAllowMutability
            ) =
            Recoil.atom (atomKey, promise { return defaultValue }, effects |> Option.defaultValue [])

        static member inline atomFamilyWithProfiling
            (
                atomKey: string,
                defaultValue,
                ?atomEffects,
                ?_persistence,
                ?_dangerouslyAllowMutability
            ) =
            atomFamily {
                key atomKey

                def
                    (fun x ->
                        Profiling.addCount atomKey
                        defaultValue x)

                effects
                    (fun x ->
                        match atomEffects with
                        | Some y -> y x
                        | None -> [])

            }

        static member inline selectorWithProfiling
            (
                atomKey: string,
                getFn,
                ?setFn,
                ?_cacheImplementation,
                ?_paramCacheImplementation,
                ?_dangerouslyAllowMutability
            ) =
            selector {
                key atomKey

                get
                    (fun (getter: SelectorGetter) ->
                        let result : 'T = getFn getter
                        Profiling.addCount atomKey
                        result)

                set
                    (fun x y ->
                        match setFn with
                        | Some setFn ->
                            setFn x y
                            Profiling.addCount $"{atomKey} (SET)"
                        | None -> ())
            }

        static member inline asyncSelectorWithProfiling
            (
                atomKey: string,
                getFn,
                ?setFn,
                ?_cacheImplementation,
                ?_paramCacheImplementation,
                ?_dangerouslyAllowMutability
            ) =
            selector {
                key atomKey

                get
                    (fun (getter: SelectorGetter) ->
                        promise {
                            let! result = getFn getter
                            Profiling.addCount atomKey
                            return result
                        })

                set
                    (fun x y ->
                        match setFn with
                        | Some setFn ->
                            setFn x y
                            Profiling.addCount $"{atomKey} (SET)"
                        | None -> ())
            }

        static member inline selectorFamilyWithProfiling
            (
                atomKey: string,
                getFn,
                ?setFn,
                ?_cacheImplementation,
                ?_paramCacheImplementation,
                ?_dangerouslyAllowMutability
            ) =
            selectorFamily {
                key atomKey

                get
                    (fun x (getter: SelectorGetter) ->
                        let result : 'T = getFn x getter
                        Profiling.addCount atomKey
                        result)

                set
                    (fun x y z ->
                        match setFn with
                        | Some setFn ->
                            setFn x y z
                            Profiling.addCount $"{atomKey} (SET)"
                        | None -> ())
            }


module Recoil =
    let getGun () =
        JS.waitForObject (fun () -> box Browser.Dom.window?lastGun :?> Gun.IGunChainReference)

    let getGunAtomKey (username: UserInteraction.Username option) (rawAtomKey: string) (keyIdentifier: string list) =
        let userBlock =
            match username with
            | Some (UserInteraction.Username username) -> $"user/{username}/"
            | _ -> ""

        let atomPath =
            match (rawAtomKey.Split "__" |> Seq.head).Trim () with
            | String.ValidString atomPath when atomPath |> Seq.last = '/' ->
                atomPath |> String.take (atomPath.Length - 1)
            | String.ValidString atomPath -> atomPath
            | _ -> failwith $"Invalid rawAtomKey: {rawAtomKey}"

        let newAtomPath =
            match keyIdentifier with
            | [] -> atomPath
            | keyIdentifier when keyIdentifier |> List.head |> Guid.TryParse |> fst ->
                let nodes = atomPath.Split "/"

                [
                    yield! nodes |> Array.take (nodes.Length - 1)
                    yield! keyIdentifier
                    nodes |> Array.last
                ]
                |> String.concat "/"
            | keyIdentifier ->
                ([
                    atomPath
                 ]
                 @ keyIdentifier)
                |> String.concat "/"


        let result = $"{nameof Fluke}/{userBlock}{newAtomPath}"

        printfn $"getGunAtomKey. result={result}"

        result

    type InputAtom<'TValue1, 'TKey> =
        | Atom of RecoilValue<'TValue1, ReadWrite>
        | AtomFamily of ('TKey -> RecoilValue<'TValue1, ReadWrite>) * 'TKey
        | AtomKey of string

    let inline getGunAtomNode
        (username: UserInteraction.Username option)
        (atom: InputAtom<_, _>)
        (keyIdentifier: string list)
        =
        async {
            let gunAtomKey =
                match atom with
                | Atom atom -> getGunAtomKey username atom.key keyIdentifier
                | AtomFamily (atomFamily, atomKey) -> getGunAtomKey username (atomFamily atomKey).key keyIdentifier
                | AtomKey key -> key

            let! gun = getGun ()
            return Gun.getGunAtomNode (Some gun) gunAtomKey, gunAtomKey
        }

    let inline gunEffect
        (username: UserInteraction.Username option)
        (atom: InputAtom<_, _>)
        (keyIdentifier: string list)
        =
        (fun (e: RecoilEffectProps<_>) ->
            match e.trigger with
            | "get" ->
                (async {
                    let! gunAtomNode, id = getGunAtomNode username atom keyIdentifier

                    match gunAtomNode with
                    | Some gunAtomNode ->
                        gunAtomNode.on
                            (fun data _key ->
                                let decoded = if box data = null then None else Gun.decode data

                                if not JS.isProduction && not JS.isTesting then
                                    if string (unbox decoded) = "HabitTracker" then
                                        Browser.Dom.window?decoded <- unbox decoded
                                        Browser.Dom.window?data <- unbox data
                                        Browser.Dom.window?data2 <- unbox View.View.HabitTracker
//                                        Browser.Dom.window?data3 <- (Gun.encode View.View.HabitTracker)
//                                        Browser.Dom.window?data4 <- Gun.decode (Gun.encode View.View.HabitTracker)
                                    printfn $"[gunEffect.onGunData()] id={id} data={unbox data} decoded={unbox decoded}"

                                match decoded with
                                | Some gunAtomNodeValue -> e.setSelf (fun _ -> gunAtomNodeValue)
                                | None -> ()
                                )
                    | None -> Browser.Dom.console.error $"[gunEffect.get] Gun node not found: {id}"
                 })
                |> Async.StartAsPromise
                |> Promise.start
            | _ -> ()

            e.onSet
                (fun value oldValue ->
                    (async {
                        if not JS.isProduction && not JS.isTesting then
                                printfn $"[gunEffect.onRecoilSet()] oldValue={oldValue}; jsTypeof-value={jsTypeof value} value={value}"

//                        let newValue = if jsTypeof value = "string" then value else Gun.encode value
//
                        if oldValue <> value then
                            let! gunAtomNode, id = getGunAtomNode username atom keyIdentifier

                            match gunAtomNode with
                            | Some gunAtomNode ->
                                Browser.Dom.window?value <- unbox value
                                let newValue = if box value = null then null else Gun.encode value
                                Gun.put gunAtomNode newValue

                                if not JS.isProduction && not JS.isTesting then
                                    printfn
                                        $"[gunEffect.onRecoilSet()] id={id} oldValue={oldValue}; newValue={newValue} jsTypeof-value={
                                                                                                                                         jsTypeof
                                                                                                                                             value
                                        }"
                            | None -> Browser.Dom.console.error $"[gunEffect.onRecoilSet] Gun node not found: {id}"
                        else
                            printfn $"[gunEffect.onRecoilSet()]. newValue==oldValue. skipping. value={value}"
                     })
                    |> Async.StartAsPromise
                    |> Promise.start)

            fun () ->
                (async {
                    let! gunAtomNode, id = getGunAtomNode username atom keyIdentifier

                    match gunAtomNode with
                    | Some gunAtomNode ->
                        if not JS.isProduction && not JS.isTesting then
                            printfn $"[gunEffect.off()] id={id} "

                        gunAtomNode.off () |> ignore
                    | None -> Browser.Dom.console.error $"[gunEffect.off()] Gun node not found: {id}"
                 })
                |> Async.StartAsPromise
                |> Promise.start)


    module Atoms =
        module rec Form =
            let rec readWriteValue =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Form}/{nameof readWriteValue}",
                    (fun (_key: string) -> null: string),
                    (fun (key: string) ->
                        [
                            (gunEffect
                                None
                                (AtomKey key)
                                [
                                    Guid.Empty |> string
                                ])
                        ])
                )

    let getAtomField atom =
        match atom with
        | Some (Atom atom) ->
            {|
                ReadOnly = Some atom
                ReadWrite =
                    Atoms.Form.readWriteValue (
                        getGunAtomKey
                            None
                            atom.key
                            [
                                Guid.Empty |> string
                            ]
                    )
                    |> Some
            |}
        | Some (AtomFamily (atom, key)) ->
            {|
                ReadOnly = Some (atom key)
                ReadWrite =
                    Atoms.Form.readWriteValue (
                        getGunAtomKey
                            None
                            (atom key).key
                            [
                                Guid.Empty |> string
                            ]
                    )
                    |> Some
            |}
        | _ -> {| ReadOnly = None; ReadWrite = None |}

    //    let useValueDefault<'TValue, 'TKey, 'TPerm when 'TPerm :> ReadOnly>
//        (atom: 'TKey -> RecoilValue<'TValue, 'TPerm>)
//        (key: 'TKey option)
//        =
//        let atom =
//            match key with
//            | Some key -> atom key
//            | None -> box (RecoilValue.lift null) :?> RecoilValue<'TValue, 'TPerm>
//
//        Recoil.useValue atom
//

    let useStateKeyDefault (atom: 'TKey -> RecoilValue<'TValue5, ReadWrite>) (key: 'TKey option) =
        let atom =
            match key with
            | Some key -> atom key
            | None -> box (RecoilValue.lift null) :?> RecoilValue<'TValue5, ReadWrite>

        let value, setValue = Recoil.useState atom

        value, (if key.IsNone then (fun _ -> ()) else setValue)

    let useStateDefault (atom: RecoilValue<'TValue6, ReadWrite> option) =
        let flatAtom =
            match atom with
            | Some atom -> atom
            | None -> box (RecoilValue.lift null) :?> RecoilValue<'TValue6, ReadWrite>

        let value, setValue = Recoil.useState flatAtom

        value, (if atom.IsNone then (fun _ -> ()) else setValue)


    [<RequireQualifiedAccess>]
    type AtomScope =
        | ReadOnly
        | ReadWrite

    let useAtomField atom atomScope =
        let atomField = getAtomField atom

        let readOnlyValue, setReadOnlyValue = useStateDefault atomField.ReadOnly
        let readWriteValue, setReadWriteValue = useStateDefault atomField.ReadWrite

        let atomFieldOptions =
            React.useMemo (
                (fun () ->
                    let readWriteValue = if readWriteValue = null then readOnlyValue else unbox (Gun.decode readWriteValue)

                    let setReadWriteValue =
                        if atom.IsSome then
                            (fun newValue -> setReadWriteValue (Gun.encode newValue))
                        else
                            (fun _ -> ())

                    let setReadOnlyValue = if atom.IsSome then setReadOnlyValue else (fun _ -> ())

                    {|
                        ReadWriteValue = readWriteValue
                        SetReadWriteValue = setReadWriteValue
                        ReadOnlyValue = readOnlyValue
                        SetReadOnlyValue = setReadOnlyValue
                        AtomField = atomField
                        AtomValue =
                            match atomScope with
                            | Some AtomScope.ReadOnly -> readOnlyValue
                            | _ -> readWriteValue
                        SetAtomValue =
                            match atomScope with
                            | Some AtomScope.ReadOnly -> setReadOnlyValue
                            | _ -> setReadWriteValue
                    |}),
                [|
                    box atomScope
                    box atom
                    box atomField
                    box readOnlyValue
                    box readWriteValue
                    box setReadOnlyValue
                    box setReadWriteValue
                |]
            )

        atomFieldOptions


[<AutoOpen>]
module RecoilGetterExtensions =

    type CallbackMethods with
        member inline this.readWriteReset (atom: 'TKey -> RecoilValue<'TValue8, ReadWrite>) key =
            promise {
                let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key)))

                match atomField.ReadWrite with
                | Some atom ->
                    let atomKey =
                        Recoil.getGunAtomKey
                            None
                            atom.key
                            [
                                Guid.Empty |> string
                            ]

                    this.set (Recoil.Atoms.Form.readWriteValue atomKey, null)
                | _ -> ()
            }

        member inline this.readWriteSet (atom: 'TKey -> RecoilValue<'TValue9, ReadWrite>, key: 'TKey, value: 'TValue9) =
            let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key)))

            match atomField.ReadWrite with
            | Some atom -> this.set (atom, value |> Gun.encode)
            | _ -> ()


        member this.scopedSet
            (atomScope: Recoil.AtomScope)
            (atom: 'TKey -> RecoilValue<'TValue10, ReadWrite>, key: 'TKey, value: 'TValue10)
            =
            match atomScope with
            | Recoil.AtomScope.ReadOnly -> this.set (atom key, value)
            | Recoil.AtomScope.ReadWrite -> this.readWriteSet (atom, key, value)

    type Snapshot with
        member this.getReadWritePromise (atom: 'TKey -> RecoilValue<'TValue11, ReadWrite>) key =
            promise {
                let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key)))

                match atomField.ReadWrite with
                | Some readWriteAtom ->
                    let! value = this.getPromise readWriteAtom

                    match Gun.decode value with
                    | Some result -> return result
                    | None -> return! this.getPromise (atom key)
                | _ -> return! this.getPromise (atom key)
            }


//// TODO: move to recoilize file?
//module Recoilize =
//    let recoilizeDebugger<'T> =
//        //        importDefault "recoilize"
//        nothing |> composeComponent

//    module Effects =
//        type Wrapper = { Value: string }
//
//        let localStorage<'T> =
//            (fun ({ node = node; onSet = onSet; setSelf = setSelf }: Recoil.EffectProps<'T>) ->
//                let storageJson =
//                    Browser.Dom.window.localStorage.getItem node.key
//                    |> Option.ofObj
//
//                let value =
//                    //                    let parsed1 = Fable.Core.JS.JSON.parse storageJson :?> Wrapper<'T> option
//                    let parsed2 =
//                        match storageJson with
//                        | Some json ->
//                            try
//                                Json.parseAs<Wrapper> json
//                            with ex ->
//                                printfn "simplejson error: %A" ex
//                                Fable.Core.JS.JSON.parse json :?> Wrapper
//                        | None -> { Value = "" }
//                    //                        | Ok wrapper -> wrapper.Value
//                        | Error error ->
//                            printfn "Internal Specific Json parse error (input: %A): %A" storageJson error
//                            None
//
//                    //                    let parsed3 =
//                        let decoder : Decoder<Wrapper<'T>> =
//                            Decode.object
//                                (fun get -> { Value = get.Required.Field "value" Decode.string })
//                        Thoth.Json.Decode.fromString decoder storageJson
//                        |> function
//                            | Ok x -> Some x
//                            | Error error ->
//                                printfn "Internal Specific Json parse error (input: %A): %A" storageJson error
//                                None
//
//                    //                    match Thoth.Json.Decode.Auto.fromString<Wrapper<'T>> storageJson with
//                    match parsed2 with
//                    //                    | Ok wrapper -> Some wrapper.Value
//                    | Error error ->
//                        printfn "json parse error (input: %A): %A" storageJson error
//                        None
//                    | { Value = value } -> setSelf (unbox value)
//                    | _ -> printfn "json parse error (key: %A; input: %A)" node.key storageJson
//
//
//                onSet (fun value _oldValue ->
//                    printfn "onSet. oldValue: %Avalue: %A" _oldValue value
//
//                    let valueJson = Json.serialize value
//                    let wrapper = { Value = valueJson }
//                    let json = Json.serialize wrapper
//                    //                    let json = Thoth.Json.Encode.Auto.toString (0, wrapper)
//                    let json = Fable.Core.JS.JSON.stringify wrapper
//                    Browser.Dom.window.localStorage.setItem (node.key, json))
//
//                //    // Subscribe to storage updates
//                //    storage.subscribe(value => setSelf(value));
//
//                fun () -> printfn "> unsubscribe")
