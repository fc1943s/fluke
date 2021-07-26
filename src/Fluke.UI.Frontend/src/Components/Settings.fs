namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.Model
open Fluke.Shared
open Fluke.UI.Frontend.State.State


module Settings =
    [<ReactComponent>]
    let GunPeersInput () =
        let tempGunPeers =
            Store.Hooks.useTempAtom
                (Some (Store.InputAtom (Store.AtomReference.Atom Store.Atoms.gunPeers)))
                (Some (Store.InputScope.Temp Gun.defaultSerializer))

        UI.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint =
                            Some (
                                UI.box
                                    (fun _ -> ())
                                    [
                                        UI.box
                                            (fun _ -> ())
                                            [
                                                str "Add a relay peer to sync data between devices"
                                            ]

                                        br []

                                        ExternalLink.ExternalLink
                                            {|
                                                Link = str "Read documentation"
                                                Href =
                                                    "https://gun.eco/docs/FAQ#what-is-the-difference-between-super-peer-and-other-peers"
                                                Props = fun _ -> ()
                                            |}
                                    ]
                            )
                        HintTitle = None
                        Label = str "Gun relay peers"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                UI.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            None
                            (fun x ->
                                x.isChecked <-
                                    match tempGunPeers.CurrentValue, tempGunPeers.TempValue with
                                    | Some _, _ -> true
                                    | _ -> false

                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            match tempGunPeers.CurrentValue, tempGunPeers.TempValue with
                                            | Some _, Some _ -> tempGunPeers.SetTempValue None
                                            | Some current, None ->
                                                tempGunPeers.SetTempValue (Some current)
                                                tempGunPeers.SetCurrentValue None
                                            | None, Some temp ->
                                                tempGunPeers.SetCurrentValue (Some temp)
                                                tempGunPeers.SetTempValue None
                                            | None, None -> tempGunPeers.SetTempValue (Some [||])
                                        })

                        InputList.InputList
                            (fun x ->
                                x.isDisabled <-
                                    match tempGunPeers.CurrentValue, tempGunPeers.TempValue with
                                    | Some _, _ -> true
                                    | _ -> false)
                            (fun _ -> ())
                            (tempGunPeers.TempValue
                             |> Option.defaultValue (
                                 tempGunPeers.CurrentValue
                                 |> Option.defaultValue [||]
                             ))
                            (Some >> tempGunPeers.SetTempValue)
                    ]
            ]

    [<ReactComponent>]
    let ApiUrlInput () =
        let tempApiUrl =
            Store.Hooks.useTempAtom
                (Some (Store.InputAtom (Store.AtomReference.Atom Store.Atoms.apiUrl)))
                (Some (Store.InputScope.Temp Gun.defaultSerializer))

        UI.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "API URL"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                UI.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            None
                            (fun x ->
                                x.isChecked <-
                                    match tempApiUrl.CurrentValue, tempApiUrl.TempValue with
                                    | Some _, _ -> true
                                    | _ -> false

                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            match tempApiUrl.CurrentValue, tempApiUrl.TempValue with
                                            | Some _, Some _ -> tempApiUrl.SetTempValue None
                                            | Some current, None ->
                                                tempApiUrl.SetTempValue (Some current)
                                                tempApiUrl.SetCurrentValue None
                                            | None, Some temp ->
                                                tempApiUrl.SetCurrentValue (Some temp)
                                                tempApiUrl.SetTempValue None
                                            | None, None -> tempApiUrl.SetTempValue (Some "")
                                        })

                        Input.Input
                            {|
                                Props =
                                    fun x ->

                                        x.isDisabled <-
                                            match tempApiUrl.CurrentValue, tempApiUrl.TempValue with
                                            | Some _, _ -> true
                                            | _ -> false

                                        x.onChange <-
                                            fun (e: KeyboardEvent) -> promise { tempApiUrl.SetValue (Some e.Value) }
                                CustomProps =
                                    fun x ->
                                        x.fixedValue <-
                                            tempApiUrl.TempValue
                                            |> Option.defaultValue (tempApiUrl.CurrentValue |> Option.defaultValue "")
                                            |> Some
                            |}
                    ]
            ]

    [<ReactComponent>]
    let rec Settings props =
        let weekStart, setWeekStart = Store.useState Atoms.User.weekStart
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
                                        Atom = Atoms.User.enableCellPopover
                                        Label = Some "Enable Cell Click Popup"
                                        Props = fun _ -> ()
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.Session.debug
                                        Label = Some "Show Debug Information"
                                        Props = fun _ -> ()
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
                                GunPeersInput ()

                                ApiUrlInput ()
                            ])
                    ]
            |}
