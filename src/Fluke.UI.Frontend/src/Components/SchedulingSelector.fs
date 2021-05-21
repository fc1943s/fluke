namespace rec Fluke.UI.Frontend.Components

open System
open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module SchedulingSelector =
    let numberInput value (setValue: int -> unit) =
        Input.Input
            (fun x ->
                x.width <- "70px"
                x.value <- value

                x.onChange <-
                    fun (e: KeyboardEvent) ->
                        promise {
                            setValue (
                                match Int32.TryParse e.Value with
                                | true, value -> value
                                | _ -> 0
                                |> int
                            )
                        }

                x.onValidate <-
                    Some
                        (fun (value, _) ->
                            match Int32.TryParse value with
                            | true, value when value >= 0 -> Some value
                            | _ -> None)

                x.inputFormat <- Some Input.InputFormat.Number)

    let inline radio value children =
        Chakra.radio
            (fun x ->
                x.colorScheme <- "purple"
                x.value <- value |> Gun.jsonEncode)
            [
                Chakra.stack
                    (fun x ->
                        x.alignItems <- "center"
                        x.direction <- "row")
                    [
                        yield! children
                    ]
            ]

    [<ReactComponent>]
    let SchedulingSelector
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId |})
        =

        let schedulingFieldOptions =
            Recoil.useAtomFieldOptions
                (Some (Recoil.AtomFamily (Atoms.Task.scheduling, input.TaskId)))
                (Some (Recoil.InputScope.ReadWrite Gun.defaultSerializer))

        Chakra.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Scheduling"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}
                Popover.Popover
                    {|
                        Trigger =
                            Button.Button
                                {|
                                    Hint = None
                                    Icon = Some (Icons.fi.FiChevronDown |> Icons.wrap, Button.IconPosition.Right)
                                    Props = fun _ -> ()
                                    Children =
                                        [
                                            schedulingFieldOptions.AtomValue
                                            |> Scheduling.Label
                                            |> str
                                        ]
                                |}
                        Body =
                            [
                                Chakra.radioGroup
                                    (fun x ->
                                        x.onChange <-
                                            fun (radioValueSelected: string) ->
                                                promise {
                                                    schedulingFieldOptions.SetAtomValue (
                                                        radioValueSelected |> Gun.jsonDecode
                                                    )
                                                }

                                        x.value <- schedulingFieldOptions.AtomValue |> Gun.jsonEncode)
                                    [
                                        Chakra.stack
                                            (fun x -> x.spacing <- "15px")
                                            [
                                                radio
                                                    (match schedulingFieldOptions.AtomValue with
                                                     | Manual WithoutSuggestion -> Some schedulingFieldOptions.AtomValue
                                                     | _ -> Some (Manual WithoutSuggestion))
                                                    [
                                                        Chakra.stack
                                                            (fun x ->
                                                                x.spacing <- "0"
                                                                x.alignItems <- "center"
                                                                x.direction <- "row")
                                                            [
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "Manual"
                                                                    ]

                                                                Hint.Hint
                                                                    (fun x ->
                                                                        x.hint <-
                                                                            (str
                                                                                "You need to change the cell status to 'Schedule' manually")

                                                                        x.hintTitle <- Some (str "Manual Scheduling"))
                                                            ]
                                                    ]

                                                radio
                                                    (match schedulingFieldOptions.AtomValue with
                                                     | Manual WithSuggestion -> Some schedulingFieldOptions.AtomValue
                                                     | _ -> Some (Manual WithSuggestion))
                                                    [
                                                        Chakra.stack
                                                            (fun x ->
                                                                x.spacing <- "0"
                                                                x.alignItems <- "center"
                                                                x.direction <- "row")
                                                            [
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "Suggested"
                                                                    ]

                                                                Hint.Hint
                                                                    (fun x ->
                                                                        x.hint <-
                                                                            (str
                                                                                "Displayed with a different color and is always visible on all views")

                                                                        x.hintTitle <- Some (str "Suggested Scheduling"))
                                                            ]
                                                    ]

                                                radio
                                                    (match schedulingFieldOptions.AtomValue with
                                                     | Recurrency (Offset (Days _)) ->
                                                         Some schedulingFieldOptions.AtomValue
                                                     | _ -> Some (Recurrency (Offset (Days 1))))
                                                    [
                                                        Chakra.stack
                                                            (fun x ->
                                                                x.alignItems <- "center"
                                                                x.direction <- "row")
                                                            [
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "Every"
                                                                    ]
                                                                numberInput
                                                                    (match schedulingFieldOptions.AtomValue with
                                                                     | Recurrency (Offset (Days n)) -> Some n
                                                                     | _ -> Some 0)
                                                                    (fun value ->
                                                                        schedulingFieldOptions.SetAtomValue (
                                                                            Recurrency (Offset (Days value))
                                                                        ))
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "days"
                                                                    ]
                                                            ]
                                                    ]

                                                radio
                                                    (match schedulingFieldOptions.AtomValue with
                                                     | Recurrency (Offset (Weeks _)) ->
                                                         Some schedulingFieldOptions.AtomValue
                                                     | _ -> Some (Recurrency (Offset (Weeks 1))))
                                                    [
                                                        Chakra.stack
                                                            (fun x ->
                                                                x.alignItems <- "center"
                                                                x.direction <- "row")
                                                            [
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "Every"
                                                                    ]
                                                                numberInput
                                                                    (match schedulingFieldOptions.AtomValue with
                                                                     | Recurrency (Offset (Weeks n)) -> Some n
                                                                     | _ -> Some 0)
                                                                    (fun value ->
                                                                        schedulingFieldOptions.SetAtomValue (
                                                                            Recurrency (Offset (Weeks value))
                                                                        ))
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "weeks"
                                                                    ]
                                                            ]
                                                    ]

                                                radio
                                                    (match schedulingFieldOptions.AtomValue with
                                                     | Recurrency (Offset (Months _)) ->
                                                         Some schedulingFieldOptions.AtomValue
                                                     | _ -> Some (Recurrency (Offset (Months 1))))
                                                    [
                                                        Chakra.stack
                                                            (fun x ->
                                                                x.alignItems <- "center"
                                                                x.direction <- "row")
                                                            [
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "Every"
                                                                    ]
                                                                numberInput
                                                                    (match schedulingFieldOptions.AtomValue with
                                                                     | Recurrency (Offset (Months n)) -> Some n
                                                                     | _ -> Some 0)
                                                                    (fun value ->
                                                                        schedulingFieldOptions.SetAtomValue (
                                                                            Recurrency (Offset (Months value))
                                                                        ))
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "months"
                                                                    ]
                                                            ]
                                                    ]
                                            ]
                                    ]
                            ]
                    |}
            ]
