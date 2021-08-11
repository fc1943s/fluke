namespace FsUi.Hooks

open Fable.Core.JsInterop
open Fable.Extras
open Fable.Core
open FsCore.Model
open FsJs
open FsStore
open FsStore.Bindings
open FsStore.Bindings.Gun.Types


module Auth =
    let inline useLogout () =
        Store.useCallbackRef
            (fun getter setter () ->
                promise {
                    printfn "before leave"
                    Store.change setter Atoms.gunTrigger ((+) 1)
                    Store.change setter Atoms.hubTrigger ((+) 1)
                    let gun = Store.value getter Selectors.Gun.gun
                    gun.user().leave ()
                    Store.set setter Atoms.username None
                    Store.set setter Atoms.gunKeys Gun.GunKeys.Default
                })

    let inline usePostSignIn () =
        Store.useCallbackRef
            (fun getter setter username ->
                promise {
                    Store.change setter Atoms.gunTrigger ((+) 1)
                    Store.change setter Atoms.hubTrigger ((+) 1)
                    let gun = Store.value getter Selectors.Gun.gun
                    let user = gun.user ()
                    let keys = user.__.sea

                    match keys with
                    | Some keys ->
                        Store.set setter Atoms.gunKeys keys
                        Store.set setter Atoms.username (Some username)
                        return Ok (username, keys)
                    | None -> return Error $"No keys found for user {user.is}"
                })

    let inline useSignIn () =
        let postSignIn = usePostSignIn ()

        Store.useCallbackRef
            (fun getter _ (username, password) ->
                promise {
                    let gun = Store.value getter Selectors.Gun.gun
                    let user = gun.user ()

                    let! ack =
                        match username, password with
                        | "", keys ->
                            printfn "keys sign in"

                            let keys =
                                try
                                    keys |> Json.decode<Gun.GunKeys>
                                with
                                | ex ->
                                    printfn $"keys decode error: {ex.Message}"
                                    Gun.GunKeys.Default

                            Gun.authKeys user keys

                        | username, password ->
                            printfn "user/pass sign in"
                            Gun.authUser user username password

                    match ack with
                    | { err = None } -> return! postSignIn (Username username)
                    | { err = Some error } -> return Error error
                })

    let inline useChangePassword () =
        Store.useCallbackRef
            (fun getter setter (password, newPassword) ->
                promise {
                    let username = Store.value getter Atoms.username
                    let gun = Store.value getter Selectors.Gun.gun
                    let user = gun.user ()

                    match username with
                    | Some (Username username) ->
                        let! ack = Gun.changeUserPassword user username password newPassword

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
                        let gun = Store.value getter Selectors.Gun.gun
                        let user = gun.user ()

                        let! ack = Gun.deleteUser user username password
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

    let inline putHashed (gun: IGunNode) data =
        promise {
            let! hash = Gun.sea.work data None None (Some {| name = Some "SHA-256" |})

            match! Gun.put (gun.get("#").get hash) data with
            | true -> return Ok hash
            | false -> return Error $"put error. data={data} hash={hash}"
        //                                    "hash#atomPath"
        //                                    "atomPath#hash"
//                                    setImmutableUsername pub username
        }

    let inline useSignUp () =
        let signIn = useSignIn ()

        Store.useCallbackRef
            (fun getter _setter (username, password) ->
                promise {
                    if username = "" || password = "" then
                        return Error "Required fields"
                    elif JSe
                             .RegExp(@"^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$")
                             .Test username
                         |> not then
                        return Error "Invalid email address"
                    else
                        let gun = Store.value getter Selectors.Gun.gun
                        let user = gun.user ()

                        printfn $"Auth.useSignUp. user.is={user.is |> JS.objectKeys}"

                        let! ack = Gun.createUser user username password

                        printfn $"Auth.useSignUp. Gun.createUser signUpAck={JS.JSON.stringify ack}"

                        return!
                            promise {
                                match ack with
                                | {
                                      err = None
                                      ok = Some 0
                                      pub = Some pub
                                  } ->

                                    let! usernameHash = putHashed user username

                                    match usernameHash with
                                    | Ok usernameHash ->
                                        match! signIn (username, password) with
                                        | Ok (username, keys) -> return Ok (username, keys)
                                        | Error error -> return Error error
                                    | Error error -> return Error error
                                | { err = Some err } -> return Error err
                                | _ -> return Error $"Invalid ack: {JS.JSON.stringify ack}"
                            }
                })
