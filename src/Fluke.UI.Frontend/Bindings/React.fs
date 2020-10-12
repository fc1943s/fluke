namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.React
open Browser.Types

module React =
    [<ImportAll "react">]
    let private react: {| StrictMode: obj -> ReactElement |} = jsNative

    [<ImportAll "react-dom">]
    let private reactDom: {| unstable_createRoot: HTMLElement -> {| render: ReactElement -> unit |} |} = jsNative

    let wrap<'T, 'U> (cmp: 'T) (props: 'U) children = ReactBindings.React.createElement (cmp, props, children)

    let strictMode children = wrap react.StrictMode () children


    //    ReactDOM.render (appMain (), document.getElementById "root")
    let render rootElement appComponent = reactDom.unstable_createRoot(rootElement).render(appComponent)
