namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open FsJs
open FsUi.Bindings
open FsUi.Components


module InputList =
    [<ReactComponent>]
    let InputList inputProps inputCustomProps atomValue setAtomValue =
        let inputList, (inputProps: Ui.IChakraProps) =
            React.useMemo (
                (fun () ->
                    let inputList =
                        match atomValue with
                        | [||] ->
                            [|
                                ""
                            |]
                        | inputList -> inputList

                    inputList, Js.newObj inputProps),
                [|
                    box atomValue
                    box inputProps
                |]
            )

        Ui.stack
            (fun x -> x.spacing <- "0")
            [
                yield!
                    inputList
                    |> Array.mapi
                        (fun i value ->
                            Ui.flex
                                (fun x ->
                                    x.position <- "relative"
                                    x.flex <- "1")
                                [
                                    Input.Input
                                        {|
                                            CustomProps =
                                                fun x ->
                                                    x.fixedValue <- Some value
                                                    inputCustomProps x
                                            Props =
                                                fun x ->
                                                    x.onChange <-
                                                        fun (e: Browser.Types.KeyboardEvent) ->
                                                            promise {
                                                                setAtomValue (
                                                                    inputList
                                                                    |> Array.mapi
                                                                        (fun i' v ->
                                                                            if i' = i then unbox e.Value else v)
                                                                )
                                                            }

                                                    match i, inputList.Length with
                                                    | 0, n when n > 1 ->
                                                        x.borderBottomLeftRadius <- "0px"
                                                        x.borderBottomRightRadius <- "0px"
                                                    | i, n when i > 0 && i = n - 1 ->
                                                        x.borderTopLeftRadius <- "0px"
                                                        x.borderTopRightRadius <- "0px"
                                                    | _, n when n > 2 -> x.borderRadius <- "0px"
                                                    | _ -> ()

                                                    x <+ inputProps
                                        |}


                                    if inputProps.isDisabled <> true then
                                        match i, inputList.Length with
                                        | 0, _ ->
                                            Tooltip.wrap
                                                (str "Add row")
                                                [
                                                    InputLabelIconButton.InputLabelIconButton
                                                        (fun x ->
                                                            x.icon <- Icons.fa.FaPlus |> Icons.render

                                                            x.onClick <-
                                                                fun _ ->
                                                                    promise {
                                                                        setAtomValue (
                                                                            [|
                                                                                ""
                                                                            |]
                                                                            |> Array.append inputList
                                                                        )
                                                                    }

                                                            x.position <- "absolute"
                                                            x.right <- "10px"
                                                            x.top <- "50%"
                                                            x.transform <- "translate(0, -50%)"
                                                            x.margin <- "0")
                                                ]
                                        | _, 1 -> nothing
                                        | _ ->
                                            Tooltip.wrap
                                                (str "Remove row")
                                                [
                                                    InputLabelIconButton.InputLabelIconButton
                                                        (fun x ->
                                                            x.icon <- Icons.fi.FiMinus |> Icons.render

                                                            x.onClick <-
                                                                fun _ ->
                                                                    promise {
                                                                        setAtomValue (
                                                                            inputList
                                                                            |> Array.indexed
                                                                            |> Array.filter (fun (i', _) -> i' <> i)
                                                                            |> Array.map snd
                                                                        )
                                                                    }

                                                            x.position <- "absolute"
                                                            x.right <- "10px"
                                                            x.top <- "50%"
                                                            x.transform <- "translate(0, -50%)"
                                                            x.margin <- "0")
                                                ]
                                ])
            ]
