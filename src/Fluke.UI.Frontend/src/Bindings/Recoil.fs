namespace Fluke.UI.Frontend.Bindings

open Fluke.Shared.Domain.UserInteraction


#nowarn "40"

open System
open Microsoft.FSharp.Core.Operators
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
    let parseValidGuid fn (text: string) =
        match Guid.TryParse text with
        | true, guid when guid = Guid.Empty -> None
        | true, guid -> Some (fn guid)
        | _ -> None

    let atomPathFromRawAtomKey (rawAtomKey: string) =
        match (rawAtomKey.Split "__" |> Seq.head).Trim () with
        | String.ValidString atomPath when atomPath |> Seq.last = '/' -> atomPath |> String.take (atomPath.Length - 1)
        | String.ValidString atomPath -> atomPath
        | _ -> failwith $"Invalid rawAtomKey: {rawAtomKey}"


    let wrapAtomPath (atomPath: string) =
        let header = $"{nameof Gun}{nameof Recoil}/"
        let header = if atomPath.StartsWith header then "" else header
        let result = $"{header}{atomPath}"

        //        JS.log (fun () -> $"wrapAtomPath. result={result} atomPath={atomPath}")
//
        result

    type InputAtom<'TValue1, 'TKey> =
        | Atom of Username * RecoilValue<'TValue1, ReadWrite>
        | AtomFamily of Username * ('TKey -> RecoilValue<'TValue1, ReadWrite>) * 'TKey
        | AtomPath of Username * atomPath: string


    let getAtomPath (rawAtomKey: string) (keyIdentifier: string list) =
        let atomPath = atomPathFromRawAtomKey rawAtomKey

        let newAtomPath =
            match keyIdentifier with
            | [] -> atomPath
            | keyIdentifier when keyIdentifier |> List.head |> Guid.TryParse |> fst ->
                let nodes = atomPath.Split "/"

                [
                    yield! nodes |> Array.take (nodes.Length - 2)

                    let secondLast = nodes.[nodes.Length - 2]

                    if secondLast |> Guid.TryParse |> fst then
                        yield! keyIdentifier
                        yield secondLast
                    else
                        yield secondLast
                        yield! keyIdentifier

                    yield nodes.[nodes.Length - 1]
                ]
                |> String.concat "/"
            | keyIdentifier ->
                ([
                    atomPath
                 ]
                 @ keyIdentifier)
                |> String.concat "/"

        wrapAtomPath newAtomPath

    let getGun () =
        JS.waitForObject (fun () -> async { return box Browser.Dom.window?lastGun :?> Gun.IGunChainReference })

    //    let getUsername () =
