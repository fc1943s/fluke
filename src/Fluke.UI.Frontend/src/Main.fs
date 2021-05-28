namespace Fluke.UI.Frontend

open Fable.Core.JsInterop
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Main =
    importSideEffects "@fontsource/roboto-condensed/300"

    importAll "../public/index.tsx"
    importAll "../public/index.ts"
    importAll "../public/index.jsx"
    importAll "../public/index.js"

    React.render (Browser.Dom.document.getElementById "root") (React.strictMode [ App.App true ])
