namespace Fluke.UI.Frontend

open Browser
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop

module Ext =
    JsInterop.importAll "typeface-roboto-condensed"


    JsInterop.importAll "../node_modules/@fortawesome/fontawesome-free/css/all.css"

    JsInterop.importAll "../node_modules/bulma/bulma.sass"
    JsInterop.importAll "../node_modules/bulma-extensions/dist/css/bulma-extensions.min.css"
    JsInterop.importAll "../node_modules/bulmaswatch/cyborg/bulmaswatch.scss"

    JsInterop.importAll "../public/index.scss"
    JsInterop.importAll "../public/index.ts"
    JsInterop.importAll "../public/index.tsx"
    JsInterop.importAll "../public/index.js"
    JsInterop.importAll "../public/index.jsx"


    let flatted : ExtTypes.IFlatted = importAll "flatted/esm"
    let moment : obj -> ExtTypes.IMoment = importAll "moment"
    let reactMarkdown : obj -> obj = importDefault "react-markdown"
    
    let useEventListener (_event: string) (_fn: KeyboardEvent -> unit) : unit = importDefault "@use-it/event-listener"
    
    Dom.window?Ext <-
        {| flatted = flatted
           moment = moment
           reactMarkdown = reactMarkdown
           useEventListener = useEventListener |}