//        async {
//            let! gun = getGun2 ()
//            let user = gun.user ()
//
//            return
//                user.is.alias
//                |> Option.defaultValue ""
//                |> Username
//        }

    let rec getInternalGunAtomNode (Username username) (atomPath: string) =
        async {
            let! gun = getGun ()

            let user = gun.user ()

            return!
                match user.is with
                | Some { alias = Some username' } when username' = username ->
                    async {
                        let nodes = atomPath.Split "/" |> Array.toList
                        let head = nodes.[0]
                        let tail = nodes |> List.skip 1

                        return
                            (Some (user.get head), tail)
                            ||> List.fold
                                    (fun result node ->
                                        result
                                        |> Option.map (fun result -> result.get node))
                    }
                | _ ->
                    async {
                        if JS.isTesting then
                            //                            if not Browser.Dom.window?creatingUser then
//                                Browser.Dom.window?creatingUser <- true
//                                let username = Templates.templatesUser.Username |> Username.Value
//
//                                printfn $"1# {JS.JSON.stringify user.is}"
////                                let! ack1 = Gun.createUser user username username
//                                Gun.createUser user username username |> Promise.start
//                                do! Async.Sleep 300 |> Async.StartAsPromise
//                                do! Async.Sleep 1000 |> Async.StartAsPromise
//                                printfn "2#"
//                                let! ack2 = Gun.authUser user username username
//                                printfn $"# ack2={JS.JSON.stringify ack2}"
//                                if ack2.wait then
//                                    do! Async.Sleep 1 |> Async.StartAsPromise
//                                    let! ack3 = Gun.authUser user username username
//                                    printfn $"# ack3={JS.JSON.stringify ack3}"
//                                    ()
////                                printfn $"# ack1={JS.JSON.stringify ack1} ack2={JS.JSON.stringify ack2}"

                            do! Async.Sleep 100

                            return! getInternalGunAtomNode (Username username) atomPath
                        else
                            return
                                failwith
                                    $"Invalid username. username={username} user.is={JS.JSON.stringify user.is} username={
                                                                                                                              username
                                    } atomPath={atomPath}"
                    }
        }

    let inline getGunAtomNode (atom: InputAtom<_, _>) (keyIdentifier: string list) =
        async {
            let username, atomPath =
                match atom with
                | Atom (username, atom) -> username, getAtomPath atom.key keyIdentifier
                | AtomFamily (username, atomFamily, atomKey) ->
                    username, getAtomPath (atomFamily atomKey).key keyIdentifier
                | AtomPath (username, atomPath) -> username, atomPath

            let! gunAtomNode = getInternalGunAtomNode username atomPath
            return username, atomPath, gunAtomNode
        }

    let getGunAtomNodeParent (atom: InputAtom<_, _>) =
        async {
            let username, atomPath =
                match atom with
                | Atom (username, atom) -> username, wrapAtomPath (atomPathFromRawAtomKey atom.key)
                | AtomFamily (username, atomFamily, atomKey) ->
                    username, wrapAtomPath (atomPathFromRawAtomKey (atomFamily atomKey).key)
                | AtomPath (username, atomPath) -> username, atomPath

            let nodes = atomPath.Split "/"

            let atomPath =
                nodes
                |> Array.take (nodes.Length - 1)
                |> String.concat "/"

            let! gunAtomNode = getInternalGunAtomNode username atomPath
            return username, atomPath, gunAtomNode
        }

    let inline gunKeyEffect<'TAtomValue, 'TValue, 'TKey when 'TValue: comparison>
        (atom: InputAtom<'TAtomValue, 'TKey>)
        (onValidate: string -> 'TValue option)
        =
        (fun (e: RecoilEffectProps<Set<'TValue>>) ->
            match e.trigger with
            | "get" ->
                (async {
                    let! _, atomPath, gunAtomNode = getGunAtomNodeParent atom

                    JS.log (fun () -> $"@@@- [gunKeyEffect] atomPath={atomPath}")

                    match gunAtomNode with
                    | Some gunAtomNode ->
                        printfn $"AA atomPath={atomPath} gunAtomNode={gunAtomNode}"

                        gunAtomNode.on
                            (fun v _k ->
                                let keys =
                                    JS.Constructors.Object.keys v
                                    |> Seq.choose onValidate
                                    |> Set.ofSeq
                                JS.log (fun () -> $"-ON atomPath={atomPath} keys={keys}")
                                e.setSelf (fun _ -> keys)

                                //                                match onValidate k with
//                                | Some v -> e.setSelf (fun oldValue -> oldValue |> Set.add v)
//                                | None -> ()
                                )

//                        gunAtomNode
//                            .map()
//                            .on (fun _v k ->
//                                JS.log (fun () -> $"ON MAP atomPath={atomPath} k={k}")
//
//                                match onValidate k with
//                                | Some v -> e.setSelf (fun oldValue -> oldValue |> Set.add v)
//                                | None -> ())

                    | None -> Browser.Dom.console.error $"[gunSetEffect.effect] Gun node not found: atomPath={atomPath}"
                 })
                |> Async.StartAsPromise
                |> Promise.start
            | _ -> ()

            e.onSet (fun _ _ -> failwith "[gunSetEffect.effect] read only atom")

            fun () ->
                (async {
                    let! _, atomPath, gunAtomNode = getGunAtomNodeParent atom

                    match gunAtomNode with
                    | Some gunAtomNode ->

                        JS.log (fun () -> "[gunSetEffect.effect] unsubscribe atom. calling off()")

                        gunAtomNode.map().off () |> ignore
                    | None ->
                        Browser.Dom.console.error $"[gunSetEffect.effect.off] Gun node not found: atomPath={atomPath}"
                 })
                |> Async.StartAsPromise
                |> Promise.start)

    let inline userDecode<'TValue> data =
        async {
            try
                let! gun = getGun ()
                let user = gun.user ()
                let keys = user.__.sea

                match keys with
                | Some keys ->
                    let! verified = Gun.sea.verify data keys.pub |> Async.AwaitPromise

                    let! decrypted =
                        Gun.sea.decrypt verified keys
                        |> Async.AwaitPromise

                    let decoded =
                        decrypted
                        |> Gun.jsonEncode<string>
                        |> Gun.jsonDecode<'TValue>

                    //            printfn $"userDecode data={data} decrypted={decrypted} verified={JS.JSON.stringify verified} typeof decrypted={jsTypeof decrypted} typeof verified={jsTypeof verified}"

                    return decoded
                | None -> return failwith $"No keys found for user {user.is}"
            with ex ->
                Browser.Dom.console.error ("[exception5]", ex)
                return raise ex
        }

    let inline userEncode<'TValue> value =
        async {
            try
                let! gun = getGun ()
                let user = gun.user ()
                let keys = user.__.sea

                match keys with
                | Some keys ->
                    let json = Gun.jsonEncode<'TValue> value
                    let! encrypted = Gun.sea.encrypt json keys |> Async.AwaitPromise
                    let! signed = Gun.sea.sign encrypted keys |> Async.AwaitPromise
                    printfn $"userEncode. keys={keys} json={json} encrypted={encrypted} signed={signed}"
                    return signed
                | None -> return failwith $"No keys found for user {user.is}"
            with ex ->
                Browser.Dom.console.error ("[exception4]", ex)
                return raise ex
        }

    let inline gunEffect<'TValue3, 'TKey> (atom: InputAtom<'TValue3, 'TKey>) (keyIdentifier: string list) =
        (fun (e: RecoilEffectProps<'TValue3>) ->
            match e.trigger with
            | "get" ->
                (async {
                    let! _, atomPath, gunAtomNode = getGunAtomNode atom keyIdentifier

                    match gunAtomNode with
                    | Some gunAtomNode ->
                        gunAtomNode.on
                            (fun data _key ->
                                async {
                                    try
                                        let! decoded =
                                            if box data = null then
                                                unbox null |> Async.lift
                                            else
                                                userDecode<'TValue3> data

                                        //                                        JS.log
//                                            (fun () ->
//                                                $"[gunEffect.onGunData()] atomPath={atomPath} data={unbox data}; typeof data={
//                                                                                                                                  jsTypeof
//                                                                                                                                      data
//                                                }; decoded={unbox decoded}; typeof decoded={jsTypeof decoded};")

                                        e.setSelf (fun _ -> unbox decoded)
                                    with ex -> Browser.Dom.console.error ("[exception1]", ex)
                                }
                                |> Async.StartAsPromise
                                |> Promise.start)

                    | None -> Browser.Dom.console.error $"[gunEffect.get] Gun node not found: {atomPath}"
                 })
                |> Async.StartAsPromise
                |> Promise.start
            | _ -> ()

            e.onSet
                (fun value oldValue ->
                    (async {
                        try
                            let! _, tempId, _ = getGunAtomNode atom keyIdentifier

                            JS.log
                                (fun () ->
                                    $"[gunEffect.onRecoilSet1()] tempIdAtomPath={tempId} typeof value={jsTypeof value} typeof oldValue={
                                                                                                                                            jsTypeof
                                                                                                                                                oldValue
                                    }; oldValue={oldValue}; oldValue.stringify={JS.JSON.stringify oldValue}; typeof value={
                                                                                                                               jsTypeof
                                                                                                                                   value
                                    }; value={value}; value.stringify={JS.JSON.stringify value};")

                            let! newValueJson =
                                if (JS.ofObjDefault null (box value)) = null then
                                    null |> Async.lift
                                else
                                    userEncode<'TValue3> value

                            let! oldValueJson =
                                if (JS.ofObjDefault null (box oldValue)) = null then
                                    null |> Async.lift
                                else
                                    userEncode<'TValue3> oldValue

                            if oldValueJson <> newValueJson then
                                let! _, atomPath, gunAtomNode = getGunAtomNode atom keyIdentifier

                                match gunAtomNode with
                                | Some gunAtomNode ->
                                    Gun.put gunAtomNode newValueJson

                                    JS.log
                                        (fun () ->
                                            $"[gunEffect.onRecoilSet2()] atomPath={atomPath} oldValue={oldValue}; newValueJson={
                                                                                                                                    newValueJson
                                            } jsTypeof-value={jsTypeof value}")
                                | None ->
                                    Browser.Dom.console.error $"[gunEffect.onRecoilSet] Gun node not found: {atomPath}"
                            else
                                if (JS.ofObjDefault null (box oldValue)) <> null then
                                    JS.log
                                        (fun () ->
                                            $"[gunEffect.onRecoilSet()]. newValue==oldValue. skipping. value={value} oldValueJson={
                                                                                                                                       oldValueJson
                                            } newValueJson={newValueJson}")
                        with ex -> Browser.Dom.console.error ("[exception2]", ex)
                     })
                    |> Async.StartAsPromise
                    |> Promise.start)

            fun () ->
                (async {
                    try
                        let! _, atomPath, gunAtomNode = getGunAtomNode atom keyIdentifier

                        match gunAtomNode with
                        | Some gunAtomNode ->
                            JS.log (fun () -> $"[gunEffect.off()] atomPath={atomPath} ")

                            gunAtomNode.off () |> ignore
                        | None -> Browser.Dom.console.error $"[gunEffect.off()] Gun node not found: {atomPath}"
                    with ex -> Browser.Dom.console.error ("[exception3]", ex)
                 })
                |> Async.StartAsPromise
                |> Promise.start)

    module Atoms =
        module rec Form =
            let rec readWriteValue =
                Recoil.atomFamilyWithProfiling (
                    $"{nameof atomFamily}/{nameof Form}/{nameof readWriteValue}",
                    (fun (_username: Username, _guid: Guid) -> null: string),
                    (fun (username: Username, guid: Guid) ->
                        [
                            gunEffect
                                (AtomFamily (username, readWriteValue, (username, guid)))
                                [
                                    string guid
                                ]
                        ])
                )

    type ReadWriteValue =
        {
            AtomKey: string
            Value: string option
        }

    module Selectors =
        module rec Form =
            let rec readWriteValue =
                Recoil.selectorFamilyWithProfiling (
                    $"{nameof selectorFamily}/{nameof Form}/{nameof readWriteValue}",
                    (fun (username: Username, atomKey: string) getter ->
                        let value = getter.get (Atoms.Form.readWriteValue (username, Crypto.getTextGuidHash atomKey))

                        if value = null then
                            null
                        else
                            match Gun.jsonDecode<ReadWriteValue> value with
                            | { Value = Some value } -> value
                            | _ -> null),
                    (fun (username: Username, atomKey: string) setter newValue ->
                        let newValue =
                            Gun.jsonEncode
                                {
                                    AtomKey = atomKey
                                    Value = newValue |> Option.ofObj
                                }

                        setter.set (Atoms.Form.readWriteValue (username, Crypto.getTextGuidHash atomKey), newValue))
                )


    type AtomField<'TValue67> =
        {
            ReadOnly: RecoilValue<'TValue67, ReadWrite> option
            ReadWrite: RecoilValue<string, ReadWrite> option
        }

    let getAtomField atom =
        match atom with
        | Some (Atom (username, atom)) ->
            {
                ReadOnly = Some atom
                ReadWrite =
                    Selectors.Form.readWriteValue (username, atom.key)
                    |> Some
            }
        | Some (AtomFamily (username, atom, key)) ->
            {
                ReadOnly = Some (atom key)
                ReadWrite =
                    Selectors.Form.readWriteValue (username, (atom key).key)
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

    let useStateOption (atom: RecoilValue<'TValue5, ReadWrite> option) =
        let flatAtom =
            match atom with
            | Some atom -> atom
            | None -> box (RecoilValue.lift null) :?> RecoilValue<'TValue5, ReadWrite>

        let value, setValue = Recoil.useState flatAtom

        (if atom.IsNone then None else Some value), (if atom.IsNone then (fun _ -> ()) else setValue)


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
        let readOnlyValue, setReadOnlyValue = useStateOption atomField.ReadOnly
        let readWriteValue, setReadWriteValue = useStateOption atomField.ReadWrite

        React.useMemo (
            (fun () ->
                let defaultJsonEncode, defaultJsonDecode = unbox Gun.defaultSerializer

                let newReadWriteValue =
                    match readWriteValue |> Option.defaultValue null with
                    | readWriteValue when readWriteValue <> null ->
                        match inputScope with
                        | Some (InputScope.ReadWrite (_, jsonDecode)) -> jsonDecode readWriteValue
                        | _ -> defaultJsonDecode readWriteValue
                    | _ -> readOnlyValue |> Option.defaultValue (unbox null)

                let setReadWriteValue =
                    if atom.IsSome then
                        (fun newValue ->
                            setReadWriteValue (
                                if box newValue = null then
                                    null
                                else
                                    match inputScope with
                                    | Some (InputScope.ReadWrite (jsonEncode, _)) -> jsonEncode newValue
                                    | _ -> defaultJsonEncode newValue
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
                        | _ -> setReadOnlyValue
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
        member inline this.readWriteReset<'TValue8, 'TKey>
            (username: Username)
            (atom: 'TKey -> RecoilValue<'TValue8, ReadWrite>)
            key
            =
            promise {
                let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (username, atom, key)))

                match atomField.ReadWrite with
                | Some atom -> this.set (atom, null)
                | _ -> ()
            }

        member inline this.readWriteSet<'TValue9, 'TKey>
            (
                username: Username,
                atom: 'TKey -> RecoilValue<'TValue9, ReadWrite>,
                key: 'TKey,
                value: 'TValue9
            ) =
            let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (username, atom, key)))

            match atomField.ReadWrite with
            | Some atom -> this.set (atom, value |> Gun.jsonEncode<'TValue9>)
            | _ -> ()


        member inline this.scopedSet<'TValue10, 'TKey>
            (username: Username)
            (atomScope: Recoil.AtomScope)
            (atom: 'TKey -> RecoilValue<'TValue10, ReadWrite>, key: 'TKey, value: 'TValue10)
            =
            match atomScope with
            | Recoil.AtomScope.ReadOnly -> this.set (atom key, value)
            | Recoil.AtomScope.ReadWrite -> this.readWriteSet<'TValue10, 'TKey> (username, atom, key, value)

    type Snapshot with
        member inline this.getReadWritePromise<'TValue11, 'TKey>
            (username: Username)
            (atom: 'TKey -> RecoilValue<'TValue11, ReadWrite>)
            key
            =
            promise {
                let atomField = Recoil.getAtomField (Some (Recoil.AtomFamily (username, atom, key)))

                match atomField.ReadWrite with
                | Some readWriteAtom ->
                    let! value = this.getPromise readWriteAtom

                    match value with
                    | value when value <> null -> return Gun.jsonDecode<'TValue11> value
                    | _ -> return! this.getPromise (atom key)
                | _ -> return! this.getPromise (atom key)
            }

//// TODO: move to recoilize file?
//module Recoilize =
//    let recoilizeDebugger<'T> =
//        //        importDefault "recoilize"
//        nothing |> composeComponent
