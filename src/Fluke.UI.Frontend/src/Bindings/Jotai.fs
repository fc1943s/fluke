namespace Fluke.UI.Frontend.Bindings

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core
open Fable.React
open System
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Microsoft.FSharp.Core.Operators


module DeepEqual =
    let deepEqual<'T> (_a: 'T) (_b: 'T) : bool = importDefault "fast-deep-equal"


module JotaiTypes =
    type Atom<'TValue> =
        abstract member toString : unit -> string
        abstract member onMount : ((int -> int) -> unit) -> unit -> unit

    //    type InputAtom<'TValue1> = {
//        Username: Username
//        Atom: Atom<'TValue1>
//    }
//
//    type InputAtomFamily<'TKey,'TValue1> = {
//                       Username    : Username
//                       Atom: 'TKey -> Atom<'TValue1>
//                       Key: 'TKey
//    }


    [<RequireQualifiedAccess>]
    type InputScope<'TValue> =
        | ReadOnly
        | ReadWrite of Gun.Serializer<'TValue>

    [<RequireQualifiedAccess>]
    type AtomScope =
        | ReadOnly
        | ReadWrite

    //    type InputAtom = InputAtom of username: Username * atomPath: AtomPath
//
//    and AtomPath = AtomPath of string
    type InputAtom<'T> = InputAtom of username: Username * atomPath: AtomPath<'T>

    and [<RequireQualifiedAccess>] AtomPath<'T> =
        | Atom of Atom<'T>
        | Path of string

    and InputScope<'TValue> with
        static member inline AtomScope<'TValue> (inputScope: InputScope<'TValue> option) =
            match inputScope with
            | Some (InputScope.ReadWrite _) -> AtomScope.ReadWrite
            | _ -> AtomScope.ReadOnly



module Jotai =
    open JotaiTypes

    type PrimitiveAtom<'TValue>  =
        class
        end



    type GetFn = Atom<obj> -> obj
    type SetFn = Atom<obj> -> obj -> unit


    type IJotai =
        //        abstract atom : (unit -> unit) * JS.Promise<'Value> -> Atom<'TValue>

        abstract Provider : obj -> obj

        abstract atom : 'TValue -> Atom<'TValue>

        abstract atom :
            (GetFn -> JS.Promise<'TValue>) * (GetFn -> SetFn -> 'TValue -> JS.Promise<unit>) option ->
            Atom<'TValue>

        abstract atom : (GetFn -> 'TValue) * (GetFn -> SetFn -> 'TValue -> unit) option -> Atom<'TValue>

        abstract useAtom : Atom<'TValue> -> 'TValue * ('TValue -> unit)

    let Jotai : IJotai = importAll "jotai"

    type IJotaiUtils =
        abstract atomWithReducer : 'TValue -> ('TValue -> 'TValue -> 'TValue) -> Atom<'TValue>
        abstract atomWithStorage : string -> 'TValue -> Atom<'TValue>

        abstract atomFamily : ('TKey -> Atom<'TValue>) -> ('TValue -> 'TValue -> bool) -> ('TKey -> Atom<'TValue>)
        abstract selectAtom : Atom<'TValue> * ('TValue -> 'U) -> Atom<'U>

        abstract waitForAll : Atom<'T> [] -> Atom<'T []>

        abstract useAtomValue : Atom<'TValue> -> 'TValue

        abstract useUpdateAtom : Atom<'TValue> -> ('TValue -> unit)

        abstract useAtomCallback : (GetFn * SetFn * 'TArg -> JS.Promise<'TValue>) -> ('TArg -> JS.Promise<'TValue>)

    let JotaiUtils : IJotaiUtils = importAll "jotai/utils"


    let wrapAtomPath (atomPath: string) =
        let header = $"{nameof Fluke}/"
        let header = if atomPath.StartsWith header then "" else header
        let result = $"{header}{atomPath}"

        //        JS.log (fun () -> $"wrapAtomPath. result={result} atomPath={atomPath}")
