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
        Store.useCallback (
            (fun get set () ->
                promise {
                    let gunNamespace = Atoms.getAtomValue get Atoms.gunNamespace
                    printfn "before leave"
                    gunNamespace.leave ()
                    Atoms.setAtomValue set Atoms.username (fun _ -> None)
                    Atoms.setAtomValue set Atoms.gunKeys (fun _ -> Gun.GunKeys.Default)
                }),
            [||]
        )

    let usePostSignIn () =
        Store.useCallback (
            (fun get set username ->
                promise {
                    let gunNamespace = Atoms.getAtomValue get Atoms.gunNamespace
                    let keys = gunNamespace.__.sea

                    match keys with
                    | Some keys ->
                        Atoms.setAtomValue set Atoms.gunKeys (fun _ -> keys)
                        Atoms.setAtomValue set Atoms.username (fun _ -> Some username)
                        return Ok (username, keys)
                    | None -> return Error $"No keys found for user {gunNamespace.is}"
                }),
            [||]
        )

    let useSignIn () =
        let postSignIn = usePostSignIn ()

        Store.useCallback (
            (fun get _set (username, password) ->
                promise {
                    let gunNamespace = Atoms.getAtomValue get Atoms.gunNamespace
                    let! ack = Gun.authUser gunNamespace username password

                    match ack with
                    | { err = None } -> return! postSignIn (Username username)
                    | { err = Some error } -> return Error error
                }),
            [|
                box postSignIn
            |]
        )

    let useChangePassword () =
        Store.useCallback (
            (fun get _set (password, newPassword) ->
                promise {
                    let username = Atoms.getAtomValue get Atoms.username

                    match username with
                    | Some (Username username) ->
                        let gunNamespace = Atoms.getAtomValue get Atoms.gunNamespace
                        let! ack = Gun.changeUserPassword gunNamespace username password newPassword

                        return!
                            promise {
                                match ack with
                                | { ok = Some 1; err = None } -> return Ok ()
                                | { err = Some error } -> return Error error
                                | _ -> return Error $"invalid ack {JS.JSON.stringify ack}"
                            }
                    | _ -> return Error "Invalid username"
                }),
            [||]
        )

    let useDeleteUser () =
        let logout = useLogout ()

        Store.useCallback (
            (fun get _set password ->
                promise {
                    let username = Atoms.getAtomValue get Atoms.username

                    match username with
                    | Some (Username username) ->
                        let gunNamespace = Atoms.getAtomValue get Atoms.gunNamespace
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
                }),
            [|
                box logout
            |]
        )

    let useSignUp () =
        let signIn = useSignIn ()
        let hydrateTemplates = Hydrate.useHydrateTemplates ()

        Store.useCallback (
            (fun get _set (username, password) ->
                promise {
                    if username = "" || username = "" then
                        return Error "Required fields"
                    elif username = (Templates.templatesUser.Username |> Username.Value) then
                        return Error "Invalid username"
                    else
                        let gunNamespace = Atoms.getAtomValue get Atoms.gunNamespace

                        printfn $"Auth.useSignUp. gunNamespace={JS.JSON.stringify gunNamespace}"

                        let! ack = Gun.createUser gunNamespace username password

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
                }),
            [|
                box hydrateTemplates
                box signIn
            |]
        )
