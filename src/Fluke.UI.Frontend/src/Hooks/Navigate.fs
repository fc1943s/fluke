namespace Fluke.UI.Frontend.Hooks

open FsStore.State
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain
open Fluke.UI.Frontend.TempUI
open FsStore



module Navigate =
    [<RequireQualifiedAccess>]
    type DockPosition =
        | Left
        | Right

    [<RequireQualifiedAccess>]
    type Anchor =
        | Information of information: Information
        | InformationAttachment of information: Information * attachmentId: AttachmentId
        | Task of databaseId: DatabaseId * taskId: TaskId
        | Cell of taskId: TaskId * date: FlukeDate
        | TaskAttachment of databaseId: DatabaseId * taskId: TaskId * attachmentId: AttachmentId
        | CellAttachment of taskId: TaskId * date: FlukeDate * attachmentId: AttachmentId

        static member inline Stringify getter anchor =
            match anchor with
            | Anchor.Information information ->
                (information |> Information.toString),
                information
                |> Information.Name
                |> InformationName.Value

            | Anchor.InformationAttachment (information, attachmentId) ->
                let attachment = Atom.get getter (Atoms.Attachment.attachment attachmentId)

                $"{information |> Information.toString} Attachment ({information
                                                                     |> Information.Name
                                                                     |> InformationName.Value})",
                attachment
                |> Option.map Attachment.Stringify
                |> Option.defaultValue "???"

            | Anchor.Task (_, taskId) ->
                let name = Atom.get getter (Atoms.Task.name taskId)
                "Task", name |> TaskName.Value

            | Anchor.Cell (taskId, date) ->
                let name = Atom.get getter (Atoms.Task.name taskId)
                "Cell", $"{name |> TaskName.Value}/{date |> FlukeDate.Stringify}"

            | Anchor.TaskAttachment (_, taskId, attachmentId) ->
                let attachment = Atom.get getter (Atoms.Attachment.attachment attachmentId)
                let name = Atom.get getter (Atoms.Task.name taskId)

                $"Task Attachment ({name |> TaskName.Value})",
                attachment
                |> Option.map Attachment.Stringify
                |> Option.defaultValue "???"

            | Anchor.CellAttachment (taskId, dateId, attachmentId) ->
                let attachment = Atom.get getter (Atoms.Attachment.attachment attachmentId)
                let name = Atom.get getter (Atoms.Task.name taskId)

                $"""Cell Attachment (Task: {name |> TaskName.Value} / Date: {dateId |> FlukeDate.Stringify})""",
                attachment
                |> Option.map Attachment.Stringify
                |> Option.defaultValue "???"

    module Actions =
        let navigate =
            Atom.Primitives.setSelector
                (fun getter setter (dockPosition, dockType, uiFlagType, uiFlag) ->
                    let deviceInfo = Atom.get getter Selectors.Store.deviceInfo

                    match dockPosition with
                    | DockPosition.Left
                    | DockPosition.Right ->
                        if deviceInfo.IsMobile then
                            Atom.set
                                setter
                                (if dockPosition = DockPosition.Left then
                                     Atoms.User.rightDock
                                 else
                                     Atoms.User.leftDock)
                                None

                        Atom.set
                            setter
                            (if dockPosition = DockPosition.Left then
                                 Atoms.User.leftDock
                             else
                                 Atoms.User.rightDock)
                            dockType

                        Atom.set setter (Atoms.User.uiFlag uiFlagType) uiFlag)

        let navigateAnchor =
            Atom.Primitives.setSelector
                (fun _getter setter anchor ->
                    let navigate = Atom.set setter navigate

                    match anchor with
                    | Anchor.Information information ->
                        navigate (
                            DockPosition.Right,
                            Some DockType.Information,
                            UIFlagType.Information,
                            UIFlag.Information information
                        )
                    | Anchor.Task (databaseId, taskId) ->
                        navigate (
                            DockPosition.Right,
                            Some DockType.Task,
                            UIFlagType.Task,
                            UIFlag.Task (databaseId, taskId)
                        )
                    | Anchor.Cell (taskId, dateId) ->
                        navigate (DockPosition.Right, Some DockType.Cell, UIFlagType.Cell, UIFlag.Cell (taskId, dateId))
                    | Anchor.InformationAttachment (information, _attachmentId) ->
                        navigate (
                            DockPosition.Right,
                            Some DockType.Information,
                            UIFlagType.Information,
                            UIFlag.Information information
                        )
                    | Anchor.TaskAttachment (databaseId, taskId, _attachmentId) ->
                        navigate (
                            DockPosition.Right,
                            Some DockType.Task,
                            UIFlagType.Task,
                            UIFlag.Task (databaseId, taskId)
                        )
                    | Anchor.CellAttachment (taskId, dateId, _attachmentId) ->
                        navigate (DockPosition.Right, Some DockType.Cell, UIFlagType.Cell, UIFlag.Cell (taskId, dateId)))
