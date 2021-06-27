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
    let rec CellForm (input: {| TaskId: TaskId; DateId: DateId |}) =
        let (TaskName taskName) = Store.useValue (Atoms.Task.name input.TaskId)

        let addAttachmentText, setAddAttachmentText = React.useState ""

        let addAttachment =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        match addAttachmentText with
                        | String.ValidString _ ->
                            let attachmentId = AttachmentId.NewId ()

                            Store.set
                                setter
                                (Atoms.Attachment.timestamp attachmentId)
                                (DateTime.Now |> FlukeDateTime.FromDateTime |> Some)

                            Store.set
                                setter
                                (Atoms.Attachment.attachment attachmentId)
                                (addAttachmentText
                                 |> Comment.Comment
                                 |> Attachment.Comment
                                 |> Some)

                            let cellAttachmentMap = Store.value getter (Atoms.Task.cellAttachmentMap input.TaskId)

                            Store.set
                                setter
                                (Atoms.Task.cellAttachmentMap input.TaskId)
                                (cellAttachmentMap
                                 |> Map.add
                                     input.DateId
                                     (cellAttachmentMap
                                      |> Map.tryFind input.DateId
                                      |> Option.defaultValue Set.empty
                                      |> Set.add attachmentId))

                            setAddAttachmentText ""
                        | _ -> ()
                    }),
                [|
                    box addAttachmentText
                    box setAddAttachmentText
                    box input
                |]
            )

        let attachments = Store.useValue (Selectors.Cell.attachments (input.TaskId, input.DateId))


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
                                                match attachments with
                                                //                                                | None -> LoadingSpinner.LoadingSpinner ()
                                                | [] ->
                                                    Chakra.box
                                                        (fun _ -> ())
                                                        [
                                                            str "No attachments found"
                                                        ]
                                                | attachments ->
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
                                                                x.textarea <- true
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

                                        if false then
                                            Vim.render
                                                {|
                                                    OnVimCreated = fun vim -> printfn $"vim {vim}"
                                                    Props = fun x -> x.height <- "150px"
                                                    Fallback = fun () -> str "wasm error"
                                                |}

                                    ]
                            ])
                    ]
            |}

    [<ReactComponent>]
    let CellFormWrapper () =
        let cellUIFlag = Store.useValue (Atoms.uiFlag Atoms.UIFlagType.Cell)

        let taskId, dateId =
            match cellUIFlag with
            | Atoms.UIFlag.Cell (taskId, dateId) -> Some taskId, Some dateId
            | _ -> None, None

        match taskId, dateId with
        | Some taskId, Some dateId -> CellForm {| TaskId = taskId; DateId = dateId |}
        | _ ->
            Chakra.box
                (fun x -> x.padding <- "15px")
                [
                    str "No cell selected"
                ]
