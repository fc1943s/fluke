namespace Fluke.UI.Frontend.Tests.Core

open Fable.React
open Feliz


module ReactErrorBoundary =
    [<AllowNullLiteral>]
    type InfoComponentObject =
        abstract componentStack: string

    type ErrorBoundaryProps =
        {
            Inner: ReactElement
            ErrorComponent: ReactElement
            OnError: exn * InfoComponentObject -> unit
        }

    type ErrorBoundaryState = { HasErrors: bool }

    type ErrorBoundary (props) =
        inherit Component<ErrorBoundaryProps, ErrorBoundaryState>(props)
        do base.setInitState ({ HasErrors = false })

        override x.componentDidCatch (error, info) =
            let info = info :?> InfoComponentObject
            x.props.OnError (error, info)
            x.setState (fun state _props -> { state with HasErrors = true })

        override x.render () =
            if (x.state.HasErrors) then
                x.props.ErrorComponent
            else
                x.props.Inner

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
