namespace Fluke.UI.Frontend.Components


open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.Model
open System
open Fluke.Shared
open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Settings =
    [<ReactComponent>]
    let rec Settings
        (input: {| Username: Username
                   Props: Chakra.IChakraProps -> unit |})
        =
        let weekStart, setWeekStart = Store.useState (Atoms.User.weekStart input.Username)
        let color, setColor = Store.useState (Atoms.User.color input.Username)

        Accordion.Accordion
            {|
                Props = input.Props
                Atom = Atoms.User.accordionFlag (input.Username, TextKey (nameof Settings))
                Items =
                    [
                        "User",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Input.Input
                                    (fun x ->
                                        x.label <- str "Day Start"
                                        x.alignSelf <- "flex-start"
                                        x.placeholder <- "00:00"

                                        x.atom <-
                                            Some (
                                                Recoil.AtomFamily (input.Username, Atoms.User.dayStart, input.Username)
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
                                            ))

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Session Duration"

                                        x.atom <-
                                            Some (
                                                Recoil.Atom (input.Username, Atoms.User.sessionDuration input.Username)
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
                                            ))

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Session Break Duration"

                                        x.atom <-
                                            Some (
                                                Recoil.Atom (
                                                    input.Username,
                                                    Atoms.User.sessionBreakDuration input.Username
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
                                            ))

                                Menu.Drawer
                                    {|
                                        Tooltip = ""
                                        Left = false
                                        Trigger =
                                            fun visible setVisible ->
                                                Chakra.box
                                                    (fun x -> x.position <- "relative")
                                                    [
                                                        Input.Input
                                                            (fun x ->
                                                                x.label <- str "Color"
                                                                x.isReadOnly <- true

                                                                x.atom <-
                                                                    Some (
                                                                        Recoil.Atom (
                                                                            input.Username,
                                                                            Atoms.User.color input.Username
                                                                        )
                                                                    ))

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
                                                                                x.borderLeftColor <- "#484848"

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
                                                            onChange = fun color -> setColor color.hex
                                                        |}
                                                ]
                                    |}

                                ChangeUserPasswordButton.ChangeUserPasswordButton ()
                            ])

                        "View",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Menu.Drawer
                                    {|
                                        Tooltip = ""
                                        Left = false
                                        Trigger =
                                            fun visible setVisible ->
                                                Chakra.box
                                                    (fun x -> x.position <- "relative")
                                                    [
                                                        Input.Input
                                                            (fun x ->
                                                                x.label <- str "Week Start"
                                                                x.isReadOnly <- true

                                                                x.atom <-
                                                                    Some (
                                                                        Recoil.Atom (
                                                                            input.Username,
                                                                            Atoms.User.weekStart input.Username
                                                                        )
                                                                    )

                                                                x.onFormat <- Some Enum.name)

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
                                                                                x.borderLeftColor <- "#484848"

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
                                                                        Menu.DrawerMenuButton
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
                                    (fun x ->
                                        x.label <- str "Days Before"

                                        x.atom <-
                                            Some (Recoil.Atom (input.Username, Atoms.User.daysBefore input.Username))

                                        x.inputFormat <- Some Input.InputFormat.Number)

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Days After"

                                        x.atom <-
                                            Some (Recoil.Atom (input.Username, Atoms.User.daysAfter input.Username))

                                        x.inputFormat <- Some Input.InputFormat.Number)

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Cell Size"

                                        x.atom <-
                                            Some (Recoil.Atom (input.Username, Atoms.User.cellSize input.Username))

                                        x.inputFormat <- Some Input.InputFormat.Number)

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.debug
                                        Label = Some "Show Debug Information"
                                        Props = fun _ -> ()
                                    |}
                            ])

                        "Connection",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                InputList.InputList
                                    (fun x ->
                                        x.label <- str "Gun peers"
                                        x.atom <- Some (Recoil.Atom (input.Username, Atoms.gunPeers)))
                            ])
                    ]
            |}
