namespace Fluke.UI.Frontend.Components

open Feliz.MaterialUI
open Browser.Types
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
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
                                   Date: FlukeDate |}) ->
            let cellId = Recoil.Atoms.Cell.cellId input.TaskId (DateId input.Date)
            let isToday = Recoil.useValue (Recoil.Selectors.RecoilFlukeDate.isToday input.Date)
            let showUser = Recoil.useValue (Recoil.Selectors.RecoilTask.showUser input.TaskId)
            let attachments = Recoil.useValue (Recoil.Atoms.Cell.attachments cellId)
            let sessions = Recoil.useValue (Recoil.Atoms.Cell.sessions cellId)
            let selected, setSelected = Recoil.useState (Recoil.Selectors.RecoilCell.selected cellId)
            let status = Recoil.useValue (Recoil.Atoms.Cell.status cellId)
            let onCellClick = React.useCallbackRef (fun () -> setSelected (not selected))

            Html.div [
                prop.classes [
                    status.CellClass
                    if selected then
                        Css.cellSelected
                    if isToday then
                        Css.cellToday
                ]
                prop.onClick (fun (_event: MouseEvent) -> onCellClick ())
                prop.children [
                    CellBorderComponent.render {| Username = input.Username; Date = input.Date |}
                    CellSessionIndicatorComponent.render {| Sessions = sessions |}
                    if showUser then
                        match status with
                        | UserStatus (user, manualCellStatus) ->
                            CellStatusUserIndicatorComponent.render {| User = user |}
                        | _ -> ()
                    TooltipPopupComponent.render {| Attachments = attachments |}
                ]
            ])
