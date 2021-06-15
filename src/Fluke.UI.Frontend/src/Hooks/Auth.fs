namespace Fluke.UI.Frontend.Hooks

open Fable.Core.JsInterop
open Fable.Core
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks


module Auth =

    let useLogout () =
        let gunNamespace = Store.useValue Selectors.gunNamespace
        let setUsername = Store.useSetState Atoms.username
        let resetGunKeys = Store.useResetState Atoms.gunKeys

        Store.useCallbackRef
            (fun _setter _ ->
                promise {
                    printfn "before leave"
                    gunNamespace.``#``.leave ()
                    setUsername None
                    resetGunKeys ()
                })

    let usePostSignIn () =
        let gunNamespace = Store.useValue Selectors.gunNamespace
        let setUsername = Store.useSetState Atoms.username
        let setGunKeys = Store.useSetState Atoms.gunKeys

        Store.useCallbackRef
            (fun _setter username ->
                promise {
                    setUsername (Some username)

                    let user = gunNamespace.``#``
                    let keys = user.__.sea

                    match keys with
                    | Some keys -> setGunKeys keys
                    | None -> failwith $"No keys found for user {user.is}"
                })

    let useSignIn () =
        let gunNamespace = Store.useValue Selectors.gunNamespace
        let postSignIn = usePostSignIn ()

        Store.useCallbackRef
            (fun _setter (username, password) ->
                promise {
                    let! ack = Gun.authUser gunNamespace.``#`` username password

                    return!
                        promise {
                            match ack with
                            | { err = None } ->
                                do! postSignIn (Username username)
                                return Ok ()
                            | { err = Some error } -> return Error error
                        }
                })

    let useChangePassword () =
        let username = Store.useValue Atoms.username
        let gunNamespace = Store.useValue Selectors.gunNamespace

        Store.useCallbackRef
            (fun _setter (password, newPassword) ->
                promise {
                    match username with
                    | Some (Username username) ->
                        let! ack = Gun.changeUserPassword gunNamespace.``#`` username password newPassword

                        return!
                            promise {
                                match ack with
                                | { ok = Some 1; err = None } -> return Ok ()
                                | { err = Some error } -> return Error error
                                | _ -> return Error $"invalid ack {JS.JSON.stringify ack}"
                            }
                    | _ -> return Error "Invalid username"
                })

    let useDeleteUser () =
        let username = Store.useValue Atoms.username
        let gunNamespace = Store.useValue Selectors.gunNamespace
        let logout = useLogout ()

        Store.useCallbackRef
            (fun _setter password ->
                promise {
                    match username with
                    | Some (Username username) ->
                        let! ack = Gun.deleteUser gunNamespace.``#`` username password
                        printfn $"ack={JS.JSON.stringify ack}"

                        return!
                            promise {
                                match ack with
                                | { ok = Some 0; err = None } ->
                                    do! logout ()
                                    return Ok ()
                                | { err = Some error } -> return Error error
                                | _ -> return Error $"invalid ack {JS.JSON.stringify ack}"
                            }
                    | _ -> return Error "Invalid username"
                })

    let useSignUp () =
        let signIn = useSignIn ()
        let gunNamespace = Store.useValue Selectors.gunNamespace
        let hydrateTemplates = Hydrate.useHydrateTemplates ()

        Store.useCallbackRef
            (fun _ (username, password) ->
                promise {
                    if username = "" || username = "" then
                        return Error "Required fields"
                    elif username = (Templates.templatesUser.Username |> Username.Value) then
                        return Error "Invalid username"
                    else
                        printfn $"Auth.useSignUp. gunNamespace={JS.JSON.stringify gunNamespace}"

                        let! ack = Gun.createUser gunNamespace.``#`` username password
                        printfn $"Auth.useSignUp. Gun.createUser signUpAck={JS.JSON.stringify ack}"
                        JS.consoleLog ("ack", ack)

                        match JS.window id with
                        | Some window -> window?signUpAck <- ack
                        | None -> ()

                        return!
                            promise {
                                match ack with
                                | {
                                      err = None
                                      ok = Some 0
                                      pub = Some _
                                  } ->
                                    match! signIn (username, password) with
                                    | Ok () ->
                                        do! hydrateTemplates (Username username)

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

                                | { err = Some err } -> return Error err
                                | _ -> return Error $"Invalid ack: {JS.JSON.stringify ack}"
                            }
                })
