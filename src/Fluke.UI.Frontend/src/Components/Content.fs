namespace Fluke.UI.Frontend.Components

open Fluke.UI.Frontend.Hooks
open FsCore
open FsStore.State
open Feliz
open Fable.React
open FsJs
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.UI.Frontend.State
open FsUi.Components


module Content =
    [<ReactComponent>]
    let LoggedContent () =
        let logger = Store.useValue Selectors.Store.logger
        let userColor = Store.useValue Atoms.User.userColor
        let inline getLocals () = $"userColor={userColor} {getLocals ()}"
        logger.Debug (fun () -> $"Content.render") getLocals
        let _hydrateTemplates = Hydrate.useHydrateTemplates ()


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
//                        Atom.set setter Atoms.User.archive (Some false)
//
//                    asyncTaskIdAtoms
//                    |> Array.iter
//                        (fun taskIdAtom ->
//                            let taskId = Atom.get getter taskIdAtom
//                            let archived = Atom.get getter (Atoms.Task.archived taskId)
//
//                            if archived.IsNone then
//                                printfn "setting task archive"
//                                Atom.set setter (Atoms.Task.archived taskId) (Some false))
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
        Profiling.addTimestamp (fun () -> "Content.render") getLocals

        let deviceInfo = Store.useValue Selectors.Store.deviceInfo
        let alias = Store.useValue Selectors.Gun.alias
        let _ = Auth.useGunAliasLoader ()

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

                        match alias with
                        | None -> LoginScreen.LoginScreen ()
                        | Some _ -> LoggedContent ()
                    ]
            ]
