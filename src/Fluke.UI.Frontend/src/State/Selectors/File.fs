namespace Fluke.UI.Frontend.State.Selectors

open Fable.Extras
open Fluke.UI.Frontend.State
open FsCore.Model
open FsJs
open FsStore

#nowarn "40"


module rec File =
    let rec byteArray =
        Store.readSelectorFamily
            Fluke.root
            Atoms.File.collection
            (nameof byteArray)
            (fun (fileId: FileId) getter ->
                let chunkCount = Store.value getter (Atoms.File.chunkCount fileId)

                match chunkCount with
                | 0 -> None
                | _ ->
                    let chunks =
                        [|
                            0 .. chunkCount - 1
                        |]
                        |> Array.map (fun i -> Atoms.File.chunk (fileId, i))
                        |> Store.waitForAll
                        |> Store.value getter

                    if chunks |> Array.exists (String.length >> (=) 0) then
                        Dom.log
                            (fun () ->
                                $"File.blob
                                        incomplete blob. skipping
                                    chunkCount={chunkCount}
                                    chunks.Length={chunks.Length}
                                    chunks.[0].Length={if chunks.Length = 0 then unbox null else chunks.[0].Length}
                                    ")

                        None
                    else
                        Dom.log
                            (fun () ->
                                $"File.blob
                                    chunkCount={chunkCount}
                                    chunks.Length={chunks.Length}
                                    chunks.[0].Length={if chunks.Length = 0 then unbox null else chunks.[0].Length}
                                    ")

                        Some (
                            chunks
                            |> String.concat ""
                            |> JS.hexStringToByteArray
                        ))


    let rec blob =
        Store.readSelectorFamily
            Fluke.root
            Atoms.File.collection
            (nameof blob)
            (fun (fileId: FileId) getter ->
                let byteArray = Store.value getter (byteArray fileId)

                byteArray
                |> Option.map (fun bytes -> JS.uint8ArrayToBlob (JSe.Uint8Array (unbox<uint8 []> bytes)) "image/png"))


    let rec objectUrl =
        Store.readSelectorFamily
            Fluke.root
            Atoms.File.collection
            (nameof objectUrl)
            (fun (fileId: FileId) getter ->
                let blob = Store.value getter (blob fileId)
                blob |> Option.map Browser.Url.URL.createObjectURL)
