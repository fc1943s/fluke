namespace Fluke.UI.Frontend.Bindings

open Fluke.Shared.Domain.UserInteraction

#nowarn "40"


open Fluke.UI.Frontend.Bindings
open Fable.Core.JsInterop
open Fable.Core
open System
open Fluke.Shared
open Microsoft.FSharp.Core.Operators
open Feliz


module Store =
    open Jotai

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
        jotaiUtils.atomFamily (fun param -> atom (atomPath, defaultValueFn param)) DeepEqual.compare

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
        jotaiUtils.atomFamily (fun param -> readSelector (atomPath, getFn param)) DeepEqual.compare

    let inline value<'TValue> (getter: GetFn) (atom: Atom<'TValue>) : 'TValue = (getter (unbox atom)) :?> 'TValue

    let inline set<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue) = setter (atom |> box |> unbox) value

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
        jotaiUtils.atomFamily (fun param -> selectAtom (atomPath, atom, selector param)) DeepEqual.compare

    let atomWithStorage (atomPath, defaultValue, map: _ -> _) =
        let internalAtom = jotaiUtils.atomWithStorage atomPath defaultValue

        jotai.atom (
            (fun getter -> value getter internalAtom),
            Some
                (fun _ setter argFn ->
                    let arg =
                        match jsTypeof argFn with
                        | "function" -> (argFn |> box |> unbox) () |> unbox
                        | _ -> argFn

                    set setter internalAtom (map arg))
        )
        |> registerAtom atomPath None
        |> fst


    module Atoms =
        let rec gunPeers =
            atomWithStorage ($"{nameof gunPeers}", ([||]: string []), Array.filter (String.IsNullOrWhiteSpace >> not))

        let rec isTesting = atom ($"{nameof isTesting}", JS.deviceInfo.IsTesting)
        let rec username = atom ($"{nameof username}", (None: Username option))
        let rec gunKeys = atom ($"{nameof gunKeys}", Gun.GunKeys.Default)


    module Selectors =
        let rec gun =
            readSelector (
                $"{nameof gun}",
                (fun getter ->
                    let isTesting = value getter Atoms.isTesting
                    let gunPeers = value getter Atoms.gunPeers

                    let gun =
                        if isTesting then
                            Gun.gun
                                {
                                    Gun.GunProps.peers = None
                                    Gun.GunProps.radisk = Some false
                                    Gun.GunProps.localStorage = None
                                    Gun.GunProps.multicast = None
                                }
                        else
                            Gun.gun
                                {
                                    Gun.GunProps.peers = Some gunPeers
                                    Gun.GunProps.radisk = Some true
                                    Gun.GunProps.localStorage = Some false
                                    Gun.GunProps.multicast = None
                                }

                    printfn $"jotai gun selector. peers={gunPeers}. gun={gun} returning gun..."

                    gun)
            )

        let rec gunNamespace =
            selectAtom (
                $"{nameof gunNamespace}",
                gun,
                fun gun ->
                    let user = gun.user ()

                    printfn $"gunNamespace selector. user.is={JS.JSON.stringify user.is} keys={user.__.sea}..."

                    user
            )

        let rec gunAtomNode =
            selectAtomFamily (
                $"{nameof gunAtomNode}",
                gunNamespace,
                (fun (username, atomPath) gunNamespace ->
                    match gunNamespace.is with
                    | Some { alias = Some username' } when username' = (username |> Username.Value) ->
                        let nodes =
                            atomPath
                            |> AtomPath.Value
                            |> String.split "/"
                            |> Array.toList

                        (Some (gunNamespace.get nodes.Head), nodes.Tail)
                        ||> List.fold
                                (fun result node ->
                                    result
                                    |> Option.map (fun result -> result.get node))
                    | _ ->
                        JS.log
                            (fun () ->
                                $"Invalid username.
                                                                                atomPath={atomPath}
                                                                                user.is={
                                                                                             JS.JSON.stringify
                                                                                                 gunNamespace.is
                                }")

                        None)
            )

    //        let rec gunAtomNode =
//            readSelectorFamily (
//                $"{nameof gunAtomNode}",
//                (fun (AtomPath atomPath) getter ->
//                    let gunNamespace = value getter gunNamespace
//
//                    match gunNamespace.is with
//                    | Some { alias = Some username } ->
//                        let nodes = atomPath |> String.split "/" |> Array.toList
//
//                        let gunNode =
//                            (Some (gunNamespace.get nodes.Head), nodes.Tail)
//                            ||> List.fold
//                                    (fun result node ->
//                                        result
//                                        |> Option.map (fun result -> result.get node))
//
//                        match gunNode with
//                        | Some gunNode -> Some (username, gunNode)
//                        | None -> None
//                    | _ ->
//                        match JS.window id with
//                        | Some window ->
//                            JS.setTimeout
//                                (fun () ->
//                                    window?lastToast (fun (x: Chakra.IToastProps) ->
//                                        x.description <- "Please log in again"))
//                                0
//                            |> ignore
//                        | None -> ()
//
//                        failwith
//                            $"Invalid username.
//                                    atomPath={atomPath}
//                                    user.is={JS.JSON.stringify gunNamespace.is}")
//            )

    // https://i.imgur.com/GB8trpT.png        :~ still trash
    let inline atomWithSync<'TKey, 'TValue> (atomPath, defaultValue: 'TValue, keyIdentifier: string list) =
        let mutable lastGunAtomNode = None
        let mutable lastUsername = None
        let mutable lastValue = None
        let mutable lastGunValue = None
        let mutable lastAtomPath = None
        let mutable lastUserAtomId = None
        let mutable lastWrapperSet = None

        //        let localSetHashes = HashSet<Guid> ()

        let assignLastGunAtomNode getter atom =
            // TODO: remove if?
            if lastAtomPath.IsNone then
                lastAtomPath <- queryAtomPath (AtomReference.Atom (unbox atom))

            JS.log (fun () -> $"assignLastGunAtomNode atom={atom} lastAtomPath={lastAtomPath}")

            let username = value getter Atoms.username

            match username, lastAtomPath with
            | Some username, Some atomPath ->
                match value getter (Selectors.gunAtomNode (username, atomPath)) with
                | Some gunAtomNode ->
                    lastUsername <- Some username
                    lastGunAtomNode <- Some ($"atom={atom} atomPath={atomPath} username={username}", gunAtomNode)
                | _ ->
                    lastUsername <- None
                    lastGunAtomNode <- None
            | _ ->
                lastUsername <- None
                lastGunAtomNode <- None

            username


        let internalAtom = jotaiUtils.atomFamily (fun _username -> jotai.atom defaultValue) DeepEqual.compare

        let gunNodePath = Gun.getGunNodePath atomPath keyIdentifier

        Profiling.addCount $"{gunNodePath} constructor"

        let baseInfo () =
            $"""gunNodePath={gunNodePath}
                atomPath={atomPath}
                keyIdentifier={keyIdentifier}
                lastValue={lastValue}
                lastUsername={lastUsername}
                lastGunAtomNode={lastGunAtomNode}
                lastAtomPath={lastAtomPath}
                lastUserAtomId={lastUserAtomId}
                """


        JS.log
            (fun () ->
                $"atomFamily constructor
                {baseInfo ()}")

        let setInternalFromGun gunAtomNode setAtom (ticks, data) =
            promise {
                try
                    let! newValue =
                        match box data with
                        | null -> unbox null |> Promise.lift
                        | _ -> Gun.userDecode<'TValue> gunAtomNode data

                    lastGunValue <- newValue

                    match lastValue with
                    | Some (lastValueTicks, lastValue) when
                        lastValueTicks > ticks
                        || lastValue |> DeepEqual.compare (unbox newValue)
                        || (unbox lastValue = null && unbox newValue = null) ->

                        Profiling.addCount $"{gunNodePath} on() skip"

                        JS.log
                            (fun () ->
                                $"gun.on() value. skipping.
                                                    jsTypeof-newValue={jsTypeof newValue}
                                                    newValue={newValue}
                                                    lastValue={lastValue}
                                                    {baseInfo ()}
                                 ")
                    | _ ->
                        Profiling.addCount $"{gunNodePath} on() assign"

                        let _lastValue =
                            match lastValue with
                            | Some (_, b) -> b
                            | _ -> unbox null

                        JS.log
                            (fun () ->
                                if _lastValue.ToString () = newValue.ToString () then
                                    Browser.Dom.console.error
                                        $"should have skipped assign
                                        _lastValue={_lastValue}
                                        typeof lastValue={jsTypeof _lastValue}
                                        newValue={newValue}
                                        typeof newValue={jsTypeof newValue}
                                        {baseInfo ()}
                                        "

                                $"gun.on() value. triggering.
                            _lastValue={_lastValue}
                            typeof lastValue={jsTypeof _lastValue}
                            newValue={newValue}
                            typeof newValue={jsTypeof newValue}
                            {baseInfo ()}
                            ")

                        //                        Browser.Dom.window?atomPath <- atomPath
//                        Browser.Dom.window?lastValue <- _lastValue
//                        Browser.Dom.window?newValue <- newValue
//                        Browser.Dom.window?deepEqual <- DeepEqual.compare

                        // setAtom internalAtom

                        setAtom newValue
                with ex -> Browser.Dom.console.error ("[exception1]", ex)
            }

        let mutable lastSubscription = None

        let subscribe =
            (fun setAtom ->
                if lastSubscription.IsNone then
                    match lastGunAtomNode with
                    | Some (key, gunAtomNode) ->
                        Profiling.addCount $"{gunNodePath} subscribe"
                        JS.log (fun () -> $"[gunEffect.on()] atomPath={atomPath} {key}")

                        Gun.batchSubscribe
                            {|
                                GunAtomNode = gunAtomNode
                                Fn = setInternalFromGun gunAtomNode setAtom
                            |}

                        //                        Gun.subscribe
//                            gunAtomNode
//                            (fun data ->
//                                setInternalFromGun gunAtomNode setAtom (DateTime.Now.Ticks, data)
//                                |> Promise.start)

                        lastSubscription <- Some DateTime.Now.Ticks
                    | None ->
                        JS.log
                            (fun () ->
                                $"[gunEffect.on()]
                                {baseInfo ()}
                             skipping subscribe, no gun atom node.")

                        lastWrapperSet <- Some setAtom)

        let unsubscribe =
            (fun _setAtom ->
                match lastSubscription with
                | Some ticks when DateTime.ticksDiff ticks < 1000. -> ()
                | _ ->
                    match lastGunAtomNode with
                    | Some (key, gunAtomNode) ->

                        Profiling.addCount $"{gunNodePath} unsubscribe"

                        JS.log
                            (fun () ->
                                $"[atomFamily.unsubscribe()]
                                {key}
                                {baseInfo ()}
                                ")

                        gunAtomNode.off () |> ignore
                        lastSubscription <- None
                    | None ->
                        JS.log
                            (fun () ->
                                $"[gunEffect.off()]
                                {baseInfo ()}
                                skipping unsubscribe, no gun atom node."))

        let debounceGunPut =
            JS.debounce
                (fun newValue ->
                    promise {
                        JS.log (fun () -> $"atomFamily.wrapper.set() debounceGunPut promise. #1")

                        try
                            match lastGunAtomNode with
                            | Some (key, gunAtomNode) ->
                                JS.log
                                    (fun () ->
                                        $"atomFamily.wrapper.set() debounceGunPut promise. #2 before encode {key}")

                                let! newValueJson =
                                    if newValue |> JS.ofNonEmptyObj |> Option.isNone then
                                        null |> Promise.lift
                                    else
                                        Gun.userEncode<'TValue> gunAtomNode newValue

                                JS.log
                                    (fun () -> $"atomFamily.wrapper.set() debounceGunPut promise. #3. before put {key}")

                                if lastGunValue.IsNone
                                   || lastGunValue
                                      |> DeepEqual.compare (unbox newValue)
                                      |> not
                                   || unbox newValue = null then

                                    let! putResult = Gun.put gunAtomNode newValueJson


                                    match putResult with
                                    | Some (ack, node) when ack.ok = Some 1 ->
                                        Browser.Dom.window?lastAck <- ack
                                        Browser.Dom.window?lastNode <- node

                                        JS.log
                                            (fun () ->
                                                $"atomFamily.wrapper.set() debounceGunPut promise result.
                                                   ack={JS.JSON.stringify ack}
                                                   node=%A{node}
                                                   newValue={newValue}
                                                   {key}
                                                   {baseInfo ()}
                                                   ")

                                    | _ ->
                                        Browser.Dom.window?lastPutResult <- putResult

                                        Browser.Dom.console.error
                                            $"atomFamily.wrapper.set() debounceGunPut promise put error.
                                                 newValue={newValue} putResult={putResult}
                                                   {key}
                                                {baseInfo ()}"
                                else
                                    JS.log
                                        (fun () ->
                                            $"atomFamily.wrapper.set() debounceGunPut promise.
                                                   put skipped
                                                   newValue[{newValue}]==lastGunValue[]
                                                   {key}
                                                   {baseInfo ()}
                                                   ")
                            | None ->
                                JS.log
                                    (fun () ->
                                        $"[gunEffect.debounceGunPut promise]
                                        skipping gun put. no gun atom node.
                                        {baseInfo ()}
                                        ")
                        with ex -> Browser.Dom.console.error ("[exception2]", ex)
                    }
                    |> Promise.start)
                1000

        let rec wrapper =
            selector (
                atomPath,
                (Some keyIdentifier),
                (fun getter ->
                    let username = assignLastGunAtomNode getter wrapper
                    let userAtom = internalAtom username

                    let result = value getter userAtom

                    Profiling.addCount $"{gunNodePath} get"

                    JS.log
                        (fun () ->
                            $"atomFamily.wrapper.get()
                                wrapper={wrapper}
                                userAtom={userAtom}
                                result={result}
                                {baseInfo ()}
                                ")

                    let userAtomId = Some (userAtom.toString ())

                    if userAtomId <> lastUserAtomId then
                        lastUserAtomId <- userAtomId

                        match lastWrapperSet with
                        | Some lastWrapperSet ->
                            printfn
                                $"subscribing
                                wrapper={wrapper}
                                userAtom={userAtom}
                                {baseInfo ()}
                            "

                            subscribe lastWrapperSet
                        | None ->
                            printfn
                                $"skipping subscribe
                                wrapper={wrapper}
                                userAtom={userAtom}
                                {baseInfo ()}
                        "

                    lastValue <- Some (DateTime.Now.Ticks, result)

                    result),
                (fun get setter newValueFn ->
                    let username = assignLastGunAtomNode get wrapper
                    let userAtom = internalAtom username

                    set
                        setter
                        userAtom
                        (unbox
                            (fun oldValue ->
                                let newValue =
                                    match jsTypeof newValueFn with
                                    | "function" -> (unbox newValueFn) oldValue |> unbox
                                    | _ -> newValueFn

                                if true
                                   || oldValue |> DeepEqual.compare newValue |> not
                                   || (lastValue.IsNone
                                       && newValue |> DeepEqual.compare defaultValue) then

                                    Profiling.addCount $"{gunNodePath} set"

                                    JS.log
                                        (fun () ->
                                            $"atomFamily.wrapper.set()
                                                    wrapper={wrapper}
                                                    userAtom={userAtom}
                                                    jsTypeof-newValue={jsTypeof newValue}
                                                    oldValue={oldValue}
                                                    newValue={newValue}
                                                    {baseInfo ()}
                                                    ")

                                    debounceGunPut newValue

                                lastValue <- Some (DateTime.Now.Ticks, newValue)

                                JS.log
                                    (fun () ->
                                        $"atomFamily.wrapper.set()
                                                    ##### lastValue setted. returning #####
                                                    wrapper={wrapper}
                                                    userAtom={userAtom}
                                                    jsTypeof-newValue={jsTypeof newValue}
                                                    oldValue={oldValue}
                                                    newValue={newValue}
                                                    {baseInfo ()}
                                                    ")

                                newValue)))
            )

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
            DeepEqual.compare

    let inline atomWithStorageSync<'TKey, 'TValue> (atomPath, defaultValue, map: _ -> _) =
        let storageAtom = atomWithStorage (atomPath, defaultValue, map)
        let syncAtom = atomWithSync<'TKey, 'TValue> (atomPath, defaultValue, [])

        let rec wrapper =
            selector (
                atomPath,
                None,
                (fun getter ->
                    match value getter syncAtom with
                    | syncValue when syncValue |> DeepEqual.compare defaultValue -> value getter storageAtom
                    | syncValue -> syncValue),
                (fun _get setter newValue ->
                    set setter storageAtom newValue
                    set setter syncAtom newValue)
            )

        wrapper


    let readWriteValue =
        let rec readWriteValue =
            atomFamilyWithSync (
                $"{nameof readWriteValue}",
                (fun (_guid: Guid) -> null: string),
                (fun (guid: Guid) ->
                    [
                        string guid
                    ])
            )

        jotaiUtils.atomFamily
            (fun (AtomPath atomPath) ->

                let guidHash = Crypto.getTextGuidHash atomPath
                let pathHash = guidHash

                JS.log (fun () -> $"readWriteValueWrapper constructor. atomPath={atomPath} guidHash={guidHash}")

                let wrapper =
                    jotai.atom (
                        (fun getter ->
                            let value = value getter (readWriteValue pathHash)
                            Profiling.addCount $"{atomPath} readWriteValue set"

                            JS.log
                                (fun () ->
                                    $"readWriteValueWrapper.get(). atomPath={atomPath} guidHash={guidHash} value={value}")

                            match value with
                            | null -> null
                            | _ ->
                                match Json.decode<string * string option> value with
                                | _, Some value -> value
                                | _ -> null),
                        Some
                            (fun _ setter newValue ->
                                Profiling.addCount $"{atomPath} readWriteValue set"

                                JS.log
                                    (fun () ->
                                        $"readWriteValueWrapper.set(). atomPath={atomPath} guidHash={guidHash} newValue={
                                                                                                                             newValue
                                        }")

                                let newValue = Json.encode (atomPath, newValue |> Option.ofObj)

                                JS.log (fun () -> $"readWriteValueWrapper.set(). newValue2={newValue}")

                                set setter (readWriteValue pathHash) (newValue |> box |> unbox))
                    )

                wrapper)
            DeepEqual.compare

    let inline useValue atom = jotaiUtils.useAtomValue atom

    let shadowedCallbackFn (fn, deps) =
        (emitJsExpr (React.useCallback, fn, deps) "$0($1,$2)")

    let useCallback (fn: GetFn -> SetFn -> 'a -> JS.Promise<'c>, deps: obj []) : ('a -> JS.Promise<'c>) =

        let fnCallback = React.useCallbackRef (fun (getter, setter, arg) -> fn getter setter arg)

        let fnCallback =
            shadowedCallbackFn (
                fnCallback,
                Array.concat [
                    deps
                    [|
                        box fnCallback
                    |]
                ]
            )

        let atom =
            React.useMemo (
                (fun () ->
                    jotai.atom (
                        unbox null,
                        Some
                            (fun getter setter (arg, resolve, err) ->
                                try
                                    resolve (fnCallback (getter, setter, arg))
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
        useCallback ((fun getter setter () -> promise { return (getter, setter) }), [||])


    let useState = jotai.useAtom

    let inline useSetState atom = jotaiUtils.useUpdateAtom atom

    let inline useSetStatePrev<'T> atom =
        let setter = jotaiUtils.useUpdateAtom<'T> atom
        fun (value: 'T -> 'T) -> setter (unbox value)

    let provider = jotai.provider

    type GetFn = Jotai.GetFn
    type SetFn = Jotai.SetFn
    type AtomReference<'T> = Jotai.AtomReference<'T>
    type Atom<'T> = Jotai.Atom<'T>
    let emptyArrayAtom = jotai.atom<obj []> [||]

    let waitForAll<'T> (atoms: Atom<'T> []) =
        match atoms with
        | [||] -> unbox emptyArrayAtom
        | _ -> jotaiUtils.waitForAll atoms


    let inline selectorFamily<'TKey, 'TValue>
        (
            atomPath,
            getFn: 'TKey -> GetFn -> 'TValue,
            setFn: 'TKey -> GetFn -> SetFn -> 'TValue -> unit
        ) =
        jotaiUtils.atomFamily (fun param -> selector (atomPath, None, getFn param, setFn param)) DeepEqual.compare

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
            DeepEqual.compare

    let inline asyncReadSelectorFamily<'TKey, 'TValue> (atomPath, getFn: 'TKey -> GetFn -> JS.Promise<'TValue>) =
        asyncSelectorFamily (
            atomPath,
            getFn,
            (fun _key _ _ _newValue -> promise { failwith $"readonly selector {atomPath}" })
        )



    [<RequireQualifiedAccess>]
    type InputScope<'TValue> =
        | ReadOnly
        | ReadWrite of Gun.Serializer<'TValue>

    and InputScope<'TValue> with
        static member inline AtomScope<'TValue> (inputScope: InputScope<'TValue> option) =
            match inputScope with
            | Some (InputScope.ReadWrite _) -> AtomScope.ReadWrite
            | _ -> AtomScope.ReadOnly

    and [<RequireQualifiedAccess>] AtomScope =
        | ReadOnly
        | ReadWrite

    type InputAtom<'T> = InputAtom of atomPath: AtomReference<'T>

    type AtomField<'TValue67> =
        {
            ReadOnly: Jotai.Atom<'TValue67> option
            ReadWrite: Jotai.Atom<string> option
        }

    let emptyAtom = jotai.atom<obj> null

    let inline getAtomField (atom: InputAtom<'TValue> option) (inputScope: AtomScope) =
        match atom with
        | Some (InputAtom atomPath) ->
            {
                ReadOnly =
                    match atomPath with
                    | AtomReference.Atom atom -> Some atom
                    | _ -> Some (unbox emptyAtom)
                ReadWrite =
                    //                    JS.log
//                        (fun () -> $"getAtomField atomPath={atomPath} queryAtomPath atomPath={queryAtomPath atomPath}")

                    match queryAtomPath atomPath, inputScope with
                    | Some atomPath, AtomScope.ReadWrite -> Some (readWriteValue atomPath)
                    | _ -> None
            }
        | _ -> { ReadOnly = None; ReadWrite = None }


    let inline readWriteSet<'TValue9, 'TKey> (setter: Jotai.SetFn, atom: Jotai.Atom<'TValue9>, value: 'TValue9) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some atom -> set setter atom (value |> Json.encode<'TValue9>)
        | _ -> ()

    let inline scopedSet<'TValue10, 'TKey>
        (setter: Jotai.SetFn)
        (atomScope: AtomScope)
        (atom: 'TKey -> Jotai.Atom<'TValue10>, key: 'TKey, value: 'TValue10)
        =
        match atomScope with
        | AtomScope.ReadOnly -> set setter (atom key) value
        | AtomScope.ReadWrite -> readWriteSet<'TValue10, 'TKey> (setter, atom key, value)

    let inline readWriteReset<'TValue8, 'TKey> (setter: Jotai.SetFn) (atom: Jotai.Atom<'TValue8>) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some atom -> set setter atom null
        | _ -> ()

    let inline getReadWrite<'TValue11, 'TKey> getter (atom: Jotai.Atom<'TValue11>) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some readWriteAtom ->
            let result = value getter readWriteAtom

            match result with
            | null -> value getter atom
            | _ -> Json.decode<'TValue11> result
        | _ -> value getter atom


    module Hooks =
        let useStateOption (atom: Jotai.Atom<'TValue5> option) =
            let flatAtom =
                React.useMemo (
                    (fun () ->
                        match atom with
                        | Some atom -> atom
                        | None -> emptyAtom :?> Jotai.Atom<'TValue5>),
                    [|
                        box atom
                    |]
                )

            let value, setValue = jotai.useAtom flatAtom

            React.useMemo (
                (fun () ->
                    (if atom.IsNone then None else Some value), (if atom.IsNone then (fun _ -> ()) else setValue)),
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
                                JS.log
                                    (fun () ->
                                        $"useAtomFieldOptins
                                readOnlyValue={readOnlyValue}
                                atom={atom}
                                readWriteValue={readWriteValue}")

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
