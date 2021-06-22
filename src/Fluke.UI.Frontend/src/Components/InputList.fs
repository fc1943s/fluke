namespace Fluke.UI.Frontend.Components

open Fable.Core
open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module InputList =
    type IProps<'TValue> =
        inherit Chakra.IChakraProps

        abstract hint : ReactElement option with get, set
        abstract hintTitle : ReactElement option with get, set
        abstract atom : JotaiTypes.InputAtom<'TValue> option with get, set
        abstract inputScope : JotaiTypes.InputScope<'TValue> option with get, set


    [<ReactComponent>]
    let InputList (props: IProps<'TValue list> -> unit) =
        let props =
            React.useMemo (
                (fun () -> JS.newObj props),
                [|
                    box props
                |]
            )

        let atomFieldOptions = Store.useAtomFieldOptions<'TValue list> props.atom props.inputScope

        Chakra.box
            (fun _ -> ())
            [
                match props.label with
                | null -> nothing
                | _ ->
                    InputLabel.InputLabel
                        {|
                            Label =
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        props.label

                                        Tooltip.wrap
                                            (str "Add row")
                                            [
                                                InputLabelIconButton.InputLabelIconButton
                                                    {|
                                                        Props =
                                                            fun x ->
                                                                x.icon <- Icons.fa.FaPlus |> Icons.render
                                                                x.marginLeft <- "5px"

                                                                x.onClick <-
                                                                    fun _ ->
                                                                        promise {
                                                                            atomFieldOptions.SetAtomValue (
                                                                                atomFieldOptions.AtomValue
                                                                                @ [
                                                                                    unbox ""
                                                                                ]
                                                                            )
                                                                        }
                                                    |}
                                            ]
                                    ]
                            Hint = props.hint
                            HintTitle = props.hintTitle
                            Props = fun x -> x.marginBottom <- "5px"
                        |}

                match props.atom with
                | Some _ ->
                    let inputList =
                        match atomFieldOptions.AtomValue with
                        | [] ->
                            [
                                unbox ""
                            ]
                        | inputList -> inputList

                    yield!
                        inputList
                        |> List.mapi
                            (fun i value ->
                                Chakra.box
                                    (fun x -> x.position <- "relative")
                                    [

                                        Input.Input
                                            {|
                                                CustomProps = fun x -> x.fixedValue <- Some value
                                                Props =
                                                    fun x ->
                                                        x.onChange <-
                                                            fun (e: Browser.Types.KeyboardEvent) ->
                                                                promise {
                                                                    atomFieldOptions.SetAtomValue (
                                                                        atomFieldOptions.AtomValue
                                                                        |> List.mapi
                                                                            (fun i' v ->
                                                                                if i' = i then unbox e.Value else v)
                                                                    )
                                                                }
                                            |}


                                        match inputList.Length with
                                        | 1 -> nothing
                                        | _ ->
                                            Tooltip.wrap
                                                (str "Remove row")
                                                [
                                                    InputLabelIconButton.InputLabelIconButton
                                                        {|
                                                            Props =
                                                                fun x ->
                                                                    x.icon <- Icons.fa.FaMinus |> Icons.render

                                                                    x.onClick <-
                                                                        fun _ ->
                                                                            promise {
                                                                                atomFieldOptions.SetAtomValue (
                                                                                    atomFieldOptions.AtomValue
                                                                                    |> Seq.indexed
                                                                                    |> Seq.filter
                                                                                        (fun (i', _) -> i' <> i)
                                                                                    |> Seq.map snd
                                                                                    |> Seq.toList
                                                                                )
                                                                            }

                                                                    x.position <- "absolute"
                                                                    x.right <- "5px"
                                                                    x.top <- "50%"
                                                                    x.transform <- "translate(0, -50%)"
                                                                    x.margin <- "0"
                                                        |}
                                                ]
                                    ])
                | _ -> nothing
            ]
