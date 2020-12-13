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

    [<ReactComponent>]
    let PersistenceObserver () =
        Profiling.addTimestamp "persistenceObserver.render"

        Recoil.useTransactionObserver (fun snapshot ->
            let nodes = snapshot.snapshot?getNodes_UNSTABLE ({| isModified = true |})

            nodes
            |> Seq.iter (fun modifiedAtom ->
                let atomLoadable = snapshot.snapshot.getLoadable modifiedAtom

                match atomLoadable.state () with
                | LoadableState.HasValue value ->
                    if false then
                        printfn $"persisting1 <{modifiedAtom.key}> <{value}>"
                | _ -> ()))

        nothing

    let rootWrapper children =
        React.memo (fun () ->
            let theme = Theme.useTheme ()

            Recoil.root [
                root.init (fun _ -> ())
                root.children [
                    Chakra.provider
                        {| resetCSS = true; theme = theme |}
                        [
                            PersistenceObserver ()
                            React.router [
                                router.children [ yield! children ]
                            ]
                        ]
                ]
            ])
        |> React.bindComponent {|  |} children
