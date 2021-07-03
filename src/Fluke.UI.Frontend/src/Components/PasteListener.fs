namespace Fluke.UI.Frontend.Components

open Browser.Types
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
                (fun getter setter (attachment, chunkCount) ->
                    promise {
                        JS.log (fun () -> $"pasted image fileId={(attachment, chunkCount)}")

                        let attachmentId =
                            Hydrate.hydrateAttachment
                                getter
                                setter
                                (Store.AtomScope.ReadOnly, (FlukeDateTime.FromDateTime DateTime.Now, attachment))

                        Store.change setter Atoms.User.clipboardAttachmentSet (Set.add attachmentId)
                    }),
                [||]
            )

        let toast = Chakra.useToast ()

        let handlePasteEvent =
            Store.useCallback (
                (fun _getter setter (event: Browser.Types.Event) ->
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
                                    with ex ->
                                        Browser.Dom.console.error ex
                                        return None
                                }
                            | None ->
                                toast (fun x -> x.description <- "Clipboard not available")
                                None |> Promise.lift
                            |> Promise.map (Option.defaultValue [||])

                        do!
                            blobs
                            |> Array.map
                                (fun blob ->
                                    promise {
                                        let! bytes = JS.blobToUint8Array blob
                                        let hexString = JS.byteArrayToHexString (bytes.Values () |> Seq.toArray)
                                        let chunkSize = 16000
                                        let chunkCount = int (Math.Ceiling (float hexString.Length / float chunkSize))

                                        let chunks =
                                            JS.chunkString
                                                hexString
                                                {|
                                                    size = chunkSize
                                                    unicodeAware = false
                                                |}

                                        //                                        Browser.Dom.window?writeBlob <- blob
//                                        Browser.Dom.window?writeHexString <- hexString
//                                        Browser.Dom.window?writeBytes <- bytes
//
                                        JS.log
                                            (fun () ->
                                                $"PasteListener.
                                        blob.size={blob.size}
                                        base64.Length={hexString.Length}
                                        bytes.Length={bytes.Length}
                                        chunkCount={chunkCount}
                                        chunks.[0].Length={chunks.[0].Length}
                                        ")

                                        let fileId = FileId.NewId ()
                                        Store.set setter (Atoms.File.chunkCount fileId) chunkCount

                                        chunks
                                        |> Array.iteri (fun i -> Store.set setter (Atoms.File.chunk (fileId, i)))

                                        let attachment = Attachment.Image fileId
                                        do! onFilePasted (attachment, chunkCount)
                                    })
                            |> Promise.Parallel
                            |> Promise.ignore

                        event.preventDefault ()

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
