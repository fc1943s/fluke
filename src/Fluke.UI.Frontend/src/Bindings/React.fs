namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.React
open Browser.Types

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
                if x.state.HasErrors then x.props.ErrorComponent else x.props.Inner

        let renderCatchSimple errorElement element =
            ofType<ErrorBoundary, _, _>
                {
                    Inner = element
                    ErrorComponent = errorElement
                    OnError = fun _ -> ()
                }
                []

        let renderCatchFn onError errorElement element =
            ofType<ErrorBoundary, _, _>
                {
                    Inner = element
                    ErrorComponent = errorElement
                    OnError = onError
                }
                []

    [<ImportAll "react">]
    let private react : {| StrictMode: obj -> ReactElement |} = jsNative

    [<ImportAll "react-dom">]
    let private reactDom : {| createRoot: HTMLElement -> {| render: ReactElement -> unit |} |} = jsNative

    let bindComponent<'C, 'P> (props: 'P) (children: seq<ReactElement>) (cmp: 'C) =
        ReactBindings.React.createElement (cmp, props, children)

    let composeComponent<'C, 'P> (cmp: 'C) (props: 'P) (children: seq<ReactElement>) =
        bindComponent<'C, 'P> props children cmp

    let strictMode children =
        bindComponent {|  |} children react.StrictMode


    //    ReactDOM.render (appMain (), document.getElementById "root")
    let render rootElement appComponent =
        reactDom
            .createRoot(rootElement)
            .render appComponent
