namespace FsUi.Bindings

open Fable.Core
open Fable.React
open Browser.Types
open Feliz


module React =
    module ReactErrorBoundary =
        [<AllowNullLiteral>]
        type InfoComponentObject =
            abstract componentStack : string

        type ErrorBoundaryProps =
            {
                Inner: ReactElement
                ErrorComponent: ReactElement
                OnError: exn * InfoComponentObject -> unit
            }

        type ErrorBoundaryState = { HasErrors: bool }

        type ErrorBoundary (props) =
            inherit Component<ErrorBoundaryProps, ErrorBoundaryState> (props)
            do base.setInitState { HasErrors = false }

            override x.componentDidCatch (error, info) =
                let info = info :?> InfoComponentObject
                x.props.OnError (error, info)
                x.setState (fun state _props -> { state with HasErrors = true })

            override x.render () =
                if x.state.HasErrors then
                    (Html.div [
                        Html.button [
                            prop.onClick (fun _e -> x.setState (fun state _props -> { state with HasErrors = false }))
                            prop.children [ str "Retry" ]
                        ]
                        x.props.ErrorComponent
                     ])
                else
                    x.props.Inner

        let inline renderCatchSimple errorElement element =
            ofType<ErrorBoundary, _, _>
                {
                    Inner = element
                    ErrorComponent = errorElement
                    OnError = fun _ -> ()
                }
                []

        let inline renderCatchFn onError errorElement element =
            ofType<ErrorBoundary, _, _>
                {
                    Inner = element
                    ErrorComponent = errorElement
                    OnError = onError
                }
                []

    [<ReactComponent>]
    let ErrorBoundary cmp =
        React.strictMode [
            //            Recoil.root [
//                root.children [
            ReactErrorBoundary.renderCatchFn
                (fun (error, info) -> printfn $"ReactErrorBoundary.renderCatchFn Error: {info.componentStack} {error}")
                (Html.div [
                    prop.classes [ "static" ]
                    prop.children [
                        str "Unhandled Exception. Check the console log."
                    ]
                 ])
                (React.fragment cmp)
            //                ]
//            ]
            ]

    [<ImportAll "react-dom">]
    let reactDom: {| createRoot: HTMLElement -> {| render: ReactElement -> unit |} |} = jsNative

    let inline bindComponent<'C, 'P> (props: 'P) children (cmp: 'C) =
        Interop.reactApi.createElement (cmp, props, children)

    let inline renderComponent<'C, 'P> (cmp: 'C) (props: 'P) children =
        bindComponent<'C, 'P> props children cmp


    //    ReactDOM.render (appMain (), document.getElementById "root")
    let inline render rootElement appComponent =
        (reactDom.createRoot rootElement)
            .render appComponent