//
        result

    let getGunNodePath (atomPath: string) (keyIdentifier: string list) =
        let newAtomPath =
            match keyIdentifier with
            | [] -> atomPath
            | keyIdentifier when keyIdentifier |> List.head |> Guid.TryParse |> fst ->
                let nodes = atomPath |> String.split "/"

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


    let private atomPathMap = Dictionary<string, string> ()
    let private atomIdMap = Dictionary<string, string> ()

    let registerAtomPathById atomPath (atom: Atom<_>) =
        atomIdMap.[atom.toString ()] <- atomPath
        atom

    let registerAtomIdByPath (atom: Atom<_>) atomPath =
        atomPathMap.[atomPath] <- atom.toString ()
        atomPath

    let registerAtom atomPath keyIdentifier atom =
        match keyIdentifier with
        | Some keyIdentifier ->
            let gunNodePath = getGunNodePath atomPath keyIdentifier
            //            printfn $"registerAtom atomPath={atomPath} gunNodePath={gunNodePath}"
            registerAtomIdByPath atom gunNodePath |> ignore
            registerAtomPathById gunNodePath atom
        | None ->
            //            printfn $"registerAtom atomPath={atomPath}. skipping registration."
            atom

    let queryAtomPath atomPath =
        match atomPath with
        | AtomPath.Atom atom ->
            match atomIdMap.TryGetValue (atom.toString ()) with
            | true, value -> Some value
            | _ -> None
        | AtomPath.Path path ->
            match atomPathMap.TryGetValue path with
            | true, value -> Some value
            | _ -> None

    let inline atomWithProfiling<'TValue> (atomPath, defaultValue: 'TValue) =
        Jotai.atom (
            (fun () ->
                Profiling.addCount atomPath
                defaultValue)
                ()
        )
        |> registerAtom atomPath None


[<AutoOpen>]
module JotaiMagic =

    type Jotai.IJotai with
        member inline _.provider children =
            ReactBindings.React.createElement (Jotai.Jotai.Provider, (), children)


    let Jotai = Jotai.Jotai



[<AutoOpen>]
module JotaiUtilsMagic =
    open Jotai
    open JotaiTypes

    let JotaiUtils = JotaiUtils

    module Atoms =
        let rec username = atomWithProfiling ($"{nameof username}", (None: Username option))

        let inline getAtomValue<'TValue> (getter: GetFn) (atom: Atom<'TValue>) : 'TValue =
            (getter (unbox atom)) :?> 'TValue

        let inline setAtomValue<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue) =
            setter (atom |> box |> unbox) value

        let inline setAtomValuePrev<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue -> 'TValue) =
            setter (atom |> box |> unbox) value

        let atomWithStorage atomPath defaultValue (map: _ -> _) =
            let internalAtom = JotaiUtils.atomWithStorage atomPath defaultValue

            Jotai.atom (
                (fun get -> getAtomValue get internalAtom),
                Some
                    (fun _get set argFn ->
                        let arg =
                            match jsTypeof argFn with
                            | "function" -> (argFn |> box |> unbox) () |> unbox
                            | _ -> argFn

                        printfn $"atomWithStorage arg={arg} map={map} ma={map arg}"
                        setAtomValue set internalAtom (map arg))
            )
            |> registerAtom atomPath None

        let isTesting = Jotai.atom JS.deviceInfo.IsTesting

        let rec gunPeers =
            atomWithStorage $"{nameof gunPeers}" ([||]: string []) (Array.filter (String.IsNullOrWhiteSpace >> not))

        let rec gunKeys = Jotai.atom Gun.GunKeys.Default

        let gun =
            Jotai.atom (
                (fun get ->
                    let isTesting = getAtomValue get isTesting
                    let gunPeers = getAtomValue get gunPeers

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

                    gun),
                None
            )

        let rec gunNamespace =
            JotaiUtils.selectAtom (
                gun,
                fun gun ->
                    //                    let username = getter.get Atoms.username
//                    let gunKeys = getter.get Atoms.gunKeys
                    let user = gun.user ()

                    match JS.window id with
                    | Some window -> window?gunNamespace <- gun
                    | None -> ()

                    printfn $"gunNamespace selector. user.is={JS.JSON.stringify user.is} keys={user.__.sea}..."

                    user
            )
