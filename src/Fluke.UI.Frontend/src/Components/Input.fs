namespace Fluke.UI.Frontend.Components

open System
open Browser.Types
open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.Shared
open Fluke.UI.Frontend.State


module Input =

    [<RequireQualifiedAccess>]
    type InputFormat =
        | Date
        | Time
        | DateTime
        | Email
        | Password
        | Number

    type IProps<'TValue, 'TKey> =
        abstract hint : ReactElement option with get, set
        abstract textarea : bool with get, set
        abstract variableHeight : bool with get, set
        abstract autoFocusOnAllMounts : bool with get, set
        abstract hintTitle : ReactElement option with get, set
        abstract atom : Store.InputAtom<'TValue> option with get, set
        abstract inputScope : Store.InputScope<'TValue> option with get, set
        abstract fixedValue : 'TValue option with get, set
        abstract onFormat : ('TValue -> string) option with get, set
        abstract onValidate : (string * 'TValue option -> 'TValue option) option with get, set
        abstract onEnterPress : (_ -> JS.Promise<unit>) option with get, set
        abstract inputFormat : InputFormat option with get, set
        abstract rightButton : ReactElement option with get, set


    [<ReactComponent>]
    let Input
        (input: {| CustomProps: IProps<'TValue, 'TKey> -> unit
                   Props: Chakra.IChakraProps -> unit |})
        =
        let darkMode = Store.useValue Atoms.User.darkMode
        let fontSize = Store.useValue Atoms.User.fontSize

        let props, customProps =
            React.useMemo (
                (fun () -> JS.newObj input.Props, JS.newObj input.CustomProps),
                [|
                    box input.CustomProps
                    box input.Props
                |]
            )

        let inputFallbackRef = React.useRef<HTMLInputElement> null

        let inputRef =
            React.useMemo (
                (fun () ->
                    props.ref
                    |> Option.ofObjUnbox
                    |> Option.defaultValue inputFallbackRef),
                [|
                    box props.ref
                    box inputFallbackRef
                |]
            )

        let tempAtom = Store.Hooks.useTempAtom<'TValue> customProps.atom customProps.inputScope

        let mounted, setMounted = React.useState false

        let currentValue, currentValueString =
            React.useMemo (
                (fun () ->
                    let value =
                        match mounted, customProps.fixedValue with
                        | _, Some value -> Some value
                        | false, None -> None
                        | true, None ->
                            match inputRef.current, box tempAtom.Value with
                            | null, _ -> None
                            | _, null ->
                                match customProps.onValidate with
                                | Some onValidate -> onValidate (inputRef.current.value, Some tempAtom.Value)
                                | None -> None
                            | _ ->
                                match customProps.atom with
                                | Some _ -> Some tempAtom.Value
                                | None -> None

                    let valueString =
                        match value with
                        | Some value when box value <> null ->
                            match customProps.onFormat with
                            | Some onFormat -> onFormat value
                            | None -> string value
                        | _ -> ""

                    value, valueString),
                [|
                    box mounted
                    box customProps.fixedValue
                    box customProps.onValidate
                    box customProps.onFormat
                    box customProps.atom
                    box inputRef
                    box tempAtom.Value
                |]
            )

        let fireChange =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        inputRef.current.dispatchEvent (Dom.createEvent "change" {| bubbles = true |})
                        |> ignore
                    }),
                [|
                    box inputRef
                |]
            )

        let alreadyFocused, setAlreadyFocused = React.useState false

        React.useEffect (
            (fun () ->
                match inputRef.current with
                | null -> ()
                | _ ->
                    inputRef.current.value <- currentValueString

                    if not alreadyFocused then
                        if props.autoFocus
                           && (not mounted || customProps.autoFocusOnAllMounts) then
                            promise {
                                inputRef.current.focus ()
                                setAlreadyFocused true
                            }
                            |> Promise.start

                    if not mounted then
                        setMounted true
                        if customProps.atom.IsSome then fireChange () |> Promise.start),
            [|
                box alreadyFocused
                box setAlreadyFocused
                box fireChange
                box props
                box customProps
                box inputRef
                box currentValueString
                box mounted
                box setMounted
            |]
        )

        let onChange =
            Store.useCallback (
                (fun _ _ (e: KeyboardEvent) ->
                    promise {
                        if inputRef.current <> null && e.target <> null then
                            match box props.onChange with
                            | null -> ()
                            | _ -> do! props.onChange e

                            let validValue =
                                match customProps.onValidate with
                                | Some onValidate ->
                                    let validValue = onValidate (e.Value, currentValue)
                                    validValue
                                | None -> Some (box e.Value :?> 'TValue)

                            let validValueString =
                                match validValue with
                                | Some validValue ->
                                    match customProps.onFormat with
                                    | Some onFormat -> onFormat validValue
                                    | None -> string validValue
                                | None -> ""

                            if validValueString <> currentValueString then
                                inputRef.current.value <- validValueString

                            if customProps.atom.IsSome then
                                match validValue with
                                | Some value -> tempAtom.SetValue value
                                | None -> tempAtom.SetValue tempAtom.ReadOnlyValue
                    }),
                [|
                    box props
                    box tempAtom
                    box currentValue
                    box currentValueString
                    box inputRef
                    box customProps.onFormat
                    box customProps.onValidate
                    box customProps.atom
                |]
            )

        let variableHeight =
            React.useMemo (
                (fun () ->
                    if not customProps.variableHeight then
                        None
                    else
                        currentValueString
                        |> Seq.map string
                        |> Seq.filter ((=) Environment.NewLine)
                        |> Seq.length
                        |> (+) 2
                        |> (*) fontSize
                        |> float
                        |> (*) 1.35
                        |> fun n -> $"{int n}px"
                        |> Some),
                [|
                    box customProps.variableHeight
                    box currentValueString
                    box fontSize
                |]
            )

        Chakra.stack
            (fun x ->
                x.spacing <- "5px"
                x.flex <- "1")
            [
                match props.label with
                | null -> nothing
                | _ ->
                    InputLabel.InputLabel
                        {|
                            Hint = customProps.hint
                            HintTitle = customProps.hintTitle
                            Label = props.label
                            Props = fun _ -> ()
                        |}

                Chakra.box
                    (fun x ->
                        x.position <- "relative"
                        x.flex <- "1")
                    [
                        (if customProps.textarea then Chakra.textarea else Chakra.input)
                            (fun x ->
                                x.onChange <- onChange
                                x.ref <- inputRef
                                x._focus <- JS.newObj (fun x -> x.borderColor <- "heliotrope")
                                x.borderColor <- if darkMode then "#484848" else "#b7b7b7"
                                x.backgroundColor <- "gray.10"
                                x.paddingBottom <- "2px"

                                match variableHeight with
                                | Some variableHeight -> x.height <- variableHeight
                                | None -> ()

                                if customProps.textarea then x.paddingTop <- "6px"

                                x.onKeyDown <-
                                    fun (e: KeyboardEvent) ->
                                        promise {
                                            match box props.onKeyDown with
                                            | null -> ()
                                            | _ -> do! props.onKeyDown e

                                            match customProps.onEnterPress with
                                            | Some onEnterPress -> if e.key = "Enter" then do! onEnterPress e
                                            | None -> ()
                                        }

                                x.``type`` <-
                                    match customProps.inputFormat with
                                    | Some inputFormat ->
                                        match inputFormat with
                                        | InputFormat.Date -> "date"
                                        | InputFormat.Time -> "time"
                                        | InputFormat.Number -> "number"
                                        | InputFormat.DateTime -> "datetime-local"
                                        | InputFormat.Email -> "email"
                                        | InputFormat.Password -> "password"
                                    | None -> null

                                input.Props x)
                            []

                        let rightButton =
                            match customProps.rightButton, customProps.inputFormat with
                            | Some rightButton, _ -> Some rightButton
                            | _, Some InputFormat.Number ->
                                let numberButtonClick (value: string) (op: float -> float) =
                                    match Double.TryParse value with
                                    | true, value ->
                                        match customProps.onValidate with
                                        | Some onValidate ->
                                            match onValidate (string (op value), currentValue) with
                                            | Some value ->
                                                inputRef.current.valueAsNumber <-
                                                    match customProps.onFormat with
                                                    | Some onFormat -> onFormat value |> unbox
                                                    | None -> unbox value
                                            | None -> ()
                                        | None -> inputRef.current.valueAsNumber <- op value
                                    | _ -> ()

                                React.fragment [
                                    Button.Button
                                        {|
                                            Hint = None
                                            Icon = Some (Icons.fa.FaSortUp |> Icons.wrap, Button.IconPosition.Left)
                                            Props =
                                                fun x ->
                                                    x.height <- "50%"
                                                    x.paddingTop <- "6px"
                                                    x.borderRadius <- "0 5px 0 0"
                                                    x.minWidth <- "26px"

                                                    x.onClick <-
                                                        (fun _ ->
                                                            promise {
                                                                numberButtonClick inputRef.current.value ((+) 1.)
                                                                do! fireChange ()
                                                            })
                                            Children = []
                                        |}

                                    Button.Button
                                        {|
                                            Hint = None
                                            Icon = Some (Icons.fa.FaSortDown |> Icons.wrap, Button.IconPosition.Left)
                                            Props =
                                                fun x ->
                                                    x.height <- "50%"
                                                    x.paddingBottom <- "6px"
                                                    x.borderRadius <- "0 0 5px 0"
                                                    x.minWidth <- "26px"

                                                    x.onClick <-
                                                        (fun _ ->
                                                            promise {
                                                                numberButtonClick
                                                                    inputRef.current.value
                                                                    (fun n -> n - 1.)

                                                                do! fireChange ()
                                                            })
                                            Children = []
                                        |}
                                ]
                                |> Some
                            | _ -> None

                        match rightButton with
                        | Some rightButton ->
                            Chakra.stack
                                (fun x ->
                                    x.position <- "absolute"
                                    x.right <- "1px"
                                    x.top <- "1px"
                                    x.bottom <- "2px"
                                    x.borderLeftWidth <- "1px"
                                    x.borderLeftColor <- if darkMode then "#484848" else "#b7b7b7"
                                    x.spacing <- "1px")
                                [
                                    rightButton
                                ]
                        | _ -> nothing
                    ]
            ]

    let inline LeftIconInput
        (input: {| Icon: ReactElement
                   CustomProps: IProps<'TValue, 'TKey> -> unit
                   Props: Chakra.IChakraProps -> unit |})
        =
        Chakra.flex
            (fun x ->
                x.flex <- "1"
                x.position <- "relative")
            [
                Chakra.flex
                    (fun x ->
                        x.zIndex <- 1
                        x.position <- "absolute"
                        x.left <- "9px"
                        x.height <- "calc(var(--chakra-fontSizes-main) * 2.45)"
                        x.alignItems <- "center")
                    [
                        input.Icon
                    ]

                Input
                    {|
                        CustomProps = input.CustomProps
                        Props =
                            fun x ->
                                x.paddingLeft <- "28px"
                                input.Props x
                    |}
            ]
