namespace Fluke.UI.Frontend.Bindings

#nowarn "40"


open Fluke.UI.Frontend.Bindings
open Fable.Core.JsInterop
open Fable.Core
open System
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open Microsoft.FSharp.Core.Operators
open Feliz


//[<AutoOpen>]
//module StoreMagic =
//    type StoreValue<'T>  =
//        class
//        end
//
//    [<Erase>]
//    type SelectorGetter =
//        [<Emit("$0.get($1)")>]
//        member _.get (_storeValue: StoreValue<'T>) : 'T = jsNative
//
//

//    type atom = unit
//
//    type atomFamily = unit
//    type selector = unit
//    type selectorFamily = unit

//    type Store = Recoil

module Store =
    open Jotai
    open JotaiTypes
    //    type InputAtom<'TValue1, 'TKey> =
//        | Atom of Username * StoreValue<'TValue1>
//        | AtomFamily of Username * ('TKey -> StoreValue<'TValue1>) * 'TKey
//        | AtomPath of Username * atomPath: string


    //    let inline useValueLoadableDefault atom def = Recoil.useValueLoadableDefault atom def
//    let inline useStateLoadableDefault atom def = Recoil.useStateLoadableDefault atom def

    //    type CallbackMethods = Feliz.Recoil.CallbackMethods
//    type AtomEffect<'T, 'U> = Feliz.Recoil.AtomEffect<'T, 'U>

    //        Recoil.gunEffect<'TValue3, 'TKey>

    //    let inline gunKeyEffect<'TAtomValue, 'TValue, 'TKey when 'TValue: comparison> =
//        Recoil.gunKeyEffect<'TAtomValue, 'TValue, 'TKey>
    //    let inline gunEffect<'TValue3, 'TKey> (atom: InputAtom<'TValue3, 'TKey>) (keyIdentifier: string list) =
//        Recoil.gunEffect atom keyIdentifier

    let inline gunEffect<'TValue3, 'TKey> = fun _ _ -> ()


    let inline atomSetterWithProfiling<'TValue>
        (
            atomPath,
            keyIdentifier,
            getFn: GetFn -> 'TValue,
            setFn: GetFn -> SetFn -> 'TValue -> unit
        ) =
        Jotai.atom (
            (fun get ->
                Profiling.addCount atomPath
                getFn get),
            Some
                (fun get set value ->
                    Profiling.addCount $"{atomPath}<"
                    let newValue = value
                    //                        match jsTypeof value with
//                         | "function" -> (unbox value) () |> unbox
//                         | _ -> value
                    setFn get set newValue)
        )
        |> registerAtom atomPath keyIdentifier

    let inline asyncAtomSetterWithProfiling<'TValue>
        (
            atomPath,
            getFn: GetFn -> JS.Promise<'TValue>,
            setFn: GetFn -> SetFn -> 'TValue -> JS.Promise<unit>
        ) =
        Jotai.atom (
            (fun get ->
                promise {
                    Profiling.addCount $"{atomPath}>"
                    let a = getFn get
                    return! a
                }),
            Some
                (fun get set newValue ->
                    promise {
                        Profiling.addCount $"{atomPath}<"
                        do! setFn get set newValue
                    })
        )
        |> registerAtom atomPath None

    let inline selectorFamilyWithProfiling<'TKey, 'TValue>
        (
            atomPath,
            getFn: 'TKey -> GetFn -> 'TValue
        ) : ('TKey -> Atom<'TValue>) =
        JotaiUtils.atomFamily
            (fun param ->
                atomSetterWithProfiling (
                    atomPath,
                    None,
                    (getFn param),
                    (fun _ -> failwith $"readonly selector {atomPath}")
                ))
            DeepEqual.deepEqual

    let inline selectorFamilySetterWithProfiling<'TKey, 'TValue>
        (
            atomPath,
            getFn: 'TKey -> GetFn -> 'TValue,
            setFn: 'TKey -> GetFn -> SetFn -> 'TValue -> unit
        ) =
        JotaiUtils.atomFamily
            (fun param -> atomSetterWithProfiling (atomPath, None, getFn param, setFn param))
            DeepEqual.deepEqual

    let inline asyncSelectorFamilyWithProfiling<'TKey, 'TValue>
        (
            atomPath,
            getFn: 'TKey -> GetFn -> JS.Promise<'TValue>,
            setFn: 'TKey -> GetFn -> SetFn -> 'TValue -> JS.Promise<unit>
        ) =
        JotaiUtils.atomFamily
            (fun param ->
                asyncAtomSetterWithProfiling (
                    (atomPath,
                     (fun get -> promise { return! getFn param get }),
                     (fun get set newValue -> promise { do! setFn param get set newValue }))
                ))
            DeepEqual.deepEqual

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


    //    let getAtomPath (atom: InputAtom) (keyIdentifier: string list) =
