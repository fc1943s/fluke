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


module Settings =
    [<ReactComponent>]
    let rec Settings props =
        let weekStart, setWeekStart = Store.useState Atoms.weekStart
        let color, setColor = Store.useState Atoms.color
        let debug = Store.useValue Atoms.debug
        let darkMode = Store.useValue Atoms.darkMode

        Accordion.Accordion
            {|
                Props = props
                Atom = Atoms.accordionFlag (TextKey (nameof Settings))
                Items =
                    [
                        "User",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (Store.InputAtom (Store.AtomReference.Atom Atoms.dayStart))

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
                                                        Store.InputAtom (Store.AtomReference.Atom Atoms.sessionDuration)
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
                                                            Store.AtomReference.Atom Atoms.sessionBreakDuration
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

                                Dropdown.Dropdown
                                    {|
                                        Tooltip = ""
                                        Left = false
                                        Trigger =
                                            fun visible setVisible ->
                                                Chakra.box
                                                    (fun x -> x.position <- "relative")
                                                    [
                                                        Input.Input
                                                            {|
                                                                CustomProps =
                                                                    fun x ->
                                                                        x.atom <-
                                                                            Some (
                                                                                Store.InputAtom (
                                                                                    Store.AtomReference.Atom Atoms.color
                                                                                )
                                                                            )
                                                                Props =
                                                                    fun x ->
                                                                        x.label <- str "Color"
                                                                        x.color <- color
                                                                        x.fontWeight <- "bold"
                                                                        x.isReadOnly <- true
                                                            |}

                                                        Chakra.stack
                                                            (fun x ->
                                                                x.position <- "absolute"
                                                                x.right <- "1px"
                                                                x.top <- "0"
                                                                x.height <- "100%"
                                                                x.placeContent <- "flex-end"
                                                                x.spacing <- "0")
                                                            [
                                                                Button.Button
                                                                    {|
                                                                        Hint = None
                                                                        Icon =
                                                                            Some (
                                                                                Icons.fi.FiChevronDown |> Icons.wrap,
                                                                                Button.IconPosition.Left
                                                                            )
                                                                        Props =
                                                                            fun x ->
                                                                                x.borderRadius <- "0 5px 5px 0"
                                                                                x.minWidth <- "26px"
                                                                                x.height <- "28px"
                                                                                x.marginBottom <- "1px"

                                                                                x.borderLeftWidth <- "1px"
                                                                                x.borderLeftColor <- if darkMode then "#484848" else "#b7b7b7"

                                                                                x.onClick <-
                                                                                    (fun _ ->
                                                                                        promise {
                                                                                            setVisible (not visible) })
                                                                        Children = []
                                                                    |}
                                                            ]
                                                    ]
                                        Body =
                                            fun _onHide ->
                                                [
                                                    ColorPicker.render
                                                        {|
                                                            color = color
                                                            onChange = fun color -> setColor (color.hex.ToUpper ())
                                                        |}
                                                ]
                                    |}

                                ChangeUserPasswordButton.ChangeUserPasswordButton ()
                            ])

                        "View",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Dropdown.Dropdown
                                    {|
                                        Tooltip = ""
                                        Left = false
                                        Trigger =
                                            fun visible setVisible ->
                                                Chakra.box
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
                                                                                        Atoms.weekStart
                                                                                )
                                                                            )

                                                                        x.onFormat <- Some Enum.name
                                                                Props =
                                                                    fun x ->
                                                                        x.label <- str "Week Start"
                                                                        x.isReadOnly <- true
                                                            |}

                                                        Chakra.stack
                                                            (fun x ->
                                                                x.position <- "absolute"
                                                                x.right <- "1px"
                                                                x.top <- "0"
                                                                x.height <- "100%"
                                                                x.placeContent <- "flex-end"
                                                                x.spacing <- "0")
                                                            [
                                                                Button.Button
                                                                    {|
                                                                        Hint = None
                                                                        Icon =
                                                                            Some (
                                                                                Icons.fi.FiChevronDown |> Icons.wrap,
                                                                                Button.IconPosition.Left
                                                                            )
                                                                        Props =
                                                                            fun x ->
                                                                                x.borderRadius <- "0 5px 5px 0"
                                                                                x.minWidth <- "26px"
                                                                                x.height <- "28px"
                                                                                x.marginBottom <- "1px"

                                                                                x.borderLeftWidth <- "1px"
                                                                                x.borderLeftColor <- if darkMode then "#484848" else "#b7b7b7"

                                                                                x.onClick <-
                                                                                    (fun _ ->
                                                                                        promise {
                                                                                            setVisible (not visible) })
                                                                        Children = []
                                                                    |}
                                                            ]
                                                    ]
                                        Body =
                                            fun onHide ->
                                                [
                                                    Chakra.stack
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
                                                    Some (Store.InputAtom (Store.AtomReference.Atom Atoms.daysBefore))

                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Days Before"
                                    |}


                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (Store.InputAtom (Store.AtomReference.Atom Atoms.daysAfter))

                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Days After"
                                    |}


                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (Store.InputAtom (Store.AtomReference.Atom Atoms.cellSize))

                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Cell Size"
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.darkMode
                                        Label = Some "Dark Mode"
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

                        "Connection",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                InputList.InputList
                                    (fun x ->
                                        x.label <- str "Gun peers"

                                        x.atom <- Some (Store.InputAtom (Store.AtomReference.Atom Store.Atoms.gunPeers)))
                            ])
                    ]
            |}
