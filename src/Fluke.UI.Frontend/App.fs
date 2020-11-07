namespace Fluke.UI.Frontend

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Components
open Fable.Core.JsInterop
open Fluke.UI.Frontend.Bindings


module App =
    let persistenceObserver =
        React.memo (fun () ->
            Profiling.addTimestamp "persistenceObserver.render"

            Recoil.useTransactionObserver (fun snapshot ->
                let nodes = snapshot.snapshot?getNodes_UNSTABLE ({| isModified = true |})
                nodes
                |> Seq.iter (fun modifiedAtom ->
                    let atomLoadable = snapshot.snapshot.getLoadable modifiedAtom
                    match atomLoadable.state () with
                    | LoadableState.HasValue value ->
                        if false then
                            printfn "persisting1 <%A> <%A>" modifiedAtom.key value
                    | _ -> ()))

            nothing)

    module RootWrapper =
        let render children =
            React.memo (fun (input: {| Children: seq<ReactElement> |}) ->
                let theme = Theme.useTheme ()

                React.strictMode
                    [
                        Recoil.root [
                            root.init Recoil.initState
                            root.localStorage (fun _hydrater -> ()
                                //                            hydrater.setAtom Recoil.Atoms.debug
                                //                            hydrater.setAtom Recoil.Atoms.view
                                //                            hydrater.setAtom Recoil.Atoms.treeSelectionIds
                                //                            hydrater.setAtom Recoil.Atoms.selectedPosition
                                //                            hydrater.setAtom Recoil.Atoms.cellSize
                                //                            hydrater.setAtom Recoil.Atoms.daysBefore
                                //                            hydrater.setAtom Recoil.Atoms.daysAfter
                                //                            hydrater.setAtom Recoil.Atoms.leftDock
                                )

                            root.children [
                                persistenceObserver ()
                                Chakra.provider
                                    {| resetCSS = true; theme = theme |}
                                    [
                                        Chakra.darkMode
                                            ()
                                            [
                                                yield! input.Children
                                            ]
                                    ]
                            ]
                        ]
                    ])
            |> fun cmp -> cmp {| Children = children |}

    let render =
        React.memo (fun () ->
            Profiling.addTimestamp "appMain.render"

            RootWrapper.render [
                DebugOverlay.render ()

                CtrlListener.render ()
                ShiftListener.render ()
                SelectionListener.render ()
                ViewUpdater.render ()
//                PositionUpdater.render ()
                GunObserver.render ()
                UserLoader.render ()

                Content.render ()
            ])
