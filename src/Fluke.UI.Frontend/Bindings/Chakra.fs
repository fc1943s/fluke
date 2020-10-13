namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module Chakra =
    open React

    [<ImportAll "@chakra-ui/core">]
    let core: {| Box: obj
                 Button: obj
                 Center: obj
                 ChakraProvider: obj
                 Checkbox: obj
                 CheckboxGroup: obj
                 DarkMode: obj
                 extendTheme: obj -> obj
                 Flex: obj
                 Grid: obj
                 IconButton: obj
                 Menu: obj
                 MenuButton: obj
                 MenuList: obj
                 MenuItem: obj
                 NumberInput: obj
                 NumberInputField: obj
                 NumberInputStepper: obj
                 NumberDecrementStepper: obj
                 NumberIncrementStepper: obj
                 SimpleGrid: obj
                 Spacer: obj
                 Stack: obj
                 TabList: obj
                 TabPanel: obj
                 TabPanels: obj
                 Tab: obj
                 Tabs: obj |} = jsNative

    [<ImportAll "@chakra-ui/theme-tools">]
    let theme: {| mode: string * string -> obj -> obj |} = jsNative



    let box<'T> = wrap core.Box
    let button<'T> = wrap core.Button
    let center<'T> = wrap core.Center
    let checkbox<'T> = wrap core.Checkbox
    let checkboxGroup<'T> = wrap core.CheckboxGroup
    let darkMode<'T> = wrap core.DarkMode
    let flex<'T> = wrap core.Flex
    let grid<'T> = wrap core.Grid
    let iconButton<'T> = wrap core.IconButton
    let menu<'T> = wrap core.Menu
    let menuButton<'T> = wrap core.MenuButton
    let menuList<'T> = wrap core.MenuList
    let menuItem<'T> = wrap core.MenuItem
    let numberInput<'T> = wrap core.NumberInput
    let numberInputField<'T> = wrap core.NumberInputField
    let numberInputStepper<'T> = wrap core.NumberInputStepper
    let numberDecrementStepper<'T> = wrap core.NumberDecrementStepper
    let numberIncrementStepper<'T> = wrap core.NumberIncrementStepper
    let provider<'T> = wrap core.ChakraProvider
    let simpleGrid<'T> = wrap core.SimpleGrid
    let spacer<'T> = wrap core.Spacer
    let stack<'T> = wrap core.Stack
    let tabList<'T> = wrap core.TabList
    let tabPanel<'T> = wrap core.TabPanel
    let tabPanels<'T> = wrap core.TabPanels
    let tab<'T> = wrap core.Tab
    let tabs<'T> = wrap core.Tabs
