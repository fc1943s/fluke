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

        let gunAtomKey = useGunAtomKey input.Atom

        let getGunAtomNode = Recoil.useCallbackRef (fun _ -> Gun.getGunAtomNode gun.ref gunAtomKey)

        //        printfn
//            $"GunBind()
//        gunAtomKey={gunAtomKey}
//        lastAtomValue={lastAtomValue}
//        atomValue={atomValue}
//        changeType={changeType}
//        "

        React.useEffect (
            (fun () ->
                promise {
                    let gunAtomNode = getGunAtomNode ()

                    printfn $"GunBind.useEffect. gunAtomKey={gunAtomKey} lastAtomValue={lastAtomValue}"

                    gunAtomNode.on
                        (fun data ->
                            match Gun.deserializeGunAtomNode data with
                            | Some gunAtomNodeValue ->

                                printfn
                                    $"GunBind.useEffect. node.on().
                                        lastAtomValue={lastAtomValue}
                                        data={JS.JSON.stringify data}
                                        gunAtomNodeValue={gunAtomNodeValue}
                                        "

                                setChangeType (Some ChangeType.Remote)

                                if lastAtomValue <> gunAtomNodeValue then
                                    printfn "different values, setting"
                                    setAtomValue gunAtomNodeValue
                                    setLastAtomValue gunAtomNodeValue
                                    gunAtomNode.off () |> ignore
                            | None -> ())

                    return
                        fun () ->
                            printfn "@@@@@@ GunBind.useEffect. rendering off "

                            let gunAtomNode = getGunAtomNode ()
                            gunAtomNode.off () |> ignore
                }
                |> Promise.start),
            [|
                box gunAtomKey
                box lastAtomValue
                box getGunAtomNode
                box setLastAtomValue
                box setAtomValue
                box setChangeType
            |]
        )

        React.useEffect (
            (fun () ->
                promise {
                    if changeType = Some ChangeType.Remote then
                        setChangeType None
                    else if box lastAtomValue = null then
                        setLastAtomValue atomValue
                    else if atomValue <> lastAtomValue then
                        printfn
                            $"GunNode.useEffect. node.put(atomValue); setLastAtomValue (Some atomValue);
                                lastAtomValue={lastAtomValue}
                                atomValue={atomValue}
                                "

                        let gunAtomNode = getGunAtomNode ()
                        gunAtomNode.off () |> ignore
                        Gun.putGunAtomNode gunAtomNode atomValue

                        setLastAtomValue atomValue
                        setChangeType (Some ChangeType.Local)
                }
                |> Promise.start),
            [|
                box getGunAtomNode
                box changeType
                box setChangeType
                box lastAtomValue
                box setLastAtomValue
                box atomValue
            |]
        )

        nothing
