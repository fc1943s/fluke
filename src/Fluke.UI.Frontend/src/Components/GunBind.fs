namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings

module GunBind =
    [<RequireQualifiedAccess>]
    type ChangeType =
        | Local
        | Remote

    [<ReactComponent>]
    let GunNode<'T when 'T: equality> (input: {| Atom: RecoilValue<'T, ReadWrite> |}) =
        let atomValue, setAtomValue = Recoil.useState<'T> input.Atom
        let lastAtomValue, setLastAtomValue = React.useState<'T> atomValue
        let changeType, setChangeType = React.useState<ChangeType option> None
        let gunNamespace = Recoil.useValue Recoil.Selectors.gunNamespace

        let rendered, setRendered = React.useState false

        let path = React.useMemo (fun () -> input.Atom.key.Split "/" |> Array.toList)

        let getNode =
            Recoil.useCallbackRef
                (fun _ ->

                    (gunNamespace.ref.get (nameof Fluke), path)
                    ||> List.fold (fun result -> result.get))

        React.useEffect (
            (fun () ->
                promise {

                    if rendered then
                        if changeType = Some ChangeType.Remote then
                            setChangeType None
                        else if atomValue <> lastAtomValue then
                            printfn
                                $"GunNode.useEffect. node.put(atomValue); setLastAtomValue (Some atomValue);  lastAtomValue={
                                                                                                                                 lastAtomValue
                                } atomValue={atomValue}"

                            setLastAtomValue atomValue

                            let node = getNode ()
                            node.put atomValue |> ignore

                }
                |> Promise.start),
            [|
                box getNode
                box changeType
                box setChangeType
                box lastAtomValue
                box setLastAtomValue
                box atomValue
                box rendered
                box gunNamespace.ref
            |]
        )

        React.useEffect (
            (fun () ->
                promise {
                    let guid = System.Guid.NewGuid ()

                    if not rendered then
                        let node = getNode ()

                        printfn $"Settings.useEffect. if not rendered then. guid={guid}"

                        node.on
                            (fun (data: 'T option) ->
                                match data with
                                | Some data ->
                                    printfn $"Settings.useEffect. node.on(). DATA: {data} path={path}"
                                    setAtomValue data
                                    setChangeType (Some ChangeType.Remote)
                                    setLastAtomValue data
                                | None -> ())

                        setRendered true

                    return
                        fun () ->
                            if rendered then
                                let node = getNode ()
                                printfn $"rendering off {guid}"
                                node.off ()
                }
                |> Promise.start),
            [|
                box path
                box getNode
                box setLastAtomValue
                box setAtomValue
                box setRendered
                box rendered
                box gunNamespace.ref
                box setChangeType
            |]
        )

        nothing

    let GunBind () =
        React.fragment [
            GunNode {| Atom = Recoil.Atoms.daysBefore |}
            GunNode {| Atom = Recoil.Atoms.daysAfter |}
        ]
