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
    let rec CellForm (taskId: TaskId) (dateId: DateId) =
        let (TaskName taskName) = Store.useValue (Atoms.Task.name taskId)

        let attachments = Store.useValue (Selectors.Cell.attachments (taskId, dateId))

        let onAttachmentAdd =
            Store.useCallback (
                (fun _ setter attachmentId ->
                    promise {
                        Store.change
                            setter
                            (Atoms.Task.cellAttachmentMap taskId)
                            (fun cellAttachmentMap ->
                                cellAttachmentMap
                                |> Map.add
                                    dateId
                                    (cellAttachmentMap
                                     |> Map.tryFind dateId
                                     |> Option.defaultValue Set.empty
                                     |> Set.add attachmentId))
                    }),
                [|
                    box taskId
                    box dateId
                |]
            )

        Accordion.Accordion
            {|
                Props = fun _ -> ()
                Atom = Atoms.accordionFlag (TextKey (nameof CellForm))
                Items =
                    [
                        "Info",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str $"""Task: {taskName}"""
                                    ]
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str $"""Date: {dateId |> DateId.Value |> FlukeDate.Stringify}"""
                                    ]
                            ])

                        "Attachments",
                        (Chakra.stack
                            (fun x ->
                                x.spacing <- "10px"
                                x.flex <- "1")
                            [
                                AttachmentPanel.AttachmentPanel attachments onAttachmentAdd
                            ])
                    ]
            |}

    [<ReactComponent>]
    let CellFormWrapper () =
        let cellUIFlag = Store.useValue (Atoms.uiFlag Atoms.UIFlagType.Cell)

        let selectedTaskIdSet = Store.useValue Selectors.Session.selectedTaskIdSet

        let taskId, dateId =
            match cellUIFlag with
            | Atoms.UIFlag.Cell (taskId, dateId) when selectedTaskIdSet.Contains taskId -> Some taskId, Some dateId
            | _ -> None, None

        match taskId, dateId with
        | Some taskId, Some dateId -> CellForm taskId dateId
        | _ ->
            Chakra.box
                (fun x -> x.padding <- "15px")
                [
                    str "No cell selected"
                ]
