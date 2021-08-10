namespace FsStore

open Fable.Core
open FsStore.Model
open FsStore.Store
open Microsoft.FSharp.Core.Operators
open Feliz
open FsCore
open FsJs
open FsStore.Bindings
open FsStore.Bindings.Jotai


[<AutoOpen>]
module HooksMagic =
    module Store =
        let inline useAtom atom = jotai.useAtom atom // AtomScope.Current

        let inline useStateOption (atom: Atom<'TValue5> option) =
            let flatAtom =
                React.useMemo (
                    (fun () ->
                        match atom with
                        | Some atom -> atom
                        | None -> emptyAtom :?> Atom<'TValue5>),
                    [|
                        box atom
                    |]
                )

            let value, setValue = useAtom flatAtom

            (if atom.IsNone then None else Some value), (if atom.IsNone then (fun _ -> ()) else setValue)

        let useTempAtom<'TValue7> (atom: InputAtom<'TValue7> option) (inputScope: InputScope<'TValue7> option) =
            let currentAtomField, tempAtomField =
                React.useMemo (
                    (fun () ->
                        let atomField = getAtomField atom (InputScope.AtomScope inputScope)
                        atomField.Current, atomField.Temp),
                    [|
                        box atom
                        box inputScope
                    |]
                )

            let currentValue, setCurrentValue = useStateOption currentAtomField
            let tempValue, setTempValue = useStateOption tempAtomField

            React.useMemo (
                (fun () ->
                    let defaultJsonEncode, _defaultJsonDecode = unbox Gun.defaultSerializer

                    let newTempValue =
                        match inputScope, tempValue |> Option.defaultValue null with
                        | _, tempValue when tempValue = ___emptyTempAtom -> unbox null
                        | _, null -> currentValue |> Option.defaultValue (unbox null)
                        | Some (InputScope.Temp (_, jsonDecode)), tempValue ->
                            try
                                Dom.log
                                    (fun () ->
                                        $"useTempAtom
                                    currentValue={currentValue}
                                    atom={atom}
                                    tempValue={tempValue}")

                                jsonDecode tempValue
                            with
                            | ex ->
                                printfn $"Error decoding tempValue={tempValue} ex={ex}"

                                currentValue
                                |> Option.defaultValue (unbox tempValue)
                        | _ ->
                            currentValue
                            |> Option.defaultValue (unbox tempValue)

                    let setTempValue =
                        if atom.IsSome then
                            (fun newValue ->
                                setTempValue (
                                    match box newValue with
                                    | null -> ___emptyTempAtom
                                    | _ ->
                                        match inputScope with
                                        | Some (InputScope.Temp (jsonEncode, _)) -> jsonEncode newValue
                                        | _ -> defaultJsonEncode newValue
                                ))
                        else
                            (fun _ -> printfn "empty set #1")

                    let setCurrentValue =
                        if atom.IsSome then
                            setCurrentValue
                        else
                            (fun _ -> printfn "empty set #2")

                    {|
                        Value =
                            match inputScope with
                            | Some (InputScope.Temp _) -> newTempValue
                            | _ -> currentValue |> Option.defaultValue (unbox null)
                        SetValue =
                            match inputScope with
                            | Some (InputScope.Temp _) -> setTempValue
                            | _ -> setCurrentValue
                        CurrentValue = currentValue |> Option.defaultValue (unbox null)
                        SetCurrentValue = setCurrentValue
                        TempValue = newTempValue
                        SetTempValue = setTempValue
                    |}),
                [|
                    box inputScope
                    box atom
                    box currentValue
                    box tempValue
                    box setCurrentValue
                    box setTempValue
                |]
            )

        let inline useValue atom = jotaiUtils.useAtomValue atom

        let inline useValueTuple a b =
            let a = useValue a
            let b = useValue b
            a, b

        let useCallbackRef (fn: GetFn -> SetFn -> 'a -> JS.Promise<'c>) : ('a -> JS.Promise<'c>) =
            let fnCallback = React.useCallbackRef (fun (getter, setter, arg) -> fn getter setter arg)

            let atom =
                React.useMemo (
                    (fun () ->
                        jotai.atom (
                            unbox null,
                            Some
                                (fun getter setter (arg, resolve, err) ->
                                    try
                                        resolve (fnCallback (getter, setter, arg))
                                    with
                                    | ex ->
                                        printfn $"atomCallback fn error: {ex}"
                                        err ex

                                    ())
                        )),
                    [|
                        box fnCallback
                    |]
                )

            let _value, setValue = useAtom atom

            let useAtomCallback =
                React.useCallback (
                    (fun arg ->
                        Promise.create (fun resolve err -> setValue (arg, resolve, err))
                        |> Promise.bind id),
                    [|
                        box setValue
                    |]
                )

            useAtomCallback

        let inline useCallbacks () =
            useCallbackRef (fun getter setter () -> promise { return (getter, setter) })


        let inline useState atom = useAtom atom

        let inline useSetState atom = jotaiUtils.useUpdateAtom atom

//    let inline useSetStatePrev<'T> atom =
//        let setter = jotaiUtils.useUpdateAtom<'T> atom
//        fun (value: 'T -> 'T) -> setter (unbox value)

