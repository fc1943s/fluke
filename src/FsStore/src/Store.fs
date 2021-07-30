namespace FsStore

open System.Collections.Generic
open Fable.Extras
open Fable.Core.JsInterop
open Fable.Core
open System
open FsStore.Model
open FsStore.Shared
open Microsoft.FSharp.Core.Operators
open FsCore
open FsJs
open FsStore.Bindings
open FsStore.Bindings.Jotai

#nowarn "40"


module Store =
    let rec readSelectorInterval interval defaultValue atomPath getFn =
        let cache = jotai.atom defaultValue
        let readSelector = Store.readSelector atomPath getFn

        let mutable lastAccessors = None
        let mutable lastLogger = None
        let mutable timeout = -1

        let getBaseInfo () =
            $"
| readSelectorInterval baseInfo:
interval={interval}
defaultValue={defaultValue}
atomPath={atomPath}
lastAccessors={lastAccessors.IsSome}
timeout={timeout} "

        let getLogger () =
            lastLogger |> Option.defaultValue Logger.Default

        printfn $"readSelectorInterval.constructor {getBaseInfo ()}"

        let wrapper =
            Store.readSelector
                atomPath
                (fun getter ->
                    lastAccessors <- Store.value getter Selectors.atomAccessors
                    lastLogger <- Store.value getter Selectors.logger |> Some
                    let cache = Store.value getter cache
                    printfn $"readSelectorInterval.wrapper.get() cache={cache} {getBaseInfo ()}"
                    cache)

        let mutable lastValue = None

        let subscribe () =
            printfn $"readSelectorInterval.onMount() {getBaseInfo ()}"

            let fn () =
                getLogger()
                    .Trace (fun () -> $"#1 readSelectorInterval.timeout {getBaseInfo ()}")

                match lastAccessors with
                | Some (getter, setter) when timeout >= 0 ->
                    let selectorValue = Store.value getter readSelector

                    if Some selectorValue <> lastValue then
                        printfn $"#2 readSelectorInterval.timeout selectorValue={selectorValue} {getBaseInfo ()}"
                        Store.set setter cache selectorValue
                        lastValue <- Some selectorValue
                | _ -> ()

            if timeout = -1 then fn ()
            timeout <- JS.setInterval fn interval

        let unsubscribe () =
            printfn $"readSelectorInterval.onUnmount() {getBaseInfo ()}"
            if timeout >= 0 then JS.clearTimeout timeout
            timeout <- -1

        wrapper?onMount <- fun _setAtom ->
                               subscribe ()
                               fun () -> unsubscribe ()

        wrapper

    let inline gunAtomNodeFromAtomPath getter username atomPath =
        match username, atomPath with
        | Some username, Some atomPath ->
            match Store.value getter (Selectors.gunAtomNode (username, atomPath)) with
            | Some gunAtomNode -> Some ($">> atomPath={atomPath} username={username}", gunAtomNode)
            | _ -> None
        | _ -> None

    let inline createSyncEngine mapGunAtomNode =
        let mutable lastAtomPath = None
        let mutable lastUsername = None
        let mutable lastGunPeers = None
        let mutable lastLogger = None
        let mutable lastGunAtomNode = None
        let mutable lastHub = None

        let getBaseInfo () =
            $"