//        let username, rawAtomKey, atomPath =
//            match atom with
//            | InputAtom.Atom metadata -> metadata.Username, (Some (metadata.Atom.toString())), None
//            | InputAtom.AtomPath metadata -> metadata.Username, None, Some metadata.AtomPath
//
//        let newAtomPath =
//            atomPath
//            |> Option.defaultValue (
//                match rawAtomKey with
//                | Some rawAtomKey -> getInternalAtomPath rawAtomKey keyIdentifier
//                | None -> failwith $"Invalid rawAtomKey: {rawAtomKey}"
//            )
//
//        username, newAtomPath

    let rec getInternalGunAtomNode (gun: Gun.IGunChainReference) (Username username) (atomPath: AtomPath<_>) =
        let user = gun.user ()

        match queryAtomPath atomPath, user.is with
        | Some atomPath, Some { alias = Some username' } when username' = username ->
            let nodes = atomPath |> String.split "/" |> Array.toList

            (Some (user.get nodes.Head), nodes.Tail)
            ||> List.fold
                    (fun result node ->
                        result
                        |> Option.map (fun result -> result.get node))
        | _ ->
            match JS.window id with
            | Some window ->
                JS.setTimeout
                    (fun () -> window?lastToast (fun (x: Chakra.IToastProps) -> x.description <- "Please log in again"))
                    0
                |> ignore
            | None -> ()

            failwith
                $"Invalid username. username={username} user.is={JS.JSON.stringify user.is} username={username} atomPath={
                                                                                                                              atomPath
                }"

    //    let inline getGunAtomNode gun (InputAtom (username, atomPath)) =
