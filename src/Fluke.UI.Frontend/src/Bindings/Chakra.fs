namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open Browser.Types
open Fable.Core
open Fluke.Shared
open Feliz


module Chakra =
    open React

    type IBreakpoints<'T> =
        abstract ``base`` : 'T with get, set
        abstract sm : 'T with get, set
        abstract md : 'T with get, set
        abstract lg : 'T with get, set
        abstract xl : 'T with get, set

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
        abstract background : string with get, set
        abstract backgroundColor : string with get, set
        abstract border : string with get, set
        abstract borderBottomColor : string with get, set
        abstract borderBottomWidth : string with get, set
        abstract borderColor : string with get, set
        abstract borderLeftColor : string with get, set
        abstract borderLeftWidth : string with get, set
        abstract borderRadius : string with get, set
        abstract borderTopRightRadius : string with get, set
        abstract borderBottomRightRadius : string with get, set
        abstract borderBottomLeftRadius : string with get, set
        abstract borderTopLeftRadius : string with get, set
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
        abstract closeOnMouseDown : bool with get, set
        abstract color : string with get, set
        abstract colorScheme : string with get, set
        abstract columns : int with get, set
        abstract computePositionOnMount : bool with get, set
        abstract content : string with get, set
        abstract cursor : string with get, set
        abstract defaultIndex : int [] with get, set
        abstract defaultIsOpen : bool with get, set
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
        abstract initialFocusRef : IRefValue<unit> with get, set
        abstract isCentered : bool with get, set
        abstract isChecked : bool with get, set
        abstract isDisabled : bool with get, set
        abstract isExternal : bool with get, set
        abstract isLazy : bool with get, set
        abstract isOpen : bool with get, set
        abstract isReadOnly : bool with get, set
        abstract justifyContent : string with get, set
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
        abstract onOpen : (unit -> unit) with get, set
        abstract opacity : float with get, set
        abstract overflow : string with get, set
        abstract overflowX : string with get, set
        abstract overflowY : string with get, set
        abstract padding : string with get, set
        abstract paddingBottom : string with get, set
        abstract paddingLeft : string with get, set
        abstract paddingRight : string with get, set
        abstract paddingTop : string with get, set
        abstract placeContent : string with get, set
        abstract placeholder : string with get, set
        abstract placement : string with get, set
        abstract portalProps : {| appendToParentPortal: bool |} with get, set
        abstract position : string with get, set
        abstract preventOverflow : bool with get, set
        abstract ref : IRefValue<_> with get, set
        abstract reduceMotion : bool with get, set
        abstract right : string with get, set
        abstract rightIcon : ReactElement with get, set
        abstract scrollBehavior : string with get, set
        abstract shouldWrapChildren : bool with get, set
        abstract size : string with get, set
        abstract spacing : string with get, set
        abstract tabIndex : int with get, set
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

    type IBreakpointsChakraProps =
        inherit IChakraProps
        abstract width : IBreakpoints<string> with get, set

    type Disclosure =
        {
            isOpen: bool
            onOpen: unit -> unit
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
                   InputGroup: obj
                   InputLeftElement: obj
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
                   Textarea: obj
                   Tooltip: obj
                   useDisclosure: unit -> Disclosure
                   useToast: unit -> System.Func<obj, unit> |} =
        jsNative

    [<ImportAll "@chakra-ui/theme-tools">]
    let themeTools : {| mode: string * string -> obj -> obj |} = jsNative

    [<ImportAll "@chakra-ui/icons">]
    let icons : {| ExternalLinkIcon: obj |} = jsNative

    let chakraMemo =
        React.memo
            (fun (input: {| Props: IChakraProps
                            Component: obj
                            Children: seq<ReactElement> |}) ->
                renderComponent input.Component input.Props input.Children)

    let inline renderChakraComponent (cmp: obj) (props: IChakraProps -> unit) (children: seq<ReactElement>) =
        let newProps = JS.newObj props

        //        chakraMemo
