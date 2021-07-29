namespace FsUi.Hooks

open Fable.Core.JsInterop
open Fable.Extras
open Fable.Core
open FsCore.Model
open FsJs
open FsStore
open FsStore.Bindings


module Auth =
    let inline useLogout () =
        Store.useCallbackRef
            (fun getter setter () ->
                promise {
                    printfn "before leave"
                    Store.change setter Atoms.gunTrigger ((+) 1)
                    Store.change setter Atoms.hubTrigger ((+) 1)
                    let gunNamespace = Store.value getter Selectors.gunNamespace
                    gunNamespace.leave ()
                    Store.set setter Atoms.username None
                    Store.set setter Atoms.gunKeys Gun.GunKeys.Default
                })

    let inline usePostSignIn () =
        Store.useCallbackRef
            (fun getter setter username ->
                promise {
                    Store.change setter Atoms.gunTrigger ((+) 1)
                    Store.change setter Atoms.hubTrigger ((+) 1)
                    let gunNamespace = Store.value getter Selectors.gunNamespace
                    let keys = gunNamespace.__.sea

                    match keys with
                    | Some keys ->
                        Store.set setter Atoms.gunKeys keys
                        Store.set setter Atoms.username (Some username)
                        return Ok (username, keys)
                    | None -> return Error $"No keys found for user {gunNamespace.is}"
                })

    let inline useSignIn () =
        let postSignIn = usePostSignIn ()

        Store.useCallbackRef
            (fun getter _ (username, password) ->
                promise {
                    let gunNamespace = Store.value getter Selectors.gunNamespace

                    let! ack = Gun.authUser gunNamespace username password

                    match ack with
                    | { err = None } -> return! postSignIn (Username username)
                    | { err = Some error } -> return Error error
                })

    let inline useChangePassword () =
        Store.useCallbackRef
            (fun getter setter (password, newPassword) ->
                promise {
                    let username = Store.value getter Atoms.username
                    let gunNamespace = Store.value getter Selectors.gunNamespace

                    match username with
                    | Some (Username username) ->
                        let! ack = Gun.changeUserPassword gunNamespace username password newPassword

                        return!
                            promise {
                                match ack with
                                | { ok = Some 1; err = None } ->
                                    Store.change setter Atoms.gunTrigger ((+) 1)
                                    Store.change setter Atoms.hubTrigger ((+) 1)
                                    return Ok ()
                                | { err = Some error } -> return Error error
                                | _ -> return Error $"invalid ack {JS.JSON.stringify ack}"
                            }
                    | _ -> return Error "Invalid username"
                })

    let inline useDeleteUser () =
        let logout = useLogout ()

        Store.useCallbackRef
            (fun getter _ password ->
                promise {
                    let username = Store.value getter Atoms.username

                    match username with
                    | Some (Username username) ->
                        let gunNamespace = Store.value getter Selectors.gunNamespace

                        let! ack = Gun.deleteUser gunNamespace username password
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

    let inline useSignUp () =
        let signIn = useSignIn ()

        Store.useCallbackRef
            (fun getter _setter (username, password) ->
                promise {
                    if username = "" || password = "" then
                        return Error "Required fields"
                    elif JSe.RegExp(@"^[^0-9][a-zA-Z0-9]+$").Test username
                         |> not then
                        return Error "Invalid username"
                    else
                        let gunNamespace = Store.value getter Selectors.gunNamespace

                        printfn $"Auth.useSignUp. gunNamespace={JS.JSON.stringify gunNamespace}"

                        let! ack = Gun.createUser gunNamespace username password

                        printfn $"Auth.useSignUp. Gun.createUser signUpAck={JS.JSON.stringify ack}"
                        Dom.consoleLog ("ack", ack)

                        match Dom.window () with
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
                                    | Ok (username, keys) -> return Ok (username, keys)
                                    | Error error -> return Error error
                                | { err = Some err } -> return Error err
                                | _ -> return Error $"Invalid ack: {JS.JSON.stringify ack}"
                            }
                })
