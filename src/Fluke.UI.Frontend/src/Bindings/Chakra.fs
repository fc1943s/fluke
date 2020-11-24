namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module Chakra =
    open React

    [<ImportAll "@chakra-ui/react">]
    let core: {| Box: obj
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
                 useToast: unit -> System.Func<obj, unit> |} = jsNative

    [<ImportAll "@chakra-ui/theme-tools">]
    let theme: {| mode: string * string -> obj -> obj |} = jsNative



    let box<'T> = composeComponent core.Box
    let button<'T> = composeComponent core.Button
    let center<'T> = composeComponent core.Center
    let checkbox<'T> = composeComponent core.Checkbox
    let checkboxGroup<'T> = composeComponent core.CheckboxGroup
    let circle<'T> = composeComponent core.Circle
    let darkMode<'T> = composeComponent core.DarkMode
    let flex<'T> = composeComponent core.Flex
    let grid<'T> = composeComponent core.Grid
    let hStack<'T> = composeComponent core.HStack
    let iconButton<'T> = composeComponent core.IconButton
    let input<'T> = composeComponent core.Input
    let menu<'T> = composeComponent core.Menu
    let menuButton<'T> = composeComponent core.MenuButton
    let menuList<'T> = composeComponent core.MenuList
    let menuItem<'T> = composeComponent core.MenuItem
    let modal<'T> = composeComponent core.Modal
    let modalBody<'T> = composeComponent core.ModalBody
    let modalContent<'T> = composeComponent core.ModalContent
    let modalCloseButton<'T> = composeComponent core.ModalCloseButton
    let modalOverlay<'T> = composeComponent core.ModalOverlay
    let numberInput<'T> = composeComponent core.NumberInput
    let numberInputField<'T> = composeComponent core.NumberInputField
    let numberInputStepper<'T> = composeComponent core.NumberInputStepper
    let numberDecrementStepper<'T> = composeComponent core.NumberDecrementStepper
    let numberIncrementStepper<'T> = composeComponent core.NumberIncrementStepper
    let provider<'T> = composeComponent core.ChakraProvider
    let simpleGrid<'T> = composeComponent core.SimpleGrid
    let spacer<'T> = composeComponent core.Spacer
    let spinner<'T> = composeComponent core.Spinner
    let stack<'T> = composeComponent core.Stack
    let tabList<'T> = composeComponent core.TabList
    let tabPanel<'T> = composeComponent core.TabPanel
    let tabPanels<'T> = composeComponent core.TabPanels
    let tab<'T> = composeComponent core.Tab
    let tabs<'T> = composeComponent core.Tabs
    let tooltip<'T> = composeComponent core.Tooltip


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
