namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.React
open Browser.Types

module React =
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
