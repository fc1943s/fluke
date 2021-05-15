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


        let result = $"{nameof Gun}{nameof Recoil}/{userBlock}{newAtomPath}"

        JS.log (fun () -> $"getGunAtomKey. result={result}")

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

    let inline gunEffect<'TValue3, 'TKey>
        (username: UserInteraction.Username option)
        (atom: InputAtom<'TValue3, 'TKey>)
        (keyIdentifier: string list)
        =
        (fun (e: RecoilEffectProps<'TValue3>) ->
            match e.trigger with
            | "get" ->
                (async {
                    let! gunAtomNode, id = getGunAtomNode username atom keyIdentifier

                    match gunAtomNode with
                    | Some gunAtomNode ->
                        gunAtomNode.on
                            (fun data _key ->
                                Browser.Dom.window?data <- unbox data

                                JS.log
                                    (fun () ->
                                        $"[gunEffect.onGunData1()] id={id} data={unbox data}; typeof data={jsTypeof data}; data.stringify={
                                                                                                                                               JS.JSON.stringify
                                                                                                                                                   data
                                        }")

                                let decoded = if box data = null then unbox null else Gun.jsonDecode<'TValue3> data
                                Browser.Dom.window?decoded <- unbox decoded

                                JS.log
                                    (fun () ->
                                        //                                            Browser.Dom.window?data2 <- unbox View.View.HabitTracker
//                                            Browser.Dom.window?data3 <- (Gun.jsonEncode View.View.HabitTracker)
//                                            Browser.Dom.window?data4 <- Gun.jsonDecode (Gun.jsonEncode View.View.HabitTracker)

                                        $"[gunEffect.onGunData2()] id={id} decoded={unbox decoded}; typeof decoded={
                                                                                                                        jsTypeof
                                                                                                                            decoded
                                        }; decoded.stringify={JS.JSON.stringify decoded}")

                                e.setSelf (fun _ -> unbox decoded))
                    | None -> Browser.Dom.console.error $"[gunEffect.get] Gun node not found: {id}"
                 })
                |> Async.StartAsPromise
                |> Promise.start
            | _ -> ()

            e.onSet
                (fun value oldValue ->
                    (async {
                        Browser.Dom.window?oldValue <- unbox oldValue
                        Browser.Dom.window?value <- unbox value

                        let! _, tempId = getGunAtomNode username atom keyIdentifier

                        JS.log
                            (fun () ->
                                $"[gunEffect.onRecoilSet1()] tempId={tempId} typeof value={jsTypeof value}
                                 typeof oldValue={jsTypeof oldValue}; oldValue={oldValue}; oldValue.stringify={
                                                                                                                   JS.JSON.stringify
                                                                                                                       oldValue
                                };
                                 typeof value={jsTypeof value}; value={value}; value.stringify={JS.JSON.stringify value};")

                        //                        let newValue = if jsTypeof value = "string" then value else Gun.encode value
//

                        let newValueJson =
                            if (JS.ofObjDefault null (box value)) = null then
                                null
                            else
                                Gun.jsonEncode<'TValue3> value

                        let getOldValueJson () =
                            if (JS.ofObjDefault null (box oldValue)) = null then
                                null
                            else
                                Gun.jsonEncode<'TValue3> oldValue

                        // TODO: remove?
                        if jsTypeof value <> jsTypeof oldValue
                           || getOldValueJson () <> newValueJson then
                            let! gunAtomNode, id = getGunAtomNode username atom keyIdentifier

                            match gunAtomNode with
                            | Some gunAtomNode ->
                                Gun.put gunAtomNode newValueJson

                                JS.log
                                    (fun () ->
                                        $"[gunEffect.onRecoilSet2()] id={id} oldValue={oldValue}; newValueJson={
                                                                                                                    newValueJson
                                        } jsTypeof-value={jsTypeof value}")
                            | None -> Browser.Dom.console.error $"[gunEffect.onRecoilSet] Gun node not found: {id}"
                        else
                            JS.log (fun () -> $"[gunEffect.onRecoilSet()]. newValue==oldValue. skipping. value={value}")
                     })
                    |> Async.StartAsPromise
                    |> Promise.start)

            fun () ->
                (async {
                    let! gunAtomNode, id = getGunAtomNode username atom keyIdentifier

                    match gunAtomNode with
                    | Some gunAtomNode ->
                        JS.log (fun () -> $"[gunEffect.off()] id={id} ")

                        gunAtomNode.off () |> ignore
                    | None -> Browser.Dom.console.error $"[gunEffect.off()] Gun node not found: {id}"
                 })
                |> Async.StartAsPromise
                |> Promise.start)


    let atomFormGuid = Guid.Parse "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"

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
                                    string atomFormGuid
                                ])
                        ])
                )

    module Selectors =
        let rec atomOption<'TValue, 'TKey> =
            Recoil.selectorFamilyWithProfiling (
                $"{nameof selectorFamily}/{nameof atomOption}",
                (fun (atom: RecoilValue<'TValue, ReadWrite> option) getter -> atom |> Option.map getter.get),
                (fun (atom: RecoilValue<'TValue, ReadWrite> option) setter newValue ->
                    match atom, newValue with
                    | Some atom, Some newValue -> setter.set (atom, newValue)
                    | _ -> ())
            )

    type AtomField<'TValue67> =
        {
            ReadOnly: RecoilValue<'TValue67, ReadWrite> option
            ReadWrite: RecoilValue<string, ReadWrite> option
        }

    let getAtomField atom =
        match atom with
        | Some (Atom atom) ->
            {
                ReadOnly = Some atom
                ReadWrite =
                    Atoms.Form.readWriteValue (
                        getGunAtomKey
                            None
                            atom.key
                            [
                                atomFormGuid |> string
                            ]
                    )
                    |> Some
            }
        | Some (AtomFamily (atom, key)) ->
            {
                ReadOnly = Some (atom key)
                ReadWrite =
                    Atoms.Form.readWriteValue (
                        getGunAtomKey
                            None
                            (atom key).key
                            [
                                atomFormGuid |> string
                            ]
                    )
                    |> Some
            }
        | _ -> { ReadOnly = None; ReadWrite = None }

    let useStateKeyDefault
        (atom: 'TKey -> RecoilValue<'TValue5, ReadWrite>)
        (key: 'TKey option)
        (defaultValue: 'TValue5)
        =
        let atom =
            match key with
            | Some key -> atom key
            | None -> box (RecoilValue.lift defaultValue) :?> RecoilValue<'TValue5, ReadWrite>

        let value, setValue = Recoil.useState atom

        value, (if key.IsNone then (fun _ -> ()) else setValue)


    //        let rec atomKeyOption<'TValue, 'TKey> =
//            Recoil.selectorFamilyWithProfiling (
//                $"{nameof selectorFamily}/{nameof atomKeyOption}",
//                (fun (props: {| Atom: 'TKey -> RecoilValue<'TValue, ReadWrite>; Key: 'TKey option |}) getter ->
//                    props.Key |> Option.map (props.Atom >> getter.get)),
//                (fun (props: {| Atom: 'TKey -> RecoilValue<'TValue, ReadWrite>; Key: 'TKey option |}) setter newValue ->
//                    match props.Key, newValue with
//                    | Some key, Some newValue -> setter.set (props.Atom key, newValue)
//                    | _ -> ())
//            )


    [<RequireQualifiedAccess>]
    type AtomScope =
        | ReadOnly
        | ReadWrite

    [<RequireQualifiedAccess>]
    type InputScope<'TValue> =
        | ReadOnly
        | ReadWrite of Gun.Serializer<'TValue>

    let useAtomFieldOptions<'TValue7, 'TKey>
        (atom: InputAtom<'TValue7, 'TKey> option)
        (inputScope: InputScope<'TValue7> option)
        =
        let atomField = getAtomField atom

        let readOnlyValue, setReadOnlyValue = Recoil.useState (Selectors.atomOption<'TValue7, 'TKey> atomField.ReadOnly)

        let readWriteValue, setReadWriteValue =
            Recoil.useState (Selectors.atomOption<string, 'TKey> atomField.ReadWrite)

        React.useMemo (
            (fun () ->
                let defaultJsonEncode, defaultJsonDecode = unbox Gun.defaultSerializer

                let newReadWriteValue =
                    match readWriteValue |> Option.defaultValue null with
                    | readWriteValue when readWriteValue <> null ->
                        printfn
                            $"useAtomFieldOptions. readOnlyValue={readOnlyValue} readWriteValue={readWriteValue} typeof readWriteValue={
                                                                                                                                            jsTypeof
                                                                                                                                                readWriteValue
                            }"

                        Browser.Dom.window?readWriteValue <- readWriteValue

                        match inputScope with
                        | Some (InputScope.ReadWrite (_, jsonDecode)) ->
                            printfn $"useAtomFieldOptions. decoder readWriteValue={jsonDecode readWriteValue}"
                            jsonDecode readWriteValue
                        | _ -> defaultJsonDecode readWriteValue
                    | _ -> readOnlyValue |> Option.defaultValue (unbox null)

                let setReadWriteValue =
                    if atom.IsSome then
                        (fun newValue ->
                            printfn
                                $"useAtomFieldOptions. ONSET. newValue={newValue} typeof newValue={jsTypeof newValue}"

                            Browser.Dom.window?newValue <- readWriteValue

                            setReadWriteValue (
                                if box newValue = null then
                                    None
                                else
                                    Some (
                                        match inputScope with
                                        | Some (InputScope.ReadWrite (jsonEncode, _)) -> jsonEncode newValue
                                        | _ -> defaultJsonEncode newValue
                                    )
                            ))
                    else
                        (fun _ -> ())

                let setReadOnlyValue = if atom.IsSome then setReadOnlyValue else (fun _ -> ())

                {|
                    ReadWriteValue = newReadWriteValue
                    SetReadWriteValue = setReadWriteValue
                    ReadOnlyValue = readOnlyValue |> Option.defaultValue (unbox null)
                    SetReadOnlyValue = setReadOnlyValue
                    AtomField = atomField
                    AtomValue =
                        match inputScope with
                        | Some (InputScope.ReadWrite _) -> newReadWriteValue
                        | _ -> readOnlyValue |> Option.defaultValue (unbox null)
                    SetAtomValue =
                        match inputScope with
                        | Some (InputScope.ReadWrite _) -> setReadWriteValue
                        | _ -> Some >> setReadOnlyValue
                |}),
            [|
                box inputScope
                box atom
                box atomField
                box readOnlyValue
                box readWriteValue
                box setReadOnlyValue
                box setReadWriteValue
            |]
        )


[<AutoOpen>]
module RecoilGetterExtensions =

    type CallbackMethods with
        member inline this.readWriteReset<'TValue8, 'TKey> (atom: 'TKey -> RecoilValue<'TValue8, ReadWrite>) key =
            promise {
                let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key)))

                match atomField.ReadWrite with
                | Some atom ->
                    let atomKey =
                        Recoil.getGunAtomKey
                            None
                            atom.key
                            [
                                Recoil.atomFormGuid |> string
                            ]

                    this.set (Recoil.Atoms.Form.readWriteValue atomKey, null)
                | _ -> ()
            }

        member inline this.readWriteSet<'TValue9, 'TKey>
            (
                atom: 'TKey -> RecoilValue<'TValue9, ReadWrite>,
                key: 'TKey,
                value: 'TValue9
            ) =
            let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key)))

            match atomField.ReadWrite with
            | Some atom -> this.set (atom, value |> Gun.jsonEncode<'TValue9>)
            | _ -> ()


        member inline this.scopedSet<'TValue10, 'TKey>
            (atomScope: Recoil.AtomScope)
            (atom: 'TKey -> RecoilValue<'TValue10, ReadWrite>, key: 'TKey, value: 'TValue10)
            =
            match atomScope with
            | Recoil.AtomScope.ReadOnly -> this.set (atom key, value)
            | Recoil.AtomScope.ReadWrite -> this.readWriteSet<'TValue10, 'TKey> (atom, key, value)

    type Snapshot with
        member inline this.getReadWritePromise<'TValue11, 'TKey>
            (atom: 'TKey -> RecoilValue<'TValue11, ReadWrite>)
            key
            =
            promise {
                let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key)))

                match atomField.ReadWrite with
                | Some readWriteAtom ->
                    let! value = this.getPromise readWriteAtom

                    if value <> null then
                        return Gun.jsonDecode<'TValue11> value
                    else
                        return! this.getPromise (atom key)
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
