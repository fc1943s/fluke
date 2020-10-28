namespace Fluke.UI.Frontend.Hooks

open Browser.Types
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.Shared.Domain
open Fable.React

module Auth =
    let useLogout () =
        let gun = Recoil.useValue Recoil.Selectors.gun
        let setUsername = Recoil.useSetState Recoil.Atoms.username
        React.useCallback
            ((fun () ->
                let user = gun.root.user ()
                printfn "before leave"
                user.leave ()
                setUsername None),
             [|
                 box gun
             |])

    let useSignIn () =
        let gun = Recoil.useValue Recoil.Selectors.gun
        let setUsername = Recoil.useSetState Recoil.Atoms.username
        React.useCallback
            ((fun username password ->
                promise {
                    let user = gun.root.user ()
                    let! ack = Gun.authUser user username password

                    return match ack.err with
                           | Some error -> Error error
                           | None ->
                               setUsername (Some (UserInteraction.Username username))
                               Ok ()
                }),
             [|
                 box gun
             |])

    let useSignUp () =
        let gun = Recoil.useValue Recoil.Selectors.gun
        React.useCallback
            ((fun username password ->
                promise {
                    if username = "" || username = "" then
                        return Error "Required fields"
                    else
                        let user = gun.root.user ()
                        let! ack = Gun.createUser user username password

                        match ack.err with
                        | None -> return Ok ()
                        | Some error -> return Error error
                }),
             [|
                 box gun
             |])
