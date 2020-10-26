namespace Fluke.UI.Frontend.Components

open Fable.Core
open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module UserLoader =
    let loadUser (setter: CallbackMethods) =
        promise {
            let! user = setter.snapshot.getPromise Recoil.Selectors.currentUser

            match user with
            | Some user ->
                setter.set (Recoil.Atoms.username, Some user.Username)
                setter.set (Recoil.Atoms.Session.user user.Username, Some user)
                setter.set (Recoil.Atoms.User.color user.Username, user.Color)
                setter.set (Recoil.Atoms.User.dayStart user.Username, user.DayStart)
                setter.set (Recoil.Atoms.User.sessionLength user.Username, user.SessionLength)
                setter.set (Recoil.Atoms.User.weekStart user.Username, user.WeekStart)
                setter.set (Recoil.Atoms.User.sessionBreakLength user.Username, user.SessionBreakLength)
            | None -> ()
        }

    let render =
        React.memo (fun () ->
            let username = Recoil.useValue Recoil.Atoms.username

            let loadUser =
                Recoil.useCallbackRef (fun setter ->
                    async {
                        Profiling.addTimestamp "UserLoader.render.loadUser"
                        do! loadUser setter |> Async.AwaitPromise
                    }
                    |> Async.StartImmediate)

            React.useEffect
                ((fun () ->
                    match username with
                    | Some _ -> ()
                    | None -> loadUser ()),
                 [|
                     box username
                 |])

            nothing)
