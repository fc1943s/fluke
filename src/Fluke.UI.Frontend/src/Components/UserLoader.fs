namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module UserLoader =

    [<ReactComponent>]
    let UserLoader () =
        let username = Recoil.useValue Atoms.username

        let loadUser =
            Recoil.useCallbackRef
                (fun setter username ->
                    promise {
                        Profiling.addTimestamp "UserLoader.render.loadUser"

                        let! user = setter.snapshot.getPromise Selectors.apiCurrentUserAsync

                        match user with
                        | Ok user ->
                            if user.Username = username then
                                setter.set (Atoms.User.color user.Username, user.Color)
                                setter.set (Atoms.User.dayStart user.Username, user.DayStart)
                                setter.set (Atoms.User.sessionLength user.Username, user.SessionLength)
                                setter.set (Atoms.User.weekStart user.Username, user.WeekStart)
                                setter.set (Atoms.User.sessionBreakLength user.Username, user.SessionBreakLength)
                        | Error _ -> ()
                    })

        React.useEffect (
            (fun () ->
                match username with
                | Some username -> loadUser username |> Promise.start
                | None -> ()),
            [|
                box loadUser
                box username
            |]
        )

        nothing
