namespace Fluke.UI.Frontend.Bindings

open Fable.Core

module React =
    [<ImportAll "react-dom">]
    let private reactDom: {| unstable_createRoot: Browser.Types.HTMLElement -> {| render: Fable.React.ReactElement -> unit |} |} =
        jsNative

    //    ReactDOM.render (appMain (), document.getElementById "app")
    let render rootElement appComponent = reactDom.unstable_createRoot(rootElement).render(appComponent)