//            {|
//                Props = newProps
//                Component = cmp
//                Children = children
//            |}
        renderComponent cmp newProps children

    type ChakraInput<'T> = 'T -> unit

    module Icons =
        let externalLinkIcon<'T> = renderChakraComponent icons.ExternalLinkIcon

    let accordion<'T> = renderChakraComponent react.Accordion
    let accordionItem<'T> = renderChakraComponent react.AccordionItem
    let accordionButton<'T> = renderChakraComponent react.AccordionButton
    let accordionIcon<'T> = renderChakraComponent react.AccordionIcon
    let accordionPanel<'T> = renderChakraComponent react.AccordionPanel
    let box<'T> = renderChakraComponent react.Box
    let button<'T> = renderChakraComponent react.Button
    let center<'T> = renderChakraComponent react.Center
    let checkbox<'T> = renderChakraComponent react.Checkbox
    let checkboxGroup<'T> = renderChakraComponent react.CheckboxGroup
    let circle<'T> = renderChakraComponent react.Circle
    let darkMode<'T> = renderChakraComponent react.DarkMode
    let flex<'T> = renderChakraComponent react.Flex
    let grid<'T> = renderChakraComponent react.Grid
    let hStack<'T> = renderChakraComponent react.HStack
    let icon<'T> = renderChakraComponent react.Icon
    let iconButton<'T> = renderChakraComponent react.IconButton
    let input<'T> = renderChakraComponent react.Input
    let inputGroup<'T> = renderChakraComponent react.InputGroup
    let inputLeftElement<'T> = renderChakraComponent react.InputLeftElement
    let link<'T> = renderChakraComponent react.Link
    let menu<'T> = renderChakraComponent react.Menu
    let menuButton<'T> = renderChakraComponent react.MenuButton
    let menuList<'T> = renderChakraComponent react.MenuList
    let menuItem<'T> = renderChakraComponent react.MenuItem
    let menuItemOption<'T> = renderChakraComponent react.MenuItemOption
    let menuOptionGroup<'T> = renderChakraComponent react.MenuOptionGroup
    let modal<'T> = renderChakraComponent react.Modal
    let modalBody<'T> = renderChakraComponent react.ModalBody
    let modalContent<'T> = renderChakraComponent react.ModalContent
    let modalCloseButton<'T> = renderChakraComponent react.ModalCloseButton
    let modalOverlay<'T> = renderChakraComponent react.ModalOverlay
    let numberInput<'T> = renderChakraComponent react.NumberInput
    let numberInputField<'T> = renderChakraComponent react.NumberInputField
    let numberInputStepper<'T> = renderChakraComponent react.NumberInputStepper
    let numberDecrementStepper<'T> = renderChakraComponent react.NumberDecrementStepper
    let numberIncrementStepper<'T> = renderChakraComponent react.NumberIncrementStepper
    let popover<'T> = renderChakraComponent react.Popover
    let popoverArrow<'T> = renderChakraComponent react.PopoverArrow
    let popoverBody<'T> = renderChakraComponent react.PopoverBody
    let popoverCloseButton<'T> = renderChakraComponent react.PopoverCloseButton
    let popoverContent<'T> = renderChakraComponent react.PopoverContent
    let popoverTrigger<'T> = renderChakraComponent react.PopoverTrigger
    let provider<'T> = renderChakraComponent react.ChakraProvider
    let radio<'T> = renderChakraComponent react.Radio
    let radioGroup<'T> = renderChakraComponent react.RadioGroup
    let simpleGrid<'T> = renderChakraComponent react.SimpleGrid
    let spacer<'T> = renderChakraComponent react.Spacer
    let spinner<'T> = renderChakraComponent react.Spinner
    let stack<'T> = renderChakraComponent react.Stack
    let tabList<'T> = renderChakraComponent react.TabList
    let tabPanel<'T> = renderChakraComponent react.TabPanel
    let tabPanels<'T> = renderChakraComponent react.TabPanels
    let tab<'T> = renderChakraComponent react.Tab
    let tabs<'T> = renderChakraComponent react.Tabs
    let textarea<'T> = renderChakraComponent react.Textarea
    let tooltip<'T> = renderChakraComponent react.Tooltip


    type IToastProps =
        abstract title : string with get, set
        abstract status : string with get, set
        abstract description : string with get, set
        abstract duration : int with get, set
        abstract isClosable : bool with get, set

    let useToast () =
        let toast = react.useToast ()

        let toastFn (props: IToastProps -> unit) =
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

        match JS.window id with
        | Some window -> window?lastToast <- toastFn
        | None -> ()

        toastFn

    let mapIfSet fn value =
        value
        |> Option.ofObjUnbox
        |> Option.bind Option.ofObjUnbox
        |> Option.filter (
            string
            >> function
            | String.ValidString _ -> true
            | _ -> false
        )
        |> Option.map fn

    let transformShiftBy x y =
        $"translate({x |> Option.defaultValue 0.}px, {y |> Option.defaultValue 0.}px)"