//        let gunAtomNode = getInternalGunAtomNode gun username atomPath
//        username, atomPath, gunAtomNode

    let atomFamilyWithProfiling<'TKey, 'TValue>
        (
            atomPath,
            defaultValue: 'TKey -> 'TValue,
            persist: 'TKey -> (Username * string list) option
        ) =
        JotaiUtils.atomFamily
            (fun param ->

                let internalAtom = Jotai.atom (defaultValue param)

                let username, keyIdentifier =
                    match persist param with
                    | Some (username, keyIdentifier) -> Some username, Some keyIdentifier
                    | None -> None, None

//                printfn $"atomFamily constructor atomPath={atomPath} param={param} keyIdentifier={keyIdentifier}"

                let rec wrapper =
                    atomSetterWithProfiling (
                        atomPath,
                        keyIdentifier |> Option.defaultValue [] |> Some,
                        (fun get ->
                            let gunAtomNode =
                                match username with
                                | Some username ->
                                    let gun = Atoms.getAtomValue get Atoms.gun
                                    getInternalGunAtomNode gun username (AtomPath.Atom wrapper)
                                | None -> None

                            let result = Atoms.getAtomValue get internalAtom

//                            printfn
//                                $"atomFamily.get() atomPath={atomPath} keyIdentifier={keyIdentifier} param={param} result={
//                                                                                                                               result
//                                }"

                            result),
                        (fun get set newValue ->
                            let gun = Atoms.getAtomValue get Atoms.gun

//                            printfn
//                                $"atomFamily.set() atomPath={atomPath} keyIdentifier={keyIdentifier} param={param} newValue={
//                                                                                                                                 newValue
//                                }"

                            Atoms.setAtomValue set internalAtom newValue)
                    )

                let subscribe setAtom =
                    ()
//                    printfn $"atomFamily debouncedSubscribe() atomPath={atomPath} param={param}"
                // setAtom (fun x ->
//                                    printfn $"jotaiAtomFamily: setting {x} + 1"
//                                    x + 1)

                let unsubscribe setAtom =
                    ()
//                    printfn $"atomFamily debouncedUnsubscribe() atomPath={atomPath} param={param}"

                let mutable lastSubscription = None

                let debouncedSubscribe =
                    //                    JS.debounce
                    (fun setAtom ->
                        if lastSubscription.IsNone then
                            subscribe setAtom
                            lastSubscription <- Some DateTime.Now.Ticks
                        else
//                            printf $"atomFamily Skipping subscribe atomPath={atomPath} param={param} diffMs={DateTime.ticksDiff (lastSubscription |>Option.defaultValue DateTime.Now.Ticks)}"
                            ())
                //                        0

                let debouncedUnsubscribe =
                    //                    JS.debounce
                    (fun setAtom ->
                        match lastSubscription with
                        | Some ticks when DateTime.ticksDiff ticks < 300. ->
                            ()
//                            printf $"atomFamily Skipping unsubscribe atomPath={atomPath} param={param} diffMs={DateTime.ticksDiff (lastSubscription |>Option.defaultValue DateTime.Now.Ticks)}"
                        | _ ->
                            unsubscribe setAtom
                            lastSubscription <- None)
                //                        0

                wrapper?onMount <- fun setAtom ->
                                       debouncedSubscribe setAtom
                                       fun () -> debouncedUnsubscribe setAtom

                wrapper)
            DeepEqual.deepEqual

    let rec private readWriteValue =
        atomFamilyWithProfiling (
            $"{nameof User}/{nameof readWriteValue}",
            (fun (_username: Username, _guid: Guid) -> null: string),
            (fun (username: Username, guid: Guid) ->
                Some (
                    username,
                    [
                        string guid
                    ]
                ))
        )

    let readWriteValueWrapper =
        JotaiUtils.atomFamily
            (fun (username: Username, atomPath: string) ->

                let guidHash = Crypto.getTextGuidHash atomPath
                let pathHash = username, guidHash

                printfn $"readWriteValueWrapper constructor. atomPath={atomPath} guidHash={guidHash}"

                let wrapper =
                    Jotai.atom (
                        (fun get ->
                            let value = Atoms.getAtomValue get (readWriteValue pathHash)

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
                                Profiling.addCount $"{atomPath}-readWriteValue<"

                                let newValue =
                                    Gun.jsonEncode
                                        {
                                            AtomPath = atomPath
                                            Value = newValue |> Option.ofObj
                                        }

                                printfn
                                    $"readWriteValueWrapper.set(). atomPath={atomPath} guidHash={guidHash} newValue={
                                                                                                                         newValue
                                    }"

                                Atoms.setAtomValue set (readWriteValue pathHash) (newValue |> box |> unbox))
                    )
                    |> registerAtom (string guidHash) None

                wrapper)
            DeepEqual.deepEqual

    let emptyAtom = Jotai.atom<obj> null

    let inline getAtomField (atom: InputAtom<'TValue> option) (inputScope: AtomScope) =
        match atom with
        | Some (InputAtom (username, atomPath)) ->
            {
                ReadOnly =
                    match atomPath with
                    | AtomPath.Atom atom -> Some atom
                    | _ -> Some (unbox emptyAtom)
                ReadWrite =
                    match queryAtomPath atomPath, inputScope with
                    | Some atomPath, AtomScope.ReadWrite -> Some (readWriteValueWrapper (username, atomPath))
                    | _ ->
                        match atomPath with
                        | AtomPath.Atom atom -> Some (unbox atom)
                        | _ -> None
            }
        | _ -> { ReadOnly = None; ReadWrite = None }

    let emptyArrayAtom = Jotai.atom<obj []> [||]

    let waitForAll<'T> (atoms: Atom<'T> []) =
        match atoms with
        | [||] -> unbox emptyArrayAtom
        | _ -> JotaiUtils.waitForAll atoms

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

        let value, setValue = Jotai.useAtom flatAtom

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
                    | Some (InputScope.ReadWrite (_, jsonDecode)), readWriteValue -> jsonDecode readWriteValue
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

    let inline useValue atom = JotaiUtils.useAtomValue atom

    let inline selectorWithProfiling (_atomKey, getFn) =
        Jotai.atom (
            getFn,
            Some
                (fun _ _ _ ->
                    failwith "@@ readonly selector"
                    ())
        )

    let useCallback (fn: GetFn -> SetFn -> 'a -> JS.Promise<'c>, deps) : ('a -> JS.Promise<'c>) =
        let fnCallback = React.useCallbackRef (fun (get, set, arg) -> fn get set arg)

        let atom =
            React.useMemo (
                (fun () ->
                    Jotai.atom (
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

        let _value, setValue = Jotai.useAtom atom

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

    let inline readWriteSet<'TValue9, 'TKey> (setFn: SetFn, username: Username, atom: Atom<'TValue9>, value: 'TValue9) =
        let atomField = getAtomField (Some (InputAtom (username, AtomPath.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some atom -> Atoms.setAtomValue setFn atom (value |> Gun.jsonEncode<'TValue9>)
        | _ -> ()

    let inline scopedSet<'TValue10, 'TKey>
        (setFn: SetFn)
        (username: Username)
        (atomScope: AtomScope)
        (atom: 'TKey -> Atom<'TValue10>, key: 'TKey, value: 'TValue10)
        =
        match atomScope with
        | AtomScope.ReadOnly -> Atoms.setAtomValue setFn (atom key) value
        | AtomScope.ReadWrite -> readWriteSet<'TValue10, 'TKey> (setFn, username, atom key, value)

    let inline readWriteReset<'TValue8, 'TKey> (setFn: SetFn) (username: Username) (atom: Atom<'TValue8>) =
        let atomField = getAtomField (Some (InputAtom (username, AtomPath.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some atom -> Atoms.setAtomValue setFn atom null
        | _ -> ()

    let inline getReadWrite<'TValue11, 'TKey> getFn (username: Username) (atom: Atom<'TValue11>) =
        let atomField = getAtomField (Some (InputAtom (username, AtomPath.Atom atom))) AtomScope.ReadWrite

        match atomField.ReadWrite with
        | Some readWriteAtom ->
            let value = Atoms.getAtomValue getFn readWriteAtom

            match value with
            | null -> Atoms.getAtomValue getFn atom
            | _ -> Gun.jsonDecode<'TValue11> value
        | _ -> Atoms.getAtomValue getFn atom

    let useState = Jotai.useAtom

    let inline useSetState atom =
        let setter = JotaiUtils.useUpdateAtom atom
        fun value -> setter (fun _ -> value)

    let useSetStatePrev = JotaiUtils.useUpdateAtom
    let inline atomWithProfiling x = atomWithProfiling x
