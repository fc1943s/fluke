namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open FsJs
open FsStore
open FsUi.Bindings
open Fluke.UI.Frontend.State
open FsUi.Components


module Content =
    [<ReactComponent>]
    let LoggedContent () =
        let userColor = Store.useValue Atoms.User.userColor

        Dom.log (fun () -> $"Content.render. userColor={userColor}")

        //        let asyncTaskIdAtoms = Store.useValue Selectors.asyncTaskIdAtoms
//        let archive = Store.useValue Atoms.User.archive
//
//        let callbacks = Store.useCallbacks ()
//
//        React.useEffect (
//            (fun () ->
//                promise {
//                    let! getter, setter = callbacks ()
//
//                    if archive.IsNone then
//                        printfn "setting archive"
//                        Store.set setter Atoms.User.archive (Some false)
//
//                    asyncTaskIdAtoms
//                    |> Array.iter
//                        (fun taskIdAtom ->
//                            let taskId = Store.value getter taskIdAtom
//                            let archived = Store.value getter (Atoms.Task.archived taskId)
//
//                            if archived.IsNone then
//                                printfn "setting task archive"
//                                Store.set setter (Atoms.Task.archived taskId) (Some false))
//                }
//                |> Promise.start),
//            [|
//                box archive
//                box asyncTaskIdAtoms
//                box callbacks
//            |]
//        )

        React.suspense (
            [
                PositionUpdater.PositionUpdater ()
                PasteListener.PasteListener ()

                if userColor.IsNone then
                    LoadingSpinner.LoadingSpinner ()
                else
                    Ui.flex
                        (fun x -> x.flex <- "1")
                        [
                            React.suspense (
                                [
                                    LeftDock.LeftDock ()
                                ],
                                LoadingSpinner.LoadingSpinner ()
                            )
                            React.suspense (
                                [
                                    ViewTabs.ViewTabs ()
                                ],
                                LoadingSpinner.LoadingSpinner ()
                            )
                            React.suspense (
                                [
                                    RightDock.RightDock ()
                                ],
                                LoadingSpinner.LoadingSpinner ()
                            )
                        ]

                    StatusBar.StatusBar ()

                React.suspense (
                    [
                        SoundPlayer.SoundPlayer ()
                    ],
                    nothing
                )
            ],
            LoadingSpinner.LoadingSpinner ()
        )

    [<ReactComponent>]
    let Content () =
        Profiling.addTimestamp "mainComponent.render"

        let sessionRestored = Store.useValue Atoms.Session.sessionRestored
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let username = Store.useValue Atoms.username

        Ui.flex
            (fun x ->
                x.flex <- "1"
                x.minHeight <- "100vh"
                x.height <- if deviceInfo.IsExtension then "590px" else null
                x.width <- if deviceInfo.IsExtension then "790px" else null)
            [
                Ui.stack
                    (fun x ->
                        x.spacing <- "0"
                        x.flex <- "1"
                        x.maxWidth <- "100vw")
                    [
                        TopBar.TopBar ()

                        match sessionRestored with
                        | false -> LoadingSpinner.LoadingSpinner ()
                        | true ->
                            match username with
                            | None -> LoginScreen.LoginScreen ()
                            | Some _ -> LoggedContent ()
                    ]
            ]
