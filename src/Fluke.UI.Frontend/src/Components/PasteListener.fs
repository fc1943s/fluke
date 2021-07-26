namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fable.Core
open Fluke.Shared
open Fluke.UI.Frontend.Bindings
open System
open Fluke.Shared.Domain.UserInteraction


module PasteListener =
    [<ReactComponent>]
    let PasteListener () =
        let onFilePasted =
            Store.useCallback (
                (fun getter setter attachment ->
                    promise {
                        JS.log (fun () -> $"pasted image attachment={attachment}")

                        let attachmentId =
                            Hydrate.hydrateAttachmentState
                                getter
                                setter
                                (Store.AtomScope.Current,
                                 {
                                     Timestamp = FlukeDateTime.FromDateTime DateTime.Now
                                     Archived = false
                                     Attachment = attachment
                                 })

                        Store.change setter Atoms.User.clipboardAttachmentIdMap (Map.add attachmentId false)
                    }),
                [||]
            )

        let toast = UI.useToast ()

        let handlePasteEvent =
            Store.useCallback (
                (fun getter setter (e: Browser.Types.Event) ->
                    promise {

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
                                            |> Promise.Parallel

                                        return Some blobs
                                    with
                                    | ex ->
                                        JS.consoleError ("handlePasteEvent clipboard error", ex)
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
                                        let! bytes = JS.blobToUint8Array blob
                                        let hexString = JS.byteArrayToHexString (bytes.Values () |> Seq.toArray)

                                        let fileId =
                                            Hydrate.hydrateFile getter setter (Store.AtomScope.Current, hexString)

                                        let attachment = Attachment.Image fileId
                                        do! onFilePasted attachment
                                    })
                            |> Promise.Parallel
                            |> Promise.ignore
                    }),
                [|
                    box toast
                    box onFilePasted
                |]
            )

        let handle =
            React.useMemo (
                (fun () -> handlePasteEvent >> Promise.start),
                [|
                    box handlePasteEvent
                |]
            )

        React.useDisposableEffect (
            (fun disposed ->
                if not disposed then
                    Browser.Dom.document.addEventListener ("paste", handle)
                else
                    Browser.Dom.document.removeEventListener ("paste", handle)),
            [|
                box handle
            |]
        )

        nothing
