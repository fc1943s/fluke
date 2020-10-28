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
    let loadUser (setter: CallbackMethods) username =
        promise {
            let! user = setter.snapshot.getPromise Recoil.Selectors.apiCurrentUser

            match user with
            | Some user ->
                if user.Username = username then
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
                Recoil.useCallbackRef (fun setter username ->
                    async {
                        Profiling.addTimestamp "UserLoader.render.loadUser"
                        do! loadUser setter username |> Async.AwaitPromise
                    }
                    |> Async.StartImmediate)

            React.useEffect
                ((fun () ->
                    match username with
                    | Some username -> loadUser username
                    | None -> ()),
                 [|
                     box username
                 |])

            nothing)
