namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Router
open Feliz.Recoil
open Fluke.UI.Frontend.Hooks
open Fable.Core.JsInterop
open Fluke.UI.Frontend.Bindings


module RootWrapper =

    [<ReactComponent>]
    let PersistenceObserver () =
        Profiling.addTimestamp "persistenceObserver.render"

        Recoil.useTransactionObserver
            (fun snapshot ->
                let nodes = snapshot.snapshot?getNodes_UNSTABLE {| isModified = true |}

                nodes
                |> Seq.iter
                    (fun modifiedAtom ->
                        let atomLoadable = snapshot.snapshot.getLoadable modifiedAtom

                        match atomLoadable.state () with
                        | LoadableState.HasValue value ->
                            if false then printfn $"persisting1 <{modifiedAtom.key}> <{value}>"
                        | _ -> ()))

        nothing

    [<ReactComponent>]
    let RootWrapper children =
        let theme = Theme.useTheme ()

        React.strictMode [
            Recoil.root [
                root.children [

                    //                        Recoilize.recoilizeDebugger
//                            {|
//                                root = Browser.Dom.document.getElementById "root"
//                            |}
//                            []
                    React.ReactErrorBoundary.renderCatchFn
                        (fun (error, info) -> printfn $"ReactErrorBoundary Error: {info.componentStack} {error}")
                        (Html.div [
                            prop.style [ style.color "white" ]
                            prop.children [ str "error" ]
                         ])
                        (Chakra.provider
                            (fun x -> x.theme <- theme)
                            [
                                PersistenceObserver ()
                                React.router [
                                    router.children [ yield! children ]
                                ]
                            ])
                ]
            ]
        ]
