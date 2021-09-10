namespace Fluke.UI.Frontend.Components

open Feliz
open FsCore
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open FsStore
open FsStore.Hooks
open FsStore.Bindings
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsUi.Components
open Fluke.UI.Frontend.Hooks
open FsStore.State


module CellForm =
    [<ReactComponent>]
    let rec CellForm taskIdAtom dateAtom =
        let taskId, dateId = Store.useValueTuple taskIdAtom dateAtom
        let (TaskName taskName) = Store.useValue (Atoms.Task.name taskId)

        let attachmentIdSet = Store.useValue (Selectors.Cell.attachmentIdSet (taskId, dateId))
        let visibleTaskSelectedDateMap = Store.useValue Selectors.Session.visibleTaskSelectedDateMap


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
                        Atom.set
                            setter
                            (Atoms.Attachment.parent attachmentId)
                            (Some (AttachmentParent.Cell (taskId, dateId)))
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
                                    Ui.box
                                        (fun x -> x.userSelect <- "text")
                                        [
                                            str $"Date: {dateId |> FlukeDate.Stringify}"
                                        ]
                                else
                                    nothing

                                Ui.stack
                                    (fun x ->
                                        x.direction <- "row"
                                        x.alignItems <- "center")
                                    [
                                        Ui.str "Status: "
                                        CellMenu.CellMenu taskIdAtom dateAtom None false
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

        let taskIdAtom, dateAtom =
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

        match taskIdAtom, dateAtom with
        | Some taskIdAtom, Some dateAtom -> CellForm taskIdAtom dateAtom
        | _ ->
            Ui.box
                (fun x -> x.padding <- "15px")
                [
                    str "No cell selected"
                ]
