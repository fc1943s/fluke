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

    let navigate =
        fun getter setter (dockPosition, dockType, uiFlagType, uiFlag) ->
            promise {
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

                    Atom.set setter (Atoms.User.uiFlag uiFlagType) uiFlag
            }

    let navigateAnchor =
        fun getter setter anchor ->
            promise {
                let navigate = navigate getter setter

                match anchor with
                | Anchor.Task (databaseId, taskId) ->
                    do!
                        navigate (
                            DockPosition.Right,
                            Some DockType.Task,
                            UIFlagType.Task,
                            UIFlag.Task (databaseId, taskId)
                        )
                | Anchor.Information information ->
                    do!
                        navigate (
                            DockPosition.Right,
                            Some DockType.Information,
                            UIFlagType.Information,
                            UIFlag.Information information
                        )
                | Anchor.InformationAttachment (information, attachmentId) ->
                    do!
                        navigate (
                            DockPosition.Right,
                            Some DockType.Information,
                            UIFlagType.Information,
                            UIFlag.Information information
                        )
                | Anchor.TaskAttachment (databaseId, taskId, attachmentId) ->
                    do!
                        navigate (
                            DockPosition.Right,
                            Some DockType.Task,
                            UIFlagType.Task,
                            UIFlag.Task (databaseId, taskId)
                        )
                | Anchor.CellAttachment (taskId, dateId, attachmentId) ->
                    do! navigate (DockPosition.Right, Some DockType.Cell, UIFlagType.Cell, UIFlag.Cell (taskId, dateId))
            }
