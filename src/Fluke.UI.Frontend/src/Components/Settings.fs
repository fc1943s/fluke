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
        let weekStart, setWeekStart = Store.useState Atoms.User.weekStart
        let color, setColor = Store.useState Atoms.User.color
        let debug = Store.useValue Atoms.debug

        Accordion.Accordion
            {|
                Props = props
                Atom = Atoms.User.accordionFlag (TextKey (nameof Settings))
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
                                                                                        Atoms.User.color
                                                                                )
                                                                            )

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
                                                                        x.label <- str "Color"
                                                                        x.color <- color |> Option.defaultValue ""
                                                                        x.fontWeight <- "bold"
                                                                        x.isReadOnly <- true
                                                            |}
                                                    ]
                                        Body =
                                            fun _onHide ->
                                                [
                                                    ColorPicker.render
                                                        {|
                                                            color = color |> Option.defaultValue ""
                                                            onChange =
                                                                fun color -> setColor (Some (color.hex.ToUpper ()))
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
                                        Atom = Atoms.User.enableCellPopover
                                        Label = Some "Enable Cell Click Popup"
                                        Props = fun _ -> ()
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
