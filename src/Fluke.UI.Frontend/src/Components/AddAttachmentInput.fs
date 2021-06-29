namespace Fluke.UI.Frontend.Components

open System
open Fable.Core.JsInterop
open Browser.Types
open Feliz
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module AddAttachmentInput =
    [<ReactComponent>]
    let rec AddAttachmentInput onAdd =
        let isTesting = Store.useValue Store.Atoms.isTesting
        let ctrlPressed = Store.useValue Atoms.ctrlPressed
        let addAttachmentText, setAddAttachmentText = React.useState ""

        let addAttachment =
            Store.useCallback (
                (fun _ setter _ ->
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

                            do! onAdd attachmentId

                            setAddAttachmentText ""
                        | _ -> ()
                    }),
                [|
                    box onAdd
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
