namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fluke.Shared.Domain
open Fluke.Shared
open Feliz.Recoil
open Fluke.UI.Frontend
open Fable.Core.JsInterop
open Feliz
open React
open Fluke.UI.Frontend.Bindings
open Fable.Core


module Recoil =
    type EffectProps<'T> =
        {
            node: {| key: string |}
            onSet: ('T -> 'T -> unit) -> unit
            trigger: string
            setSelf: ('T -> 'T) -> unit
        }

    module Atoms =
        module rec Form =
            //            let rec initialValue =
//                atomFamily {
//                    key $"{nameof atomFamily}/{nameof Form}/{nameof initialValue}"
//                    def (fun (_key: obj) -> null: obj)
//                }

            let rec readWriteValue =
                atomFamily {
                    key $"{nameof atomFamily}/{nameof Form}/{nameof readWriteValue}"
                    def (fun (_key: obj) -> null: obj)
                }

    //            let rec readWriteValueMounted =
//                atomFamily {
//                    key $"{nameof atomFamily}/{nameof Form}/{nameof readWriteValueMounted}"
//                    def (fun (_key: obj) -> false)
//                }

    let wrapAtomField<'TValue, 'TKey> (atom: RecoilValue<'TValue, ReadWrite>) =
        {|
            ReadOnly = atom
            ReadWrite = box (Atoms.Form.readWriteValue atom.key) :?> RecoilValue<'TValue, ReadWrite>
        |}

    [<RequireQualifiedAccess>]
    type AtomScope =
        | ReadOnly
        | ReadWrite

    type InputAtom<'TValue, 'TKey> =
        | Atom of RecoilValue<'TValue, ReadWrite>
        | AtomFamily of ('TKey -> RecoilValue<'TValue, ReadWrite>) * 'TKey

    let getAtomField<'TValue, 'TKey> atom =
        let flatAtom =
            match atom with
            | Some (Atom atom) -> atom
            | Some (AtomFamily (atom, key: 'TKey)) -> atom key
            | _ -> (box (RecoilValue.lift null)) :?> RecoilValue<'TValue, ReadWrite>

        wrapAtomField<'TValue, 'TKey> flatAtom

    let useValueDefault<'TKey, 'TValue, 'TPerm when 'TPerm :> ReadOnly>
        (atom: 'TKey -> RecoilValue<'TValue, 'TPerm>)
        (key: 'TKey option)
        =
        let atom =
            match key with
            | Some key -> atom key
            | None -> box (RecoilValue.lift null) :?> RecoilValue<'TValue, 'TPerm>

        Recoil.useValue atom

    let useStateDefault<'TKey, 'TValue> (atom: 'TKey -> RecoilValue<'TValue, ReadWrite>) (key: 'TKey option) =
        let atom =
            match key with
            | Some key -> atom key
            | None -> box (RecoilValue.lift null) :?> RecoilValue<'TValue, ReadWrite>

        let value, setValue = Recoil.useState atom

        value, (if key.IsNone then (fun _ -> ()) else setValue)


    let useAtomField<'TValue, 'TKey when 'TValue: equality> atom atomScope =
        let atomField = getAtomField<'TValue, 'TKey> atom

        let readOnlyValue, setReadOnlyValue = Recoil.useState atomField.ReadOnly
        let readWriteValue, setReadWriteValue = Recoil.useState atomField.ReadWrite

        //        let setInitialValue = Recoil.useSetState (Atoms.Form.initialValue atomField.ReadOnly.key)
//
//        let readWriteValueMounted, setReadWriteValueMounted =
//            Recoil.useState (Atoms.Form.readWriteValueMounted atomField.ReadOnly.key)

        //        React.useEffect (
//            (fun () ->
//                if not readWriteValueMounted then
//                    setInitialValue readOnlyValue
//
//                    if readOnlyValue <> readWriteValue then
//                        setReadWriteValue readOnlyValue
//
//                    setReadWriteValueMounted true
//
//                ),
//            [|
//                box setInitialValue
//                box readWriteValueMounted
//                box setReadWriteValueMounted
//                box setReadWriteValue
//                box readOnlyValue
//                box readWriteValue
//            |]
//        )

        let atomFieldOptions =
            React.useMemo (
                (fun () ->
                    let readWriteValue = if box readWriteValue = null then readOnlyValue else readWriteValue
                    let setReadWriteValue = if atom.IsSome then setReadWriteValue else (fun _ -> ())
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
                    //                    box readWriteValueMounted
                    box atom
                    box atomField
                    box readOnlyValue
                    box readWriteValue
                    box setReadOnlyValue
                    box setReadWriteValue
                |]
            )

        atomFieldOptions

    let getGunAtomKey (username: UserInteraction.Username option) (atomKey: string) =

        let result =
            $"""{nameof Fluke}/{
                                    match username with
                                    | Some (UserInteraction.Username username) -> $"user/{username}/"
                                    | _ -> ""
            }{
                (atomKey.Split "__" |> Seq.head)
                    //                    .Replace("__withFallback", "")
//                    .Replace("\"", "")
//                    .Replace("\\", "")
//                    .Replace("__", "/")
//                    .Replace(".", "/")
//                    .Replace("[", "/")
//                    .Replace("]", "/")
//                    .Replace(",", "/")
//                    .Replace("//", "/")
                    .Trim ()
            }"""

        match result with
        | String.ValidString _ when result |> Seq.last = '/' -> result |> String.take (result.Length - 1)
        | _ -> result

    let getGun () =
        JS.waitForObject (fun () -> box Browser.Dom.window?lastGun :?> Gun.IGunChainReference<obj>)

    let getGunAtomNode (username: UserInteraction.Username option) (atom: RecoilValue<_, _>) (keySuffix: string) =
        async {
            let! gun = getGun ()

            let gunAtomKey = getGunAtomKey username atom.key

            let newId = $"""{gunAtomKey}{keySuffix}"""

            //            if not JS.isProduction && not JS.isTesting then
            //                printfn
////                    $"""getGunAtomNode. gunAtomKey={gunAtomKey} atom.key={atom.key} newKey={newId}
////                usernamestr={atom.key.Replace ((JSe.RegExp "__\[.*?\]"), "")} """

            //                printfn $"""getGunAtomNode. newId={newId}"""

            return Gun.getGunAtomNode gun newId, newId

        }

    let inline gunEffect
        (username: UserInteraction.Username option)
        (atomFamily: 'TKey -> RecoilValue<'TValue, _>)
        (atomKey: 'TKey)
        (keySuffix: string)
        =
        (fun (e: EffectProps<'TValue>) ->
            let atom = atomFamily atomKey

            match e.trigger with
            | "get" ->
                (async {
                    let! gunAtomNode, id = getGunAtomNode username atom keySuffix

                    gunAtomNode.on
                        (fun data key ->
                            if not JS.isProduction && not JS.isTesting then
                                printfn
                                    $"gunEffect. gunAtomNode.on() effect. id={id} key={key} data={JS.JSON.stringify data}"

                            match Gun.deserializeGunAtomNode data with
                            | Some gunAtomNodeValue -> e.setSelf (fun _ -> gunAtomNodeValue)
                            | None -> ())
                 })
                |> Async.StartAsPromise
                |> Promise.start
            | _ -> ()

            e.onSet
                (fun value oldValue ->
                    (async {
                        if oldValue <> value then
                            let! gunAtomNode, id = getGunAtomNode username atom keySuffix
                            Gun.putGunAtomNode gunAtomNode value

                            if not JS.isProduction && not JS.isTesting then
                                printfn
                                    $"gunEffect. onSet. id={id} oldValue: {JS.JSON.stringify oldValue}; newValue: {
                                                                                                                       JS.JSON.stringify
                                                                                                                           value
                                    }"
                        else
                            printfn $"gunEffect. onSet. value=oldValue. skipping. newValue: {JS.JSON.stringify value}"
                     })
                    |> Async.StartAsPromise
                    |> Promise.start)

            fun () ->
                (async {
                    let! gunAtomNode, _ = getGunAtomNode username atom keySuffix

                    if not JS.isProduction && not JS.isTesting then
                        printfn "gunEffect. unsubscribe atom. calling selected.off ()"

                    gunAtomNode.off () |> ignore
                 })
                |> Async.StartAsPromise
                |> Promise.start)


[<AutoOpen>]
module RecoilMagic =

    type CallbackMethods with
        member this.readWriteReset<'T, 'U> (atom: 'T -> RecoilValue<'U, ReadWrite>) key =
            promise {
                this.set (Recoil.Atoms.Form.readWriteValue (atom key).key, null)
            }

        member this.readWriteSet<'T, 'U> (atom: 'T -> RecoilValue<'U, ReadWrite>, key: 'T, value: 'U) =
            let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key)))
            this.set (atomField.ReadWrite, value)

        member this.scopedSet<'T, 'U>
            (atomScope: Recoil.AtomScope)
            (atom: 'T -> RecoilValue<'U, ReadWrite>, key: 'T, value: 'U)
            =
            match atomScope with
            | Recoil.AtomScope.ReadOnly -> this.set (atom key, value)
            | Recoil.AtomScope.ReadWrite -> this.readWriteSet (atom, key, value)

    type Snapshot with
        member this.getReadWritePromise atom key =
            promise {
                let! readOnlyValue = this.getPromise (atom key)

                let! readWriteValue =
                    this.getPromise
                        (Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key))))
                            .ReadWrite

                return if box readWriteValue = null then readOnlyValue else readWriteValue
            }


    type AtomStateWithEffects<'T, 'U, 'V> =
        {
            State: AtomState.ReadWrite<'T, 'U, 'V>
            Effects: (Recoil.EffectProps<'T> -> unit -> unit) list
        }

    type AtomCE.AtomBuilder with
        [<CustomOperation("effects")>]
        member inline _.Effects
            (
                state: AtomState.ReadWrite<'T, 'U, 'V>,
                effects: (Recoil.EffectProps<'T> -> unit -> unit) list
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
            Effects: 'P -> (Recoil.EffectProps<'T> -> unit -> unit) list
        }

    type AtomFamilyCE.AtomFamilyBuilder with
        [<CustomOperation("effects")>]
        member inline _.Effects
            (
                state: AtomFamilyState.ReadWrite<'P -> 'U, 'U, 'V, 'P>,
                effects: 'P -> (Recoil.EffectProps<'T> -> unit -> unit) list
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


// TODO: move to recoilize file?
module Recoilize =
    let recoilizeDebugger<'T> =
        //        importDefault "recoilize"
        nothing |> composeComponent

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
