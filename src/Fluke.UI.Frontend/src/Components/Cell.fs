namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module Cell =
    open UserInteraction
    open State

    [<ReactComponent>]
    let Cell
        (input: {| Username: Username
                   TaskId: Recoil.Atoms.Task.TaskId
                   DateId: DateId
                   SemiTransparent: bool |})
        =
        Profiling.addCount "CellComponent.render"

        let (DateId referenceDay) = input.DateId
        let isTesting = Recoil.useValue Recoil.Atoms.isTesting
        let status = Recoil.useValue (Recoil.Atoms.Cell.status (input.TaskId, input.DateId))
        let sessions = Recoil.useValue (Recoil.Atoms.Cell.sessions (input.TaskId, input.DateId))
        let attachments = Recoil.useValue (Recoil.Atoms.Cell.attachments (input.TaskId, input.DateId))
        let showUser = Recoil.useValue (Recoil.Selectors.Task.showUser input.TaskId)
        let isToday = Recoil.useValue (Recoil.Selectors.FlukeDate.isToday referenceDay)
        let selected, setSelected = Recoil.useState (Recoil.Selectors.Cell.selected (input.TaskId, input.DateId))
        let cellMenuOpened, setCellMenuOpened = Recoil.useState Recoil.Atoms.cellMenuOpened
        let isCurrentCellMenuOpened = cellMenuOpened = Some (input.TaskId, input.DateId)
        //            let gun = Recoil.useValue Recoil.Atoms.gun

        let onCellClick =
            Recoil.useCallbackRef
                (fun setter ->
                    promise {
                        let! ctrlPressed = setter.snapshot.getPromise Recoil.Atoms.ctrlPressed
                        let! shiftPressed = setter.snapshot.getPromise Recoil.Atoms.shiftPressed

                        if ctrlPressed || shiftPressed then
                            setSelected (not selected)
                            setCellMenuOpened None
                        else
                            setSelected false

                            if isCurrentCellMenuOpened then
                                setCellMenuOpened None
                            else
                                setCellMenuOpened (Some (input.TaskId, input.DateId))

                    //                gun.get("test").get("test2").put(1)
                    })

        let selectableStatusList =
            [
                Completed
                Postponed None
                Dismissed
                Scheduled
            ]

        Chakra.center
            {|
                ``data-testid`` =
                    if isTesting then
                        Some $"cell-{input.TaskId}-{referenceDay.DateTime.ToShortDateString ()}"
                    else
                        None
                onClick = (fun (_event: MouseEvent) -> onCellClick ())
                width = "17px"
                height = "17px"
                lineHeight = "17px"
                backgroundColor =
                    (TempUI.cellStatusColor status)
                    + (if isToday then
                           "aa"
                       elif input.SemiTransparent then
                           "d9"
                       else
                           "")
                position = "relative"
                textAlign = "center"
                border =
                    if selected then
                        "1px solid #ffffff55 !important"
                    else
                        "none"
            |}
            [
                if isCurrentCellMenuOpened then
                    Chakra.stack
                        {|
                            spacing = 0
                            borderWidth = "1px"
                            borderColor = TempUI.cellStatusColor Disabled
                            position = "absolute"
                            top = "10px"
                            left = "10px"
                            zIndex = 1
                            boxShadow = "0px 0px 2px 1px #262626"
                        |}
                        [
                            Chakra.simpleGrid
                                {| columns = 2 |}
                                [
                                    yield!
                                        selectableStatusList
                                        |> List.map
                                            (fun status ->
                                                let color = TempUI.manualCellStatusColor status

                                                Chakra.tooltip
                                                    {|
                                                        bg = "gray.10"
                                                        label =
                                                            match status with
                                                            | Postponed until ->
                                                                Chakra.box
                                                                    ()
                                                                    [
                                                                        match until with
                                                                        | None -> "Postpone until tomorrow"
                                                                        | Some until ->
                                                                            $"Postpone until X (30s remaining) {until}"
                                                                        |> str
                                                                    ]
                                                            | Completed ->
                                                                Chakra.box
                                                                    ()
                                                                    [
                                                                        str "Complete"
                                                                    ]
                                                            | Dismissed ->
                                                                Chakra.box
                                                                    ()
                                                                    [
                                                                        Chakra.box
                                                                            ()
                                                                            [
                                                                                str "Dismiss"
                                                                            ]
                                                                        Chakra.box
                                                                            ()
                                                                            [
                                                                                str """???"""
                                                                            ]
                                                                    ]
                                                            | Scheduled ->
                                                                Chakra.box
                                                                    {| padding = "4px" |}
                                                                    [
                                                                        Chakra.box
                                                                            ()
                                                                            [
                                                                                str "Schedule"
                                                                            ]
                                                                        Chakra.box
                                                                            {| marginTop = "8px" |}
                                                                            [
                                                                                str
                                                                                    """Manually schedule a task,
overriding any other behavior.
"""
                                                                            ]
                                                                    ]
                                                        hasArrow = true
                                                        placement =
                                                            match status with
                                                            | Postponed _ -> "right"
                                                            | Completed -> "left"
                                                            | Dismissed -> "left"
                                                            | Scheduled -> "right"
                                                        zIndex = 20000
                                                    |}
                                                    [
                                                        Chakra.box
                                                            {|
                                                                height = "15px"
                                                                width = "15px"
                                                                backgroundColor = color
                                                                cursor = "pointer"
                                                            |}
                                                            []
                                                    ])
                                ]
                            match status with
                            | UserStatus (_, status) when selectableStatusList |> List.contains status ->
                                Chakra.tooltip
                                    {|
                                        bg = "gray.10"
                                        label = "Clear"
                                        placement = "bottom"
                                        hasArrow = true
                                    |}
                                    [
                                        Chakra.iconButton
                                            {|
                                                icon = Icons.mdClear ()
                                                backgroundColor = TempUI.cellStatusColor Disabled
                                                _hover = {| opacity = 0.8 |}
                                                variant = "outline"
                                                border = 0
                                                width = "30px"
                                                height = "15px"
                                                borderRadius = 0
                                                onClick = fun () -> ()
                                            |}
                                            []
                                    ]
                            | _ -> ()
                        ]
                CellSessionIndicator.CellSessionIndicator {| Status = status; Sessions = sessions |}
                if not selected then
                    CellBorder.CellBorder
                        {|
                            Username = input.Username
                            Date = referenceDay
                        |}
                if showUser then
                    match status with
                    | UserStatus (user, _manualCellStatus) -> CellStatusUserIndicator.CellStatusUserIndicator user
                    | _ -> ()

                TooltipPopup.TooltipPopup attachments
            ]
