namespace Fluke.UI.Frontend

open Browser.Dom
open Fable.Core.JsInterop
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Main =

    importAll "typeface-roboto-condensed"

    importAll "../public/index.tsx"
    importAll "../public/index.ts"
    importAll "../public/index.jsx"
    importAll "../public/index.js"

    React.render (document.getElementById "root") (React.strictMode [ App.App true ])
