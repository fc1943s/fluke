namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.React
open Browser.Types

module React =
    [<ImportAll "react">]
    let private react: {| StrictMode: obj -> ReactElement |} = jsNative

    [<ImportAll "react-dom">]
    let private reactDom: {| unstable_createRoot: HTMLElement -> {| render: ReactElement -> unit |} |} = jsNative


    let strictMode children = ReactBindings.React.createElement (react.StrictMode, (), children)


    //    ReactDOM.render (appMain (), document.getElementById "app")
    let render rootElement appComponent = reactDom.unstable_createRoot(rootElement).render(appComponent)
