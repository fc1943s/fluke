namespace Fluke.UI.Frontend.Components

open FsCore
open FsJs
open FsCore.BaseModel
open FsStore.Bindings.Gun
open FsStore.Model
open FsUi.State
open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.Model
open Fluke.Shared
open Fluke.UI.Frontend.State.State
open FsUi.Components
open FsStore.State


module Settings =
    let gunPeersReference = AtomReference.Atom Atoms.gunPeers

    [<ReactComponent>]
    let GunPeersInput () =
        let tempGunPeers = Store.useTempState gunPeersReference
        let gunSync, setGunSync = Store.useState Atoms.gunSync

        Ui.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint =
                            Some (
                                Ui.box
                                    (fun _ -> ())
                                    [
                                        Ui.str "Add a relay peer to sync data between devices"

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

                Ui.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            None
                            (fun x ->
                                x.isChecked <- gunSync
                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            if not gunSync then
                                                tempGunPeers.SetCurrentValue tempGunPeers.TempValue
                                            else
                                                tempGunPeers.SetTempValue tempGunPeers.CurrentValue

                                            setGunSync (not gunSync)
                                        //                                            match tempGunOptions.CurrentValue, tempGunOptions.TempValue with
//                                            | GunOptions.Sync _, GunOptions.Sync _ ->
//                                                tempGunOptions.SetCurrentValue GunOptions.Minimal
//                                            | GunOptions.Sync peers, GunOptions.Minimal ->
//                                                tempGunOptions.SetTempValue (GunOptions.Sync peers)
//                                                tempGunOptions.SetCurrentValue GunOptions.Minimal
//                                            | GunOptions.Minimal, GunOptions.Sync peers ->
//                                                tempGunOptions.SetCurrentValue (GunOptions.Sync peers)
//                                                tempGunOptions.SetTempValue GunOptions.Minimal
//                                            | GunOptions.Minimal, GunOptions.Minimal ->
//                                                tempGunOptions.SetTempValue (GunOptions.Sync [||])
                                        })

                        InputList.InputList
                            (fun x -> x.isDisabled <- gunSync)
                            (fun _ -> ())
                            (tempGunPeers.TempValue |> Array.map GunPeer.Value)
                            (Array.map GunPeer >> tempGunPeers.SetTempValue)
                    ]
            ]

    let hubUrlsReference = AtomReference.Atom Atoms.hubUrls

    [<ReactComponent>]
    let HubUrlInput () =
        let tempHubUrls = Store.useTempState hubUrlsReference
        let hubSync, setHubSync = Store.useState Atoms.hubSync

        Ui.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Hub URL"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Ui.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            None
                            (fun x ->
                                x.isChecked <- hubSync

                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            if not hubSync then
                                                tempHubUrls.SetCurrentValue tempHubUrls.TempValue
                                            else
                                                tempHubUrls.SetTempValue tempHubUrls.CurrentValue

                                            setHubSync (not hubSync)

                                        //                                            match tempHubUrls.CurrentValue, tempHubUrls.TempValue with
//                                            | Some _, Some _ -> tempHubUrls.SetTempValue None
//                                            | Some current, None ->
//                                                tempHubUrls.SetTempValue (Some current)
//                                                tempHubUrls.SetCurrentValue None
//                                            | None, Some temp ->
//                                                tempHubUrls.SetCurrentValue (Some temp)
//                                                tempHubUrls.SetTempValue None
//                                            | None, None -> tempHubUrls.SetTempValue (Some "")
                                        })

                        InputList.InputList
                            (fun x -> x.isDisabled <- hubSync)
                            (fun _ -> ())
                            tempHubUrls.TempValue
                            tempHubUrls.SetTempValue
                    ]
            ]

    [<ReactComponent>]
    let rec Settings props =
        let weekStart, setWeekStart = Store.useState Atoms.User.weekStart
        let userColor, setUserColor = Store.useState Atoms.User.userColor
        let logLevel, setLogLevel = Store.useState Atoms.logLevel

        Accordion.AccordionAtom
            {|
                Props = props
                Atom = Atoms.User.accordionHiddenFlag AccordionType.Settings
                Items =
                    [
                        str "User",
                        (Ui.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <- Some (InputAtom (AtomReference.Atom Atoms.User.dayStart))

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
                                                    Some (InputAtom (AtomReference.Atom Atoms.User.sessionDuration))

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
                                                        InputAtom (AtomReference.Atom Atoms.User.sessionBreakDuration)
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
                        (Ui.stack
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
                                                x.atom <- Some (InputAtom (AtomReference.Atom Atoms.User.daysBefore))
                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Days Before"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <- Some (InputAtom (AtomReference.Atom Atoms.User.daysAfter))
                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Days After"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <- Some (InputAtom (AtomReference.Atom Atoms.User.cellHeight))
                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Cell Height"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <- Some (InputAtom (AtomReference.Atom Atoms.User.cellWidth))
                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Cell Width"
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <- Some (InputAtom (AtomReference.Atom Atoms.Ui.fontSize))
                                                x.inputFormat <- Some Input.InputFormat.Number
                                        Props = fun x -> x.label <- str "Font Size"
                                    |}

                                Dropdown.EnumDropdown<Logger.LogLevel>
                                    logLevel
                                    setLogLevel
                                    (fun x -> x.label <- str "Log Level")

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.Ui.darkMode
                                        Label = Some "Dark Mode"
                                        Props = fun _ -> ()
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.Ui.systemUiFont
                                        Label = Some "System UI Font"
                                        Props = fun _ -> ()
                                    |}

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.User.enableCellPopover
                                        Label = Some "Enable Cell Click Popup"
                                        Props = fun _ -> ()
                                    |}
                            ])

                        str "Cell Colors",
                        (Ui.stack
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
                        (Ui.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                GunPeersInput ()
                                HubUrlInput ()
                            ])
                    ]
            |}
