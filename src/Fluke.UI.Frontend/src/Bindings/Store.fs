namespace Fluke.UI.Frontend.Bindings

#nowarn "40"


open Fluke.UI.Frontend.Bindings
open Fable.Core.JsInterop
open Fable.Core
open System
open Fluke.Shared
open Microsoft.FSharp.Core.Operators
open Feliz
open Browser.Types


module Store =
    open Jotai
    open JotaiTypes

    let inline selector<'TValue>
        (
            atomPath,
            keyIdentifier,
            getFn: GetFn -> 'TValue,
            setFn: GetFn -> SetFn -> 'TValue -> unit
        ) =
        jotai.atom (
            (fun get ->
                Profiling.addCount atomPath
                getFn get),
            Some
                (fun get set value ->
                    Profiling.addCount $"{atomPath} set"
                    let newValue = value
                    //                        match jsTypeof value with
//                         | "function" -> (unbox value) () |> unbox
//                         | _ -> value
                    setFn get set newValue)
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

    let inline asyncSelector<'TValue>
        (
            atomPath,
            keyIdentifier,
            getFn: GetFn -> JS.Promise<'TValue>,
            setFn: GetFn -> SetFn -> 'TValue -> JS.Promise<unit>
        ) =
        jotai.atom (
            (fun get ->
                promise {
                    Profiling.addCount $"{atomPath}"
                    let a = getFn get
                    return! a
                }),
            Some
                (fun get set newValue ->
                    promise {
                        Profiling.addCount $"{atomPath} set"
                        do! setFn get set newValue
                    })
        )
        |> registerAtom atomPath keyIdentifier
        |> fst

    let inline readSelectorFamily<'TKey, 'TValue>
        (
            atomPath,
            getFn: 'TKey -> GetFn -> 'TValue
        ) : ('TKey -> Atom<'TValue>) =
        jotaiUtils.atomFamily
            (fun param -> selector (atomPath, None, (getFn param), (fun _ -> failwith $"readonly selector {atomPath}")))
            DeepEqual.deepEqual

    let inline selectorFamily<'TKey, 'TValue>
        (
            atomPath,
            getFn: 'TKey -> GetFn -> 'TValue,
            setFn: 'TKey -> GetFn -> SetFn -> 'TValue -> unit
        ) =
        jotaiUtils.atomFamily (fun param -> selector (atomPath, None, getFn param, setFn param)) DeepEqual.deepEqual

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
                     (fun get set newValue -> promise { do! setFn param get set newValue }))
                ))
            DeepEqual.deepEqual

    let inline asyncReadSelectorFamily<'TKey, 'TValue> (atomPath, getFn: 'TKey -> GetFn -> JS.Promise<'TValue>) =
        asyncSelectorFamily (
            atomPath,
            getFn,
            (fun _key _get _set _newValue -> promise { failwith $"readonly selector {atomPath}" })
        )

    type AtomField<'TValue67> =
        {
            ReadOnly: Atom<'TValue67> option
            ReadWrite: Atom<string> option
        }

    type ReadWriteValue =
        {
            AtomPath: string
            Value: string option
        }

    let rec gunAtomNode =
        readSelectorFamily (
            $"{nameof gunAtomNode}",
            (fun (atomPath: AtomPath<obj>) get ->
                let gunNamespace = Atoms.getAtomValue get Atoms.gunNamespace

                match queryAtomPath atomPath, gunNamespace.is with
                | Some atomPath, Some { alias = Some _username } ->
                    let nodes = atomPath |> String.split "/" |> Array.toList

                    (Some (gunNamespace.get nodes.Head), nodes.Tail)
                    ||> List.fold
                            (fun result node ->
                                result
                                |> Option.map (fun result -> result.get node))
                | _ ->
                    match JS.window id with
                    | Some window ->
                        JS.setTimeout
                            (fun () ->
                                window?lastToast (fun (x: Chakra.IToastProps) -> x.description <- "Please log in again"))
                            0
                        |> ignore
                    | None -> ()

                    failwith
                        $"Invalid username.
                                atomPath={atomPath}
                                user.is={JS.JSON.stringify gunNamespace.is}")
        )

    let inline userEncode<'TValue> (gun: Gun.IGunChainReference) (value: 'TValue) =
        promise {
            try
                let user = gun.user ()
                let keys = user.__.sea

                match keys with
                | Some keys ->
                    let json =
                        value
                        |> Gun.jsonEncode<'TValue>
                        |> Gun.jsonEncode<string>

                    //                    printfn $"userEncode value={value} json={json}"
//
                    let! encrypted = Gun.sea.encrypt json keys

                    let! signed = Gun.sea.sign encrypted keys
                    //                    JS.log (fun () -> $"userEncode. json={json} encrypted={encrypted} signed={signed}")
                    return signed
                | None -> return failwith $"No keys found for user {user.is}"
            with ex ->
                Browser.Dom.console.error ("[exception4]", ex)
                return raise ex
        }

    let inline userDecode<'TValue> (gun: Gun.IGunChainReference) data =
        promise {
            try
                let user = gun.user ()
                let keys = user.__.sea

                match keys |> Option.ofObjUnbox with
                | Some (Some keys) ->
                    let! verified = Gun.sea.verify data keys.pub
                    let! decrypted = Gun.sea.decrypt verified keys
                    //
//                    printfn
//                        $"userDecode
//                    decrypted={decrypted}
//                    typeof decrypted={jsTypeof decrypted}"

                    let decoded = decrypted |> Gun.jsonDecode<'TValue option>

                    //                    printfn $"userDecode decoded={decoded}"
//
                    return decoded
                | _ -> return failwith $"No keys found for user {user.is}"
            with ex ->
                Browser.Dom.console.error ("[exception5]", ex)
                return raise ex
        }

    module Gun =
        let batchData =
            Batcher.batcher
                (Array.map
                    (fun (item: {| Fn: int64 * string -> JS.Promise<unit>
                                   Timestamp: int64
                                   Data: string |}) -> item.Fn (item.Timestamp, item.Data))
                 >> Promise.Parallel
                 >> Promise.start)
                {| interval = 1000 |}

        let batchSubscribe =
            Batcher.batcher
                (Array.map
                    (fun (item: {| GunAtomNode: Gun.IGunChainReference
                                   Fn: int64 * string -> JS.Promise<unit> |}) ->
                        promise {
                            item.GunAtomNode.on
                                (fun data _key ->
                                    batchData
                                        {|
                                            Timestamp = DateTime.Now.Ticks
                                            Data = data
                                            Fn = item.Fn
                                        |})
                        })
                 >> Promise.Parallel
                 >> Promise.start)
                {| interval = 1000 |}


    let inline atomWithSync<'TKey, 'TValue> (atomPath, defaultValue: 'TValue, keyIdentifier: string list) =
        let mutable lastGunAtomNode = None
        let mutable lastValue = None

        let assignLastGunAtomNode get atom =
            match lastGunAtomNode with
            | None -> lastGunAtomNode <- Atoms.getAtomValue get (gunAtomNode (AtomPath.Atom (unbox atom)))
            | _ -> ()

        let internalAtom = jotai.atom defaultValue

        let gunNodePath = getGunNodePath atomPath keyIdentifier

        Profiling.addCount $"{gunNodePath} constructor"
        //                JS.log
//                    (fun () ->
//                        $"atomFamily constructor gunAtomPath={gunAtomPath} atomPath={atomPath} param={param} keyIdentifier={
//                                                                                                                                keyIdentifier
//                        }")

        let rec wrapper =
            selector (
                atomPath,
                (Some keyIdentifier),
                (fun get ->
                    assignLastGunAtomNode get wrapper

                    let result = Atoms.getAtomValue get internalAtom

                    Profiling.addCount $"{gunNodePath} get"

                    //                                JS.log
//                                    (fun () ->
//                                        $"atomFamily.get() atomPath={atomPath} keyIdentifier={keyIdentifier}
//                                                param={param} result={result}")

                    lastValue <- Some (DateTime.Now.Ticks, result)

                    result),
                (fun get set newValueFn ->
                    assignLastGunAtomNode get wrapper

                    Atoms.setAtomValue
                        set
                        internalAtom
                        (unbox
                            (fun oldValue ->
                                let newValue =
                                    match jsTypeof newValueFn with
                                    | "function" -> (unbox newValueFn) oldValue |> unbox
                                    | _ -> newValueFn

                                if true
                                   || oldValue |> DeepEqual.deepEqual newValue |> not
                                   || (lastValue.IsNone
                                       && newValue |> DeepEqual.deepEqual defaultValue) then

                                    Profiling.addCount $"{gunNodePath} set"

                                    //                                            JS.log
//                                                (fun () ->
//                                                    $"atomFamily.set()
//                                                    atomPath={atomPath} keyIdentifier={keyIdentifier}
//                                                    param={param} jsTypeof-newValue={jsTypeof newValue}
//                                                    oldValue={oldValue} newValue={newValue}
//                                                    newValueFn={newValueFn}
//                                                    lastValue={lastValue}
//                                                    ")

                                    promise {
                                        try
                                            match lastGunAtomNode with
                                            | Some gunAtomNode ->

                                                let! newValueJson =
                                                    if newValue |> JS.ofNonEmptyObj |> Option.isNone then
                                                        null |> Promise.lift
                                                    else
                                                        userEncode<'TValue> gunAtomNode newValue

                                                Gun.put gunAtomNode newValueJson
                                            | None ->
                                                Browser.Dom.console.error
                                                    $"[gunEffect.onRecoilSet] Gun node not found: {atomPath}"
                                        with ex -> Browser.Dom.console.error ("[exception2]", ex)
                                    }
                                    |> Promise.start

                                lastValue <- Some (DateTime.Now.Ticks, newValue)

                                newValue)))
            )

        let setInternalFromGun gunAtomNode setAtom (ticks, data) =
            promise {
                try
                    let! newValue =
                        match box data with
                        | null -> unbox null |> Promise.lift
                        | _ -> userDecode<'TValue> gunAtomNode data

                    match lastValue with
                    | Some (lastValueTicks, lastValue) when
                        lastValueTicks > ticks
                        || lastValue |> DeepEqual.deepEqual (unbox newValue)
                        || (unbox lastValue = null && unbox newValue = null) ->

                        Profiling.addCount $"{gunNodePath} on() skip"
                    //                                printfn
//                                    $"on() value. skipping. atomPath={atomPath} lastValue={lastValue} newValue={newValue}"
                    | _ ->
                        Profiling.addCount $"{gunNodePath} on() assign"
                        //                                printfn
//                                    $"on() value. triggering. atomPath={atomPath} lastValue={lastValue} newValue={
//                                                                                                                      newValue
//                                    }"

                        setAtom newValue
                with ex -> Browser.Dom.console.error ("[exception1]", ex)
            }

        let mutable lastSubscription = None

        let subscribe =
            (fun setAtom ->
                if lastSubscription.IsNone then
                    match lastGunAtomNode with
                    | Some gunAtomNode ->
                        Profiling.addCount $"{gunNodePath} subscribe"
                        //                                JS.log (fun () -> $"[gunEffect.on()] atomPath={atomPath}")

                        Gun.batchSubscribe
                            {|
                                GunAtomNode = gunAtomNode
                                Fn = setInternalFromGun gunAtomNode setAtom
                            |}

                        lastSubscription <- Some DateTime.Now.Ticks
                    | None -> Browser.Dom.console.error $"[gunEffect.get] Gun node not found: {atomPath}")

        let unsubscribe =
            (fun _setAtom ->
                match lastSubscription with
                | Some ticks when DateTime.ticksDiff ticks < 1000. -> ()
                | _ ->
                    try
                        match lastGunAtomNode with
                        | Some gunAtomNode ->

                            Profiling.addCount $"{gunNodePath} unsubscribe"
                            //                                    JS.log (fun () -> $"[atomFamily.unsubscribe()] atomPath={atomPath} param={param}")

                            gunAtomNode.off () |> ignore
                            lastSubscription <- None
                        | None -> Browser.Dom.console.error $"[gunEffect.off()] Gun node not found: {atomPath}"
                    with ex -> Browser.Dom.console.error ("[exception3]", ex))

        wrapper?onMount <- fun setAtom ->
                               subscribe setAtom
                               fun () -> unsubscribe setAtom

        wrapper

    let inline atomFamilyWithSync<'TKey, 'TValue>
        (
            atomPath,
            defaultValueFn: 'TKey -> 'TValue,
            persist: 'TKey -> string list
        ) =
        jotaiUtils.atomFamily
            (fun param -> atomWithSync (atomPath, defaultValueFn param, persist param))
            DeepEqual.deepEqual

    let inline atomFamily<'TKey, 'TValue> (atomPath, defaultValueFn: 'TKey -> 'TValue) =
        jotaiUtils.atomFamily (fun param -> atom (atomPath, defaultValueFn param)) DeepEqual.deepEqual

    let readWriteValue =
        let rec readWriteValue =
            atomFamilyWithSync (
                $"{nameof User}/{nameof readWriteValue}",
                (fun (_guid: Guid) -> null: string),
                (fun (guid: Guid) ->
                    [
                        string guid
                    ])
            )

        jotaiUtils.atomFamily
            (fun (atomPath: string) ->

                let guidHash = Crypto.getTextGuidHash atomPath
                let pathHash = guidHash

                printfn $"readWriteValueWrapper constructor. atomPath={atomPath} guidHash={guidHash}"

                let wrapper =
                    jotai.atom (
                        (fun get ->
                            let value = Atoms.getAtomValue get (readWriteValue pathHash)
                            Profiling.addCount $"{atomPath} readWriteValue set"

                            printfn
                                $"readWriteValueWrapper.get(). atomPath={atomPath} guidHash={guidHash} value={value}"

                            match value with
                            | null -> null
                            | _ ->
                                match Gun.jsonDecode<ReadWriteValue> value with
                                | { Value = Some value } -> value
                                | _ -> null),
                        Some
                            (fun _get set newValue ->
                                Profiling.addCount $"{atomPath} readWriteValue set"

                                printfn
                                    $"readWriteValueWrapper.set(). atomPath={atomPath} guidHash={guidHash} newValue={
                                                                                                                         newValue
                                    }"

                                let newValue =
                                    Gun.jsonEncode
                                        {
                                            AtomPath = atomPath
                                            Value = newValue |> Option.ofObj
                                        }

                                printfn $"readWriteValueWrapper.set(). newValue2={newValue}"

                                Atoms.setAtomValue set (readWriteValue pathHash) (newValue |> box |> unbox))
                    )

                wrapper)
            DeepEqual.deepEqual

    let emptyAtom = jotai.atom<obj> null

    type InputAtom<'T> = InputAtom of atomPath: AtomPath<'T>

    let inline getAtomField (atom: InputAtom<'TValue> option) (inputScope: AtomScope) =
        match atom with
        | Some (InputAtom atomPath) ->
            {
                ReadOnly =
                    match atomPath with
                    | AtomPath.Atom atom -> Some atom
                    | _ -> Some (unbox emptyAtom)
                ReadWrite =
                    printfn
                        $"getAtomField
                    atomPath={atomPath}
                    queryAtomPath atomPath={queryAtomPath atomPath}
                    inputScope={inputScope}
                    "

                    match queryAtomPath atomPath, inputScope with
                    | Some atomPath, AtomScope.ReadWrite -> Some (readWriteValue atomPath)
                    | _ -> None
            }
        | _ -> { ReadOnly = None; ReadWrite = None }


    let useStateOption (atom: Atom<'TValue5> option) =
        let flatAtom =
            React.useMemo (
                (fun () ->
                    match atom with
                    | Some atom -> atom
                    | None -> emptyAtom :?> Atom<'TValue5>),
                [|
                    box atom
                |]
            )

        let value, setValue = jotai.useAtom flatAtom

        React.useMemo (
            (fun () -> (if atom.IsNone then None else Some value), (if atom.IsNone then (fun _ -> ()) else setValue)),
            [|
                box atom
                box value
                box setValue
            |]
        )


    let useAtomFieldOptions<'TValue7> (atom: InputAtom<'TValue7> option) (inputScope: InputScope<'TValue7> option) =
        let atomField =
            React.useMemo (
                (fun () -> getAtomField atom (InputScope.AtomScope inputScope)),
                [|
                    box atom
                    box inputScope
                |]
            )

        let readOnlyValue, setReadOnlyValue = useStateOption atomField.ReadOnly
        let readWriteValue, setReadWriteValue = useStateOption atomField.ReadWrite

        React.useMemo (
            (fun () ->
                let defaultJsonEncode, _defaultJsonDecode = unbox Gun.defaultSerializer

                let newReadWriteValue =
                    match inputScope, readWriteValue |> Option.defaultValue null with
                    | _, null -> readOnlyValue |> Option.defaultValue (unbox null)
                    | Some (InputScope.ReadWrite (_, jsonDecode)), readWriteValue ->
                        try
                            printfn
                                $"useAtomFieldOptins
                            readOnlyValue={readOnlyValue}
                            atom={atom}
                            readWriteValue={readWriteValue}"

                            jsonDecode readWriteValue
                        with ex ->
                            printfn $"Error decoding readWriteValue={readWriteValue} ex={ex}"

                            readOnlyValue
                            |> Option.defaultValue (unbox readWriteValue)
                    | _ ->
                        readOnlyValue
                        |> Option.defaultValue (unbox readWriteValue)
                //                    | _ -> defaultJsonDecode readWriteValue

                let setReadWriteValue =
                    if atom.IsSome then
                        (fun newValue ->
                            setReadWriteValue (
                                match box newValue with
                                | null -> null
                                | _ ->
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

    let inline useValue atom = jotaiUtils.useAtomValue atom


    let shadowedCallbackFn (fn, deps) =
        (emitJsExpr (React.useCallback, fn, deps) "$0($1,$2)")

    let useCallback (fn: GetFn -> SetFn -> 'a -> JS.Promise<'c>, deps: obj []) : ('a -> JS.Promise<'c>) =

        let fnCallback = React.useCallbackRef (fun (get, set, arg) -> fn get set arg)

        let fnCallback =
            shadowedCallbackFn (
                fnCallback,
                Array.concat [
                    deps
                    [|
                        fnCallback
                    |]
                ]
            )

        let atom =
            React.useMemo (
                (fun () ->
                    jotai.atom (
                        unbox null,
                        Some
                            (fun get set (arg, resolve, err) ->
                                try
                                    resolve (fnCallback (get, set, arg))
                                with ex ->
                                    printfn $"atomCallback fn error: {ex}"
                                    err ex

                                ())
                    )),
                [|
                    box fnCallback
                |]
            )

        let _value, setValue = jotai.useAtom atom

        let useAtomCallback =
            React.useCallback (
                (fun arg ->
                    Promise.create (fun resolve err -> setValue (arg, resolve, err))
                    |> Promise.bind id),
                [|
                    box setValue
                |]
            )

        useAtomCallback

    let inline useCallbacks () =
        useCallback ((fun get set () -> promise { return (get, set) }), [||])

    let inline readWriteSet<'TValue9, 'TKey> (setFn: SetFn, atom: Atom<'TValue9>, value: 'TValue9) =
        let atomField = getAtomField (Some (InputAtom (AtomPath.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some atom -> Atoms.setAtomValue setFn atom (value |> Gun.jsonEncode<'TValue9>)
        | _ -> ()

    let inline scopedSet<'TValue10, 'TKey>
        (setFn: SetFn)
        (atomScope: AtomScope)
        (atom: 'TKey -> Atom<'TValue10>, key: 'TKey, value: 'TValue10)
        =
        match atomScope with
        | AtomScope.ReadOnly -> Atoms.setAtomValue setFn (atom key) value
        | AtomScope.ReadWrite -> readWriteSet<'TValue10, 'TKey> (setFn, atom key, value)

    let inline readWriteReset<'TValue8, 'TKey> (setFn: SetFn) (atom: Atom<'TValue8>) =
        let atomField = getAtomField (Some (InputAtom (AtomPath.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some atom -> Atoms.setAtomValue setFn atom null
        | _ -> ()

    let inline getReadWrite<'TValue11, 'TKey> getFn (atom: Atom<'TValue11>) =
        let atomField = getAtomField (Some (InputAtom (AtomPath.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some readWriteAtom ->
            let value = Atoms.getAtomValue getFn readWriteAtom

            match value with
            | null -> Atoms.getAtomValue getFn atom
            | _ -> Gun.jsonDecode<'TValue11> value
        | _ -> Atoms.getAtomValue getFn atom

    let useState = jotai.useAtom

    let inline useSetState atom = jotaiUtils.useUpdateAtom atom

    let inline useSetStatePrev<'T> atom =
        let setter = jotaiUtils.useUpdateAtom<'T> atom
        fun (value: 'T -> 'T) -> setter (unbox value)

    let inline atom x = atom x

    let provider = jotai.provider
    let atomWithStorage = jotaiUtils.atomWithStorage

    type GetFn = Jotai.GetFn
    type SetFn = Jotai.SetFn
    type AtomScope = JotaiTypes.AtomScope
    type InputScope<'T> = JotaiTypes.InputScope<'T>
    type AtomPath<'T> = JotaiTypes.AtomPath<'T>
    type Atom<'T> = JotaiTypes.Atom<'T>
    let emptyArrayAtom = jotai.atom<obj []> [||]

    let waitForAll<'T> (atoms: Atom<'T> []) =
        match atoms with
        | [||] -> unbox emptyArrayAtom
        | _ -> jotaiUtils.waitForAll atoms
