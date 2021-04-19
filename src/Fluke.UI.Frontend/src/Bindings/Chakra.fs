namespace Fluke.UI.Frontend.Bindings

open Browser.Types
open Fable.Core
open Feliz


module Chakra =
    open React

    type IChakraProps =
        abstract _after : IChakraProps with get, set
        abstract _focus : IChakraProps with get, set
        abstract _hover : IChakraProps with get, set
        abstract _selected : IChakraProps with get, set
        abstract ``as`` : obj with get, set
        abstract align : string with get, set
        abstract autoFocus : bool with get, set
        abstract backgroundColor : string with get, set
        abstract border : string with get, set
        abstract borderBottomColor : string with get, set
        abstract borderBottomWidth : string with get, set
        abstract borderColor : string with get, set
        abstract borderLeftColor : string with get, set
        abstract borderLeftWidth : string with get, set
        abstract borderRadius : int with get, set
        abstract borderRightColor : string with get, set
        abstract borderRightWidth : string with get, set
        abstract borderTopColor : string with get, set
        abstract borderTopWidth : string with get, set
        abstract borderWidth : string with get, set
        abstract bottom : string with get, set
        abstract boxShadow : string with get, set
        abstract children : seq<ReactElement> with get, set
        abstract className : string with get, set
        abstract color : string with get, set
        abstract columns : int with get, set
        abstract content : string with get, set
        abstract cursor : string with get, set
        abstract ``data-testid`` : string with get, set
        abstract direction : string with get, set
        abstract disabled : bool with get, set
        abstract display : string with get, set
        abstract flex : int with get, set
        abstract flexBasis : int with get, set
        abstract flexDirection : string with get, set
        abstract fontFamily : string with get, set
        abstract fontSize : string with get, set
        abstract fontWeight : string with get, set
        abstract hasArrow : bool with get, set
        abstract height : string with get, set
        abstract href : string with get, set
        abstract icon : obj with get, set
        abstract id : string with get, set
        abstract index : int with get, set
        abstract isCentered : bool with get, set
        abstract isChecked : bool with get, set
        abstract isDisabled : bool with get, set
        abstract isExternal : bool with get, set
        abstract isLazy : bool with get, set
        abstract isOpen : bool with get, set
        abstract label : ReactElement with get, set
        abstract left : string with get, set
        abstract lineHeight : string with get, set
        abstract margin : string with get, set
        abstract marginBottom : string with get, set
        abstract marginLeft : string with get, set
        abstract marginRight : string with get, set
        abstract marginTop : string with get, set
        abstract maxHeight : string with get, set
        abstract maxWidth : string with get, set
        abstract minHeight : string with get, set
        abstract minWidth : string with get, set
        abstract onChange : (KeyboardEvent -> JS.Promise<unit>) with get, set
        abstract onClick : (MouseEvent -> JS.Promise<unit>) with get, set
        abstract onClose : (unit -> JS.Promise<unit>) with get, set
        abstract onKeyDown : (KeyboardEvent -> JS.Promise<unit>) with get, set
        abstract opacity : float with get, set
        abstract overflow : string with get, set
        abstract overflowX : string with get, set
        abstract overflowY : string with get, set
        abstract padding : string with get, set
        abstract paddingBottom : string with get, set
        abstract paddingLeft : string with get, set
        abstract paddingRight : string with get, set
        abstract paddingTop : string with get, set
        abstract placeholder : string with get, set
        abstract placement : string with get, set
        abstract position : string with get, set
        abstract ref : IRefValue<_> with get, set
        abstract right : string with get, set
        abstract size : string with get, set
        abstract spacing : string with get, set
        abstract textAlign : string with get, set
        abstract textOverflow : string with get, set
        abstract textShadow : string with get, set
        abstract theme : obj with get, set
        abstract top : string with get, set
        abstract transform : string with get, set
        abstract transformOrigin : string with get, set
        abstract ``type`` : string with get, set
        abstract value : obj with get, set
        abstract variant : string with get, set
        abstract width : string with get, set
        abstract whiteSpace : string with get, set
        abstract zIndex : int with get, set

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
                   Icon: obj
                   IconButton: obj
                   Input: obj
                   Link: obj
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
                   Popover: obj
                   PopoverArrow: obj
                   PopoverBody: obj
                   PopoverCloseButton: obj
                   PopoverContent: obj
                   PopoverTrigger: obj
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

    [<ImportAll "@chakra-ui/icons">]
    let icons : {| ExternalLinkIcon: obj |} = jsNative

    let composeChakraComponent cmp (props: IChakraProps -> unit) = composeComponent cmp (JS.newObj props)

    type ChakraInput<'T> = 'T -> unit

    module Icons =
        let externalLinkIcon<'T> = composeChakraComponent icons.ExternalLinkIcon

    let box<'T> = composeChakraComponent react.Box
    let button<'T> = composeChakraComponent react.Button
    let center<'T> = composeChakraComponent react.Center
    let checkbox<'T> = composeChakraComponent react.Checkbox
    let checkboxGroup<'T> = composeChakraComponent react.CheckboxGroup
    let circle<'T> = composeChakraComponent react.Circle
    let darkMode<'T> = composeChakraComponent react.DarkMode
    let flex<'T> = composeChakraComponent react.Flex
    let grid<'T> = composeChakraComponent react.Grid
    let hStack<'T> = composeChakraComponent react.HStack
    let icon<'T> = composeChakraComponent react.Icon
    let iconButton<'T> = composeChakraComponent react.IconButton
    let input<'T> = composeChakraComponent react.Input
    let link<'T> = composeChakraComponent react.Link
    let menu<'T> = composeChakraComponent react.Menu
    let menuButton<'T> = composeChakraComponent react.MenuButton
    let menuList<'T> = composeChakraComponent react.MenuList
    let menuItem<'T> = composeChakraComponent react.MenuItem
    let modal<'T> = composeChakraComponent react.Modal
    let modalBody<'T> = composeChakraComponent react.ModalBody
    let modalContent<'T> = composeChakraComponent react.ModalContent
    let modalCloseButton<'T> = composeChakraComponent react.ModalCloseButton
    let modalOverlay<'T> = composeChakraComponent react.ModalOverlay
    let numberInput<'T> = composeChakraComponent react.NumberInput
    let numberInputField<'T> = composeChakraComponent react.NumberInputField
    let numberInputStepper<'T> = composeChakraComponent react.NumberInputStepper
    let numberDecrementStepper<'T> = composeChakraComponent react.NumberDecrementStepper
    let numberIncrementStepper<'T> = composeChakraComponent react.NumberIncrementStepper
    let popover<'T> = composeChakraComponent react.Popover
    let popoverArrow<'T> = composeChakraComponent react.PopoverArrow
    let popoverBody<'T> = composeChakraComponent react.PopoverBody
    let popoverCloseButton<'T> = composeChakraComponent react.PopoverCloseButton
    let popoverContent<'T> = composeChakraComponent react.PopoverContent
    let popoverTrigger<'T> = composeChakraComponent react.PopoverTrigger
    let provider<'T> = composeChakraComponent react.ChakraProvider
    let simpleGrid<'T> = composeChakraComponent react.SimpleGrid
    let spacer<'T> = composeChakraComponent react.Spacer
    let spinner<'T> = composeChakraComponent react.Spinner
    let stack<'T> = composeChakraComponent react.Stack
    let tabList<'T> = composeChakraComponent react.TabList
    let tabPanel<'T> = composeChakraComponent react.TabPanel
    let tabPanels<'T> = composeChakraComponent react.TabPanels
    let tab<'T> = composeChakraComponent react.Tab
    let tabs<'T> = composeChakraComponent react.Tabs
    let tooltip<'T> = composeChakraComponent react.Tooltip


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
