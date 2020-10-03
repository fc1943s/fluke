namespace Fluke.UI.Frontend

open System.Collections.Generic
open Browser
open Fable.React
open Fable.Core
open Fable.Core.JsInterop


module Chakra =

    [<ImportAll "@chakra-ui/core">]
    let chakraCore: {| Box: obj
                       Button: obj
                       ChakraProvider: obj
                       Checkbox: obj
                       DarkMode: obj
                       extendTheme: obj -> obj
                       Menu: obj
                       MenuButton: obj
                       MenuList: obj
                       MenuItem: obj |} = jsNative

    [<ImportAll "@chakra-ui/theme-tools">]
    let chakraTheme: {| mode: string * string -> obj -> obj |} = jsNative


    let wrap<'T, 'U> (comp: 'T) (props: 'U) children = ReactBindings.React.createElement (comp, props, children)

    let box<'T> = wrap chakraCore.Box
    let button<'T> = wrap chakraCore.Button
    let checkbox<'T> = wrap chakraCore.Checkbox
    let darkMode<'T> = wrap chakraCore.DarkMode
    let menu<'T> = wrap chakraCore.Menu
    let menuButton<'T> = wrap chakraCore.MenuButton
    let menuList<'T> = wrap chakraCore.MenuList
    let menuItem<'T> = wrap chakraCore.MenuItem
    let provider<'T> = wrap chakraCore.ChakraProvider

    let theme =
        chakraCore.extendTheme
            ({|
                 config = {| initialColorMode = "dark" |}
                 styles =
                     {|
                         ``global`` =
                             fun props ->
                                 {|
                                     body =
                                         {|
                                             backgroundColor = chakraTheme.mode ("white", "#212121") props
                                         |}
                                 |}
                     |}
             |} :> obj)

module JS =

    [<Emit("(w => $0 instanceof w[$1])(window)")>]
    let instanceof (_obj: obj, _typeName: string): bool = jsNative

module Ext =

    [<ImportAll "crypto-js">]
    let crypto: {| SHA3: string -> obj |} = jsNative


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

    [<ImportAll "react-dom">]
    let reactDom: {| unstable_createRoot: Browser.Types.HTMLElement -> {| render: Fable.React.ReactElement -> unit |} |} =
        jsNative

    let domRefs = Dictionary<string, obj> ()
    Dom.window?fluke <- domRefs
    let setDom key value = domRefs.[key] <- value

    setDom (nameof reactMarkdown) reactMarkdown
    setDom (nameof playSound) playSound
    setDom (nameof recoilLogger) recoilLogger
