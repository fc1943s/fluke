namespace Fluke.UI.Frontend.Bindings

open Browser.Types
open System
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
        abstract alignItems : string with get, set
        abstract alignSelf : string with get, set
        abstract allowMultiple : bool with get, set
        abstract autoFocus : bool with get, set
        abstract backgroundColor : string with get, set
        abstract border : string with get, set
        abstract borderBottomColor : string with get, set
        abstract borderBottomWidth : string with get, set
        abstract borderColor : string with get, set
        abstract borderLeftColor : string with get, set
        abstract borderLeftWidth : string with get, set
        abstract borderRadius : string with get, set
        abstract borderRightColor : string with get, set
        abstract borderRightWidth : string with get, set
        abstract borderTopColor : string with get, set
        abstract borderTopWidth : string with get, set
        abstract borderWidth : string with get, set
        abstract bottom : string with get, set
        abstract boxShadow : string with get, set
        abstract children : seq<ReactElement> with get, set
        abstract className : string with get, set
        abstract closeOnSelect : bool with get, set
        abstract closeOnBlur : bool with get, set
        abstract color : string with get, set
        abstract colorScheme : string with get, set
        abstract columns : int with get, set
        abstract content : string with get, set
        abstract cursor : string with get, set
        abstract defaultIndex : int [] with get, set
        abstract defaultValue : obj with get, set
        abstract direction : string with get, set
        abstract disabled : bool with get, set
        abstract display : string with get, set
        abstract flex : string with get, set
        abstract flexBasis : int with get, set
        abstract flexDirection : string with get, set
        abstract flip : bool with get, set
        abstract fontFamily : string with get, set
        abstract fontSize : string with get, set
        abstract fontWeight : string with get, set
        abstract hasArrow : bool with get, set
        abstract height : string with get, set
        abstract href : string with get, set
        abstract icon : obj with get, set
        abstract id : string with get, set
        abstract index : obj with get, set
        abstract isCentered : bool with get, set
        abstract isChecked : bool with get, set
        abstract isDisabled : bool with get, set
        abstract isExternal : bool with get, set
        abstract isLazy : bool with get, set
        abstract isOpen : bool with get, set
        abstract justifyItems : string with get, set
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
        abstract minChildWidth : string with get, set
        abstract minHeight : string with get, set
        abstract minWidth : string with get, set
        abstract onChange : (_ -> JS.Promise<unit>) with get, set
        abstract onClick : (MouseEvent -> JS.Promise<unit>) with get, set
        abstract onClose : (unit -> JS.Promise<unit>) with get, set
        abstract onKeyDown : (KeyboardEvent -> JS.Promise<unit>) with get, set
        abstract onOpen : (unit -> JS.Promise<unit>) with get, set
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
        abstract preventOverflow : bool with get, set
        abstract ref : IRefValue<_> with get, set
        abstract reduceMotion : bool with get, set
        abstract right : string with get, set
        abstract rightIcon : ReactElement with get, set
        abstract scrollBehavior : string with get, set
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
        abstract visibility : string with get, set
        abstract width : string with get, set
        abstract whiteSpace : string with get, set
        abstract zIndex : int with get, set

    type Disclosure =
        {
            isOpen: bool
            onOpen: unit -> JS.Promise<unit>
            onClose: unit -> unit
        }

    [<ImportAll "@chakra-ui/react">]
    let react : {| Accordion: obj
                   AccordionItem: obj
                   AccordionButton: obj
                   AccordionIcon: obj
                   AccordionPanel: obj
                   Box: obj
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
                   MenuItemOption: obj
                   MenuOptionGroup: obj
                   Modal: obj
                   ModalBody: obj
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
                   Radio: obj
                   RadioGroup: obj
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
                   useDisclosure: unit -> Disclosure
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

    let accordion<'T> = composeChakraComponent react.Accordion
    let accordionItem<'T> = composeChakraComponent react.AccordionItem
    let accordionButton<'T> = composeChakraComponent react.AccordionButton
    let accordionIcon<'T> = composeChakraComponent react.AccordionIcon
    let accordionPanel<'T> = composeChakraComponent react.AccordionPanel
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
    let menuItemOption<'T> = composeChakraComponent react.MenuItemOption
    let menuOptionGroup<'T> = composeChakraComponent react.MenuOptionGroup
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
    let radio<'T> = composeChakraComponent react.Radio
    let radioGroup<'T> = composeChakraComponent react.RadioGroup
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


    type IToastProps =
        abstract title : string with get, set
        abstract status : string with get, set
        abstract description : string with get, set
        abstract duration : int with get, set
        abstract isClosable : bool with get, set

    let useToast () =
        let toast = react.useToast ()

        fun (props: IToastProps -> unit) ->
            toast.Invoke (
                JS.newObj
                    (fun (x: IToastProps) ->
                        x.title <- "Error"
                        x.status <- "error"
                        x.description <- "Error"
                        x.duration <- 4000
                        x.isClosable <- true
                        props x)
            )

    let trySetProp fn value =
        match value |> Option.ofObj with
        | Some value when
            value <> null
            && value
               |> string
               |> String.IsNullOrWhiteSpace
               |> not -> fn value
        | _ -> ()

    let transformShiftBy x y =
        $"translate({x |> Option.defaultValue 0}px, {y |> Option.defaultValue 0}px)"
