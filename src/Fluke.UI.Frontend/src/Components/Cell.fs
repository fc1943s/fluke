namespace Fluke.UI.Frontend.Components

open Fable.React
open Fable.Core.JsInterop
open Feliz
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module Cell =
    open UserInteraction
    open State

    [<ReactComponent>]
    let Cell
        (input: {| Username: Username
                   TaskId: TaskId
                   DateId: DateId
                   SemiTransparent: bool |})
        =
        Profiling.addCount "CellComponent.render"

        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)
        let isTesting = Recoil.useValue Atoms.isTesting
        let databaseId = Recoil.useValue (Atoms.Task.databaseId input.TaskId)
        let access = Recoil.useValue (Selectors.Database.access databaseId)
        let status, setStatus = Recoil.useState (Selectors.Cell.status (input.Username, input.TaskId, input.DateId))
        let sessions = Recoil.useValue (Atoms.Cell.sessions (input.TaskId, input.DateId))
        let attachments = Recoil.useValue (Atoms.Cell.attachments (input.TaskId, input.DateId))
        let showUser = Recoil.useValue (Selectors.Task.showUser input.TaskId)
        let isToday = Recoil.useValue (Selectors.FlukeDate.isToday (input.DateId |> DateId.Value))
        let cellMenuOpened, setCellMenuOpened = Recoil.useState (Atoms.User.cellMenuOpened input.Username)
        let isCurrentCellMenuOpened = cellMenuOpened = Some (input.TaskId, input.DateId)

        let selected, setSelected =
            Recoil.useState (Selectors.Cell.selected (input.Username, input.TaskId, input.DateId))
        //            let gun = Recoil.useValue Atoms.gun

        let onCellClick =
            Recoil.useCallbackRef
                (fun setter _ ->
                    promise {
                        let! ctrlPressed = setter.snapshot.getPromise Atoms.ctrlPressed
                        let! shiftPressed = setter.snapshot.getPromise Atoms.shiftPressed

                        if ctrlPressed || shiftPressed then
                            setSelected (not selected)
                            setCellMenuOpened None
                        else
                            setSelected false

                            if isCurrentCellMenuOpened then
                                setCellMenuOpened None
                            elif access = Some Access.ReadWrite then
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
            (fun x ->
                if isTesting then
                    x?``data-testid`` <- $"cell-{input.TaskId}-{
                                                                    (input.DateId |> DateId.Value |> FlukeDate.DateTime)
                                                                        .ToShortDateString ()
                    }"

                x.onClick <- onCellClick
                x.width <- $"{cellSize}px"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"

                x.backgroundColor <-
                    (TempUI.cellStatusColor status)
                    + (if isToday then "aa"
                       elif input.SemiTransparent then "d9"
                       else "")

                x.position <- "relative"
                x.textAlign <- "center"
                x.borderColor <- if selected then "#ffffffAA" else "transparent"
                x.borderWidth <- "1px"

                if access = Some Access.ReadWrite then
                    x.cursor <- "pointer"
                    x._hover <- JS.newObj (fun x -> x.borderColor <- "#ffffff55"))
            [
                if isCurrentCellMenuOpened then
                    Chakra.stack
                        (fun x ->
                            x.spacing <- "0"
                            x.borderWidth <- "1px"
                            x.borderColor <- TempUI.cellStatusColor Disabled
                            x.position <- "absolute"
                            x.top <- "10px"
                            x.left <- "10px"
                            x.zIndex <- 1
                            x.boxShadow <- "0px 0px 2px 1px #262626")
                        [
                            Chakra.simpleGrid
                                (fun x ->
                                    x.columns <- 2
                                    x.width <- "30px")
                                [
                                    yield!
                                        selectableStatusList
                                        |> List.map
                                            (fun status ->
                                                let color = TempUI.manualCellStatusColor status

                                                let tooltipLabel =
                                                    match status with
                                                    | Postponed until ->
                                                        Chakra.box
                                                            (fun _ -> ())
                                                            [
                                                                match until with
                                                                | None -> "Postpone until tomorrow"
                                                                | Some until ->
                                                                    $"Postpone until X (30s remaining) {until}"
                                                                |> str
                                                            ]
                                                    | Completed ->
                                                        Chakra.box
                                                            (fun _ -> ())
                                                            [
                                                                str "Complete"
                                                            ]
                                                    | Dismissed ->
                                                        Chakra.box
                                                            (fun _ -> ())
                                                            [
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "Dismiss"
                                                                    ]
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str """???"""
                                                                    ]
                                                            ]
                                                    | Scheduled ->
                                                        Chakra.box
                                                            (fun x -> x.padding <- "4px")
                                                            [
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        str "Schedule"
                                                                    ]
                                                                Chakra.box
                                                                    (fun x -> x.marginTop <- "8px")
                                                                    [
                                                                        str
                                                                            """Manually schedule a task,
overriding any other behavior.
"""
                                                                    ]
                                                            ]


                                                Tooltip.wrap
                                                    tooltipLabel
                                                    [
                                                        Chakra.iconButton
                                                            (fun x ->
                                                                x._hover <- JS.newObj (fun x -> x.opacity <- 0.8)
                                                                x.variant <- "outline"
                                                                x.backgroundColor <- color
                                                                x.border <- "0"
                                                                x.minWidth <- "15px"
                                                                x.height <- "15px"
                                                                x.borderRadius <- "0"

                                                                x.onClick <-
                                                                    fun _ ->
                                                                        promise {
                                                                            setStatus (
                                                                                UserStatus (input.Username, status)
                                                                            )
                                                                        })
                                                            []
                                                    ])
                                ]
                            match status with
                            | UserStatus (_, status) when selectableStatusList |> List.contains status ->
                                Tooltip.wrap
                                    (str "Clear")
                                    [
                                        Chakra.iconButton
                                            (fun x ->
                                                x.icon <- Icons.md.MdClear |> Icons.render
                                                x.backgroundColor <- TempUI.cellStatusColor Disabled
                                                x._hover <- JS.newObj (fun x -> x.opacity <- 0.8)
                                                x.variant <- "outline"
                                                x.border <- "0"
                                                x.width <- "30px"
                                                x.height <- "15px"
                                                x.borderRadius <- "0"
                                                x.onClick <- fun _ -> promise { setStatus Disabled })
                                            []
                                    ]
                            | _ -> ()
                        ]
                CellSessionIndicator.CellSessionIndicator
                    {|
                        Status = status
                        Sessions = sessions
                    |}
                if not selected then
                    CellBorder.CellBorder
                        {|
                            Username = input.Username
                            Date = input.DateId |> DateId.Value
                        |}
                if showUser then
                    match status with
                    | UserStatus (username, _manualCellStatus) ->
                        CellStatusUserIndicator.CellStatusUserIndicator {| Username = username |}
                    | _ -> ()

                TooltipPopup.TooltipPopup
                    {|
                        Username = input.Username
                        Attachments = attachments
                    |}
            ]
