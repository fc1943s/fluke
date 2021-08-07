namespace FsUi.Bindings

open FsCore
open FsJs
open Fable.Core.JsInterop
open Browser.Types
open Fable.Core
open Fable.React
open Feliz


module UI =
    open React

    type IBreakpoints<'T> =
        abstract ``base`` : 'T with get, set
        abstract sm : 'T with get, set
        abstract md : 'T with get, set
        abstract lg : 'T with get, set
        abstract xl : 'T with get, set

    type IChakraProps =
        abstract _active : IChakraProps with get, set
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
        abstract flexFlow : string with get, set
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
        abstract key : string with get, set
        abstract label : ReactElement with get, set
        abstract left : string with get, set
        abstract lineHeight : string with get, set
        abstract margin : string with get, set
        abstract marginBottom : string with get, set
        abstract marginLeft : string with get, set
        abstract marginRight : string with get, set
        abstract marginTop : string with get, set
        abstract max : int with get, set
        abstract maxHeight : string with get, set
        abstract maxWidth : string with get, set
        abstract min : int with get, set
        abstract minChildWidth : string with get, set
        abstract minHeight : string with get, set
        abstract minWidth : string with get, set
        abstract onChange : (_ -> JS.Promise<unit>) with get, set
        abstract onClick : (MouseEvent -> JS.Promise<unit>) with get, set
        abstract onClose : (unit -> JS.Promise<unit>) with get, set
        abstract onFocus : (_ -> JS.Promise<unit>) with get, set
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
        abstract src : string with get, set
        abstract style : IChakraProps with get, set
        abstract tabIndex : int with get, set
        abstract textAlign : string with get, set
        abstract textOverflow : string with get, set
        abstract textShadow : string with get, set
        abstract theme : obj with get, set
        abstract title : string with get, set
        abstract top : string with get, set
        abstract transform : string with get, set
        abstract transformOrigin : string with get, set
        abstract ``type`` : string with get, set
        abstract userSelect : string with get, set
        abstract value : obj with get, set
        abstract variant : string with get, set
        abstract verticalAlign : string with get, set
        abstract visibility : string with get, set
        abstract WebkitAppRegion : string with get, set
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
    let react: {| Accordion: obj
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
                  Image: obj
                  Input: obj
                  InputGroup: obj
                  InputLeftElement: obj
                  LightMode: obj
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
                  Slider: obj
                  SliderTrack: obj
                  SliderFilledTrack: obj
                  SliderThumb: obj
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
                  useMenuContext: unit -> obj
                  useMenuButton: obj -> IChakraProps
                  useToast: unit -> System.Func<obj, unit> |} =
        jsNative

    [<ImportAll "@chakra-ui/theme-tools">]
    let themeTools: {| mode: string * string -> obj -> obj |} = jsNative

    //    let chakraMemo =
//        React.memo
//            (fun (input: {| Props: IChakraProps
//                            Component: obj
//                            Children: seq<ReactElement> |}) ->
//                renderComponent input.Component input.Props input.Children)





    //    [<ReactComponent>]
//    let MemoChakraComponent (cmp: obj) (props: IChakraProps -> unit) (children: seq<ReactElement>) =
//        let newProps, children =
//            React.useMemo (
//                (fun () -> JS.newObj props, children),
//                [|
//                    box props
//                    box children
//                |]
//            )
//
//        renderComponent cmp newProps children

    let inline renderChakraComponent (cmp: obj) (props: IChakraProps -> unit) children =
        renderComponent cmp (JS.newObj props) children

    //        chakraMemo
