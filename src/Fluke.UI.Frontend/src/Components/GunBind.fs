namespace Fluke.UI.Frontend.Components

open Fable.React
open Fable.Core
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

    let useGunAtomKey (atom: RecoilValue<'T, ReadWrite>) =
        let username = Recoil.useValue Recoil.Atoms.username

        React.useMemo (
            (fun () -> Recoil.getGunAtomKey username atom),
            [|
                box atom
                box username
            |]
        )

    [<ReactComponent>]
    let inline GunBind<'T when 'T: equality> (input: {| Atom: RecoilValue<'T, ReadWrite> |}) =
        let atomValue, setAtomValue = Recoil.useState<'T> input.Atom
        let lastAtomValue, setLastAtomValue = React.useState<'T> atomValue
        let changeType, setChangeType = React.useState<ChangeType option> None
        let gun = Recoil.useValue Recoil.Selectors.gun
        let gunNamespace = Recoil.useValue Recoil.Selectors.gunNamespace

        let rendered, setRendered = React.useState false

        let gunAtomKey = useGunAtomKey input.Atom

        let getGunAtomNode = Recoil.useCallbackRef (fun _ -> Gun.getGunAtomNode gun.ref gunAtomKey)


        React.useEffect (
            (fun () ->
                promise {
                    if not rendered then
                        let gunAtomNode = getGunAtomNode ()

                        gunAtomNode.on
                            (fun data ->
                                match Gun.deserializeGunAtomNode data with
                                | Some gunAtomNodeValue ->

                                    printfn
                                        $"GunBind.useEffect. node.on().
                                        data={JS.JSON.stringify data} gunAtomKey={gunAtomKey}
                                        gunAtomNodeValue={gunAtomNodeValue}"

                                    setAtomValue gunAtomNodeValue
                                    setChangeType (Some ChangeType.Remote)
                                    setLastAtomValue gunAtomNodeValue
                                | None -> ())

                        setRendered true

                    return
                        fun () ->
                            if rendered then
                                let node = getGunAtomNode ()
                                printfn "rendering off"
                                node.off ()
                }
                |> Promise.start),
            [|
                box gunAtomKey
                box getGunAtomNode
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
                                gunAtomKey={gunAtomKey}
                                "

                            setLastAtomValue atomValue

                            let node = getGunAtomNode ()
                            Gun.putGunAtomNode node atomValue
                }
                |> Promise.start),
            [|
                box gunAtomKey
                box getGunAtomNode
                box changeType
                box setChangeType
                box lastAtomValue
                box setLastAtomValue
                box atomValue
                box rendered
            |]
        )

        nothing
