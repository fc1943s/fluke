namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Components
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsUi.Components


module FilterForm =
    let inline SchedulingInput (scheduling: Scheduling option) setScheduling =
        Ui.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Scheduling"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Ui.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            (if scheduling.IsNone then Some "Enable" else None)
                            (fun x ->
                                x.isChecked <- scheduling.IsSome
                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            setScheduling (
                                                if scheduling.IsSome then None else Some (Manual WithoutSuggestion)
                                            )
                                        })

                        match scheduling with
                        | Some scheduling ->
                            SchedulingSelector.SchedulingDropdown
                                scheduling
                                (fun scheduling -> setScheduling (Some scheduling))
                        | None -> nothing
                    ]
            ]

    [<ReactComponent>]
    let FilterForm () =
        let filter, setFilter = Store.useState Atoms.User.filter

        Accordion.AccordionAtom
            {|
                Props = fun x -> x.flex <- "1"
                Atom = Atoms.User.accordionHiddenFlag AccordionType.TaskForm
                Items =
                    [
                        str "Filter",
                        (Ui.stack
                            (fun x ->
                                x.spacing <- "15px"
                                x.flex <- "1")
                            [
                                Input.LeftIconInput
                                    {|
                                        Icon = Icons.bs.BsSearch |> Icons.render
                                        CustomProps = fun x -> x.fixedValue <- Some filter.Filter
                                        Props =
                                            fun x ->
                                                x.autoFocus <- true
                                                x.placeholder <- "Filter"

                                                x.onChange <-
                                                    fun (e: KeyboardEvent) ->
                                                        promise { setFilter { filter with Filter = string e.Value } }
                                    |}

                                InformationSelector.InformationSelector
                                    {|
                                        DisableResource = true
                                        SelectionType = InformationSelector.InformationSelectionType.Information
                                        Information = Some filter.Information.Information
                                        OnSelect =
                                            fun information ->
                                                setFilter
                                                    { filter with
                                                        Information =
                                                            {| filter.Information with
                                                                Information = information
                                                            |}
                                                    }
                                    |}
                            ])

                        str "Task",
                        (Ui.stack
                            (fun x -> x.spacing <- "15px")
                            [
                                SchedulingInput
                                    filter.Scheduling
                                    (fun scheduling -> setFilter { filter with Scheduling = scheduling })

                                TaskForm.PriorityInput
                                    filter.Task.Priority
                                    (fun priority ->
                                        setFilter
                                            { filter with
                                                Task = { filter.Task with Priority = priority }
                                            })

                                TaskForm.DurationInput
                                    filter.Task.Duration
                                    (fun duration ->
                                        setFilter
                                            { filter with
                                                Task = { filter.Task with Duration = duration }
                                            })

                                TaskForm.PendingAfterInput
                                    filter.Task.PendingAfter
                                    (fun pendingAfter ->
                                        setFilter
                                            { filter with
                                                Task =
                                                    { filter.Task with
                                                        PendingAfter = pendingAfter
                                                    }
                                            })

                                TaskForm.MissedAfterInput
                                    filter.Task.MissedAfter
                                    (fun missedAfter ->
                                        setFilter
                                            { filter with
                                                Task =
                                                    { filter.Task with
                                                        MissedAfter = missedAfter
                                                    }
                                            })
                            ])
                    ]
            |}
