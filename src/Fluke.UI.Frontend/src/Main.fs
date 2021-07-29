namespace Fluke.UI.Frontend

open Fable.Core
open FsUi.Bindings
open Feliz
open Fable.Core.JsInterop
open FsJs


module Main =
    importSideEffects "@fontsource/roboto-condensed/300.css"

    importAll "../public/index.tsx"
    importAll "../public/index.ts"
    importAll "../public/index.jsx"
    importAll "../public/index.js"

    exportDefault (
        let cmp = React.strictMode [ App.App true ]

        match Dom.window () with
        | Some window ->
            React.render (window.document.getElementById "root") cmp
            JS.undefined
        | None -> cmp
    )
