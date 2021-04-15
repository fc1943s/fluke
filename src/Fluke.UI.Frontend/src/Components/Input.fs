namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Fluke.Shared
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


    type IProps<'TValue, 'TKey> =
        abstract label : string with get, set
        abstract hint : ReactElement option with get, set
        abstract hintTitle : ReactElement option with get, set
        abstract autoFocus : bool with get, set
        abstract placeholder : string with get, set
        abstract atom : Recoil.InputAtom<'TValue, 'TKey> option with get, set
        abstract atomScope : Recoil.AtomScope option with get, set
        abstract value : 'TValue option with get, set
        abstract onFormat : ('TValue -> string) option with get, set
        abstract onValidate : (string -> 'TValue option) option with get, set
        abstract onEnterPress : (unit -> JS.Promise<unit>) option with get, set
        abstract onChange : (KeyboardEvent -> JS.Promise<unit>) option with get, set
        abstract onKeyDown : (KeyboardEvent -> JS.Promise<unit>) option with get, set
        abstract inputFormat : InputFormat option with get, set

    [<ReactComponent>]
    let Input (input: IProps<'TValue, 'TKey>) =
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
                        | Some value ->
                            match input.onFormat with
                            | Some onFormat -> onFormat value
                            | None -> string value
                        | None -> ""

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
                        if inputRef.current <> null then
                            match input.onChange with
                            | Some onChange -> do! onChange e
                            | None -> ()

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
            {| spacing = "5px" |}
            [
                match input.label with
                | String.ValidString ->
                    Chakra.flex
                        {|  |}
                        [
                            str $"{input.label}:"
                            Hint.Hint (
                                Dom.newObj
                                    (fun x ->
                                        x.hint <- input.hint

                                        x.hintTitle <-
                                            Some (
                                                match input.hintTitle with
                                                | Some hintTitle -> hintTitle
                                                | None -> str input.label
                                            ))
                            )
                        ]
                | _ -> ()

                Chakra.input
                    {|
                        autoFocus = input.autoFocus
                        placeholder = input.placeholder
                        ref = inputRef
                        onChange = onChange
                        onKeyDown =
                            fun (e: KeyboardEvent) ->
                                promise {
                                    match input.onKeyDown with
                                    | Some onKeyDown -> do! onKeyDown e
                                    | None -> ()

                                    match input.onEnterPress with
                                    | Some onEnterPress ->
                                        if e.key = "Enter" then
                                            do! onEnterPress ()
                                    | None -> ()
                                }
                        ``type`` =
                            match input.inputFormat with
                            | Some inputFormat ->
                                match inputFormat with
                                | InputFormat.Date -> "date"
                                | InputFormat.Time -> "time"
                                | InputFormat.DateTime -> "datetime-local"
                                | InputFormat.Email -> "email"
                                | InputFormat.Password -> "password"
                                |> Some
                            | None -> None
                        paddingTop =
                            match input.inputFormat, deviceInfo with
                            | Some InputFormat.Password, { IsEdge = true } -> Some "7px"
                            | _ -> None
                    |}
                    []
            ]
