namespace Fluke.UI.Frontend.Components

open Fable.React
open Fable.Core
open Feliz
open Thoth.Json
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared
open Fluke.UI.Frontend.Bindings

module GunBind =
    [<RequireQualifiedAccess>]
    type ChangeType =
        | Local
        | Remote


    [<ReactComponent>]
    let inline GunBind<'T when 'T: equality> (input: {| Atom: RecoilValue<'T, ReadWrite> |}) =
        let atomValue, setAtomValue = Recoil.useState<'T> input.Atom
        let lastAtomValue, setLastAtomValue = React.useState<'T> atomValue
        let changeType, setChangeType = React.useState<ChangeType option> None
        let gun = Recoil.useValue Recoil.Selectors.gun
        let gunNamespace = Recoil.useValue Recoil.Selectors.gunNamespace

        let rendered, setRendered = React.useState false

        let atomKey =
            $"""{nameof Fluke}/{
                                    input
                                        .Atom
                                        .key
                                        .Replace("__withFallback", "")
                                        .Replace("\"", "")
                                        .Replace("\\", "")
                                        .Replace("__", "/")
                                        .Replace(".", "/")
                                        .Replace("[", "/")
                                        .Replace("]", "/")
                                        .Replace(",", "/")
                                        .Replace("//", "/")
                                        .Trim ()
            }"""

        let atomKey =
            match atomKey with
            | String.ValidString when atomKey |> Seq.last = '/' -> atomKey |> String.take (atomKey.Length - 1)
            | _ -> atomKey

        let getNode =
            Recoil.useCallbackRef
                (fun _ ->
                    (gun.ref, atomKey.Split "/" |> Array.toList)
                    ||> List.fold (fun result -> result.get))

        React.useEffect (
            (fun () ->
                promise {
                    if not rendered then
                        let node = getNode ()

                        node.on
                            (fun (data: obj) ->
                                match data :?> string option with
                                | Some data ->
                                    printfn
                                        $"GunBind.useEffect. node.on(). DATA={JS.JSON.stringify data} atomKey={atomKey}"

                                    let newValue = Decode.Auto.fromString<'T> data

                                    match newValue with
                                    | Ok newValue ->
                                        setAtomValue newValue
                                        setChangeType (Some ChangeType.Remote)
                                        setLastAtomValue newValue
                                    | Error error -> Browser.Dom.console.error error

                                | None -> ())

                        setRendered true

                    return
                        fun () ->
                            if rendered then
                                let node = getNode ()
                                printfn $"rendering off"
                                node.off ()
                }
                |> Promise.start),
            [|
                box atomKey
                box getNode
                box setLastAtomValue
                box setAtomValue
                box setRendered
                box rendered
                box setChangeType
            |]
        )

        React.useEffect (
            (fun () ->
                promise {
                    if rendered then
                        if changeType = Some ChangeType.Remote then
                            setChangeType None
                        else if box lastAtomValue = null then
                            setLastAtomValue atomValue
                        else if atomValue <> lastAtomValue then
                            printfn
                                $"GunNode.useEffect. node.put(atomValue); setLastAtomValue (Some atomValue);
                                lastAtomValue={lastAtomValue}
                                atomValue={atomValue}
                                atomKey={atomKey}
                                "

                            setLastAtomValue atomValue

                            let node = getNode ()

                            node.put (Encode.Auto.toString (0, atomValue))
                            |> ignore
                }
                |> Promise.start),
            [|
                box atomKey
                box getNode
                box changeType
                box setChangeType
                box lastAtomValue
                box setLastAtomValue
                box atomValue
                box rendered
            |]
        )

        nothing
