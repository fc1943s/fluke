namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module CellForm =
    [<ReactComponent>]
    let rec CellForm taskIdAtom dateIdAtom =
        let taskId = Store.useValue taskIdAtom
        let dateId = Store.useValue dateIdAtom
        let (TaskName taskName) = Store.useValue (Atoms.Task.name taskId)

        let attachmentIdSet = Store.useValue (Selectors.Cell.attachmentIdSet (taskId, dateId))

        let attachmentIdList =
            React.useMemo (
                (fun () -> attachmentIdSet |> Set.toList),
                [|
                    box attachmentIdSet
                |]
            )

        let onAttachmentAdd =
            Store.useCallback (
                (fun _ setter attachmentId ->
                    promise {
                        Store.change
                            setter
                            (Atoms.Task.cellAttachmentIdMap taskId)
                            (fun cellAttachmentIdMap ->
                                cellAttachmentIdMap
                                |> Map.add
                                    dateId
                                    (cellAttachmentIdMap
                                     |> Map.tryFind dateId
                                     |> Option.defaultValue Set.empty
                                     |> Set.add attachmentId))
                    }),
                [|
                    box taskId
                    box dateId
                |]
            )

        let onAttachmentDelete =
            Store.useCallback (
                (fun getter setter attachmentId ->
                    promise {
                        Store.change
                            setter
                            (Atoms.Task.cellAttachmentIdMap taskId)
                            (fun cellAttachmentIdMap ->
                                cellAttachmentIdMap
                                |> Map.add
                                    dateId
                                    (cellAttachmentIdMap
                                     |> Map.tryFind dateId
                                     |> Option.defaultValue Set.empty
                                     |> Set.remove attachmentId))

                        do! Store.deleteRoot getter (Atoms.Attachment.attachment attachmentId)
                        return true
                    }),
                [|
                    box taskId
                    box dateId
                |]
            )

        Accordion.Accordion
            {|
                Props = fun x -> x.flex <- "1"
                Atom = Atoms.User.accordionHiddenFlag AccordionType.CellForm
                Items =
                    [
                        str "Info",
                        (UI.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                UI.box
                                    (fun x -> x.userSelect <- "text")
                                    [
                                        str $"""Task: {taskName}"""
                                    ]
                                UI.box
                                    (fun x -> x.userSelect <- "text")
                                    [
                                        str $"""Date: {dateId |> DateId.Value |> FlukeDate.Stringify}"""
                                    ]
                                UI.stack
                                    (fun x ->
                                        x.direction <- "row"
                                        x.alignItems <- "center")
                                    [
                                        UI.box
                                            (fun _ -> ())
                                            [
                                                str "Status: "
                                            ]
                                        CellMenu.CellMenu taskIdAtom dateIdAtom None false
                                    ]
                            ])

                        str "Attachments",
                        (UI.stack
                            (fun x ->
                                x.spacing <- "10px"
                                x.flex <- "1")
                            [
                                AttachmentPanel.AttachmentPanel
                                    AddAttachmentInput.AttachmentPanelType.Cell
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

        let taskIdAtom, dateIdAtom =
            React.useMemo (
                (fun () ->
                    match cellUIFlag with
                    | UIFlag.Cell (taskId, dateId) when
                        selectedTaskIdListByArchive
                        |> List.contains taskId
                        ->
                        Some (Jotai.jotai.atom taskId), Some (Jotai.jotai.atom dateId)
                    | _ -> None, None),
                [|
                    box cellUIFlag
                    box selectedTaskIdListByArchive
                |]
            )

        match taskIdAtom, dateIdAtom with
        | Some taskIdAtom, Some dateIdAtom -> CellForm taskIdAtom dateIdAtom
        | _ ->
            UI.box
                (fun x -> x.padding <- "15px")
                [
                    str "No cell selected"
                ]
