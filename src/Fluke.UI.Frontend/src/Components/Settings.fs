namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.Extras
open Fable.React
open Fable.Core.JsInterop
open Feliz
open System
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.Model
open Fluke.Shared


module Settings =
    [<ReactComponent>]
    let ColorDropdown color setColor label =
        Dropdown.Dropdown
            {|
                Tooltip = ""
                Left = false
                Trigger =
                    fun visible setVisible ->
                        UI.box
                            (fun x -> x.position <- "relative")
                            [
                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.fixedValue <- color |> Color.Value |> Some

                                                x.rightButton <-
                                                    Some (

                                                        Button.Button
                                                            {|
                                                                Hint = None
                                                                Icon =
                                                                    Some (
                                                                        Icons.io5.IoCaretDown |> Icons.render,
                                                                        Button.IconPosition.Left
                                                                    )
                                                                Props =
                                                                    fun x ->
                                                                        x.borderRadius <- "0 5px 5px 0"

                                                                        x.right <- "0"
                                                                        x.top <- "0"
                                                                        x.bottom <- "0"
                                                                        x.minWidth <- "26px"

                                                                        x.onClick <-
                                                                            (fun _ ->
                                                                                promise { setVisible (not visible) })
                                                                Children = []
                                                            |}
                                                    )
                                        Props =
                                            fun x ->
                                                x.label <- str label
                                                x.color <- color |> Color.Value
                                                x.fontWeight <- "bold"
                                                x.isReadOnly <- true

                                                x.onChange <-
                                                    fun (e: KeyboardEvent) ->
                                                        promise {
                                                            match e.Value with
                                                            | value when (JSe.RegExp @"#[a-fA-F0-9]{6}").Test value ->
                                                                setColor (Color value)
                                                            | _ -> ()
                                                        }
                                    |}
                            ]
                Body =
                    fun _onHide ->
                        [
                            ColorPicker.render
                                {|
                                    color = color |> Color.Value
                                    onChange = fun color -> setColor (Color (color.hex.ToUpper ()))
                                |}
                        ]
            |}

    [<ReactComponent>]
    let ColorDropdownAtom atom label =
        let value, setValue = Store.useState atom
        ColorDropdown value setValue label


    [<ReactComponent>]
    let rec Settings props =
        let weekStart, setWeekStart = Store.useState Atoms.User.weekStart
        let debug = Store.useValue Atoms.debug

        let userColor, setUserColor = Store.useState Atoms.User.userColor

        Accordion.Accordion
            {|
                Props = props
                Atom = Atoms.User.accordionFlag AccordionType.Settings
                Items =
                    [
                        "User",
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

                                ColorDropdown
                                    (userColor |> Option.defaultValue Color.Default)
                                    (Some >> setUserColor)
                                    "User Color"

                                ChangeUserPasswordButton.ChangeUserPasswordButton ()
                            ])

                        "View",
                        (UI.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Dropdown.Dropdown
                                    {|
                                        Tooltip = ""
                                        Left = false
                                        Trigger =
                                            fun visible setVisible ->
                                                UI.box
                                                    (fun x -> x.position <- "relative")
                                                    [
                                                        Input.Input
                                                            {|
                                                                CustomProps =
                                                                    fun x ->
                                                                        x.atom <-
                                                                            Some (
                                                                                Store.InputAtom (
                                                                                    Store.AtomReference.Atom
                                                                                        Atoms.User.weekStart
                                                                                )
                                                                            )

                                                                        x.onFormat <- Some Enum.name

                                                                        x.rightButton <-
                                                                            Some (

                                                                                Button.Button
                                                                                    {|
                                                                                        Hint = None
                                                                                        Icon =
                                                                                            Some (
                                                                                                Icons.io5.IoCaretDown
                                                                                                |> Icons.render,
                                                                                                Button.IconPosition.Left
                                                                                            )
                                                                                        Props =
                                                                                            fun x ->
                                                                                                x.borderRadius <-
                                                                                                    "0 5px 5px 0"

                                                                                                x.right <- "0"
                                                                                                x.top <- "0"
                                                                                                x.bottom <- "0"
                                                                                                x.minWidth <- "26px"

                                                                                                x.onClick <-
                                                                                                    (fun _ ->
                                                                                                        promise {
                                                                                                            setVisible (
                                                                                                                not
                                                                                                                    visible
                                                                                                            )
                                                                                                        })
                                                                                        Children = []
                                                                                    |}
                                                                            )
                                                                Props =
                                                                    fun x ->
                                                                        x.label <- str "Week Start"
                                                                        x.isReadOnly <- true
                                                            |}
                                                    ]
                                        Body =
                                            fun onHide ->
                                                [
                                                    UI.stack
                                                        (fun x ->
                                                            x.flex <- "1"
                                                            x.spacing <- "1px"
                                                            x.padding <- "1px"
                                                            x.marginBottom <- "6px"
                                                            x.maxHeight <- "217px"
                                                            x.overflowY <- "auto"
                                                            x.flexBasis <- 0)
                                                        [
                                                            yield!
                                                                Enum.ToList<DayOfWeek> ()
                                                                |> List.sortBy (
                                                                    int
                                                                    >> fun x -> if int weekStart >= x then x * x else x
                                                                )
                                                                |> Seq.map
                                                                    (fun dayOfWeek ->
                                                                        DropdownMenuButton.DropdownMenuButton
                                                                            {|
                                                                                Label = Enum.name dayOfWeek
                                                                                OnClick =
                                                                                    fun () ->
                                                                                        promise {
                                                                                            setWeekStart dayOfWeek

                                                                                            onHide ()
                                                                                        }
                                                                                Checked = weekStart = dayOfWeek
                                                                            |}

                                                                        )
                                                        ]
                                                ]
                                    |}


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

                        "Cell Colors",
                        (UI.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                ColorDropdownAtom Atoms.User.cellColorDisabled "Disabled"
                                ColorDropdownAtom Atoms.User.cellColorSuggested "Suggested"
                                ColorDropdownAtom Atoms.User.cellColorPending "Pending"
                                ColorDropdownAtom Atoms.User.cellColorMissed "Missed"
                                ColorDropdownAtom Atoms.User.cellColorMissedToday "Missed Today"
                                ColorDropdownAtom Atoms.User.cellColorPostponedUntil "Postponed Until"
                                ColorDropdownAtom Atoms.User.cellColorPostponed "Postponed"
                                ColorDropdownAtom Atoms.User.cellColorCompleted "Completed"
                                ColorDropdownAtom Atoms.User.cellColorDismissed "Dismissed"
                                ColorDropdownAtom Atoms.User.cellColorScheduled "Scheduled"
                            ])

                        "Connection",
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
