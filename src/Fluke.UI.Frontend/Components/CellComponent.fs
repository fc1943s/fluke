namespace Fluke.UI.Frontend.Components

open Feliz.MaterialUI
open Browser.Types
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Model
open Fluke.Shared


module CellComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

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
                                   Date: FlukeDate
                                   SemiTransparent: bool |}) ->
            let cellId = Recoil.Atoms.Cell.cellId input.TaskId (DateId input.Date)
            let isToday = Recoil.useValue (Recoil.Selectors.FlukeDate.isToday input.Date)
            let showUser = Recoil.useValue (Recoil.Selectors.Task.showUser input.TaskId)
            let attachments = Recoil.useValue (Recoil.Atoms.Cell.attachments cellId)
            let sessions = Recoil.useValue (Recoil.Atoms.Cell.sessions cellId)
            let status = Recoil.useValue (Recoil.Atoms.Cell.status cellId)
            let selected, setSelected = Recoil.useState (Recoil.Selectors.Cell.selected cellId)
            let onCellClick = React.useCallbackRef (fun () -> setSelected (not selected))

            Chakra.center
                {|
                    ``data-testid`` = sprintf "cell-%A" cellId
                    onClick = (fun (_event: MouseEvent) -> onCellClick ())
                    width = "17px"
                    height = "17px"
                    lineHeight = "17px"
                    backgroundColor =
                        status.CellColor
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
                    CellBorderComponent.render {| Username = input.Username; Date = input.Date |}
                    CellSessionIndicatorComponent.render {| Status = status; Sessions = sessions |}
                    if showUser then
                        match status with
                        | UserStatus (user, manualCellStatus) ->
                            CellStatusUserIndicatorComponent.render {| User = user |}
                        | _ -> ()
                    TooltipPopupComponent.render {| Attachments = attachments |}

                ])
