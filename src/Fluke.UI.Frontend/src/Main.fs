namespace Fluke.UI.Frontend

open Fable.Core
open Fable.Core.JsInterop
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Main =
    importSideEffects "@fontsource/roboto-condensed/300.css"

    importAll "../static/index.tsx"
    importAll "../static/index.ts"
    importAll "../static/index.jsx"
    importAll "../static/index.js"

    exportDefault (
        let cmp = React.strictMode [ App.App true ]

        match JS.window id with
        | Some window ->
            React.render (window.document.getElementById "root") cmp
            JS.undefined
        | None -> cmp
    )
