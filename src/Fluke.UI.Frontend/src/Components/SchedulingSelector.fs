namespace rec Fluke.UI.Frontend.Components

open System
open Fluke.Shared
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
                            | true, value when value > 0 -> Some value
                            | _ -> None)

                x.inputFormat <- Some Input.InputFormat.Number)

    let inline radio flexDirection value children =
        Chakra.radio
            (fun x ->
                x.colorScheme <- "purple"
                x.value <- value |> Gun.jsonEncode
                x.flexDirection <- flexDirection)
            [
                yield! children
            ]

    let manualRadio value =
        radio
            "row"
            (match value with
             | Manual WithoutSuggestion -> Some value
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
                                x.hint <- (str "You need to change the cell status to 'Schedule' manually")

                                x.hintTitle <- Some (str "Manual Scheduling"))
                    ]
            ]

    let suggestedRadio value =
        radio
            "row"
            (match value with
             | Manual WithSuggestion -> Some value
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
                                x.hint <- (str "Displayed with a different color and is always visible on all views")

                                x.hintTitle <- Some (str "Suggested Scheduling"))
                    ]
            ]

    let offsetDaysRadio value setValue =
        radio
            "row"
            (match value with
             | Recurrency (Offset (Days _)) -> Some value
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
                            (match value with
                             | Recurrency (Offset (Days n)) -> Some n
                             | _ -> Some 1)
                            (fun value -> setValue (Recurrency (Offset (Days value))))
                        Chakra.box
                            (fun _ -> ())
                            [
                                str "days"
                            ]
                    ]
            ]

    let offsetWeeksRadio value setValue =
        radio
            "row"
            (match value with
             | Recurrency (Offset (Weeks _)) -> Some value
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
                            (match value with
                             | Recurrency (Offset (Weeks n)) -> Some n
                             | _ -> Some 1)
                            (fun value -> setValue (Recurrency (Offset (Weeks value))))
                        Chakra.box
                            (fun _ -> ())
                            [
                                str "weeks"
                            ]
                    ]
            ]

    let offsetMonthsRadio value setValue =
        radio
            "row"
            (match value with
             | Recurrency (Offset (Months _)) -> Some value
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
                            (match value with
                             | Recurrency (Offset (Months n)) -> Some n
                             | _ -> Some 1)
                            (fun value -> setValue (Recurrency (Offset (Months value))))
                        Chakra.box
                            (fun _ -> ())
                            [
                                str "months"
                            ]
                    ]
            ]

    let fixedWeeklyRadio value setValue weekStart =
        Chakra.stack
            (fun x ->
                x.alignItems <- "center"
                x.direction <- "row")
            [
                Chakra.box
                    (fun _ -> ())
                    [
                        str "Weekly: "
                    ]

                yield!
                    Enum.ToList<DayOfWeek> ()
                    |> List.sortBy (
                        int
                        >> fun x -> if int weekStart >= x then x * x else x
                    )
                    |> Seq.map
                        (fun dayOfWeek ->
                            Chakra.stack
                                (fun x ->
                                    x.alignItems <- "center"
                                    x.spacing <- "2px")
                                [

                                    Checkbox.Checkbox
                                        {|
                                            Props =
                                                fun x ->
                                                    x.isChecked <-
                                                        match value with
                                                        | Recurrency (Fixed fixedRecurrencyList) when
                                                            fixedRecurrencyList
                                                            |> List.contains (Weekly dayOfWeek) -> true
                                                        | _ -> false

                                                    x.onChange <-
                                                        fun _ ->
                                                            promise {
                                                                setValue (
                                                                    match value with
                                                                    | Recurrency (Fixed fixedRecurrencyList) ->
                                                                        let tmp =
                                                                            fixedRecurrencyList
                                                                            |> List.map
                                                                                (function
                                                                                | Weekly weekly -> Some weekly, None
                                                                                | recurrency -> None, Some recurrency)

                                                                        let weeklySet =
                                                                            tmp |> List.choose fst |> Set.ofList

                                                                        let otherRecurrencies = tmp |> List.choose snd

                                                                        let newSet = weeklySet |> Set.toggle dayOfWeek

                                                                        let newList =
                                                                            otherRecurrencies
                                                                            @ (newSet |> Set.map Weekly |> Set.toList)

                                                                        match newList with
                                                                        | [] -> Manual WithoutSuggestion
                                                                        | newList -> Recurrency (Fixed newList)
                                                                    | _ -> Recurrency (Fixed [ Weekly dayOfWeek ])
                                                                )
                                                            }
                                        |}
                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            str (Enum.name dayOfWeek)
                                        ]
                                ])
            ]

    let fixedMonthlyRadio value setValue =
        Chakra.stack
            (fun x ->
                x.alignItems <- "center"
                x.direction <- "row")
            [
                Chakra.box
                    (fun _ -> ())
                    [
                        str "Monthly: "
                    ]

                Chakra.stack
                    (fun _ -> ())
                    [
                        yield!
                            [
                                [
                                    1 .. 14
                                ]
                                [
                                    15 .. 31
                                ]
                            ]
                            |> List.map (List.map Day)
                            |> List.map (
                                List.map
                                    (fun day ->
                                        Chakra.stack
                                            (fun x ->
                                                x.alignItems <- "center"
                                                x.spacing <- "2px")
                                            [
                                                Checkbox.Checkbox
                                                    {|
                                                        Props =
                                                            fun x ->
                                                                x.isChecked <-
                                                                    match value with
                                                                    | Recurrency (Fixed fixedRecurrencyList) when
                                                                        fixedRecurrencyList
                                                                        |> List.contains (Monthly day) -> true
                                                                    | _ -> false

                                                                x.onChange <-
                                                                    fun _ ->
                                                                        promise {
                                                                            setValue (
                                                                                match value with
                                                                                | Recurrency (Fixed fixedRecurrencyList) ->
                                                                                    let tmp =
                                                                                        fixedRecurrencyList
                                                                                        |> List.map
                                                                                            (function
                                                                                            | Monthly day ->
                                                                                                Some day, None
                                                                                            | recurrency ->
                                                                                                None, Some recurrency)

                                                                                    let monthlySet =
                                                                                        tmp
                                                                                        |> List.choose fst
                                                                                        |> Set.ofList

                                                                                    let otherRecurrencies =
                                                                                        tmp |> List.choose snd

                                                                                    let newSet =
                                                                                        monthlySet |> Set.toggle day

                                                                                    let newList =
                                                                                        otherRecurrencies
                                                                                        @ (newSet
                                                                                           |> Set.map Monthly
                                                                                           |> Set.toList)

                                                                                    match newList with
                                                                                    | [] -> Manual WithoutSuggestion
                                                                                    | newList ->
                                                                                        Recurrency (Fixed newList)
                                                                                | _ ->
                                                                                    Recurrency (Fixed [ Monthly day ])
                                                                            )
                                                                        }
                                                    |}
                                                Chakra.box
                                                    (fun _ -> ())
                                                    [
                                                        str (day |> Day.Value |> string)
                                                    ]
                                            ])
                            )
                            |> List.map
                                (fun x ->
                                    Chakra.stack
                                        (fun x -> x.direction <- "row")
                                        [
                                            yield! x
                                        ])
                    ]
            ]

    let inline fixedYearlyRadio value setValue =
        let rows =
            match value with
            | Recurrency (Fixed fixedRecurrencyList) ->
                fixedRecurrencyList
                |> List.mapi
                    (fun i ->
                        function
                        | Yearly (day, month) -> Some (Some (i, day, month)), None
                        | fixedRecurrency -> None, Some fixedRecurrency)
            | _ -> []
            |> function
            | rows when rows |> List.choose fst |> List.isEmpty ->
                [
                    Some None, None
                ]
            | rows -> rows

        React.fragment [
            yield!
                rows
                |> List.filter
                    (function
                    | Some _, _ -> true
                    | _ -> false)
                |> List.map fst
                |> List.mapi
                    (fun i row ->
                        Chakra.stack
                            (fun x ->
                                x.alignItems <- "center"
                                x.direction <- "row")
                            [
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str "Yearly: "

                                        match i, row with
                                        | 0, Some (Some _) ->
                                            Tooltip.wrap
                                                (str "Add row")
                                                [
                                                    InputLabelIconButton.InputLabelIconButton
                                                        {|
                                                            Props =
                                                                fun x ->
                                                                    x.icon <- Icons.fa.FaPlus |> Icons.render

                                                                    x.onClick <-
                                                                        fun _ ->
                                                                            promise {
                                                                                let fixedRecurrencyList =
                                                                                    rows
                                                                                    |> List.choose
                                                                                        (function
                                                                                        | Some (Some (_, day, month)), _ ->
                                                                                            Some (Yearly (day, month))
                                                                                        | _, Some fixedRecurrency ->
                                                                                            Some fixedRecurrency
                                                                                        | _ -> None)

                                                                                setValue (
                                                                                    Recurrency (
                                                                                        Fixed (
                                                                                            fixedRecurrencyList
                                                                                            @ [
                                                                                                Yearly (
                                                                                                    Day 1,
                                                                                                    Month.January
                                                                                                )
                                                                                            ]
                                                                                        )
                                                                                    )
                                                                                )
                                                                            }
                                                        |}
                                                ]
                                        | i, Some _ when i > 0 ->
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
                                                                                let fixedRecurrencyList =
                                                                                    rows
                                                                                    |> List.choose
                                                                                        (function
                                                                                        | Some (Some (i', _, _)), _ when
                                                                                            i' = i -> None
                                                                                        | Some (Some (_, day, month)), _ ->
                                                                                            Some (Yearly (day, month))
                                                                                        | _, Some fixedRecurrency ->
                                                                                            Some fixedRecurrency
                                                                                        | _ -> None)

                                                                                setValue (
                                                                                    match fixedRecurrencyList with
                                                                                    | [] -> Manual WithoutSuggestion
                                                                                    | fixedRecurrencyList ->
                                                                                        Recurrency (
                                                                                            Fixed fixedRecurrencyList
                                                                                        )
                                                                                )
                                                                            }
                                                        |}
                                                ]
                                        | _ -> ()
                                    ]

                                Chakra.radioGroup
                                    (fun x ->
                                        x.onChange <-
                                            fun (radioValueSelected: string) ->
                                                promise {
                                                    let yearlySelected = radioValueSelected |> Gun.jsonDecode

                                                    match yearlySelected with
                                                    | Some (i, day, month) ->
                                                        let fixedRecurrencyList =
                                                            rows
                                                            |> List.choose
                                                                (function
                                                                | Some (Some (i', _, _)), _ when i' = i ->
                                                                    Some (Yearly (day, month))
                                                                | Some (Some (_, day, month)), _ ->
                                                                    Some (Yearly (day, month))
                                                                | _, Some fixedRecurrency -> Some fixedRecurrency
                                                                | _ -> None)

                                                        let fixedRecurrencyList =
                                                            match i with
                                                            | -1 ->
                                                                fixedRecurrencyList
                                                                @ [
                                                                    Yearly (day, month)
                                                                ]
                                                            | _ -> fixedRecurrencyList

                                                        setValue (Recurrency (Fixed fixedRecurrencyList))
                                                    | _ -> ()
                                                }

                                        x.value <-
                                            match row with
                                            | Some yearly -> yearly
                                            | _ -> None
                                            |> Gun.jsonEncode)
                                    [
                                        Chakra.stack
                                            (fun _ -> ())
                                            [
                                                Chakra.stack
                                                    (fun x -> x.direction <- "row")
                                                    [
                                                        yield!
                                                            Enum.ToList<Month> ()
                                                            |> List.map
                                                                (fun month ->
                                                                    radio
                                                                        "column"
                                                                        (match row with
                                                                         | Some (Some (i, day, month')) when
                                                                             month = month' -> Some (i, day, month)
                                                                         | Some (Some (i, day, _)) ->
                                                                             Some (i, day, month)
                                                                         | _ -> Some (-1, Day 1, month))
                                                                        [
                                                                            Chakra.box
                                                                                (fun x -> x.marginLeft <- "-6px")
                                                                                [
                                                                                    str (Enum.name month)
                                                                                ]
                                                                        ])
                                                    ]

                                                yield!
                                                    [
                                                        [
                                                            1 .. 14
                                                        ]
                                                        [
                                                            15 .. 31
                                                        ]
                                                    ]
                                                    |> List.map (
                                                        List.map
                                                            (fun dayNumber ->
                                                                Chakra.stack
                                                                    (fun x ->
                                                                        x.alignItems <- "center"
                                                                        x.spacing <- "2px")
                                                                    [
                                                                        radio
                                                                            "column"
                                                                            (match row with
                                                                             | Some (Some (i, day, month)) when
                                                                                 day = Day dayNumber ->
                                                                                 Some (i, day, month)
                                                                             | Some (Some (i, _, month)) ->
                                                                                 Some (i, Day dayNumber, month)
                                                                             | _ ->
                                                                                 Some (-1, Day dayNumber, Month.January))
                                                                            [
                                                                                Chakra.box
                                                                                    (fun x ->
                                                                                        x.marginLeft <- "-6px"
                                                                                        x.marginTop <- "3px")
                                                                                    [
                                                                                        str (dayNumber |> string)
                                                                                    ]
                                                                            ]
                                                                    ])
                                                    )
                                                    |> List.map
                                                        (fun x ->
                                                            Chakra.stack
                                                                (fun x -> x.direction <- "row")
                                                                [
                                                                    yield! x
                                                                ])
                                            ]
                                    ]
                            ])
        ]

    [<ReactComponent>]
    let SchedulingSelector
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId |})
        =

        let weekStart = Recoil.useValue (Atoms.User.weekStart input.Username)

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
                                    Props = fun x -> x.whiteSpace <- "normal"
                                    Children =
                                        [
                                            schedulingFieldOptions.AtomValue
                                            |> Scheduling.Label
                                            |> str
                                        ]
                                |}
                        Body =
                            fun (_disclosure, _initialFocusRef) ->
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
                                                (fun x ->
                                                    x.spacing <- "25px"
                                                    x.padding <- "5px")
                                                [
                                                    manualRadio schedulingFieldOptions.AtomValue
                                                    suggestedRadio schedulingFieldOptions.AtomValue
                                                    offsetDaysRadio
                                                        schedulingFieldOptions.AtomValue
                                                        schedulingFieldOptions.SetAtomValue
                                                    offsetWeeksRadio
                                                        schedulingFieldOptions.AtomValue
                                                        schedulingFieldOptions.SetAtomValue
                                                    offsetMonthsRadio
                                                        schedulingFieldOptions.AtomValue
                                                        schedulingFieldOptions.SetAtomValue
                                                    fixedWeeklyRadio
                                                        schedulingFieldOptions.AtomValue
                                                        schedulingFieldOptions.SetAtomValue
                                                        weekStart
                                                    fixedMonthlyRadio
                                                        schedulingFieldOptions.AtomValue
                                                        schedulingFieldOptions.SetAtomValue
                                                    fixedYearlyRadio
                                                        schedulingFieldOptions.AtomValue
                                                        schedulingFieldOptions.SetAtomValue

                                                ]
                                        ]
                                ]
                    |}
            ]
