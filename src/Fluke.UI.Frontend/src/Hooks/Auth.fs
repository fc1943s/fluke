namespace Fluke.UI.Frontend.Hooks

open Feliz
open Fable.Core
open Feliz.Recoil
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain
open Fable.Core.JsInterop


module Auth =
    let useLogout () =
        let gunNamespace = Recoil.useValue Selectors.gunNamespace
        let setUsername = Recoil.useSetState Atoms.username
        let resetGunKeys = Recoil.useResetState Atoms.gunKeys

        Recoil.useCallbackRef
            (fun _setter _ ->
                promise {
                    printfn "before leave"
                    gunNamespace.ref.leave ()
                    setUsername None
                    resetGunKeys ()
                })

    let usePostSignIn () =
        let gunNamespace = Recoil.useValue Selectors.gunNamespace
        let setUsername = Recoil.useSetState Atoms.username
        let setGunKeys = Recoil.useSetState Atoms.gunKeys

        Recoil.useCallbackRef
            (fun _setter username ->
                promise {
                    setUsername (Some username)
                    setGunKeys gunNamespace.ref.``_``.sea
                })

    let useSignIn () =
        let gunNamespace = Recoil.useValue Selectors.gunNamespace
        let postSignIn = usePostSignIn ()

        Recoil.useCallbackRef
            (fun _setter username password ->
                promise {
                    let! ack = Gun.authUser gunNamespace.ref username password

                    return!
                        promise {
                            match ack.err with
                            | None ->
                                do! postSignIn (UserInteraction.Username username)
                                return Ok ()
                            | Some error -> return Error error
                        }
                })

    let useSignUp () =
        let signIn = useSignIn ()
        let gunNamespace = Recoil.useValue Selectors.gunNamespace

        Recoil.useCallbackRef
            (fun _ username password ->
                promise {
                    if username = "" || username = "" then
                        return Error "Required fields"
                    elif username = (TempData.testUser.Username |> Username.Value) then
                        return Error "Invalid username"
                    else
                        printfn $"Auth.useSignUp. gun.user() result: {JS.JSON.stringify gunNamespace.ref}"

                        let! ack = Gun.createUser gunNamespace.ref username password
                        printfn "Auth.useSignUp. Gun.createUser signUpAck:"
                        Browser.Dom.console.log ack
                        Browser.Dom.window?signUpAck <- ack

                        return!
                            promise {
                                match ack.err with
                                | None ->
                                    match! signIn username password with
                                    | Ok () ->


                                        //                                        gunNamespace
//                                            .ref
//                                            .get("fluke")
//                                            .put {| username = username |}
//                                        |> ignore

                                        return Ok ()
                                    | Error error -> return Error error
                                //                                    do! postSignIn (UserInteraction.Username username)

                                //                                    gunNamespace
//                                        .get("fluke")
//                                        .put {| username = username |}
//                                    |> ignore
                                //                                let usernamePut =
//                                    gunNamespace
//                                        .ref
//                                        .get("fluke")
//                                        .put {| username = username |}
//
//                                printfn $"sign up username put = {JS.JSON.stringify usernamePut}"

                                | Some error -> return Error error
                            }
                })
