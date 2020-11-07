namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend


module UserLoader =
    let render =
        React.memo (fun () ->
            let username = Recoil.useValue Recoil.Atoms.username

            let loadUser =
                Recoil.useCallbackRef (fun setter username ->
                    promise {
                        Profiling.addTimestamp "UserLoader.render.loadUser"
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
                    })

            React.useEffect
                ((fun () ->
                    match username with
                    | Some username -> loadUser username |> Promise.start
                    | None -> ()),
                 [|
                     box username
                 |])

            nothing)
