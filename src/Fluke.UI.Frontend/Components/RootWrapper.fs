namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Router
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks
open Fable.Core.JsInterop
open Fluke.UI.Frontend.Bindings


module RootWrapper =
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

    let render children =
        React.memo (fun (input: {| Children: seq<ReactElement> |}) ->
            let theme = Theme.useTheme ()

            Recoil.root [
                root.init Recoil.initState
                root.children [
                    Chakra.provider
                        {| resetCSS = true; theme = theme |}
                        [
                            persistenceObserver ()
                            React.router [
                                router.children [
                                    yield! input.Children
                                ]
                            ]
                        ]
                ]
            ])
        |> fun cmp -> cmp {| Children = children |}
