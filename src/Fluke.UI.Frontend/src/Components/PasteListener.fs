namespace Fluke.UI.Frontend.Components

open System
open Fluke.UI.Frontend.State.State
open FsCore
open FsStore.Model
open FsUi.Hooks
open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open FsStore.Hooks
open FsJs
open FsStore
open FsUi.Bindings
open Fluke.Shared.Domain.UserInteraction


module PasteListener =
    [<ReactComponent>]
    let PasteListener () =
        let onFilePasted =
            Store.useCallbackRef
                (fun getter setter attachment ->
                    promise {
                        let logger = Store.value getter Selectors.logger
                        logger.Debug (fun () -> $"pasted image attachment={attachment}")

                        let attachmentId =
                            Hydrate.hydrateAttachmentState
                                getter
                                setter
                                (AtomScope.Current,
                                 AttachmentParent.None,
                                 {
                                     Timestamp = FlukeDateTime.FromDateTime DateTime.Now
                                     Archived = false
                                     Attachment = attachment
                                 })

                        Store.change
                            setter
                            Atoms.User.clipboardAttachmentIdMap
                            (fun map -> map |> Map.add attachmentId false)
                    })

        let toast = Ui.useToast ()

        let handlePasteEvent =
            Store.useCallbackRef
                (fun getter setter (e: Browser.Types.Event) ->
                    promise {
                        let logger = Store.value getter Selectors.logger

                        let! blobs =
                            match Browser.Navigator.navigator.clipboard with
                            | Some clipboard ->
                                promise {
                                    try
                                        let! read = clipboard.read ()

                                        let! blobs =
                                            read
                                            |> Array.collect
                                                (fun clipboardItem ->
                                                    clipboardItem.types
                                                    |> Array.filter (fun t -> t.StartsWith "image/")
                                                    |> Array.map (fun x -> clipboardItem.getType (unbox x)))
                                            |> Promise.all

                                        return Some blobs
                                    with
                                    | ex ->
                                        logger.Error (fun () -> $"handlePasteEvent clipboard error={ex}")
                                        return None
                                }
                            | None ->
                                toast (fun x -> x.description <- "Clipboard not available")
                                None |> Promise.lift
                            |> Promise.map (Option.defaultValue [||])

                        if blobs.Length > 0 then e.preventDefault ()

                        do!
                            blobs
                            |> Array.map
                                (fun blob ->
                                    promise {
                                        let! hexString = Js.blobToHexString blob
                                        let fileId = Hydrate.hydrateFile setter (AtomScope.Current, hexString)

                                        match fileId with
                                        | Some fileId ->
                                            let attachment = Attachment.Image fileId
                                            do! onFilePasted attachment
                                        | None ->
                                            toast
                                                (fun x ->
                                                    x.description <-
                                                        $"Error hydrating file. hexString.Length={hexString.Length}")
                                    })
                            |> Promise.all
                            |> Promise.ignore
                    })

        let handle =
            React.useMemo (
                (fun () -> handlePasteEvent >> Promise.start),
                [|
                    box handlePasteEvent
                |]
            )

        let isMountedRef = React.useIsMountedRef ()

        React.useEffect (
            (fun () ->
                if isMountedRef.current then
                    Browser.Dom.document.addEventListener ("paste", handle)
                else
                    Browser.Dom.document.removeEventListener ("paste", handle)),
            [|
                box isMountedRef
                box handle
            |]
        )

        nothing
