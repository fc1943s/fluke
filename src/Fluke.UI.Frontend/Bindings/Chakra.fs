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
                 HStack: obj
                 IconButton: obj
                 Input: obj
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
                 Spinner: obj
                 Stack: obj
                 TabList: obj
                 TabPanel: obj
                 TabPanels: obj
                 Tab: obj
                 Tabs: obj
                 useToast: unit -> System.Func<obj, unit> |} = jsNative

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
    let hStack<'T> = wrap core.HStack
    let iconButton<'T> = wrap core.IconButton
    let input<'T> = wrap core.Input
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
    let spinner<'T> = wrap core.Spinner
    let stack<'T> = wrap core.Stack
    let tabList<'T> = wrap core.TabList
    let tabPanel<'T> = wrap core.TabPanel
    let tabPanels<'T> = wrap core.TabPanels
    let tab<'T> = wrap core.Tab
    let tabs<'T> = wrap core.Tabs


    type ToastState =
        {
            Title: string
            Status: string
            Description: string
            Duration: int
            IsClosable: bool
        }

    type ToastBuilder () =
        member inline _.Yield _ =
            {
                Title = "Error"
                Status = "error"
                Description = "Message"
                Duration = 4000
                IsClosable = true
            }

        [<CustomOperation("description")>]
        member inline this.Description (state, description) = { state with Description = description }

        [<CustomOperation("title")>]
        member inline _.Title (state, title) = { state with Title = title }

        [<CustomOperation("status")>]
        member inline _.Status (state, status) = { state with Status = status }

        member inline _.Run (state: ToastState) =
            fun () ->
                let toast = core.useToast ()
                toast.Invoke state

    let useToast = ToastBuilder ()

