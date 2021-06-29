namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fable.React
open Fable.Core


module GunObserver =

    [<ReactComponent>]
    let GunObserver () =
        let gun = Store.useValue Store.Selectors.gun
        let gunNamespace = Store.useValue Store.Selectors.gunNamespace
        //        let appKeys = Gun.gunHooks.useGunKeys Browser.Dom.window?SEA (fun () -> null) false
        let gunKeys, setGunKeys = Store.useState Store.Atoms.gunKeys

        //        let gunState =
//            Gun.gunHooks.useGunState
//                (gunNamespace.ref.get ("fluke"))
//                {|
//                    appKeys = gunKeys
//                    sea = Browser.Dom.window?SEA
//                |}
//
//        printfn $"GunObserver. gunState={JS.JSON.stringify gunState}"
//        printfn "GunObserver. setted dom.gunState"
//        Browser.Dom.window?gunState <- gunState

        //        const [appKeys, setAppKeys] = useGunKeys(
//          sea,
//          () =>
//            JSON.parse(localStorage.getItem('existingKeysInLocalStorage')) || null,
//        );
//        const [user, isLoggedIn] = useGunKeyAuth(gun, appKeys);

        //        let setUsername = Recoil.useSetState Atoms.username

        let setSessionRestored = Store.useSetState Atoms.sessionRestored

        printfn "GunObserver.render: Constructor"


        React.useEffect (
            (fun () ->
                //                let recall = Browser.Dom.window.sessionStorage.getItem "recall"
//                printfn $"recall {recall}"
//
//                match recall with
//                | null
//                | "" -> setSessionRestored true
//                | _ -> ()
//
//                printfn "before recall"
//
//                try
//                    if true then
//                        gunNamespace.ref.recall (
//                            {| sessionStorage = true |},
//                            (fun ack ->
//                                match ack.put with
//                                | Some put -> setUsername (Some (UserInteraction.Username put.alias))
//                                | None -> printfn "Empty ack"
//
//                                setKeys (Some ack.sea)
//
//                                setSessionRestored true
//
//                                printfn "ACK:"
//                                Browser.Dom.console.log ack
//                                Browser.Dom.window?recallAck <- ack
//                                Dom.set "ack" ack)
//                        )
//                with ex -> printfn $"ERROR: {ex}"

                //                printfn "after recall"

                printfn "before newRecall"

                printfn $"gunKeys={JS.JSON.stringify gunKeys}"
                setSessionRestored true

                printfn "after newRecall"),
            [|
                box gunNamespace
                box gunKeys
            |]
        )

        React.useDisposableEffect (
            (fun disposed ->
                gun.on (
                    "auth",
                    (fun () ->
                        if not disposed then
                            match gunNamespace.is with
                            | Some { alias = Some username } ->
                                printfn $"GunObserver.render: .on(auth) effect. setUsername. username={username}"

                                let keys = gunNamespace.__.sea

                                match keys with
                                | Some keys -> setGunKeys keys
                                | None -> failwith $"No keys found for user {gunNamespace.is}"

                            //                                gunState.put ({| username = username |} |> toPlainJsObj)
//                                |> Promise.start
                            //                                setUsername (Some (UserInteraction.Username username))
                            | _ -> printfn $"GunObserver.render: Auth occurred without username: {gunNamespace.is}"
                        else
                            printfn $"GunObserver.render: already disposed gunNamespace={gunNamespace}")
                )),
            [|
            //                box gun
//                box gunNamespace
            |]
        )

        nothing
