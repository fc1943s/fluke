namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module Chakra =
    open React

    [<ImportAll "@chakra-ui/react">]
    let react : {| Box: obj
                   Button: obj
                   Center: obj
                   ChakraProvider: obj
                   Checkbox: obj
                   CheckboxGroup: obj
                   Circle: obj
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
                   Modal: obj
                   ModalBody: obj
                   ModalCloseButton: obj
                   ModalContent: obj
                   ModalOverlay: obj
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
                   Tooltip: obj
                   useToast: unit -> System.Func<obj, unit> |} =
        jsNative

    [<ImportAll "@chakra-ui/theme-tools">]
    let themeTools : {| mode: string * string -> obj -> obj |} = jsNative



    let box<'T> = composeComponent react.Box
    let button<'T> = composeComponent react.Button
    let center<'T> = composeComponent react.Center
    let checkbox<'T> = composeComponent react.Checkbox
    let checkboxGroup<'T> = composeComponent react.CheckboxGroup
    let circle<'T> = composeComponent react.Circle
    let darkMode<'T> = composeComponent react.DarkMode
    let flex<'T> = composeComponent react.Flex
    let grid<'T> = composeComponent react.Grid
    let hStack<'T> = composeComponent react.HStack
    let iconButton<'T> = composeComponent react.IconButton
    let input<'T> = composeComponent react.Input
    let menu<'T> = composeComponent react.Menu
    let menuButton<'T> = composeComponent react.MenuButton
    let menuList<'T> = composeComponent react.MenuList
    let menuItem<'T> = composeComponent react.MenuItem
    let modal<'T> = composeComponent react.Modal
    let modalBody<'T> = composeComponent react.ModalBody
    let modalContent<'T> = composeComponent react.ModalContent
    let modalCloseButton<'T> = composeComponent react.ModalCloseButton
    let modalOverlay<'T> = composeComponent react.ModalOverlay
    let numberInput<'T> = composeComponent react.NumberInput
    let numberInputField<'T> = composeComponent react.NumberInputField
    let numberInputStepper<'T> = composeComponent react.NumberInputStepper
    let numberDecrementStepper<'T> = composeComponent react.NumberDecrementStepper
    let numberIncrementStepper<'T> = composeComponent react.NumberIncrementStepper
    let provider<'T> = composeComponent react.ChakraProvider
    let simpleGrid<'T> = composeComponent react.SimpleGrid
    let spacer<'T> = composeComponent react.Spacer
    let spinner<'T> = composeComponent react.Spinner
    let stack<'T> = composeComponent react.Stack
    let tabList<'T> = composeComponent react.TabList
    let tabPanel<'T> = composeComponent react.TabPanel
    let tabPanels<'T> = composeComponent react.TabPanels
    let tab<'T> = composeComponent react.Tab
    let tabs<'T> = composeComponent react.Tabs
    let tooltip<'T> = composeComponent react.Tooltip


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
        member inline this.Description (state, description) =
            { state with Description = description }

        [<CustomOperation("title")>]
        member inline _.Title (state, title) = { state with Title = title }

        [<CustomOperation("status")>]
        member inline _.Status (state, status) = { state with Status = status }

        member inline _.Run (state: ToastState) =
            fun () ->
                let toast = react.useToast ()
                toast.Invoke state

    let useToast = ToastBuilder ()
