namespace Fluke.UI.Frontend

open Browser
open Fable.Core
open Fable.Core.JsInterop


module Ext =

    //    importAll "./node_modules/@fortawesome/fontawesome-free/css/all.css"

    //    importAll "./node_modules/bulma/bulma.sass"
//    importAll "./node_modules/bulma-extensions/dist/css/bulma-extensions.min.css"
//    importAll "./node_modules/bulmaswatch/cyborg/bulmaswatch.scss"

    importAll "typeface-roboto-condensed"

    importAll "./public/index.scss"
    importAll "./public/index.tsx"
    importAll "./public/index.ts"
    importAll "./public/index.jsx"
    importAll "./public/index.js"

    //    [<Emit "(new Audio($0)).play();">]
    [<Emit "(() => { var audio = new Audio($0); audio.volume = 0.5; return audio; })().play();">]
    let playSound (_file: string): unit = jsNative

    let reactMarkdown: obj -> obj = importDefault "react-markdown"

    //    let useEventListener (_event: string) (_fn: KeyboardEvent -> unit) : unit = importDefault "@use-it/event-listener"

    let recoilLogger: obj -> obj = importDefault "recoil-logger"

    Dom.window?Ext <- {|
                          reactMarkdown = reactMarkdown
                          playSound = playSound
                          recoilLogger = recoilLogger
                      |}
