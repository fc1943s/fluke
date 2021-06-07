namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Content =

    [<ReactComponent>]
    let Migrations (input: {| Username: Username |}) =
        Recoil.useEffect (
            (fun setter ->
                promise {
                    let! joinSet = setter.snapshot.getPromise (Atoms.User.joinSet input.Username)
                    let! databaseIdSet = setter.snapshot.getPromise (Atoms.User.databaseIdSet input.Username)

                    if not joinSet.IsEmpty then
                        let databaseIdSet =
                            joinSet
                            |> Set.choose
                                (function
                                | Join.Database databaseId -> Some databaseId
                                | _ -> None)
                            |> Set.union databaseIdSet

                        let! _ =
                            databaseIdSet
                            |> Seq.map
                                (fun databaseId ->
                                    promise {
                                        let! taskIdSet =
                                            setter.snapshot.getPromise (
                                                Atoms.Database.taskIdSet (input.Username, databaseId)
                                            )

                                        let newTaskIdSet =
                                            joinSet
                                            |> Set.choose
                                                (function
                                                | Join.Task (databaseId', taskId) when databaseId' = databaseId ->
                                                    Some taskId
                                                | _ -> None)
                                            |> Set.union taskIdSet

                                        setter.set (Atoms.Database.taskIdSet (input.Username, databaseId), newTaskIdSet)
                                    })
                            |> Promise.Parallel

                        setter.set (Atoms.User.databaseIdSet input.Username, databaseIdSet)

                        printfn "# clearing joinSet (commented)"
//                        setter.set (Atoms.User.joinSet input.Username, Set.empty)
                }),
            [|
                box input.Username
            |]
        )

        nothing

    [<ReactComponent>]
    let Content () =
        Profiling.addTimestamp "mainComponent.render"
        let username = Recoil.useValue Atoms.username
        let sessionRestored = Recoil.useValue Atoms.sessionRestored
        let initialPeerSkipped = Recoil.useValue Atoms.initialPeerSkipped
        let gunPeers = Recoil.useValue Selectors.gunPeers
        let deviceInfo = Recoil.useValue Selectors.deviceInfo

        Chakra.flex
            (fun x ->
                x.flex <- "1"
                x.minHeight <- "100vh"
                x.height <- if deviceInfo.IsExtension then "590px" else null
                x.width <- if deviceInfo.IsExtension then "790px" else null)
            [
                match sessionRestored with
                | false -> LoadingSpinner.LoadingSpinner ()
                | true ->
                    match username with
                    | None ->
                        match gunPeers, initialPeerSkipped with
                        | [], false -> InitialPeers.InitialPeers ()
                        | _ -> LoginScreen.LoginScreen ()
                    | Some username ->
                        React.suspense (
                            [
                                Migrations {| Username = username |}
                                PositionUpdater.PositionUpdater {| Username = username |}
                                PositionUpdater.SessionDataUpdater {| Username = username |}

                                Chakra.stack
                                    (fun x ->
                                        x.spacing <- "0"
                                        x.flex <- "1"
                                        x.borderWidth <- "1px"
                                        x.borderColor <- "whiteAlpha.300"
                                        x.maxWidth <- "100vw")
                                    [
                                        TopBar.TopBar ()

                                        Chakra.flex
                                            (fun x -> x.flex <- "1")
                                            [
                                                LeftDock.LeftDock {| Username = username |}
                                                ViewTabs.ViewTabs {| Username = username |}
                                            ]

                                        StatusBar.StatusBar {| Username = username |}
                                    ]

                                SoundPlayer.SoundPlayer {| Username = username |}
                            ],
                            LoadingSpinner.LoadingSpinner ()
                        )
            ]
