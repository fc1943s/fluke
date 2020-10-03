namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core


module Chakra =
    [<ImportAll "@chakra-ui/core">]
    let core: {| Box: obj
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
    let private chakraTheme: {| mode: string * string -> obj -> obj |} = jsNative


    let private wrap<'T, 'U> (comp: 'T) (props: 'U) children = ReactBindings.React.createElement (comp, props, children)

    let box<'T> = wrap core.Box
    let button<'T> = wrap core.Button
    let checkbox<'T> = wrap core.Checkbox
    let darkMode<'T> = wrap core.DarkMode
    let menu<'T> = wrap core.Menu
    let menuButton<'T> = wrap core.MenuButton
    let menuList<'T> = wrap core.MenuList
    let menuItem<'T> = wrap core.MenuItem
    let provider<'T> = wrap core.ChakraProvider

    let theme =
        core.extendTheme
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
