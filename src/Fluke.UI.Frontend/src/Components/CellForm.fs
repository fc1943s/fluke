namespace Fluke.UI.Frontend.Components

open Feliz
open FsCore
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open FsStore
open FsStore.Bindings
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsUi.Components


module CellForm =
    [<ReactComponent>]
    let rec CellForm taskIdAtom dateIdAtom =
        let taskId, dateId = Store.useValueTuple taskIdAtom dateIdAtom
        let (TaskName taskName) = Store.useValue (Atoms.Task.name taskId)

        let attachmentIdSet = Store.useValue (Selectors.Cell.attachmentIdSet (taskId, dateId))
        let visibleTaskSelectedDateIdMap = Store.useValue Selectors.Session.visibleTaskSelectedDateIdMap


        let attachmentIdList =
            React.useMemo (
                (fun () -> attachmentIdSet |> Set.toList),
                [|
                    box attachmentIdSet
                |]
            )

        let onAttachmentAdd =
            Store.useCallbackRef
                (fun _ setter attachmentId ->
                    promise {
                        Store.set
                            setter
                            (Atoms.Attachment.parent attachmentId)
                            (Some (AttachmentParent.Cell (taskId, dateId)))
                    })

        let onAttachmentDelete =
            Store.useCallbackRef
                (fun getter _setter attachmentId ->
                    promise {
                        do! Store.deleteRoot getter (Atoms.Attachment.attachment attachmentId)
                        return true
                    })

        Accordion.Accordion
            {|
                Props = fun x -> x.flex <- "1"
                Atom = Atoms.User.accordionHiddenFlag AccordionType.CellForm
                Items =
                    [
                        str "Info",
                        (Ui.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                if visibleTaskSelectedDateIdMap
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

                                if visibleTaskSelectedDateIdMap
                                   |> Map.values
                                   |> Seq.fold Set.union Set.empty
                                   |> Set.count
                                   <= 1 then
                                    Ui.box
                                        (fun x -> x.userSelect <- "text")
                                        [
                                            str
                                                $"Date: {dateId
                                                         |> DateId.ValueOrDefault
                                                         |> FlukeDate.Stringify}"
                                        ]
                                else
                                    nothing

                                Ui.stack
                                    (fun x ->
                                        x.direction <- "row"
                                        x.alignItems <- "center")
                                    [
                                        Ui.str "Status: "
                                        CellMenu.CellMenu taskIdAtom dateIdAtom None false
                                    ]
                            ])

                        str "Attachments",
                        (Ui.stack
                            (fun x ->
                                x.spacing <- "10px"
                                x.flex <- "1")
                            [
                                AttachmentPanel.AttachmentPanel
                                    (AttachmentParent.Cell (taskId, dateId))
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
            Ui.box
                (fun x -> x.padding <- "15px")
                [
                    str "No cell selected"
                ]
