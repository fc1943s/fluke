namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open FsCore
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsUi.Components
open Fluke.UI.Frontend.Hooks
open FsStore.State


module CellForm =
    [<ReactComponent>]
    let rec CellForm taskId date =
        let (TaskName taskName) = Store.useValue (Atoms.Task.name taskId)
        let attachmentIdSet = Store.useValue (Selectors.Cell.attachmentIdSet (CellRef (taskId, date)))
        let visibleTaskSelectedDateMap = Store.useValue Selectors.Session.visibleTaskSelectedDateMap

        let attachmentIdList =
            React.useMemo (
                (fun () -> attachmentIdSet |> Set.toList),
                [|
                    box attachmentIdSet
                |]
            )

        let setCellUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Cell)

        //        let setDate = Store.useCallbackRef (fun _ setter date -> promise {
//            Atom.change setter (Atoms.User.uiFlag UIFlagType.Cell) (
//                fun cellUIFlag ->
//                    match cellUIFlag with
//                                        | UIFlag.Cell (taskId, _) -> (UIFlag.Cell (taskId, date))
//                                        | uiFlag -> uiFlag
//                )
//        })

        let onAttachmentAdd =
            Store.useCallbackRef
                (fun _ setter attachmentId ->
                    promise {
                        Atom.set
                            setter
                            (Atoms.Attachment.parent attachmentId)
                            (Some (AttachmentParent.Cell (taskId, date)))
                    })

        let onAttachmentDelete =
            Store.useCallbackRef
                (fun getter _setter attachmentId ->
                    promise {
                        do! Hydrate.deleteRecord getter Atoms.Attachment.collection (attachmentId |> AttachmentId.Value)
                        return true
                    })

        Accordion.AccordionAtom
            {|
                Props = fun x -> x.flex <- "1"
                Atom = Atoms.User.accordionHiddenFlag AccordionType.CellForm
                Items =
                    [
                        str "Info",
                        (Ui.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                if visibleTaskSelectedDateMap
                                   |> Map.keys
                                   |> Seq.length
                                   <= 1 then
                                    Ui.box
                                        (fun x -> x.userSelect <- "text")
                                        [
                                            str $"""Task: {taskName}"""
                                        ]
                                else
                                    nothing

                                if visibleTaskSelectedDateMap
                                   |> Map.values
                                   |> Seq.fold Set.union Set.empty
                                   |> Set.count
                                   <= 1 then
                                    Ui.stack
                                        (fun x ->
                                            x.direction <- "row"
                                            x.alignItems <- "baseline"
                                            x.flexFlow <- "wrap")
                                        [
                                            Ui.str "Date:"

                                            Input.Input
                                                {|
                                                    CustomProps =
                                                        fun x ->
                                                            x.inputFormat <- Some Input.InputFormat.Number
                                                            x.fixedValue <- date.Day |> Day.Value |> string |> Some
                                                            x.containerProps <- Some (fun x -> x.flex <- "1")
                                                    Props =
                                                        fun x ->
                                                            x.minWidth <- "75px"

                                                            x.onChange <-
                                                                fun (e: KeyboardEvent) ->
                                                                    promise {
                                                                        match e.Value with
                                                                        | String.Valid day ->
                                                                            setCellUIFlag (
                                                                                UIFlag.Cell (
                                                                                    taskId,
                                                                                    { date with Day = Day (int day) }
                                                                                    |> FlukeDate.DateTime
                                                                                    |> FlukeDate.FromDateTime
                                                                                )
                                                                            )
                                                                        | _ -> ()
                                                                    }
                                                |}

                                            Dropdown.EnumDropdown<Month>
                                                date.Month
                                                (fun month ->
                                                    setCellUIFlag (UIFlag.Cell (taskId, { date with Month = month })))
                                                (fun _ -> ())

                                            Input.Input
                                                {|
                                                    CustomProps =
                                                        fun x ->
                                                            x.inputFormat <- Some Input.InputFormat.Number
                                                            x.fixedValue <- date.Year |> Year.Value |> string |> Some
                                                            x.containerProps <- Some (fun x -> x.flex <- "1")
                                                    Props =
                                                        fun x ->
                                                            x.minWidth <- "85px"

                                                            x.onChange <-
                                                                fun (e: KeyboardEvent) ->
                                                                    promise {
                                                                        match e.Value with
                                                                        | String.Valid year ->
                                                                            setCellUIFlag (
                                                                                UIFlag.Cell (
                                                                                    taskId,
                                                                                    { date with Year = Year (int year) }
                                                                                    |> FlukeDate.DateTime
                                                                                    |> FlukeDate.FromDateTime
                                                                                )
                                                                            )
                                                                        | _ -> ()
                                                                    }
                                                |}
                                        ]
                                else
                                    nothing

                                Ui.stack
                                    (fun x ->
                                        x.direction <- "row"
                                        x.alignItems <- "center")
                                    [
                                        Ui.str "Status: "
                                        CellMenu.CellMenu taskId date None false
                                    ]
                            ])

                        str "Attachments",
                        (Ui.stack
                            (fun x ->
                                x.spacing <- "10px"
                                x.flex <- "1")
                            [
                                AttachmentPanel.AttachmentPanel
                                    (AttachmentParent.Cell (taskId, date))
                                    (Some onAttachmentAdd)
                                    onAttachmentDelete
                                    attachmentIdList
                            ])
                    ]
            |}

    [<ReactComponent>]
    let CellFormWrapper () =
        let cellUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Cell)

        let selectedTaskIdListByArchive = Store.useValue Selectors.Session.selectedTaskIdListByArchive

        let taskId, date =
            React.useMemo (
                (fun () ->
                    match cellUIFlag with
                    | UIFlag.Cell (taskId, date) when
                        selectedTaskIdListByArchive
                        |> List.contains taskId
                        ->
                        Some taskId, Some date
                    | _ -> None, None),
                [|
                    box cellUIFlag
                    box selectedTaskIdListByArchive
                |]
            )

        match taskId, date with
        | Some taskId, Some date -> CellForm taskId date
        | _ ->
            Ui.box
                (fun x -> x.padding <- "15px")
                [
                    str "No cell selected"
                ]
