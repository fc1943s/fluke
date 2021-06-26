namespace Fluke.UI.Frontend.Bindings

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core
open Fable.React
open System
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings


module DeepEqual =
    let fastDeepEqual<'T> (_a: 'T) (_b: 'T) : bool = importDefault "fast-deep-equal/react"

    let deepEqual<'T> (a: 'T) (b: 'T) =
        //        if unbox a <> null && a?toString <> null && jsTypeof a <> "boolean" then
//            a?toString <- emitJsExpr () "Object.prototype.toString"
//            b?toString <- emitJsExpr () "Object.prototype.toString"
        if unbox a <> null
           && unbox b <> null
           && a?CompareTo <> null then
            (a?CompareTo b) = 0
        else
            fastDeepEqual a b

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
        abstract Provider : obj -> obj

        abstract atom : 'TValue -> Atom<'TValue>

        abstract atom :
            (GetFn -> JS.Promise<'TValue>) * (GetFn -> SetFn -> 'TValue -> JS.Promise<unit>) option ->
            Atom<'TValue>

        abstract atom : (GetFn -> 'TValue) * (GetFn -> SetFn -> 'TValue -> unit) option -> Atom<'TValue>
        abstract useAtom : Atom<'TValue> -> 'TValue * ('TValue -> unit)

    let jotai : IJotai = importAll "jotai"

    type IJotaiUtils =
        abstract atomFamily : ('TKey -> Atom<'TValue>) -> ('TValue -> 'TValue -> bool) -> ('TKey -> Atom<'TValue>)
        abstract atomWithDefault : (GetFn -> 'TValue) -> Atom<'TValue>
        abstract atomWithReducer : 'TValue -> ('TValue -> 'TValue -> 'TValue) -> Atom<'TValue>
        abstract atomWithStorage : string -> 'TValue -> Atom<'TValue>
        abstract selectAtom : Atom<'TValue> * ('TValue -> 'U) -> Atom<'U>
        abstract useAtomValue : Atom<'TValue> -> 'TValue
        abstract useUpdateAtom : Atom<'TValue> -> ('TValue -> unit)
        abstract useAtomCallback : (GetFn * SetFn * 'TArg -> JS.Promise<'TValue>) -> ('TArg -> JS.Promise<'TValue>)
        abstract waitForAll : Atom<'T> [] -> Atom<'T []>

    let jotaiUtils : IJotaiUtils = importAll "jotai/utils"


    let wrapAtomPath (atomPath: string) =
        let header = $"{nameof Fluke}/"
        let header = if atomPath.StartsWith header then "" else header
        $"{header}{atomPath}"

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
        JS.log (fun () -> $"registerAtom atomPath={atomPath} keyIdentifier={keyIdentifier} atom={atom}")

        match keyIdentifier with
        | Some keyIdentifier ->
            let gunNodePath = getGunNodePath atomPath keyIdentifier
            registerAtomIdByPath atom gunNodePath |> ignore
            let atom = registerAtomPathById gunNodePath atom
            atom, Some gunNodePath
        | None -> atom, None

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

    let inline atom<'TValue> (atomPath, defaultValue: 'TValue) =
        jotai.atom (
            (fun () ->
                Profiling.addCount atomPath
                defaultValue)
                ()
        )
        |> registerAtom atomPath None
        |> fst


[<AutoOpen>]
module JotaiMagic =

    type Jotai.IJotai with
        member inline _.provider children =
            ReactBindings.React.createElement (Jotai.jotai.Provider, (), children)


[<AutoOpen>]
module JotaiUtilsMagic =
    open Jotai
    open JotaiTypes


    module Atoms =
        let rec username = atom ($"{nameof username}", (None: Username option))

        let inline getAtomValue<'TValue> (getter: GetFn) (atom: Atom<'TValue>) : 'TValue =
            (getter (unbox atom)) :?> 'TValue

        let inline setAtomValue<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue) =
            setter (atom |> box |> unbox) value

        let inline setAtomValuePrev<'TValue> (setter: SetFn) (atom: Atom<'TValue>) (value: 'TValue -> 'TValue) =
            setter (atom |> box |> unbox) value

        let atomWithStorage atomPath defaultValue (map: _ -> _) =
            let internalAtom = jotaiUtils.atomWithStorage atomPath defaultValue

            jotai.atom (
                (fun get -> getAtomValue get internalAtom),
                Some
                    (fun _get set argFn ->
                        let arg =
                            match jsTypeof argFn with
                            | "function" -> (argFn |> box |> unbox) () |> unbox
                            | _ -> argFn

                        setAtomValue set internalAtom (map arg))
            )
            |> registerAtom atomPath None
            |> fst

        let isTesting = jotai.atom JS.deviceInfo.IsTesting

        let rec gunPeers =
            atomWithStorage $"{nameof gunPeers}" ([||]: string []) (Array.filter (String.IsNullOrWhiteSpace >> not))

        let rec gunKeys = jotai.atom Gun.GunKeys.Default

        let gun =
            jotai.atom (
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
            jotaiUtils.selectAtom (
                gun,
                fun gun ->
                    let user = gun.user ()

                    printfn $"gunNamespace selector. user.is={JS.JSON.stringify user.is} keys={user.__.sea}..."

                    user
            )

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
                        (fun () ->
                            window?lastToast (fun (x: Chakra.IToastProps) -> x.description <- "Please log in again"))
                        0
                    |> ignore
                | None -> ()

                failwith
                    $"Invalid username. username={username} user.is={JS.JSON.stringify user.is} username={username} atomPath={
                                                                                                                                  atomPath
                    } "
