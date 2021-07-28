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
    open Store

    let inline gunAtomNodeFromAtomPath getter username atomPath =
        match username, atomPath with
        | Some username, Some atomPath ->
            match value getter (Selectors.gunAtomNode (username, atomPath)) with
            | Some gunAtomNode -> Some ($">> atomPath={atomPath} username={username}", gunAtomNode)
            | _ -> None
        | _ -> None

    let testKeysCache = Dictionary<string, Set<string>> ()

    let splitAtomPath atomPath =
        let matches =
            (JSe.RegExp @"(.*?)\/([\w-]{36})\/\w+.*?")
                .Match atomPath
            |> Option.ofObj
            |> Option.defaultValue Seq.empty
            |> Seq.toList

        match matches with
        | _match :: root :: guid :: _key -> Some (root, guid)
        | _ -> None

    // https://i.imgur.com/GB8trpT.png        :~ still trash
    let inline atomWithSync<'TKey, 'TValue> (collection, atomPath, defaultValue: 'TValue, keyIdentifier: string list) =
        let mutable lastGunAtomNode = None
        let mutable lastUsername = None
        let mutable lastHub = None
        let mutable lastValue = None
        let mutable lastGunValue2 = None
        let mutable lastApiValue = None
        let mutable lastAtomPath = None
        let mutable lastUserAtomId = None
        let mutable lastWrapperSet = None
        let mutable syncPaused = false

        let assignLastGunAtomNode getter atom =
            if lastAtomPath.IsNone then
                lastAtomPath <- queryAtomPath (AtomReference.Atom (unbox atom))


            Dom.log
                (fun () ->
                    match lastAtomPath with
                    | Some (AtomPath atomPath) when atomPath.Contains "devicePing" |> not ->
                        $"assignLastGunAtomNode lastAtomPath={atomPath} atom={atom}"
                    | _ -> null)

            let username = value getter Atoms.username
            lastUsername <- username
            lastGunAtomNode <- gunAtomNodeFromAtomPath getter username lastAtomPath

            match lastAtomPath, lastGunAtomNode with
            | Some _atomPath, Some _ -> lastHub <- value getter Selectors.hub
            | _ -> ()

            username


        let internalAtom = jotaiUtils.atomFamily (fun _username -> jotai.atom defaultValue) Object.compare

        let gunNodePath = Gun.getGunNodePath collection atomPath keyIdentifier

        Profiling.addCount $"{gunNodePath} constructor"

        let baseInfo () =
            $"""gunNodePath={gunNodePath}
atomPath={atomPath}
keyIdentifier={keyIdentifier}
lastValue={lastValue}
lastGunValue={lastGunValue2}
lastApiValue={lastApiValue}
lastGunAtomNode={lastGunAtomNode}
lastAtomPath={lastAtomPath}
lastUserAtomId={lastUserAtomId} """


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
                match lastGunAtomNode with
                | Some (key, gunAtomNode) ->

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
                                    Gun.batchData setAtom newValue
                                with
                                | ex ->
                                    Dom.consoleError ("[exception8]", ex, newValue)
                                    raise ex
                    with
                    | ex ->
                        Dom.consoleError ("[exception1]", ex, newValue)
                        lastSubscription <- None)
                250

        let mutable lastApiSubscription = None

        let subscribe =
            JS.debounce
                (fun setAtom ->
                    lastWrapperSet <- Some setAtom

                    match lastGunAtomNode, lastSubscription with
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
                            match lastHub with
                            | Some hub ->
                                promise {
                                    try
                                        match lastApiSubscription, lastUsername with
                                        | Some _, _ -> Dom.consoleError "sub already present"
                                        | None, None -> Dom.consoleError "username is none (subscription)"
                                        | None, Some username ->
                                            let subscription =
                                                Gun.batchApiSubscribe
                                                    hub
                                                    (Sync.Request.Get (username, gunNodePath))
                                                    (fun (_ticks, msg: Sync.Response) ->
                                                        Dom.log
                                                            (fun () ->
                                                                $"[wrapper.next() HUB stream subscribe]
                                                                                {baseInfo ()}
                                                                                msg={msg}")

                                                        match msg with
                                                        | Sync.Response.GetResult result ->
                                                            promise {
                                                                let! newValue =
                                                                    match result |> Option.defaultValue null with
                                                                    | null -> unbox null |> Promise.lift
                                                                    | result -> Gun.userDecode<'TValue> gunKeys result

                                                                lastApiValue <- newValue

                                                                setInternalFromSync
                                                                    setAtom
                                                                    (DateTime.Now.Ticks, newValue)
                                                            }
                                                        | _ -> promise { () })


                                            lastApiSubscription <- Some subscription
                                    with
                                    | ex -> Dom.consoleError $"api.get, setInternalFromGun, error={ex.Message}"
                                }
                                |> Promise.start
                            | None ->
                                Dom.log
                                    (fun () ->
                                        $"[wrapper.on() API subscribe]
    {baseInfo ()}
    skipping.                                   ")

                            match lastAtomPath with
                            | Some (AtomPath _atomPath) ->
                                //                                if false then
                                Gun.batchSubscribe
                                    gunAtomNode
                                    (fun (ticks, data) ->
                                        promise {
                                            let! newValue =
                                                match data |> Option.defaultValue null with
                                                | null -> unbox null |> Promise.lift
                                                | result -> Gun.userDecode<'TValue> gunKeys result

                                            lastGunValue2 <- newValue


                                            setInternalFromSync setAtom (ticks, newValue)


                                            if lastApiValue |> Object.compare newValue then
                                                Dom.logFiltered
                                                    newValue
                                                    (fun () ->
                                                        $"debouncedPut() API (update from gun) SKIPPED
            newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
            {baseInfo ()}                           ")
                                            else
                                                match lastAtomPath, lastHub, lastUsername with
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
                                                                    printfn "$$$$ API PUT ERROR (backend console)"
                                                                else
                                                                    lastApiValue <- newValue
                                                            | response ->
                                                                Dom.consoleError ("#00002 response:", response)
                                                        with
                                                        | ex -> Dom.consoleError $"$$$$ api.set, error={ex.Message}"
                                                    }
                                                    |> Promise.start
                                                | _ ->
                                                    Dom.logFiltered
                                                        newValue
                                                        (fun () ->
                                                            $"[$$$$ wrapper.on() API put]
                {baseInfo ()}
                skipping.                                                               ")
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
    skipping subscribe, no gun atom node."))
                100

        let putFromUi newValue =
            promise {
                Dom.logFiltered newValue (fun () -> "atomFamily.wrapper.set() debounceGunPut promise. #1")

                try
                    match lastGunAtomNode with
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
    lastApiValue={lastApiValue}
    {baseInfo ()}                            ")

                        match lastApiValue with
                        | Some lastApiValue when
                            lastApiValue |> Object.compare newValue
                            || unbox lastApiValue = null
                            ->
                            Dom.logFiltered
                                newValue
                                (fun () ->
                                    $"debouncedPut() API SKIPPED
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
{baseInfo ()}                           ")
                        | _ ->
                            match lastAtomPath, lastHub, lastUsername with
                            | Some (AtomPath atomPath), Some hub, Some username ->
                                promise {
                                    try
                                        let! response =
                                            hub.invokeAsPromise (Sync.Request.Set (username, atomPath, newValueJson))

                                        match response with
                                        | Sync.Response.SetResult result ->
                                            if not result then
                                                printfn "API PUT ERROR (backend console)"
                                            else
                                                lastApiValue <- Some newValue
                                        | response -> Dom.consoleError ("#90592 response:", response)
                                    with
                                    | ex -> Dom.consoleError $"api.set, error={ex.Message}"
                                }
                                |> Promise.start
                            | _ ->
                                Dom.logFiltered
                                    newValue
                                    (fun () ->
                                        $"[wrapper.on() API put]
    {baseInfo ()}
    skipping.                                                               ")

                        match lastGunValue2 with
                        | Some lastGunValue when lastGunValue |> Object.compare newValue ->
                            Dom.logFiltered
                                newValue
                                (fun () ->
                                    $"debouncedPut() SKIPPED
newValue={newValue} jsTypeof-newValue={jsTypeof newValue}
{baseInfo ()}                           ")
                        | _ ->
                            if lastGunValue2.IsNone
                               || lastGunValue2
                                  |> Object.compare (unbox newValue)
                                  |> not
                               || unbox newValue = null then

                                let! putResult = Gun.put gunAtomNode newValueJson

                                if putResult then
                                    lastGunValue2 <- Some newValue

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
            }

        let batchPutFromUi newValue =
            Batcher.batch (Batcher.BatchType.Set (fun () -> putFromUi newValue |> Promise.start))

        let debouncedPutFromUi = JS.debounce batchPutFromUi 100

        let rec wrapper =
            selector (
                atomPath,
                (Some (collection, keyIdentifier)),
                (fun getter ->
                    let logger = value getter Selectors.log
                    let username = assignLastGunAtomNode getter wrapper
                    let userAtom = internalAtom username

                    let result =
                        value getter userAtom
                        |> Option.ofObjUnbox
                        |> Option.defaultValue defaultValue

                    Profiling.addCount $"{gunNodePath} get"

                    logger.debug
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

                    result),
                (fun getter setter newValueFn ->
                    let username = assignLastGunAtomNode getter wrapper
                    let userAtom = internalAtom username

                    Profiling.addCount $"{gunNodePath} set"

                    set
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
__x={(newValueOption, lastGunValue2, lastApiValue)} y={unbox newValueOption = unbox lastGunValue2
                                                       && unbox lastGunValue2 = unbox lastApiValue}
{baseInfo ()}                                           ")


                                if box newValueOption = box lastGunValue2
                                   && box lastGunValue2 = box lastApiValue then
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
            )

        if keyIdentifier
           <> [
               string Guid.Empty
           ] then
            wrapper?onMount <- fun setAtom ->
                                   subscribe setAtom
                                   fun () -> unsubscribe ()

        wrapper

    let inline selectAtomSyncKeys
        (
            collection,
            atomPath: string,
            atomFamily: 'TKey -> Atom<_>,
            key: 'TKey,
            onFormat: string -> 'TKey
        ) : Atom<Atom<'TKey> []> =
        Profiling.addCount $"{atomPath} :selectAtomSyncKeys"

        let atom = atomFamily key
        Dom.log (fun () -> "@@ #1")

        let mutable lastGunAtomNode = None
        let mutable lastUsername = None
        let mutable lastHub = None
        let mutable lastAtomPath = None
        let mutable lastGunPeers = None

        let assignLastGunAtomNode getter =
            if lastAtomPath.IsNone then
                lastAtomPath <- queryAtomPath (AtomReference.Atom (unbox atom))

            Dom.log (fun () -> $"@@ assignLastGunAtomNode lastAtomPath={lastAtomPath} atom={atom}")

            let username = value getter Atoms.username
            lastUsername <- username

            let gunPeers = value getter Atoms.gunPeers
            lastGunPeers <- gunPeers

            lastGunAtomNode <-
                gunAtomNodeFromAtomPath getter username lastAtomPath
                |> Option.map (fun (key, node) -> key, node.back().back ())

            match lastAtomPath, lastGunAtomNode with
            | Some _atomPath, Some _ -> lastHub <- value getter Selectors.hub
            | _ -> ()

            username

        let internalAtom = jotaiUtils.atomFamily (fun _username -> jotai.atom [||]) Object.compare

        let keyIdentifier = []
        let gunNodePath = Gun.getGunNodePath collection atomPath keyIdentifier

        Profiling.addCount $"@@ {gunNodePath} constructor"

        let baseInfo () =
            $"""@@ gunNodePath={gunNodePath}
atomPath={atomPath}
keyIdentifier={keyIdentifier}
lastGunAtomNode={lastGunAtomNode}
lastAtomPath={lastAtomPath} """

        Dom.log
            (fun () ->
                $"@@ atomFamily constructor
{baseInfo ()}           ")

        let mutable lastValue: Set<'TKey> option = None

        let rec wrapper =
            selector (
                atomPath,
                None,
                (fun getter ->
                    let username = assignLastGunAtomNode getter
                    let userAtom = internalAtom username

                    Profiling.addCount $"{gunNodePath} get"


                    let result =
                        if not JS.jestWorkerId then
                            value getter userAtom
                        else
                            match lastAtomPath with
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

                    result),
                (fun getter setter newValueFn ->
                    let username = assignLastGunAtomNode getter
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

                                Dom.log (fun () -> $"@@ newValue={newValue} newValueFn={newValueFn}")

                                newValue)))
            )

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
                (fun setAtom ->
                    Dom.log (fun () -> "@@ #3")

                    match lastGunAtomNode, lastSubscription with
                    | _, Some _ ->
                        Dom.log
                            (fun () ->
                                $"@@ [atomKeys gun.on() subscribing]
                                                       {baseInfo ()}
                                                    skipping subscribe, lastSubscription is set.")
                    | Some (key, gunAtomNode), None ->
                        Profiling.addCount $"@@ {gunNodePath} subscribe"
                        Dom.log (fun () -> $"@@ [atomKeys gun.on() subscribing] atomPath={atomPath} {key}")

                        let batchKeysAtom = batchKeys setAtom

                        if lastGunPeers.IsSome then

                            gunAtomNode
                                .map()
                                .on (fun data gunKey ->

                                    Dom.log
                                        (fun () ->
                                            $"
                                    @@$ atomKeys gun.on() API filter fetching/subscribing] @@@ gunAtomNode.map().on result
                                      data={data} typeof data={jsTypeof data} gunKey={gunKey} typeof gunKey={jsTypeof gunKey}
                                      atomPath={atomPath} lastAtomPath={lastAtomPath} key={key}
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
                                $"@@ [atomKeys gun.on() API filter fetching/subscribing] @@@ atomPath={atomPath} lastAtomPath={lastAtomPath} {key}")

                        //                        (db?data?find {| selector = {| key = atomPath |} |})?``$``?subscribe (fun items ->
                        match lastAtomPath, lastHub, lastUsername with
                        | Some (AtomPath atomPath), Some hub, Some username ->
                            promise {
                                try
                                    let collection =
                                        match atomPath.Split '/' |> Array.toList with
                                        | app :: [ _ ] -> Some app
                                        | _ :: collection :: _ -> Some collection
                                        | _ -> None

                                    match collection with
                                    | Some collection ->
                                        let! response = hub.invokeAsPromise (Sync.Request.Filter (username, collection))

                                        match response with
                                        | Sync.Response.FilterResult items ->
                                            if items.Length > 0 then
                                                Dom.log
                                                    (fun () ->
                                                        $"@@ atomKeys gun.on() API filter fetching/subscribing] @@@
                                                setting keys locally. items.Length={items.Length}
                                                atomPath={atomPath} lastAtomPath={lastAtomPath} {key}")

                                                batchKeysAtom (items |> Array.map onFormat)
                                            else
                                                Dom.log
                                                    (fun () ->
                                                        $"@@ atomKeys gun.on() API filter fetching/subscribing] @@@
                                                skipping. items.Length=0
                                                atomPath={atomPath} lastAtomPath={lastAtomPath} {key}")

                                            Dom.log
                                                (fun () ->
                                                    $"@@ [wrapper.on() API KEYS subscribe]
                                                    atomPath={atomPath}
                                                    items={JS.JSON.stringify items}
                                                            {baseInfo ()}
                                                         ")
                                        | response -> Dom.consoleError ("#84842 response:", response)
                                    | None -> Dom.consoleError ("#04943 invalid collection", collection)
                                with
                                | ex -> Dom.consoleError $"@@ api.filter, error={ex.Message}"
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
                                                    skipping subscribe, no gun atom node."))
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
                match lastGunAtomNode with
                | Some (key, _gunAtomNode) ->

                    Profiling.addCount $"@@ {gunNodePath} unsubscribe"

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

        wrapper?onMount <- fun setAtom ->
                               subscribe setAtom
                               fun _ -> unsubscribe ()

        jotaiUtils.splitAtom wrapper


    let inline atomFamilyWithSync<'TKey, 'TValue>
        (
            databaseHeader,
            atomPath,
            defaultValueFn: 'TKey -> 'TValue,
            persist: 'TKey -> string list
        ) =
        jotaiUtils.atomFamily
            (fun param -> atomWithSync (databaseHeader, atomPath, defaultValueFn param, persist param))
            Object.compare

    let inline atomWithStorageSync<'TKey, 'TValue> (collection, atomPath, defaultValue) =
        let storageAtom = atomWithStorage (collection, atomPath, defaultValue)
        let syncAtom = atomWithSync<'TKey, 'TValue> (collection, atomPath, defaultValue, [])

        let mutable lastSetAtom = None
        let mutable lastValue = None

        let rec wrapper =
            selector (
                atomPath,
                None,
                (fun getter ->
                    match value getter syncAtom, value getter storageAtom with
                    | syncValue, storageValue when
                        syncValue |> Object.compare defaultValue
                        && (storageValue |> Object.compare defaultValue
                            || (value getter Atoms.username).IsNone
                            || lastValue.IsNone)
                        ->
                        value getter storageAtom
                    | syncValue, _ ->
                        match lastSetAtom with
                        | Some lastSetAtom when
                            lastValue.IsNone
                            || lastValue
                               |> Object.compare (Some syncValue)
                               |> not
                            ->
                            lastValue <- Some syncValue
                            lastSetAtom syncValue
                        | _ -> ()

                        syncValue),
                (fun _get setter newValue ->
                    if lastValue.IsNone
                       || lastValue |> Object.compare (Some newValue) |> not then
                        lastValue <- Some newValue
                        set setter syncAtom newValue

                    set setter storageAtom newValue)
            )

        wrapper?onMount <- fun setAtom ->
                               lastSetAtom <- setAtom
                               fun () -> lastSetAtom <- None

        wrapper


    let tempValue =
        let rec tempValue =
            atomFamilyWithSync (
                collection,
                $"{nameof tempValue}",
                (fun (_guid: Guid) -> null: string),
                (fun (guid: Guid) ->
                    [
                        string guid
                    ])
            )

        jotaiUtils.atomFamily
            (fun (AtomPath atomPath) ->

                let guidHash = Crypto.getTextGuidHash atomPath
                let atom = tempValue guidHash

                Dom.log (fun () -> $"tempValueWrapper constructor. atomPath={atomPath} guidHash={guidHash}")

                let wrapper =
                    jotai.atom (
                        (fun getter ->
                            let value = value getter atom
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

                                set setter atom (newValue |> box |> unbox))
                    )

                wrapper)
            Object.compare


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





    [<RequireQualifiedAccess>]
    type InputScope<'TValue> =
        | Current
        | Temp of Gun.Serializer<'TValue>

    and InputScope<'TValue> with
        static member inline AtomScope<'TValue> (inputScope: InputScope<'TValue> option) =
            match inputScope with
            | Some (InputScope.Temp _) -> AtomScope.Temp
            | _ -> AtomScope.Current

    and [<RequireQualifiedAccess>] AtomScope =
        | Current
        | Temp

    type InputAtom<'T> = InputAtom of atomPath: AtomReference<'T>

    type AtomField<'TValue67> =
        {
            Current: Jotai.Atom<'TValue67> option
            Temp: Jotai.Atom<string> option
        }

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


    let inline setTempValue<'TValue9, 'TKey> (setter: Jotai.SetFn, atom: Jotai.Atom<'TValue9>, value: 'TValue9) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.Temp

        match atomField.Temp with
        | Some atom -> set setter atom (value |> Json.encode<'TValue9>)
        | _ -> ()

    let inline scopedSet<'TValue10, 'TKey>
        (setter: Jotai.SetFn)
        (atomScope: AtomScope)
        (atom: 'TKey -> Jotai.Atom<'TValue10>, key: 'TKey, value: 'TValue10)
        =
        match atomScope with
        | AtomScope.Current -> set setter (atom key) value
        | AtomScope.Temp -> setTempValue<'TValue10, 'TKey> (setter, atom key, value)

    let inline resetTempValue<'TValue8, 'TKey> (setter: Jotai.SetFn) (atom: Jotai.Atom<'TValue8>) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.Temp

        match atomField.Temp with
        | Some atom -> set setter atom null
        | _ -> ()

    let rec ___emptyTempAtom = nameof ___emptyTempAtom

    let inline getTempValue<'TValue11, 'TKey> getter (atom: Jotai.Atom<'TValue11>) =
        let atomField = getAtomField (Some (InputAtom (AtomReference.Atom atom))) AtomScope.Temp

        match atomField.Temp with
        | Some tempAtom ->
            let result = value getter tempAtom

            match result with
            | result when result = ___emptyTempAtom -> unbox null
            | null -> value getter atom
            | _ -> Json.decode<'TValue11> result
        | _ -> value getter atom

    let deleteRoot getter atom =
        promise {
            let username = value getter Atoms.username
            let atomPath = queryAtomPath (AtomReference.Atom atom)
            let gunAtomNode = gunAtomNodeFromAtomPath getter username atomPath

            match gunAtomNode with
            | Some (_key, gunAtomNode) ->
                let! _putResult = Gun.put (gunAtomNode.back ()) (unbox null)
                ()
            | None -> ()
        }
