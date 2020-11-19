namespace Fluke.UI.Frontend.Components

open Feliz.MaterialUI
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

    let useStyles =
        Styles.makeStyles (fun (styles: StyleCreator<{| hovered: bool |}>) _theme ->
            {|
                root =
                    styles.create (fun props ->
                        [
                            if props.hovered then
                                style.zIndex 1
                        ])
                name =
                    styles.create (fun props ->
                        [
                            style.overflow.hidden
                            if props.hovered then
                                style.backgroundColor "#222"
                            else
                                style.whitespace.nowrap
                                style.textOverflow.ellipsis
                        ])
            |})

    let render =
        React.memo (fun (input: {| Username: Username
                                   TaskId: Recoil.Atoms.Task.TaskId
                                   DateId: DateId
                                   SemiTransparent: bool |}) ->
            Profiling.addCount "CellComponent.render"

            let (DateId referenceDay) = input.DateId
            let status = Recoil.useValue (Recoil.Atoms.Cell.status (input.TaskId, input.DateId))
            let sessions = Recoil.useValue (Recoil.Atoms.Cell.sessions (input.TaskId, input.DateId))
            let attachments = Recoil.useValue (Recoil.Atoms.Cell.attachments (input.TaskId, input.DateId))
            let showUser = Recoil.useValue (Recoil.Selectors.Task.showUser input.TaskId)
            let isToday = Recoil.useValue (Recoil.Selectors.FlukeDate.isToday referenceDay)
            let selected, setSelected = Recoil.useState (Recoil.Selectors.Cell.selected (input.TaskId, input.DateId))
            let selectedCell, setSelectedCell = Recoil.useState Recoil.Atoms.selectedCell
            let isMainSelection = selectedCell = Some (input.TaskId, input.DateId)
            //            let gun = Recoil.useValue Recoil.Atoms.gun
            let onCellClick =
                React.useCallbackRef (fun () ->
                    setSelected (not selected)
                    setSelectedCell (Some (input.TaskId, input.DateId))
                    //                gun.get("test").get("test2").put(1)
                    )

            Chakra.center
                {|
                    ``data-testid`` = sprintf "cell-%A-%A" input.TaskId (referenceDay.DateTime.ToShortDateString ())
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
                    if isMainSelection then
                        Chakra.box
                            ()
                            [
                                Chakra.simpleGrid
                                    {|
                                        columns = 2
                                        borderWidth = "1px"
                                        borderColor = TempUI.cellStatusColor Disabled
                                        position = "absolute"
                                        top = "10px"
                                        left = "10px"
                                        zIndex = 1
                                        boxShadow = "0px 0px 2px 1px #262626"
                                    |}
                                    [
                                        yield!
                                            [
                                                Completed
                                                Postponed None
                                                Dismissed
                                                Scheduled
                                            ]
                                            |> List.map (fun status ->
                                                let color = TempUI.manualCellStatusColor status

                                                Chakra.tooltip
                                                    {|
                                                        bg = "gray.10%"
                                                        label =
                                                            match status with
                                                            | Postponed until ->
                                                                Chakra.box
                                                                    ()
                                                                    [
                                                                        match until with
                                                                        | None -> "Postponed until tomorrow"
                                                                        | Some until ->
                                                                            sprintf
                                                                                "Postponed until X (30s remaining) %A"
                                                                                until
                                                                        |> str
                                                                    ]
                                                            | Completed ->
                                                                Chakra.box
                                                                    ()
                                                                    [
                                                                        str "Completed"
                                                                    ]
                                                            | Dismissed ->
                                                                Chakra.box
                                                                    ()
                                                                    [
                                                                        Chakra.box
                                                                            ()
                                                                            [
                                                                                str "Dismissed"
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
                                                                                str "Scheduled"
                                                                            ]
                                                                        Chakra.box
                                                                            {| marginTop = "8px" |}
                                                                            [
                                                                                str """Manually schedule a task,
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
                            ]
                    CellSessionIndicator.render {| Status = status; Sessions = sessions |}
                    if not selected then
                        CellBorder.render
                            {|
                                Username = input.Username
                                Date = referenceDay
                            |}
                    if showUser then
                        match status with
                        | UserStatus (user, manualCellStatus) -> CellStatusUserIndicator.render {| User = user |}
                        | _ -> ()

                    TooltipPopup.render {| Attachments = attachments |}
                ])
