namespace rec Fluke.UI.Frontend.Components

open System
open FsCore
open Fluke.Shared
open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open FsJs
open FsStore
open FsUi.Bindings
open Fluke.UI.Frontend.State
open FsUi.Components


module SchedulingSelector =

    [<ReactComponent>]
    let NumberInput value (setValue: int -> unit) =
        Input.Input
            {|
                CustomProps =
                    fun x ->
                        x.fixedValue <- value
                        x.onValidate <- Some (fst >> String.parseIntMin 1)
                        x.inputFormat <- Some Input.InputFormat.Number
                Props =
                    fun x ->
                        x.width <- "70px"

                        x.onChange <-
                            fun (e: KeyboardEvent) ->
                                promise {
                                    e.Value
                                    |> String.parseIntMin 1
                                    |> Option.defaultValue 1
                                    |> setValue
                                }
            |}

    let inline radio flexDirection value children =
        Radio.Radio
            (fun x ->
                x.value <- value |> Json.encode
                x.flexDirection <- flexDirection)
            children

    [<ReactComponent>]
    let ManualRadio value =
        radio
            "row"
            (match value with
             | Manual WithoutSuggestion -> Some value
             | _ -> Some (Manual WithoutSuggestion))
            [
                Ui.stack
                    (fun x ->
                        x.spacing <- "0"
                        x.alignItems <- "center"
                        x.direction <- "row")
                    [
                        Ui.str "Manual"

                        Hint.Hint
                            (fun x ->
                                x.hint <- (str "You need to change the cell status to 'Schedule' manually")

                                x.hintTitle <- Some (str "Manual Scheduling"))
                    ]
            ]

    [<ReactComponent>]
    let SuggestedRadio value =
        radio
            "row"
            (match value with
             | Manual WithSuggestion -> Some value
             | _ -> Some (Manual WithSuggestion))
            [
                Ui.stack
                    (fun x ->
                        x.spacing <- "0"
                        x.alignItems <- "center"
                        x.direction <- "row")
                    [
                        Ui.str "Suggested"

                        Hint.Hint
                            (fun x ->
                                x.hint <- (str "Displayed with a different color and is always visible on all views")

                                x.hintTitle <- Some (str "Suggested Scheduling"))
                    ]
            ]


    [<ReactComponent>]
    let OffsetDaysRadio value setValue =
        radio
            "row"
            (match value with
             | Recurrency (Offset (Days _)) -> Some value
             | _ -> Some (Recurrency (Offset (Days 1))))
            [
                Ui.stack
                    (fun x ->
                        x.alignItems <- "center"
                        x.direction <- "row")
                    [
                        Ui.str "Every"
                        NumberInput
                            (match value with
                             | Recurrency (Offset (Days n)) -> Some n
                             | _ -> Some 1)
                            (fun value -> setValue (Recurrency (Offset (Days value))))
                        Ui.str "days"
                    ]
            ]

    [<ReactComponent>]
    let OffsetWeeksRadio value setValue =
        radio
            "row"
            (match value with
             | Recurrency (Offset (Weeks _)) -> Some value
             | _ -> Some (Recurrency (Offset (Weeks 1))))
            [
                Ui.stack
                    (fun x ->
                        x.alignItems <- "center"
                        x.direction <- "row")
                    [
                        Ui.str "Every"
                        NumberInput
                            (match value with
                             | Recurrency (Offset (Weeks n)) -> Some n
                             | _ -> Some 1)
                            (fun value -> setValue (Recurrency (Offset (Weeks value))))
                        Ui.str "weeks"
                    ]
            ]

    [<ReactComponent>]
    let OffsetMonthsRadio value setValue =
        radio
            "row"
            (match value with
             | Recurrency (Offset (Months _)) -> Some value
             | _ -> Some (Recurrency (Offset (Months 1))))
            [
                Ui.stack
                    (fun x ->
                        x.alignItems <- "center"
                        x.direction <- "row")
                    [
                        Ui.str "Every"
                        NumberInput
                            (match value with
                             | Recurrency (Offset (Months n)) -> Some n
                             | _ -> Some 1)
                            (fun value -> setValue (Recurrency (Offset (Months value))))
                        Ui.str "months"
                    ]
            ]


    [<ReactComponent>]
    let FixedWeeklyRadio value setValue weekStart =
        Ui.stack
            (fun x ->
                x.alignItems <- "center"
                x.direction <- "row")
            [
                Ui.str "Weekly: "

                yield!
                    Enum.ToList<DayOfWeek> ()
                    |> List.sortBy (
                        int
                        >> fun x -> if int weekStart >= x then x * x else x
                    )
                    |> Seq.map
                        (fun dayOfWeek ->
                            Ui.stack
                                (fun x ->
                                    x.alignItems <- "center"
                                    x.spacing <- "2px")
                                [

                                    Checkbox.Checkbox
                                        None
                                        (fun x ->
                                            x.isChecked <-
                                                match value with
                                                | Recurrency (Fixed fixedRecurrencyList) when
                                                    fixedRecurrencyList
                                                    |> List.contains (Weekly dayOfWeek)
                                                    ->
                                                    true
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

                                                                let weeklySet = tmp |> List.choose fst |> Set.ofSeq

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
                                                    })
                                    Ui.str (Enum.name dayOfWeek)
                                ])
            ]


    [<ReactComponent>]
    let FixedMonthlyRadio value setValue =
        Ui.stack
            (fun x ->
                x.alignItems <- "center"
                x.direction <- "row")
            [
                Ui.str "Monthly: "

                Ui.stack
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
                                        Ui.stack
                                            (fun x ->
                                                x.alignItems <- "center"
                                                x.spacing <- "2px")
                                            [
                                                Checkbox.Checkbox
                                                    None
                                                    (fun x ->
                                                        x.isChecked <-
                                                            match value with
                                                            | Recurrency (Fixed fixedRecurrencyList) when
                                                                fixedRecurrencyList |> List.contains (Monthly day)
                                                                ->
                                                                true
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
                                                                                    | Monthly day -> Some day, None
                                                                                    | recurrency ->
                                                                                        None, Some recurrency)

                                                                            let monthlySet =
                                                                                tmp |> List.choose fst |> Set.ofSeq

                                                                            let otherRecurrencies =
                                                                                tmp |> List.choose snd

                                                                            let newSet = monthlySet |> Set.toggle day

                                                                            let newList =
                                                                                otherRecurrencies
                                                                                @ (newSet
                                                                                   |> Set.toList
                                                                                   |> List.map Monthly)

                                                                            match newList with
                                                                            | [] -> Manual WithoutSuggestion
                                                                            | newList -> Recurrency (Fixed newList)
                                                                        | _ -> Recurrency (Fixed [ Monthly day ])
                                                                    )
                                                                })
                                                Ui.str (day |> Day.Value |> string)
                                            ])
                            )
                            |> List.map
                                (fun x ->
                                    Ui.stack
                                        (fun x -> x.direction <- "row")
                                        [
                                            yield! x
                                        ])
                    ]
            ]


    [<ReactComponent>]
    let FixedYearlyRadio value setValue =
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
                    rows
                    @ [
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
                        Ui.stack
                            (fun x ->
                                x.alignItems <- "center"
                                x.direction <- "row")
                            [
                                Ui.box
                                    (fun _ -> ())
                                    [
                                        str "Yearly: "

                                        match i, row with
                                        | 0, Some (Some _) ->
                                            Tooltip.wrap
                                                (str "Add row")
                                                [
                                                    InputLabelIconButton.InputLabelIconButton
                                                        (fun x ->
                                                            x.margin <- "4px"
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
                                                                                        Yearly (Day 1, Month.January)
                                                                                    ]
                                                                                )
                                                                            )
                                                                        )
                                                                    })
                                                ]
                                        | i, Some _ when i > 0 ->
                                            Tooltip.wrap
                                                (str "Remove row")
                                                [
                                                    InputLabelIconButton.InputLabelIconButton
                                                        (fun x ->
                                                            x.margin <- "4px"
                                                            x.icon <- Icons.fi.FiMinus |> Icons.render

                                                            x.onClick <-
                                                                fun _ ->
                                                                    promise {
                                                                        let fixedRecurrencyList =
                                                                            rows
                                                                            |> List.choose
                                                                                (function
                                                                                | Some (Some (i', _, _)), _ when i' = i ->
                                                                                    None
                                                                                | Some (Some (_, day, month)), _ ->
                                                                                    Some (Yearly (day, month))
                                                                                | _, Some fixedRecurrency ->
                                                                                    Some fixedRecurrency
                                                                                | _ -> None)

                                                                        setValue (
                                                                            match fixedRecurrencyList with
                                                                            | [] -> Manual WithoutSuggestion
                                                                            | fixedRecurrencyList ->
                                                                                Recurrency (Fixed fixedRecurrencyList)
                                                                        )
                                                                    })
                                                ]
                                        | _ -> nothing
                                    ]

                                Ui.radioGroup
                                    (fun x ->
                                        x.onChange <-
                                            fun (radioValueSelected: string) ->
                                                promise {
                                                    let yearlySelected = radioValueSelected |> Json.decode

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

                                                        let newFixedRecurrencyList =
                                                            match i with
                                                            | -1 ->
                                                                fixedRecurrencyList
                                                                @ [
                                                                    Yearly (day, month)
                                                                ]
                                                            | _ -> fixedRecurrencyList

                                                        setValue (Recurrency (Fixed newFixedRecurrencyList))
                                                    | _ -> ()
                                                }

                                        x.value <-
                                            match row with
                                            | Some yearly -> yearly
                                            | _ -> None
                                            |> Json.encode)
                                    [
                                        Ui.stack
                                            (fun _ -> ())
                                            [
                                                Ui.stack
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
                                                                             month = month'
                                                                             ->
                                                                             Some (i, day, month)
                                                                         | Some (Some (i, day, _)) ->
                                                                             Some (i, day, month)
                                                                         | _ -> Some (-1, Day 1, month))
                                                                        [
                                                                            str (Enum.name month)
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
                                                                Ui.stack
                                                                    (fun x ->
                                                                        x.alignItems <- "center"
                                                                        x.spacing <- "2px")
                                                                    [
                                                                        radio
                                                                            "column"
                                                                            (match row with
                                                                             | Some (Some (i, day, month)) when
                                                                                 day = Day dayNumber
                                                                                 ->
                                                                                 Some (i, day, month)
                                                                             | Some (Some (i, _, month)) ->
                                                                                 Some (i, Day dayNumber, month)
                                                                             | _ ->
                                                                                 Some (-1, Day dayNumber, Month.January))
                                                                            [
                                                                                str (dayNumber |> string)
                                                                            ]
                                                                    ])
                                                    )
                                                    |> List.map
                                                        (fun x ->
                                                            Ui.stack
                                                                (fun x -> x.direction <- "row")
                                                                [
                                                                    yield! x
                                                                ])
                                            ]
                                    ]
                            ])
        ]

    [<ReactComponent>]
    let SchedulingDropdown scheduling setScheduling =
        let weekStart = Store.useValue Atoms.User.weekStart

        Dropdown.Dropdown
            {|
                Tooltip = ""
                Left = true
                Trigger =
                    fun visible setVisible ->
                        Button.Button
                            {|
                                Hint = None
                                Icon =
                                    Some (
                                        (if visible then Icons.fi.FiChevronUp else Icons.fi.FiChevronDown)
                                        |> Icons.render,
                                        Button.IconPosition.Right
                                    )
                                Props =
                                    fun x ->
                                        x.whiteSpace <- "normal"
                                        x.onClick <- fun _ -> promise { setVisible (not visible) }
                                Children =
                                    [
                                        scheduling |> Scheduling.Label |> str
                                    ]
                            |}
                Body =
                    fun _onHide ->
                        [
                            Ui.radioGroup
                                (fun x ->
                                    x.overflow <- "auto"
                                    x.maxHeight <- "350px"

                                    x.onChange <-
                                        fun (radioValueSelected: string) ->
                                            promise { setScheduling (radioValueSelected |> Json.decode) }

                                    x.value <- scheduling |> Json.encode)
                                [
                                    Ui.stack
                                        (fun x ->
                                            x.spacing <- "18px"
                                            x.padding <- "5px")
                                        [
                                            ManualRadio scheduling
                                            SuggestedRadio scheduling
                                            OffsetDaysRadio scheduling setScheduling
                                            OffsetWeeksRadio scheduling setScheduling
                                            OffsetMonthsRadio scheduling setScheduling
                                            FixedWeeklyRadio scheduling setScheduling weekStart
                                            FixedMonthlyRadio scheduling setScheduling
                                            FixedYearlyRadio scheduling setScheduling
                                        ]
                                ]
                        ]
            |}