| createSyncEngine baseInfo:
lastAtomPath={lastAtomPath}
lastUsername={lastUsername}
lastGunPeers={lastGunPeers}
lastGunAtomNode={lastGunAtomNode} "

        {|
            GetAtomPath = fun () -> lastAtomPath
            GetUsername = fun () -> lastUsername
            GetGunPeers = fun () -> lastGunPeers
            GetLogger = fun () -> lastLogger |> Option.defaultValue Logger.Default
            GetGunAtomNode = fun () -> lastGunAtomNode
            GetHub = fun () -> lastHub
            GetBaseInfo = getBaseInfo
            SetProviders =
                fun getter atom ->
                    if lastAtomPath.IsNone then
                        lastAtomPath <- queryAtomPath (AtomReference.Atom (unbox atom))

                    Profiling.addCount $"createSyncEngine.setProviders {lastAtomPath}"

                    match lastAtomPath with
                    | Some (AtomPath atomPath) ->
                        Dom.logFiltered
                            atomPath
                            (fun () -> $"createSyncEngine.setProviders atom={atom} {getBaseInfo ()}")
                    | _ -> ()

                    lastUsername <- Store.value getter Atoms.username
                    lastGunPeers <- Store.value getter Atoms.gunPeers
                    lastLogger <- Some (Store.value getter Selectors.logger)

                    lastGunAtomNode <-
                        gunAtomNodeFromAtomPath getter lastUsername lastAtomPath
                        |> Option.map (mapGunAtomNode |> Option.defaultValue id)

                    match lastAtomPath, lastGunAtomNode with
                    | Some _, Some _ -> lastHub <- Store.value getter Selectors.hub
                    | _ -> ()
        |}

    let testKeysCache = Dictionary<string, Set<string>> ()

    let inline splitAtomPath atomPath =
        let matches =
            (JSe.RegExp @"(.*?)\/([\w-]{36})\/\w+.*?")
                .Match atomPath
            |> Option.ofObj
            |> Option.defaultValue Seq.empty
            |> Seq.toList

        match matches with
        | _match :: root :: guid :: _key -> Some (root, guid)
        | _ -> None

    let inline wrapSetAtom setAtom value =
        promise {
            setAtom value
            return Object.newDisposable (fun () -> Dom.log (fun () -> "wrapSetAtom Dispose."))
        }

    // https://i.imgur.com/GB8trpT.png        :~ still trash
    let inline atomWithSync<'TKey, 'TValue> collection atomPath (defaultValue: 'TValue) (keyIdentifier: string list) =
        let mutable lastValue = None
        let mutable lastGunValue = None
        let mutable lastHubValue = None
        let mutable lastUserAtomId = None
        let mutable lastWrapperSet = None
        let mutable syncPaused = false
        let mutable lastHubSubscription = None

        let syncEngine = createSyncEngine None
        let gunNodePath = Gun.getGunNodePath collection atomPath keyIdentifier

        let internalAtom = jotaiUtils.atomFamily (fun _username -> jotai.atom defaultValue) Object.compare

        let baseInfo () =
            $"""
| atomWithSync baseInfo:
lastValue={lastValue}
lastGunValue={lastGunValue}
lastHubValue={lastHubValue}
lastUserAtomId={lastUserAtomId}
gunNodePath={gunNodePath}
atomPath={atomPath}
keyIdentifier={keyIdentifier}
{syncEngine.GetBaseInfo ()}
"""

        Dom.log
            (fun () ->
                $"atomFamily constructor
                {baseInfo ()}")

        let mutable lastSubscription = None

        let unsubscribe () =
            match lastSubscription with
            | Some ticks when DateTime.ticksDiff ticks < 1000. ->
                Dom.log
                    (fun () ->
                        $"[wrapper.off()]
{baseInfo ()}
skipping unsubscribe. jotai resubscribe glitch.")
            | Some _ ->
                match syncEngine.GetGunAtomNode () with
                | Some (key, gunAtomNode: Gun.Types.IGunChainReference) ->

                    Profiling.addCount $"{gunNodePath} unsubscribe"

                    Dom.log
                        (fun () ->
                            $"[atomFamily.unsubscribe()]
{key}
{baseInfo ()} (######## actually skipped) ")

                    if false then
                        gunAtomNode.off () |> ignore
                        lastSubscription <- None
                | None ->
                    Dom.log
                        (fun () ->
                            $"[wrapper.off()]
{baseInfo ()}
skipping unsubscribe, no gun atom node.")
            | None ->
                Dom.log
                    (fun () ->
                        $"[wrapper.off()]
                                {baseInfo ()}
                                skipping unsubscribe. no last subscription found.")

        let setInternalFromSync setAtom =
            JS.debounce
                (fun (ticks, newValue) ->
                    try
                        Dom.logFiltered
                            newValue
                            (fun () ->
                                $"gun.on() value. start.
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
lastValue={lastValue}
ticks={ticks}
{baseInfo ()}                               ")

                        match syncPaused, lastValue with
                        | true, _ ->
                            Dom.logFiltered
                                newValue
                                (fun () ->
                                    $"gun.on() value. skipping. Sync paused.
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
lastValue={lastValue}
ticks={ticks}
{baseInfo ()}                                       ")
                        | _, Some (lastValueTicks, lastValue) when
                            lastValueTicks > ticks
                            || lastValue |> Object.compare (unbox newValue)
                            || (unbox lastValue = null && unbox newValue = null)
                            || (match lastValue |> Option.ofObjUnbox, newValue |> Option.ofObjUnbox with
                                | Some _, None -> true
                                | _ -> false)
                            ->

                            Profiling.addCount $"{gunNodePath} on() skip"

                            Dom.logFiltered
                                newValue
                                (fun () ->
                                    $"gun.on() value. skipping.
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
lastValue={lastValue}
ticks={ticks}
{baseInfo ()}                                   ")
                        | _ ->
                            if unbox newValue = JS.undefined then
                                Dom.logFiltered
                                    newValue
                                    (fun () ->
                                        $"gun.on() value. skipping. newValue=undefined
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
lastValue={lastValue}
ticks={ticks}
{baseInfo ()}                                       ")
                            else
                                try
                                    Profiling.addCount $"{gunNodePath} on() assign"

                                    Dom.logFiltered
                                        newValue
                                        (fun () ->
                                            let _lastValue =
                                                match unbox lastValue with
                                                | Some (_, b) -> b
                                                | _ -> null

                                            if string _lastValue = string newValue then
                                                (Dom.consoleError
                                                    $"should have skipped assign
        lastValue={lastValue} typeof _lastValue={jsTypeof _lastValue}
        newValue={newValue} typeof newValue={jsTypeof newValue}
        ticks={ticks}
        {baseInfo ()}                                      ")

                                            $"gun.on() value. triggering. ##
        lastValue={lastValue} typeof _lastValue={jsTypeof _lastValue}
        newValue={newValue} typeof newValue={jsTypeof newValue}
        ticks={ticks}
        {baseInfo ()}                                        ")

                                    //                        Browser.Dom.window?atomPath <- atomPath
                                    //                        Browser.Dom.window?lastValue <- _lastValue
                                    //                        Browser.Dom.window?newValue <- newValue
                                    //                        Browser.Dom.window?deepEqual <- Object.compare

                                    // setAtom internalAtom

                                    Gun.batchData (fun (_ticks, data) -> setAtom data) newValue
                                with
                                | ex ->
                                    Dom.consoleError ("[exception8]", ex, newValue)
                                    raise ex
                    with
                    | ex ->
                        Dom.consoleError ("[exception1]", ex, newValue)
                        lastSubscription <- None)
                250

        let subscribe =
            JS.debounce
                (fun (setAtom: 'TValue option -> unit) ->
                    promise {
                        lastWrapperSet <- Some setAtom
                        let wrappedSetAtom = wrapSetAtom setAtom

                        match syncEngine.GetGunAtomNode (), lastSubscription with
                        | _, Some _ ->
                            Dom.log
                                (fun () ->
                                    $"[wrapper.on() subscribe]
        {baseInfo ()}
        skipping subscribe, lastSubscription is set.")
                        | Some (key, gunAtomNode), None ->
                            let gunKeys =
                                let user = gunAtomNode.user ()
                                user.__.sea

                            Profiling.addCount $"{gunNodePath} subscribe"

                            Dom.log
                                (fun () ->
                                    $"[wrapper.on() subscribe] batch subscribing.
        {baseInfo ()}
        key={key}               ")

                            //                    gunAtomNode.off () |> ignore

                            match gunKeys with
                            | Some gunKeys ->
                                match syncEngine.GetHub () with
                                | Some hub ->
                                    promise {
                                        try
                                            match lastHubSubscription, syncEngine.GetUsername () with
                                            | Some _, _ -> Dom.consoleError "sub already present"
                                            | None, None -> Dom.consoleError "username is none (subscription)"
                                            | None, Some username ->
                                                let subscription =
                                                    Gun.batchHubSubscribe
                                                        hub
                                                        (Sync.Request.Get (username, gunNodePath))
                                                        (fun (_ticks, msg: Sync.Response) ->
                                                            Dom.log
                                                                (fun () ->
                                                                    $"[wrapper.next() HUB stream subscribe]
                                                                                    {baseInfo ()}
                                                                                    msg={msg}")

                                                            promise {
                                                                match msg with
                                                                | Sync.Response.GetResult result ->
                                                                    let! newValue =
                                                                        match result |> Option.defaultValue null with
                                                                        | null -> unbox null |> Promise.lift
                                                                        | result ->
                                                                            Gun.userDecode<'TValue> gunKeys result

                                                                    lastHubValue <- newValue

                                                                    setInternalFromSync
                                                                        wrappedSetAtom
                                                                        (DateTime.Now.Ticks, newValue)
                                                                | _ -> ()

                                                                return
                                                                    Object.newDisposable
                                                                        (fun () ->
                                                                            Dom.log
                                                                                (fun () ->
                                                                                    $"[wrapper.next() HUB stream subscribe]. Dispose.
                                                                                      {baseInfo ()}
                                                                                      msg={msg}"))
                                                            })

                                                lastHubSubscription <- Some subscription
                                        with
                                        | ex -> Dom.consoleError $"hub.get, setInternalFromGun, error={ex.Message}"
                                    }
                                    |> Promise.start
                                | None ->
                                    Dom.log
                                        (fun () ->
                                            $"[wrapper.on() HUB subscribe]
        {baseInfo ()}
        skipping.                                   ")

                                match syncEngine.GetGunPeers (), syncEngine.GetAtomPath () with
                                | Some _, Some (AtomPath _atomPath) ->
                                    //                                if false then
                                    Gun.batchSubscribe
                                        gunAtomNode
                                        (fun (ticks, data) ->
                                            promise {
                                                let! newValue =
                                                    match data |> Option.defaultValue null with
                                                    | null -> unbox null |> Promise.lift
                                                    | result -> Gun.userDecode<'TValue> gunKeys result

                                                lastGunValue <- newValue


                                                setInternalFromSync wrappedSetAtom (ticks, newValue)


                                                if lastHubValue.IsNone
                                                   || lastHubValue |> Object.compare newValue then
                                                    Dom.logFiltered
                                                        newValue
                                                        (fun () ->
                                                            $"debouncedPut() HUB (update from gun) SKIPPED
                newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
                {baseInfo ()}                           ")
                                                else
                                                    match syncEngine.GetAtomPath (),
                                                          syncEngine.GetHub (),
                                                          syncEngine.GetUsername () with
                                                    | Some (AtomPath atomPath), Some hub, Some username ->
                                                        promise {
                                                            try
                                                                let! response =
                                                                    hub.invokeAsPromise (
                                                                        Sync.Request.Set (
                                                                            username,
                                                                            atomPath,
                                                                            data |> Option.defaultValue null
                                                                        )
                                                                    )

                                                                match response with
                                                                | Sync.Response.SetResult result ->
                                                                    if not result then
                                                                        printfn "$$$$ HUB PUT ERROR (backend console)"
                                                                    else

                                                                        Dom.logFiltered
                                                                            newValue
                                                                            (fun () ->
                                                                                $"subscribe() hub set from gun
                                    newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
                                    {baseInfo ()}                           ")

                                                                        lastHubValue <- newValue
                                                                | response ->
                                                                    Dom.consoleError ("#00002 response:", response)
                                                            with
                                                            | ex -> Dom.consoleError $"$$$$ hub.set, error={ex.Message}"
                                                        }
                                                        |> Promise.start
                                                    | _ ->
                                                        Dom.logFiltered
                                                            newValue
                                                            (fun () ->
                                                                $"[$$$$ wrapper.on() HUB put]
                    {baseInfo ()}
                    skipping.                                                               ")

                                                return
                                                    Object.newDisposable
                                                        (fun () ->
                                                            Dom.log
                                                                (fun () ->
                                                                    $"[$$$$ wrapper.on() HUB put]. Dispose. {baseInfo ()} "))
                                            })

                                    lastSubscription <- Some DateTime.Now.Ticks
                                | _ ->
                                    Dom.log
                                        (fun () ->
                                            $"[wrapper.on() Gun subscribe]
        {baseInfo ()}
        skipping.                               ")
                            | _ ->
                                Dom.log
                                    (fun () ->
                                        $"[wrapper.on() subscribe]
        {baseInfo ()}
        skipping. gun keys empty")



                        | None, _ ->
                            Dom.log
                                (fun () ->
                                    $"[wrapper.on() subscribe]
        {baseInfo ()}
        skipping subscribe, no gun atom node.")
                    }
                    |> Promise.start)
                100

        let putFromUi newValue =
            promise {
                Dom.logFiltered newValue (fun () -> "atomFamily.wrapper.set() debounceGunPut promise. #1")

                try
                    match syncEngine.GetGunAtomNode () with
                    | Some (key, gunAtomNode) ->
                        Dom.logFiltered
                            newValue
                            (fun () -> $"atomFamily.wrapper.set() debounceGunPut promise. #2 before encode {key}")

                        let! newValueJson =
                            if newValue |> JS.ofNonEmptyObj |> Option.isNone then
                                null |> Promise.lift
                            else
                                Gun.userEncode<'TValue> gunAtomNode newValue

                        Dom.logFiltered
                            newValue
                            (fun () ->
                                $"atomFamily.wrapper.set() debounceGunPut promise. #3.
before put {key} newValue={newValue}
    lastHubValue={lastHubValue}
    {baseInfo ()}                            ")

                        match lastHubValue with
                        | Some lastHubValue when
                            lastHubValue |> Object.compare newValue
                            || unbox lastHubValue = null
                            ->
                            Dom.logFiltered
                                newValue
                                (fun () ->
                                    $"debouncedPut() HUB SKIPPED
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
{baseInfo ()}                           ")
                        | _ ->
                            match syncEngine.GetAtomPath (), syncEngine.GetHub (), syncEngine.GetUsername () with
                            | Some (AtomPath atomPath), Some hub, Some username ->
                                promise {
                                    try
                                        let! response =
                                            hub.invokeAsPromise (Sync.Request.Set (username, atomPath, newValueJson))

                                        match response with
                                        | Sync.Response.SetResult result ->
                                            if not result then
                                                printfn "HUB PUT ERROR (backend console)"
                                            else
                                                lastHubValue <- Some newValue
                                        | response -> Dom.consoleError ("#90592 response:", response)
                                    with
                                    | ex -> Dom.consoleError $"hub.set, error={ex.Message}"
                                }
                                |> Promise.start
                            | _ ->
                                Dom.logFiltered
                                    newValue
                                    (fun () ->
                                        $"[wrapper.on() HUB put]
    {baseInfo ()}
    skipping.                                                               ")

                        match lastGunValue with
                        | Some lastGunValue when lastGunValue |> Object.compare newValue ->
                            Dom.logFiltered
                                newValue
                                (fun () ->
                                    $"debouncedPut() SKIPPED
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
{baseInfo ()}                           ")
                        | _ ->
                            if lastGunValue.IsNone
                               || lastGunValue
                                  |> Object.compare (unbox newValue)
                                  |> not
                               || unbox newValue = null then

                                let! putResult = Gun.put gunAtomNode newValueJson

                                if putResult then
                                    lastGunValue <- Some newValue

                                    Dom.logFiltered
                                        newValue
                                        (fun () ->
                                            $"atomFamily.wrapper.set() debounceGunPut promise result.
    newValue={newValue}
    {key}
    {baseInfo ()}                                           ")
                                else
                                    Browser.Dom.window?lastPutResult <- putResult

                                    match Dom.window () with
                                    | Some window ->
                                        if window?Cypress = null then
                                            Dom.consoleError
                                                $"atomFamily.wrapper.set() debounceGunPut promise put error.
     newValue={newValue} putResult={putResult}
     {key}
                                      {baseInfo ()}         "
                                    | None -> ()

                    | None ->
                        Dom.logFiltered
                            newValue
                            (fun () ->
                                $"[gunEffect.debounceGunPut promise]
skipping gun put. no gun atom node.
  {baseInfo ()}                                 ")
                with
                | ex -> Dom.consoleError ("[exception2]", ex, newValue)

                syncPaused <- false

                return
                    Object.newDisposable
                        (fun () ->
                            Dom.logFiltered
                                newValue
                                (fun () ->
                                    $"atomFamily.wrapper.set() putFromUi. Disposed (empty)
    newValue={newValue}
    {baseInfo ()}                                           "))
            }

        let batchPutFromUi newValue =
            Batcher.batch (Batcher.BatchType.Set (fun () -> putFromUi newValue))

        let debouncedPutFromUi = JS.debounce batchPutFromUi 100

        let rec wrapper =
            Store.selector
                atomPath
                (Some (collection, keyIdentifier))
                (fun getter ->
                    syncEngine.SetProviders getter wrapper
                    let userAtom = internalAtom (syncEngine.GetUsername ())

                    let result =
                        Store.value getter userAtom
                        |> Option.ofObjUnbox
                        |> Option.defaultValue defaultValue

                    Profiling.addCount $"{gunNodePath} get"

                    syncEngine
                        .GetLogger()
                        .Debug (fun () ->
                            if (string result
                                |> Option.ofObjUnbox
                                |> Option.defaultValue "")
                                .StartsWith "Ping " then
                                null
                            else
                                $"atomFamily.wrapper.get()
wrapper={wrapper}
userAtom={userAtom}
result={result}
{baseInfo ()}               ")

                    Dom.log
                        (fun () ->
                            if (string result
                                |> Option.ofObjUnbox
                                |> Option.defaultValue "")
                                .StartsWith "Ping " then
                                null
                            else
                                $"atomFamily.wrapper.get()
wrapper={wrapper}
userAtom={userAtom}
result={result}
{baseInfo ()}               ")

                    let userAtomId = Some (userAtom.toString ())

                    if userAtomId <> lastUserAtomId then
                        lastUserAtomId <- userAtomId

                        match lastWrapperSet, lastSubscription with
                        | Some lastWrapperSet, None ->
                            Dom.log
                                (fun () ->
                                    $"atomFamily.wrapper.get() subscribing
wrapper={wrapper}
userAtom={userAtom}
{baseInfo ()}                       ")

                            subscribe lastWrapperSet
                        | _ ->
                            Dom.log
                                (fun () ->
                                    $"atomFamily.wrapper.get() skipping subscribe
wrapper={wrapper}
userAtom={userAtom}
{baseInfo ()}                           ")

                    lastValue <- Some (DateTime.Now.Ticks, result)

                    result)
                (fun getter setter newValueFn ->
                    syncEngine.SetProviders getter wrapper
                    let userAtom = internalAtom (syncEngine.GetUsername ())

                    Profiling.addCount $"{gunNodePath} set"

                    Store.set
                        setter
                        userAtom
                        (unbox
                            (fun oldValue ->
                                let newValue =
                                    match jsTypeof newValueFn with
                                    | "function" -> (unbox newValueFn) oldValue |> unbox
                                    | _ -> newValueFn

                                //                                if true
//                                   || oldValue |> Object.compare newValue |> not
//                                   || (lastValue.IsNone
//                                       && newValue |> Object.compare defaultValue) then
                                let newValueOption = newValue |> Option.ofObjUnbox

                                Dom.logFiltered
                                    newValue
                                    (fun () ->
                                        $"atomFamily.wrapper.set()
wrapper={wrapper}
userAtom={userAtom}
oldValue={oldValue}
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
__x={(newValueOption, lastGunValue, lastHubValue)} y={unbox newValueOption = unbox lastGunValue
                                                      && unbox lastGunValue = unbox lastHubValue}
                                                       z={box newValueOption = box lastGunValue
                                                          && box lastGunValue = box lastHubValue}
{baseInfo ()}                                           ")


                                if box newValueOption = box lastGunValue
                                   && box lastGunValue = box lastHubValue then
                                    Dom.logFiltered
                                        newValue
                                        (fun () ->
                                            $"atomFamily.wrapper.set(). skipped debouncedPut
wrapper={wrapper}
userAtom={userAtom}
oldValue={oldValue}
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
{baseInfo ()}                                   ")
                                else

                                    syncPaused <- true
                                    debouncedPutFromUi newValue

                                lastValue <- Some (DateTime.Now.Ticks, newValue)

                                if JS.jestWorkerId then
                                    match splitAtomPath gunNodePath with
                                    | Some (root, guid) ->
                                        let newSet =
                                            match testKeysCache.TryGetValue root with
                                            | true, guids -> guids |> Set.add guid
                                            | _ -> Set.singleton guid

                                        testKeysCache.[root] <- newSet
                                    | None -> ()

                                newValue)))

        if keyIdentifier
           <> [
               string Guid.Empty
           ] then
            wrapper?onMount <- fun (setAtom: 'TValue option -> unit) ->
                                   subscribe setAtom
                                   fun () -> unsubscribe ()

        wrapper

    let inline selectAtomSyncKeys
        atomPath
        (atomFamily: 'TKey -> Atom<_>)
        (key: 'TKey)
        (onFormat: string -> 'TKey)
        : Atom<Atom<'TKey> []> =
        let referenceAtom = atomFamily key

        let syncEngine = createSyncEngine (Some (fun (key, node) -> key, node.back().back ()))

        let internalAtom = jotaiUtils.atomFamily (fun _username -> jotai.atom [||]) Object.compare

        let baseInfo () =
            $"""
| atomWithSync baseInfo:
{syncEngine.GetBaseInfo ()}
"""

        Dom.log (fun () -> $"@@ selectAtomSyncKeys constructor {baseInfo ()}           ")

        let mutable lastValue: Set<'TKey> option = None

        let rec wrapper =
            Store.selector
                atomPath
                None
                (fun getter ->
                    syncEngine.SetProviders getter referenceAtom
                    let userAtom = internalAtom (syncEngine.GetUsername ())

                    let result =
                        if not JS.jestWorkerId then
                            Store.value getter userAtom
                        else
                            match syncEngine.GetAtomPath () with
                            | Some (AtomPath atomPath) ->
                                match splitAtomPath atomPath with
                                | Some (root, _guid) ->
                                    match testKeysCache.TryGetValue root with
                                    | true, guids -> guids |> Set.toArray |> Array.map onFormat
                                    | _ -> [||]
                                | None -> [||]
                            | None -> [||]

                    Dom.log
                        (fun () ->
                            $"@@ atomFamily.wrapper.get()
                                    wrapper={wrapper}
                                    userAtom={userAtom}
                                    result={result}
                                    {baseInfo ()} ")

                    result)
                (fun getter setter newValueFn ->
                    syncEngine.SetProviders getter referenceAtom
                    let userAtom = internalAtom (syncEngine.GetUsername ())

                    Store.set
                        setter
                        userAtom
                        (unbox
                            (fun oldValue ->
                                let newValue =
                                    match jsTypeof newValueFn with
                                    | "function" -> (unbox newValueFn) oldValue |> unbox
                                    | _ -> newValueFn

                                Dom.log (fun () -> $"@@ newValue={newValue} newValueFn={newValueFn}")

                                newValue)))

        let mutable lastSubscription = None

        let batchKeys setAtom data =
            Gun.batchKeys
                (fun itemsArray ->
                    let items =
                        itemsArray
                        |> Array.collect snd
                        |> Array.append (
                            lastValue
                            |> Option.defaultValue Set.empty
                            |> Set.toArray
                        )
                        |> Array.distinct

                    lastValue <- Some (items |> Set.ofArray)

                    Dom.log
                        (fun () ->
                            $"@@ [atomKeys debouncedSet gun.on() data]
                                                           atomPath={atomPath}
                                                           items.len={items.Length}
                                                           {key} ")

                    items)
                setAtom
                data

        let subscribe =
            JS.debounce
                (fun (setAtom: 'TKey [] -> unit) ->
                    promise {
                        Dom.log (fun () -> "@@ #3")

                        match syncEngine.GetGunAtomNode (), lastSubscription with
                        | _, Some _ ->
                            Dom.log
                                (fun () ->
                                    $"@@ [atomKeys gun.on() subscribing]
                                                       {baseInfo ()}
                                                    skipping subscribe, lastSubscription is set.")
                        | Some (key, gunAtomNode), None ->
                            Dom.log (fun () -> $"@@ [atomKeys gun.on() subscribing] atomPath={atomPath} {key}")

                            let wrappedSetAtom = wrapSetAtom setAtom
                            let batchKeysAtom = batchKeys wrappedSetAtom

                            if syncEngine.GetGunPeers().IsSome then

                                gunAtomNode
                                    .map()
                                    .on (fun data gunKey ->

                                        Dom.log
                                            (fun () ->
                                                $"
                                    @@$ atomKeys gun.on() HUB filter fetching/subscribing] @@@ gunAtomNode.map().on result
                                      data={data} typeof data={jsTypeof data} gunKey={gunKey} typeof gunKey={jsTypeof gunKey}
                                      atomPath={atomPath} syncEngine.atomPath={syncEngine.GetAtomPath ()} key={key}
                                           ")

                                        if data <> null then
                                            let newValue =
                                                [|
                                                    onFormat gunKey
                                                |]


                                            batchKeysAtom newValue)

                                //                        gunAtomNode.on
                                //                            (fun data _key ->
                                //                                let result =
                                //                                    JS.Constructors.Object.entries data
                                //                                    |> unbox<(string * obj) []>
                                //                                    |> Array.filter
                                //                                        (fun (guid, value) ->
                                //                                            guid.Length = 36
                                //                                            && guid <> string Guid.Empty
                                //                                            && value <> null)
                                //                                    |> Array.map fst
                                //
                                //                                if result.Length > 0 then
                                //                                    setData result
                                //                                else
                                //                                    Dom.log(fun () -> $"@@ atomKeys gun.on() API filter fetching/subscribing] @@@
                                //                                    skipping. result.Length=0
                                //                                    atomPath={atomPath} lastAtomPath={lastAtomPath} {key}")
                                //                                    )

                                lastSubscription <- Some DateTime.Now.Ticks

                            Dom.log
                                (fun () ->
                                    $"@@ [atomKeys gun.on() HUB filter fetching/subscribing] @@@ atomPath={atomPath} syncEngine.atomPath={syncEngine.GetAtomPath ()} {key}")

                            //                        (db?data?find {| selector = {| key = atomPath |} |})?``$``?subscribe (fun items ->
                            match syncEngine.GetAtomPath (), syncEngine.GetHub (), syncEngine.GetUsername () with
                            | Some (AtomPath atomPath), Some hub, Some username ->
                                promise {
                                    try
                                        let collection =
                                            match atomPath |> String.split "/" |> Array.toList with
                                            | app :: [ _ ] -> Some app
                                            | _ :: collection :: _ -> Some collection
                                            | _ -> None

                                        match collection with
                                        | Some collection ->
                                            let! response =
                                                hub.invokeAsPromise (Sync.Request.Filter (username, collection))

                                            match response with
                                            | Sync.Response.FilterResult items ->
                                                if items.Length > 0 then
                                                    Dom.log
                                                        (fun () ->
                                                            $"@@ atomKeys gun.on() HUB filter fetching/subscribing] @@@
                                                setting keys locally. items.Length={items.Length}
                                                atomPath={atomPath} syncEngine.atomPath={syncEngine.GetAtomPath ()} {key}")

                                                    batchKeysAtom (items |> Array.map onFormat)
                                                else
                                                    Dom.log
                                                        (fun () ->
                                                            $"@@ atomKeys gun.on() HUB filter fetching/subscribing] @@@
                                                skipping. items.Length=0
                                                atomPath={atomPath} syncEngine.atomPath={syncEngine.GetAtomPath ()} {key}")

                                                Dom.log
                                                    (fun () ->
                                                        $"@@ [wrapper.on() HUB KEYS subscribe]
                                                    atomPath={atomPath}
                                                    items={JS.JSON.stringify items}
                                                            {baseInfo ()}
                                                         ")
                                            | response -> Dom.consoleError ("#84842 response:", response)
                                        | None -> Dom.consoleError ("#04943 invalid collection", collection)
                                    with
                                    | ex -> Dom.consoleError $"@@ hub.filter, error={ex.Message}"
                                }
                                |> Promise.start

                            //                        (collection?find ())?``$``?subscribe (fun items ->
                            //                            Dom.log
                            //                                (fun () ->
                            //                                    $"@@ [wrapper.on() RX KEYS subscribe]
                            //                                    atomPath={atomPath}
                            //                                    items={JS.JSON.stringify items}
                            //                                            {baseInfo ()}
                            //                                         "))
                            | _ ->
                                Dom.log
                                    (fun () ->
                                        $"@@ [wrapper.on() RX KEYS subscribe]
                                        {baseInfo ()}
                                     skipping.")

                        | None, _ ->
                            Dom.log
                                (fun () ->
                                    $"@@ [atomKeys gun.on() subscribing]
                                                       {baseInfo ()}
                                                    skipping subscribe, no gun atom node.")
                    }
                    |> Promise.start)
                100

        let unsubscribe () =
            match lastSubscription with
            | Some ticks when DateTime.ticksDiff ticks < 1000. ->
                Dom.log
                    (fun () ->
                        $"@@ [atomKeys gun.off()]
                                                    {baseInfo ()}
                                                    skipping unsubscribe. jotai resubscribe glitch.")
            | Some _ ->
                match syncEngine.GetGunAtomNode () with
                | Some (key, _gunAtomNode) ->

                    Dom.log
                        (fun () ->
                            $"@@  [atomFamily.unsubscribe()]
                               {key}
                               {baseInfo ()}
                               ############ (actually skipped)
                               ")

                //                    gunAtomNode.off () |> ignore
//                    lastSubscription <- None
                | None ->
                    Dom.log
                        (fun () ->
                            $"@@  [atomKeys gun.off()]
                                                               {baseInfo ()}
                                                               skipping unsubscribe, no gun atom node.")
            | None ->
                Dom.log
                    (fun () ->
                        $"[atomKeys gun.off()]
                                {baseInfo ()}
                                skipping unsubscribe. no last subscription found.")

        wrapper?onMount <- fun (setAtom: 'TKey [] -> unit) ->
                               subscribe setAtom
                               fun _ -> unsubscribe ()

        jotaiUtils.splitAtom wrapper


    let inline atomFamilyWithSync<'TKey, 'TValue>
        collection
        atomPath
        (defaultValueFn: 'TKey -> 'TValue)
        (persist: 'TKey -> string list)
        =
        jotaiUtils.atomFamily
            (fun param -> atomWithSync collection atomPath (defaultValueFn param) (persist param))
            Object.compare

    let inline atomWithStorageSync<'TKey, 'TValue> collection atomPath defaultValue =
        let storageAtom = Store.atomWithStorage collection atomPath defaultValue
        let syncAtom = atomWithSync<'TKey, 'TValue> collection atomPath defaultValue []

        let mutable lastSetAtom: ('TValue option -> unit) option = None
        let mutable lastValue = None

        let rec wrapper =
            Store.selector
                atomPath
                None
                (fun getter ->
                    match Store.value getter syncAtom, Store.value getter storageAtom with
                    | syncValue, storageValue when
                        syncValue |> Object.compare defaultValue
                        && (storageValue |> Object.compare defaultValue
                            || (Store.value getter Atoms.username).IsNone
                            || lastValue.IsNone)
                        ->
                        Store.value getter storageAtom
                    | syncValue, _ ->
                        match lastSetAtom with
                        | Some lastSetAtom when
                            lastValue.IsNone
                            || lastValue
                               |> Object.compare (Some syncValue)
                               |> not
                            ->
                            lastValue <- Some syncValue
                            lastSetAtom (Some syncValue)
                        | _ -> ()

                        syncValue)
                (fun _get setter newValue ->
                    if lastValue.IsNone
                       || lastValue |> Object.compare (Some newValue) |> not then
                        lastValue <- Some newValue
                        Store.set setter syncAtom newValue

                    Store.set setter storageAtom newValue)

        wrapper?onMount <- fun (setAtom: 'TValue option -> unit) ->
                               lastSetAtom <- Some setAtom
                               fun () -> lastSetAtom <- None

        wrapper


    let tempValue =
        let rec tempValue =
            atomFamilyWithSync
                FsStore.collection
                $"{nameof tempValue}"
                (fun (_guid: Guid) -> null: string)
                (fun (guid: Guid) ->
                    [
                        string guid
                    ])

        jotaiUtils.atomFamily
            (fun (AtomPath atomPath) ->

                let guidHash = Crypto.getTextGuidHash atomPath
                let atom = tempValue guidHash

                Dom.log (fun () -> $"tempValueWrapper constructor. atomPath={atomPath} guidHash={guidHash}")

                let wrapper =
                    jotai.atom (
                        (fun getter ->
                            let value = Store.value getter atom
                            Profiling.addCount $"{atomPath} tempValue set"

                            Dom.log
                                (fun () ->
                                    $"tempValueWrapper.get(). atomPath={atomPath} guidHash={guidHash} value={value}")

                            match value with
                            | null -> null
                            | _ ->
                                match Json.decode<string * string option> value with
                                | _, Some value -> value
                                | _ -> null),
                        Some
                            (fun _ setter newValue ->
                                Profiling.addCount $"{atomPath} tempValue set"

                                Dom.log
                                    (fun () ->
                                        $"tempValueWrapper.set(). atomPath={atomPath}
                                        guidHash={guidHash} newValue={newValue}")

                                let newValue = Json.encode (atomPath, newValue |> Option.ofObj)

                                Dom.log (fun () -> $"tempValueWrapper.set(). newValue2={newValue}")

                                Store.set setter atom (newValue |> box |> unbox))
                    )

                wrapper)
            Object.compare


    let provider = jotai.provider

    let emptyArrayAtom = jotai.atom<obj []> [||]

    let inline waitForAll<'T> (atoms: Atom<'T> []) =
        match atoms with
        | [||] -> unbox emptyArrayAtom
        | _ -> jotaiUtils.waitForAll atoms



    let emptyAtom = jotai.atom<obj> null

    let inline getAtomField (atom: InputAtom<'TValue> option) (inputScope: AtomScope) =
        match atom with
        | Some (InputAtom atomPath) ->
            {
                Current =
                    match atomPath with
                    | AtomReference.Atom atom -> Some atom
                    | _ -> Some (unbox emptyAtom)
                Temp =
                    //                    Dom.log
//                        (fun () -> $"getAtomField atomPath={atomPath} queryAtomPath atomPath={queryAtomPath atomPath}")

                    match queryAtomPath atomPath, inputScope with
                    | Some atomPath, AtomScope.Temp -> Some (tempValue atomPath)
                    | _ -> None
            }
        | _ -> { Current = None; Temp = None }


    let inline setTempValue<'TValue9, 'TKey> (setter: SetFn) (atom: Atom<'TValue9>) (value: 'TValue9) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.Temp

        match atomField.Temp with
        | Some atom -> Store.set setter atom (value |> Json.encode<'TValue9>)
        | _ -> ()

    let inline scopedSet<'TValue10, 'TKey>
        (setter: SetFn)
        (atomScope: AtomScope)
        (atom: 'TKey -> Atom<'TValue10>, key: 'TKey, value: 'TValue10)
        =
        match atomScope with
        | AtomScope.Current -> Store.set setter (atom key) value
        | AtomScope.Temp -> setTempValue<'TValue10, 'TKey> setter (atom key) value

    let inline resetTempValue<'TValue8, 'TKey> (setter: SetFn) (atom: Atom<'TValue8>) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.Temp

        match atomField.Temp with
        | Some atom -> Store.set setter atom null
        | _ -> ()

    let rec ___emptyTempAtom = nameof ___emptyTempAtom

    let inline getTempValue<'TValue11, 'TKey> getter (atom: Atom<'TValue11>) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.Temp

        match atomField.Temp with
        | Some tempAtom ->
            let result = Store.value getter tempAtom

            match result with
            | result when result = ___emptyTempAtom -> unbox null
            | null -> Store.value getter atom
            | _ -> Json.decode<'TValue11> result
        | _ -> Store.value getter atom

    let inline deleteRoot getter atom =
        promise {
            let username = Store.value getter Atoms.username
            let atomPath = queryAtomPath (AtomReference.Atom atom)
            let gunAtomNode = gunAtomNodeFromAtomPath getter username atomPath

            match gunAtomNode with
            | Some (_key, gunAtomNode) ->
                let! _putResult = Gun.put (gunAtomNode.back ()) (unbox null)
                ()
            | None -> ()
        }
