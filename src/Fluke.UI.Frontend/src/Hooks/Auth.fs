namespace Fluke.UI.Frontend.Hooks

open Feliz
open Fable.Core
open Feliz.UseListener
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.Shared.Domain

module Auth =
    let useLogout () =
        let gun = Recoil.useValue Recoil.Selectors.gun
        let setUsername = Recoil.useSetState Recoil.Atoms.username

        React.useCallback (
            (fun () ->
                let user = gun.root.user ()
                printfn "before leave"
                user.leave ()
                setUsername None),
            [|
                box gun
            |]
        )

    let useSignIn () =
        let gun = Recoil.useValue Recoil.Selectors.gun
        let setUsername = Recoil.useSetState Recoil.Atoms.username

        React.useCallback (
            (fun username password ->
                promise {
                    let user = gun.root.user ()
                    let! ack = Gun.authUser user username password

                    return
                        match ack.err with
                        | None ->
                            setUsername (Some (UserInteraction.Username username))
                            Ok ()
                        | Some error -> Error error
                }),
            [|
                box gun
            |]
        )

    let useSignUp () =
        let gun = Recoil.useValue Recoil.Selectors.gun
        let setUsername = Recoil.useSetState Recoil.Atoms.username

        React.useCallbackRef
            (fun username password ->
                promise {
                    if username = "" || username = "" then
                        return Error "Required fields"
                    else
                        printfn $"Auth.useSignUp. gun before gun.user(): {JS.JSON.stringify gun.root}"
                        let user = gun.root.user ()
                        printfn $"Auth.useSignUp. gun.user() result: {JS.JSON.stringify user}"
                        let! ack = Gun.createUser user username password
                        printfn $"Auth.useSignUp. Gun.createUser ack: {JS.JSON.stringify ack}"

                        return
                            match ack.err with
                            | None ->
                                setUsername (Some (UserInteraction.Username username))
                                Ok ()
                            | Some error -> Error error
                })
