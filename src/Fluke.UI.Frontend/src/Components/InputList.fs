namespace Fluke.UI.Frontend.Components

open Fable.Core
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module InputList =
    type IProps<'TValue, 'TKey> =
        inherit Chakra.IChakraProps

        abstract hint : ReactElement option with get, set
        abstract hintTitle : ReactElement option with get, set
        abstract atom : Recoil.InputAtom<'TValue, 'TKey> option with get, set
        abstract atomScope : Recoil.AtomScope option with get, set

    [<ReactComponent>]
    let InputList<'TValue, 'TKey when 'TValue: equality> (input: IProps<'TValue list, 'TKey>) =
        let atomFieldOptions = Recoil.useAtomField<'TValue list, 'TKey> input.atom input.atomScope

        Chakra.box
            (fun _ -> ())
            [
                match input.label with
                | null -> nothing
                | _ ->
                    InputLabel.InputLabel
                        {|
                            Label =
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        input.label

                                        Tooltip.wrap
                                            (str "Add row")
                                            [
                                                InputLabelIconButton.InputLabelIconButton
                                                    {|
                                                        Props =
                                                            JS.newObj
                                                                (fun x ->
                                                                    x.icon <- Icons.fa.FaPlus |> Icons.render

                                                                    x.onClick <-
                                                                        fun _ ->
                                                                            promise {
                                                                                atomFieldOptions.SetAtomValue (
                                                                                    atomFieldOptions.AtomValue
                                                                                    @ [
                                                                                        unbox ""
                                                                                    ]
                                                                                )
                                                                            })
                                                    |}
                                            ]
                                    ]
                            Hint = input.hint
                            HintTitle = input.hintTitle
                            Props = JS.newObj (fun x -> x.marginBottom <- "5px")
                        |}

                match input.atom with
                | Some (Recoil.InputAtom.Atom _) ->
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
                                        Input.Input (
                                            JS.newObj
                                                (fun x ->
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

                                                    x.value <- Some value)
                                        )

                                        match inputList.Length with
                                        | 1 -> nothing
                                        | _ ->
                                            Tooltip.wrap
                                                (str "Remove row")
                                                [
                                                    InputLabelIconButton.InputLabelIconButton
                                                        {|
                                                            Props =
                                                                JS.newObj
                                                                    (fun x ->
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
                                                                        x.margin <- "0")
                                                        |}
                                                ]
                                    ])
                | _ -> nothing
            ]
