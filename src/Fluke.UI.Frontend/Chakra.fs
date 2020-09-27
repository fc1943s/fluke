namespace Fluke.UI.Frontend

open Fable.React
open Fable.Core


module Chakra =

    [<ImportAll "@chakra-ui/core">]
    let chakraCore: {| Box: obj
                       Button: obj
                       ChakraProvider: obj
                       DarkMode: obj
                       extendTheme: obj -> obj
                       Menu: obj
                       MenuButton: obj
                       MenuList: obj
                       MenuItemOption: obj
                       MenuOptionGroup: obj |} = jsNative

    [<ImportAll "@chakra-ui/theme-tools">]
    let chakraTheme: {| mode: string * string -> obj -> obj |} = jsNative

    let wrap<'T, 'U> (comp: 'T) (props: 'U) children = ReactBindings.React.createElement (comp, props, children)

    let box<'T> = wrap chakraCore.Box
    let button<'T> = wrap chakraCore.Button
    let darkMode<'T> = wrap chakraCore.DarkMode
    let menu<'T> = wrap chakraCore.Menu
    let menuButton<'T> = wrap chakraCore.MenuButton
    let menuList<'T> = wrap chakraCore.MenuList
    let menuItemOption<'T> = wrap chakraCore.MenuItemOption
    let menuOptionGroup<'T> = wrap chakraCore.MenuOptionGroup
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
