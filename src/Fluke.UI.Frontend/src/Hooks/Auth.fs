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
        Store.useCallbackRef
            (fun setter _ ->
                promise {
                    let! gunNamespace = setter.snapshot.getPromise Selectors.gunNamespace
                    printfn "before leave"
                    gunNamespace.``#``.leave ()
                    setter.set (Atoms.username, (fun _ -> None))
                    setter.reset Atoms.gunKeys
                })

    let usePostSignIn () =
        Store.useCallbackRef
            (fun setter username ->
                promise {
                    let! gunNamespace = setter.snapshot.getPromise Selectors.gunNamespace
                    let user = gunNamespace.``#``
                    let keys = user.__.sea

                    return
                        match keys with
                        | Some keys ->
                            setter.set (Atoms.gunKeys, (fun _ -> keys))
                            setter.set (Atoms.username, (fun _ -> Some username))
                            Ok (username, keys)
                        | None -> Error $"No keys found for user {user.is}"
                })

    let useSignIn () =
        let postSignIn = usePostSignIn ()

        Store.useCallbackRef
            (fun setter (username, password) ->
                promise {
                    let! gunNamespace = setter.snapshot.getPromise Selectors.gunNamespace
                    let! ack = Gun.authUser gunNamespace.``#`` username password

                    match ack with
                    | { err = None } -> return! postSignIn (Username username)
                    | { err = Some error } -> return Error error
                })

    let useChangePassword () =
        Store.useCallbackRef
            (fun setter (password, newPassword) ->
                promise {
                    let! username = setter.snapshot.getPromise Atoms.username

                    match username with
                    | Some (Username username) ->
                        let! gunNamespace = setter.snapshot.getPromise Selectors.gunNamespace
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
        let logout = useLogout ()

        Store.useCallbackRef
            (fun setter password ->
                promise {
                    let! username = setter.snapshot.getPromise Atoms.username

                    match username with
                    | Some (Username username) ->
                        let! gunNamespace = setter.snapshot.getPromise Selectors.gunNamespace
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
        let hydrateTemplates = Hydrate.useHydrateTemplates ()

        Store.useCallbackRef
            (fun setter (username, password) ->
                promise {
                    if username = "" || username = "" then
                        return Error "Required fields"
                    elif username = (Templates.templatesUser.Username |> Username.Value) then
                        return Error "Invalid username"
                    else
                        let! gunNamespace = setter.snapshot.getPromise Selectors.gunNamespace

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
                                    | Ok (username, keys) ->
                                        do! hydrateTemplates username

                                        //                                        gunNamespace
                                        //                                            .ref
                                        //                                            .get("fluke")
                                        //                                            .put {| username = username |}
                                        //                                        |> ignore

                                        return Ok (username, keys)
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
