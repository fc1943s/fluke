namespace Fluke.UI.Frontend.Hooks

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module UserLoader =
    let hook =
        React.memo (fun () ->
            let username = Recoil.useValue Recoil.Atoms.username

            let loadUser =
                Recoil.useCallbackRef (fun setter ->
                    async {
                        Profiling.addTimestamp "UserLoader.hook.loadUser"
                        let! user = setter.snapshot.getAsync Recoil.Selectors.currentUser
                        match user with
                        | Some user ->
                            setter.set (Recoil.Atoms.username, Some user.Username)
                            setter.set (Recoil.Atoms.Session.user user.Username, Some user)
                        | None -> ()
                    }
                    |> Async.StartImmediate)

            React.useEffect
                ((fun () ->
                    match username with
                    | Some _ -> ()
                    | None -> loadUser ()),
                 [|
                     username :> obj
                 |])

            nothing)
