namespace Fluke.UI.Frontend.Components

open System
open Browser.Types
open Feliz
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module CellForm =

    [<ReactComponent>]
    let rec CellForm
        (input: {| Username: Username
                   TaskId: TaskId
                   DateId: DateId |})
        =
        let (TaskName taskName) = Store.useValue (Atoms.Task.name (input.Username, input.TaskId))

        let addAttachmentText, setAddAttachmentText = React.useState ""

        let addAttachment =
            Store.useCallbackRef
                (fun setter _ ->
                    promise {
                        match addAttachmentText with
                        | String.ValidString _ ->
                            let attachmentId = AttachmentId.NewId ()

                            setter.set (
                                Atoms.Attachment.timestamp (input.Username, attachmentId),
                                fun _ -> DateTime.Now |> FlukeDateTime.FromDateTime |> Some
                            )

                            setter.set (
                                Atoms.Attachment.attachment (input.Username, attachmentId),
                                fun _ ->
                                    addAttachmentText
                                    |> Comment.Comment
                                    |> Attachment.Comment
                                    |> Some
                            )

                            setter.set (
                                Atoms.Task.cellAttachmentMap (input.Username, input.TaskId),
                                fun oldMap ->
                                    oldMap
                                    |> Map.add
                                        input.DateId
                                        (oldMap
                                         |> Map.tryFind input.DateId
                                         |> Option.defaultValue Set.empty
                                         |> Set.add attachmentId)
                            )

                            setAddAttachmentText ""
                        | _ -> ()
                    })

        let attachments =
            Store.useValueLoadable (Selectors.Cell.attachments (input.Username, input.TaskId, input.DateId))


        Accordion.Accordion
            {|
                Props = fun _ -> ()
                Atom = Atoms.User.accordionFlag (input.Username, TextKey (nameof CellForm))
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
                                        str
                                            $"""Date: {
                                                           input.DateId
                                                           |> DateId.Value
                                                           |> FlukeDate.Stringify
                                            }"""
                                    ]
                            ])

                        "Attachments",
                        (Chakra.stack
                            (fun x ->
                                x.spacing <- "10px"
                                x.flex <- "1")
                            [
                                Chakra.stack
                                    (fun x ->
                                        x.spacing <- "15px"
                                        x.flex <- "1")
                                    [
                                        Chakra.stack
                                            (fun x ->
                                                x.flex <- "1"
                                                x.overflowY <- "auto"
                                                x.flexBasis <- 0)
                                            [
                                                match attachments.valueMaybe () with
                                                | None -> LoadingSpinner.LoadingSpinner ()
                                                | Some [] ->
                                                    Chakra.box
                                                        (fun _ -> ())
                                                        [
                                                            str "No attachments found"
                                                        ]
                                                | Some attachments ->
                                                    Chakra.stack
                                                        (fun x -> x.spacing <- "10px")
                                                        [
                                                            yield!
                                                                attachments
                                                                |> List.map
                                                                    (fun (moment, attachment) ->
                                                                        Chakra.stack
                                                                            (fun x -> x.flex <- "1")
                                                                            [
                                                                                match attachment with
                                                                                | Attachment.Comment (Comment.Comment comment) ->

                                                                                    Chakra.flex
                                                                                        (fun x ->
                                                                                            x.justifyContent <-
                                                                                                "space-between"

                                                                                            x.color <- "whiteAlpha.600")
                                                                                        [
                                                                                            Chakra.box
                                                                                                (fun _ -> ())
                                                                                                [
                                                                                                    str "Comment"
                                                                                                ]

                                                                                            Chakra.box
                                                                                                (fun _ -> ())
                                                                                                [
                                                                                                    str (
                                                                                                        moment
                                                                                                        |> FlukeDateTime.Stringify
                                                                                                    )
                                                                                                ]
                                                                                        ]

                                                                                    Chakra.box
                                                                                        (fun _ -> ())
                                                                                        [
                                                                                            str comment
                                                                                        ]
                                                                                | _ -> str "???"
                                                                            ])
                                                        ]
                                            ]

                                        Chakra.flex
                                            (fun _ -> ())
                                            [
                                                Input.LeftIconInput
                                                    {|
                                                        Icon = Icons.fi.FiPaperclip |> Icons.render
                                                        CustomProps =
                                                            fun x ->
                                                                x.onEnterPress <- Some addAttachment
                                                                x.fixedValue <- Some addAttachmentText
                                                        Props =
                                                            fun x ->
                                                                x.placeholder <- "Add Attachment"
                                                                x.autoFocus <- true
                                                                x.borderBottomRightRadius <- "0"
                                                                x.borderTopRightRadius <- "0"

                                                                x.onChange <-
                                                                    (fun (e: KeyboardEvent) ->
                                                                        promise { setAddAttachmentText e.Value })
                                                    |}


                                                Button.Button
                                                    {|
                                                        Hint = None
                                                        Icon =
                                                            Some (
                                                                Icons.fa.FaPlus |> Icons.wrap,
                                                                Button.IconPosition.Left
                                                            )
                                                        Props =
                                                            fun x ->
                                                                x.borderBottomLeftRadius <- "0"
                                                                x.borderTopLeftRadius <- "0"
                                                                x.onClick <- addAttachment
                                                        Children = []
                                                    |}
                                            ]
                                    ]
                            ])
                    ]
            |}

    [<ReactComponent>]
    let CellFormWrapper (input: {| Username: Username |}) =
        let cellUIFlag = Store.useValue (Atoms.User.uiFlag (input.Username, Atoms.User.UIFlagType.Cell))

        let taskId, dateId =
            match cellUIFlag with
            | Some (Atoms.User.UIFlag.Cell (taskId, dateId)) -> Some taskId, Some dateId
            | _ -> None, None

        match taskId, dateId with
        | Some taskId, Some dateId ->
            CellForm
                {|
                    Username = input.Username
                    TaskId = taskId
                    DateId = dateId
                |}
        | _ -> str "No cell selected"
