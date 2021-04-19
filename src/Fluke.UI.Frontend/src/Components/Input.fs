namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Feliz.Recoil
open Fable.Core


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
        inherit Chakra.IChakraProps

        abstract hint : ReactElement option with get, set
        abstract hintTitle : ReactElement option with get, set
        abstract atom : Recoil.InputAtom<'TValue, 'TKey> option with get, set
        abstract atomScope : Recoil.AtomScope option with get, set
        abstract value : 'TValue option with get, set
        abstract onFormat : ('TValue -> string) option with get, set
        abstract onValidate : (string -> 'TValue option) option with get, set
        abstract onEnterPress : (_ -> JS.Promise<unit>) option with get, set
        abstract inputFormat : InputFormat option with get, set

    [<ReactComponent>]
    let inline Input (input: IProps<'TValue, 'TKey>) =
        let atomFieldOptions = Recoil.useAtomField<'TValue, 'TKey> input.atom

        let atomValue =
            React.useMemo (
                (fun () ->
                    match input.atomScope with
                    | Some Recoil.AtomScope.ReadOnly -> atomFieldOptions.ReadOnlyValue
                    | _ -> atomFieldOptions.ReadWriteValue),
                [|
                    box input.atomScope
                    box atomFieldOptions.ReadOnlyValue
                    box atomFieldOptions.ReadWriteValue
                |]
            )

        let setAtomValue =
            React.useMemo (
                (fun () ->
                    match input.atomScope with
                    | Some Recoil.AtomScope.ReadOnly -> atomFieldOptions.SetReadOnlyValue
                    | _ -> atomFieldOptions.SetReadWriteValue),
                [|
                    box input.atomScope
                    box atomFieldOptions.SetReadOnlyValue
                    box atomFieldOptions.SetReadWriteValue
                |]
            )

        //        let (AtomFieldFamily atom) = input.atom
//        let resetAtom = Recoil.useResetState atom
//        let atom, setAtom = Recoil.useState atom
        let deviceInfo = Recoil.useValue Recoil.Selectors.deviceInfo

        let inputRef = React.useRef<HTMLInputElement> null

        let mounted, setMounted = React.useState false

        let _currentValue, currentValueString =
            React.useMemo (
                (fun () ->
                    let value =
                        match mounted, input.value with
                        | _, Some value -> Some value
                        | false, None -> None
                        | true, None ->
                            match inputRef.current, box atomValue with
                            | null, _ -> None
                            | _, null ->
                                match input.onValidate with
                                | Some onValidate -> onValidate inputRef.current.value
                                | None -> None
                            | _ ->
                                match input.atom with
                                | Some _ -> Some atomValue
                                | None -> None


                    let valueString =
                        match value with
                        | Some value when box value <> null ->
                            match input.onFormat with
                            | Some onFormat -> onFormat value
                            | None -> string value
                        | _ -> ""

                    value, valueString),
                [|
                    box mounted
                    box input.atom
                    box input.value
                    box input.onValidate
                    box input.onFormat
                    box inputRef
                    box atomValue
                |]
            )

        React.useEffect (
            (fun () ->
                if inputRef.current <> null then
                    inputRef.current.value <- currentValueString

                    if not mounted then
                        if input.atom.IsSome then
                            inputRef.current.dispatchEvent (Dom.createEvent "change" {| bubbles = true |})
                            |> ignore

                        setMounted true

                ),
            [|
                box input.atom
                box inputRef
                box currentValueString
                box mounted
                box setMounted
            |]
        )

        let onChange =
            Recoil.useCallbackRef
                (fun _setter (e: KeyboardEvent) ->
                    promise {
                        if inputRef.current <> null && e.target <> null then
                            match box input.onChange with
                            | null -> ()
                            | _ -> do! input.onChange e

                            let validValue =
                                match input.onValidate with
                                | Some onValidate ->
                                    let validValue = onValidate e.Value
                                    validValue
                                | None -> Some (box e.Value :?> 'TValue)

                            let validValueString =
                                match validValue with
                                | Some validValue ->
                                    match input.onFormat with
                                    | Some onFormat -> onFormat validValue
                                    | None -> string validValue
                                | None -> ""

                            if validValueString <> currentValueString then
                                inputRef.current.value <- validValueString

                            if input.atom.IsSome then
                                match validValue with
                                | Some value -> setAtomValue value
                                | None -> setAtomValue atomFieldOptions.ReadOnlyValue
                    })

        Chakra.stack
            (fun x -> x.spacing <- "5px")
            [

                GunBind.GunBind
                    {|
                        Atom =
                            match input.atomScope with
                            | Some Recoil.AtomScope.ReadOnly -> atomFieldOptions.AtomField.ReadOnly
                            | _ -> atomFieldOptions.AtomField.ReadWrite
                    |}

                if input.label <> null then
                    Chakra.flex
                        (fun _ -> ())
                        [
                            str $"{input.label}:"
                            Hint.Hint (
                                JS.newObj
                                    (fun x ->
                                        x.hint <- input.hint

                                        x.hintTitle <-
                                            Some (
                                                match input.hintTitle with
                                                | Some hintTitle -> hintTitle
                                                | None -> input.label
                                            ))
                            )
                        ]
                else
                    nothing

                Chakra.input
                    (fun x ->
                        x.onChange <- onChange
                        x.ref <- inputRef
                        x._focus <- JS.newObj (fun x -> x.borderColor <- "heliotrope")
                        x.autoFocus <- input.autoFocus
                        x.placeholder <- input.placeholder

                        x.onKeyDown <-
                            fun (e: KeyboardEvent) ->
                                promise {
                                    match box input.onKeyDown with
                                    | null -> ()
                                    | _ -> do! input.onKeyDown e


                                    match box input.onChange with
                                    | null -> ()
                                    | _ -> do! input.onChange e

                                    match input.onEnterPress with
                                    | Some onEnterPress -> if e.key = "Enter" then do! onEnterPress ()
                                    | None -> ()
                                }

                        x.``type`` <-
                            match input.inputFormat with
                            | Some inputFormat ->
                                match inputFormat with
                                | InputFormat.Date -> "date"
                                | InputFormat.Time -> "time"
                                | InputFormat.Number -> "number"
                                | InputFormat.DateTime -> "datetime-local"
                                | InputFormat.Email -> "email"
                                | InputFormat.Password -> "password"
                            | None -> null

                        x.paddingTop <-
                            match input.inputFormat, deviceInfo with
                            | Some InputFormat.Password, { IsEdge = true } -> "7px"
                            | _ -> null)
                    []
            ]
