namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
open Fable.Core
open FsCore.Model
open Feliz
open FsJs
open FsStore
open FsStore.Bindings
open FsUi.Bindings
open FsUi.Hooks
open Fluke.UI.Frontend.State
open Fable.React


module GunObserver =

    [<ReactComponent>]
    let GunObserver () =
        let gun = Store.useValue Selectors.Gun.gun
        //        let appKeys = Gun.gunHooks.useGunKeys Browser.Dom.window?SEA (fun () -> null) false
        let setUsername = Store.useSetState Atoms.username
        let setGunKeys = Store.useSetState Atoms.gunKeys

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

        //        let setSessionRestored = Store.useSetState Atoms.Session.sessionRestored

        printfn "GunObserver.render: Constructor"


        //        React.useEffect (
//            (fun () ->
//                //                let recall = Browser.Dom.window.sessionStorage.getItem "recall"
////                printfn $"recall {recall}"
////
////                match recall with
////                | null
////                | "" -> setSessionRestored true
////                | _ -> ()
////
////                printfn "before recall"
////
////                try
////                    if true then
////                        gunNamespace.ref.recall (
////                            {| sessionStorage = true |},
////                            (fun ack ->
////                                match ack.put with
////                                | Some put -> setUsername (Some (UserInteraction.Username put.alias))
////                                | None -> printfn "Empty ack"
////
////                                setKeys (Some ack.sea)
////
////                                setSessionRestored true
////
////                                printfn "ACK:"
////                                Browser.Dom.console.log ack
////                                Browser.Dom.window?recallAck <- ack
////                                Dom.set "ack" ack)
////                        )
////                with ex -> printfn $"ERROR: {ex}"
//
//                //                printfn "after recall"
//
//                printfn "before newRecall"
//
//                printfn $"gunKeys={gunKeys |> Some |> JS.objectKeys}"
//                setSessionRestored true
//
//                printfn "after newRecall"),
//            [|
//                box gunNamespace
//                box  setSessionRestored
//                box gunKeys
//            |]
//        )

        React.useDisposableEffect (
            (fun disposed ->
                gun.on (
                    Gun.GunEvent "auth",
                    (fun () ->
                        if not disposed then
                            let user = gun.user ()

                            match user.is with
                            | Some {
                                       alias = Some (Gun.GunUserAlias.Alias (Gun.Alias username))
                                   } ->
                                printfn $"GunObserver.render: .on(auth) effect. setUsername. username={username}"

                                let keys = user.__.sea

                                match keys with
                                | Some keys ->
                                    setUsername (Some (Username username))
                                    setGunKeys keys
                                | None -> failwith $"GunObserver.render: No keys found for user {username}"

                            //                                gunState.put ({| username = username |} |> toPlainJsObj)
                            //                                |> Promise.start
                            //                                setUsername (Some (UserInteraction.Username username))
                            | Some {
                                       alias = Some (Gun.GunUserAlias.GunKeys { pub = Some pub })
                                   } ->
                                match Dom.window () with
                                | Some window -> window?gun <- gun
                                | None -> ()

                                gun
                                    .get(Gun.GunNodeSlice $"#{nameof Gun.data}")
                                    .get(Gun.RadQuery {| ``.`` = {| ``*`` = pub |} |})
                                    .map()
                                    .once (fun encryptedUsername k ->
                                        printfn $"@@@@@@@@@ encryptedUsername={encryptedUsername} k={k} pub={pub}"

                                        match encryptedUsername with
                                        | Gun.GunValue.NodeReference gunNodeSlice ->
                                            gun
                                                .user(pub)
                                                .get(Gun.GunNodeSlice (nameof Gun.data))
                                                .get(gunNodeSlice)
                                                .once (fun a b ->
                                                    printfn $"gun once! a={a} b={b}"
                                                    ())
                                        | _ -> ())

                                let username = pub
                                printfn $"GunObserver.render: fetched username: {username} (still a key)"
                            | _ ->
                                match Dom.window () with
                                | Some window -> window?gun <- gun
                                | None -> ()

                                // @@@@ getImmutableUsername pub

                                printfn
                                    $"GunObserver.render: Auth occurred without username: {user.is |> JS.objectKeys}"
                        else
                            printfn $"GunObserver.render: already disposed gun={gun}")
                )),
            [|
                box gun
            |]
        )

        nothing