//            {|
//                Props = newProps
//                Component = cmp
//                Children = children
//            |}

    type ChakraInput<'T> = 'T -> unit

    let inline accordion<'T> props children =
        renderChakraComponent react.Accordion props children

    let inline accordionItem<'T> props children =
        renderChakraComponent react.AccordionItem props children

    let inline accordionButton<'T> props children =
        renderChakraComponent react.AccordionButton props children

    let inline accordionIcon<'T> props children =
        renderChakraComponent react.AccordionIcon props children

    let inline accordionPanel<'T> props children =
        renderChakraComponent react.AccordionPanel props children

    let inline box<'T> props children =
        renderChakraComponent react.Box props children

    let inline button<'T> props children =
        renderChakraComponent react.Button props children

    let inline center<'T> props children =
        renderChakraComponent react.Center props children

    let inline checkbox<'T> props children =
        renderChakraComponent react.Checkbox props children

    let inline checkboxGroup<'T> props children =
        renderChakraComponent react.CheckboxGroup props children

    let inline circle<'T> props children =
        renderChakraComponent react.Circle props children

    let inline darkMode<'T> props children =
        renderChakraComponent react.DarkMode props children

    let inline flex<'T> props children =
        renderChakraComponent react.Flex props children

    let inline grid<'T> props children =
        renderChakraComponent react.Grid props children

    let inline hStack<'T> props children =
        renderChakraComponent react.HStack props children

    let inline icon<'T> props children =
        renderChakraComponent react.Icon props children

    let inline iconButton<'T> props children =
        renderChakraComponent react.IconButton props children

    let inline image<'T> props children =
        renderChakraComponent react.Image props children

    let inline input<'T> props children =
        renderChakraComponent react.Input props children

    let inline inputGroup<'T> props children =
        renderChakraComponent react.InputGroup props children

    let inline inputLeftElement<'T> props children =
        renderChakraComponent react.InputLeftElement props children

    let inline lightMode<'T> props children =
        renderChakraComponent react.LightMode props children

    let inline link<'T> props children =
        renderChakraComponent react.Link props children

    let inline menu<'T> props children =
        renderChakraComponent react.Menu props children

    let inline menuButton<'T> props children =
        renderChakraComponent react.MenuButton props children

    let inline menuList<'T> props children =
        renderChakraComponent react.MenuList props children

    let inline menuItem<'T> props children =
        renderChakraComponent react.MenuItem props children

    let inline menuItemOption<'T> props children =
        renderChakraComponent react.MenuItemOption props children

    let inline menuOptionGroup<'T> props children =
        renderChakraComponent react.MenuOptionGroup props children

    let inline modal<'T> props children =
        renderChakraComponent react.Modal props children

    let inline modalBody<'T> props children =
        renderChakraComponent react.ModalBody props children

    let inline modalContent<'T> props children =
        renderChakraComponent react.ModalContent props children

    let inline modalCloseButton<'T> props children =
        renderChakraComponent react.ModalCloseButton props children

    let inline modalOverlay<'T> props children =
        renderChakraComponent react.ModalOverlay props children

    let inline numberInput<'T> props children =
        renderChakraComponent react.NumberInput props children

    let inline numberInputField<'T> props children =
        renderChakraComponent react.NumberInputField props children

    let inline numberInputStepper<'T> props children =
        renderChakraComponent react.NumberInputStepper props children

    let inline numberDecrementStepper<'T> props children =
        renderChakraComponent react.NumberDecrementStepper props children

    let inline numberIncrementStepper<'T> props children =
        renderChakraComponent react.NumberIncrementStepper props children

    let inline popover<'T> props children =
        renderChakraComponent react.Popover props children

    let inline popoverArrow<'T> props children =
        renderChakraComponent react.PopoverArrow props children

    let inline popoverBody<'T> props children =
        renderChakraComponent react.PopoverBody props children

    let inline popoverCloseButton<'T> props children =
        renderChakraComponent react.PopoverCloseButton props children

    let inline popoverContent<'T> props children =
        renderChakraComponent react.PopoverContent props children

    let inline popoverTrigger<'T> props children =
        renderChakraComponent react.PopoverTrigger props children

    let inline provider<'T> props children =
        renderChakraComponent react.ChakraProvider props children

    let inline radio<'T> props children =
        renderChakraComponent react.Radio props children

    let inline radioGroup<'T> props children =
        renderChakraComponent react.RadioGroup props children

    let inline simpleGrid<'T> props children =
        renderChakraComponent react.SimpleGrid props children

    let inline slider<'T> props children =
        renderChakraComponent react.Slider props children

    let inline sliderTrack<'T> props children =
        renderChakraComponent react.SliderTrack props children

    let inline sliderFilledTrack<'T> props children =
        renderChakraComponent react.SliderFilledTrack props children

    let inline sliderThumb<'T> props children =
        renderChakraComponent react.SliderThumb props children

    let inline spacer<'T> props children =
        renderChakraComponent react.Spacer props children

    let inline spinner<'T> props children =
        renderChakraComponent react.Spinner props children

    let inline stack<'T> props children =
        renderChakraComponent react.Stack props children

    let inline tabList<'T> props children =
        renderChakraComponent react.TabList props children

    let inline tabPanel<'T> props children =
        renderChakraComponent react.TabPanel props children

    let inline tabPanels<'T> props children =
        renderChakraComponent react.TabPanels props children

    let inline tab<'T> props children =
        renderChakraComponent react.Tab props children

    let inline tabs<'T> props children =
        renderChakraComponent react.Tabs props children

    let inline textarea<'T> props children =
        renderChakraComponent react.Textarea props children

    let inline tooltip<'T> props children =
        renderChakraComponent react.Tooltip props children

    let inline str text =
        box
            (fun _ -> ())
            [
                str text
            ]

    type IToastProps =
        abstract title : string with get, set
        abstract status : string with get, set
        abstract description : string with get, set
        abstract duration : int with get, set
        abstract isClosable : bool with get, set

    let inline useDisclosure () =
        react.useDisclosure ()

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

        match Dom.window () with
        | Some window -> window?lastToast <- toastFn
        | None -> ()

        toastFn

    let inline mapIfSet fn value =
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


    let inline setTestId (props: IChakraProps) (value: obj) =
        if Dom.deviceInfo.IsTesting then props?``data-testid`` <- value

    let inline transformShiftBy x y =
        $"translate({x |> Option.defaultValue 0.}px, {y |> Option.defaultValue 0.}px)"
