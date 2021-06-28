namespace Fluke.UI.Frontend.Components

open System
open Fable.Core.JsInterop
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
    let rec AddAttachmentInput taskId dateId =
        let isTesting = Store.useValue Store.Atoms.isTesting
        let ctrlPressed = Store.useValue Atoms.ctrlPressed
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

                            let cellAttachmentMap = Store.value getter (Atoms.Task.cellAttachmentMap taskId)

                            Store.set
                                setter
                                (Atoms.Task.cellAttachmentMap taskId)
                                (cellAttachmentMap
                                 |> Map.add
                                     dateId
                                     (cellAttachmentMap
                                      |> Map.tryFind dateId
                                      |> Option.defaultValue Set.empty
                                      |> Set.add attachmentId))

                            setAddAttachmentText ""
                        | _ -> ()
                    }),
                [|
                    box taskId
                    box dateId
                    box addAttachmentText
                    box setAddAttachmentText
                |]
            )

        if true then
            Chakra.flex
                (fun x -> x.alignItems <- "flex-end")
                [
                    Input.LeftIconInput
                        {|
                            Icon = Icons.fi.FiPaperclip |> Icons.render
                            CustomProps =
                                fun x ->
                                    x.textarea <- true
                                    x.fixedValue <- Some addAttachmentText

                                    x.onEnterPress <-
                                        Some (fun _ -> promise { if ctrlPressed then do! addAttachment () })
                            Props =
                                fun x ->
                                    x.placeholder <- "Add Attachment"
                                    x.autoFocus <- true
                                    x.maxHeight <- "200px"
                                    x.borderBottomRightRadius <- "0"
                                    x.borderTopRightRadius <- "0"

                                    x.onChange <- (fun (e: KeyboardEvent) -> promise { setAddAttachmentText e.Value })
                        |}


                    Button.Button
                        {|
                            Hint = None
                            Icon = Some (Icons.fa.FaPlus |> Icons.wrap, Button.IconPosition.Left)
                            Props =
                                fun x ->
                                    if isTesting then x?``data-testid`` <- "Add Attachment"
                                    x.borderBottomLeftRadius <- "0"
                                    x.borderTopLeftRadius <- "0"
                                    x.onClick <- fun _ -> addAttachment ()
                            Children = []
                        |}
                ]
        else
            Vim.render
                {|
                    OnVimCreated = fun vim -> printfn $"vim {vim}"
                    Props = fun x -> x.height <- "150px"
                    Fallback = fun () -> str "wasm error"
                |}

    [<ReactComponent>]
    let AttachmentList taskId dateId =
        let attachments = Store.useValue (Selectors.Cell.attachments (taskId, dateId))

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
                                                                    x.justifyContent <- "space-between"

                                                                    x.color <- "whiteAlpha.600")
                                                                [
                                                                    Chakra.box
                                                                        (fun _ -> ())
                                                                        [
                                                                            str "Comment"
                                                                        ]

                                                                    Chakra.box
                                                                        (fun x -> x.lineHeight <- "16px")
                                                                        [
                                                                            str (moment |> FlukeDateTime.Stringify)
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

                AddAttachmentInput taskId dateId
            ]

    [<ReactComponent>]
    let rec CellForm (taskId: TaskId) (dateId: DateId) =
        let (TaskName taskName) = Store.useValue (Atoms.Task.name taskId)

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
                                AttachmentList taskId dateId
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
