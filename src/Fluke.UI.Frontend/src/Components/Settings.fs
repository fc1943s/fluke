namespace Fluke.UI.Frontend.Components

open Fable.React
open Fable.Core.JsInterop
open Feliz
open System
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.Model
open Fluke.Shared
open Fable.Core


module Settings =
    [<ReactComponent>]
    let rec Settings props =
        let weekStart, setWeekStart = Store.useState Atoms.User.weekStart
        let debug = Store.useValue Atoms.debug

        let userColor, setUserColor = Store.useState Atoms.User.userColor

        Accordion.Accordion
            {|
                Props = props
                Atom = Atoms.User.accordionHiddenFlag AccordionType.Settings
                Items =
                    [
                        str "User",
                        (UI.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (Store.AtomReference.Atom Atoms.User.dayStart)
                                                    )

                                                x.inputFormat <- Some Input.InputFormat.Time
                                                x.onFormat <- Some FlukeTime.Stringify

                                                x.onValidate <-
                                                    Some (
                                                        fst
                                                        >> DateTime.TryParse
                                                        >> function
                                                            | true, value -> value
                                                            | _ -> DateTime.Parse "00:00"
                                                        >> FlukeTime.FromDateTime
                                                        >> Some
                                                    )
                                        Props =
                                            fun x ->
                                                x.label <- str "Day Start"
                                                x.alignSelf <- "flex-start"
                                                x.placeholder <- "00:00"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (
                                                            Store.AtomReference.Atom Atoms.User.sessionDuration
                                                        )
                                                    )

                                                x.inputFormat <- Some Input.InputFormat.Number
                                                x.onFormat <- Some (Minute.Value >> string)

                                                x.onValidate <-
                                                    Some (
                                                        fst
                                                        >> String.parseIntMin 1
                                                        >> Option.defaultValue 1
                                                        >> Minute
                                                        >> Some
                                                    )
                                        Props = fun x -> x.label <- str "Session Duration"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (
                                                            Store.AtomReference.Atom Atoms.User.sessionBreakDuration
                                                        )
                                                    )

                                                x.inputFormat <- Some Input.InputFormat.Number
                                                x.onFormat <- Some (Minute.Value >> string)

                                                x.onValidate <-
                                                    Some (
                                                        fst
                                                        >> String.parseIntMin 1
                                                        >> Option.defaultValue 1
                                                        >> Minute
                                                        >> Some
                                                    )
                                        Props = fun x -> x.label <- str "Session Break Duration"
                                    |}

                                Dropdown.ColorDropdown
                                    (userColor |> Option.defaultValue Color.Default)
                                    (Some >> setUserColor)
                                    (fun x -> x.label <- str "User Color")

                                ChangeUserPasswordButton.ChangeUserPasswordButton ()

                                // DeleteUserButton.DeleteUserButton ()
                            ])

                        str "View",
                        (UI.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Dropdown.EnumDropdown<DayOfWeek>
                                    weekStart
                                    setWeekStart
                                    (fun x -> x.label <- str "Week Start")

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (Store.AtomReference.Atom Atoms.User.daysBefore)
                                                    )

                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Days Before"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (Store.AtomReference.Atom Atoms.User.daysAfter)
                                                    )

                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Days After"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (Store.AtomReference.Atom Atoms.User.cellSize)
                                                    )

                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Cell Size"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (Store.AtomReference.Atom Atoms.User.fontSize)
                                                    )

                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Font Size"
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.User.darkMode
                                        Label = Some "Dark Mode"
                                        Props = fun _ -> ()
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.User.systemUiFont
                                        Label = Some "System UI Font"
                                        Props = fun _ -> ()
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.User.filterTasksByView
                                        Label = Some "Filter Tasks by View"
                                        Props = fun _ -> ()
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.User.enableCellPopover
                                        Label = Some "Enable Cell Click Popup"
                                        Props = fun _ -> ()
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.debug
                                        Label = Some "Show Debug Information"
                                        Props =
                                            fun x ->
                                                x.onClick <-
                                                    fun _ ->
                                                        promise {
                                                            match JS.window id with
                                                            | None -> ()
                                                            | Some window -> window?Debug <- not debug
                                                        }
                                    |}
                            ])

                        str "Cell Colors",
                        (UI.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorDisabled
                                    (fun x -> x.label <- str "Disabled")

                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorSuggested
                                    (fun x -> x.label <- str "Suggested")

                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorPending
                                    (fun x -> x.label <- str "Pending")

                                Dropdown.ColorDropdownAtom Atoms.User.cellColorMissed (fun x -> x.label <- str "Missed")

                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorMissedToday
                                    (fun x -> x.label <- str "Missed Today")

                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorPostponedUntil
                                    (fun x -> x.label <- str "Postponed Until")

                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorPostponed
                                    (fun x -> x.label <- str "Postponed")

                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorCompleted
                                    (fun x -> x.label <- str "Completed")

                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorDismissed
                                    (fun x -> x.label <- str "Dismissed")

                                Dropdown.ColorDropdownAtom
                                    Atoms.User.cellColorScheduled
                                    (fun x -> x.label <- str "Scheduled")
                            ])

                        str "Connection",
                        (UI.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                InputList.InputList
                                    (fun x ->
                                        x.label <- str "Gun peers"
                                        x.atom <- Some (Store.InputAtom (Store.AtomReference.Atom Store.Atoms.gunPeers)))
                            ])
                    ]
            |}
