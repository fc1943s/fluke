namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fable.Extras
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Hooks
open Fable.Core


module AttachmentComment =
    [<ReactComponent>]
    let AttachmentComment text =
        let youtubeIdList =
            React.useMemo (
                (fun () ->
                    (JSe.RegExp (
                        @"((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#&?\n ]*)",
                        JSe.RegExpFlag().i.g
                    ))
                        .MatchAll text
                    |> Seq.choose
                        (fun matches ->
                            let matches = matches |> Seq.toList
                            if matches.Length = 8 then Some matches.[7] else None)
                    |> Seq.toList),
                [|
                    box text
                |]
            )

        let youtubeImgList =
            React.useMemo (
                (fun () ->
                    youtubeIdList
                    |> List.map (fun youtubeVideoId -> $"https://img.youtube.com/vi/{youtubeVideoId}/maxresdefault.jpg")),
                [|
                    box youtubeIdList
                |]
            )

        let youtubeMetadataMap, setYoutubeMetadataMap = React.useState Map.empty

        React.useEffect (
            (fun () ->
                async {
                    let! newMetadataList =
                        youtubeIdList
                        |> List.map
                            (fun youtubeId ->
                                $"https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={youtubeId}")
                        |> List.map Fable.SimpleHttp.Http.get
                        |> Async.Parallel

                    if newMetadataList.Length > 0 then
                        newMetadataList
                        |> Array.map
                            (fun (code, content) ->
                                if code <> 200 then
                                    None
                                else
                                    Some (content |> Json.decode<{| title: string |}>))
                        |> Array.mapi
                            (fun i metadata ->
                                match metadata with
                                | Some metadata -> Some (youtubeImgList.[i], metadata)
                                | None -> None)
                        |> Array.choose id
                        |> Map.ofArray
                        |> setYoutubeMetadataMap
                }
                |> Async.StartAsPromise
                |> Promise.start),
            [|
                box youtubeIdList
                box setYoutubeMetadataMap
                box youtubeImgList
            |]
        )

        UI.box
            (fun x ->
                x.userSelect <- "text"
                x.overflow <- "auto"
                x.paddingTop <- "2px"
                x.paddingBottom <- "2px"
                x.maxHeight <- "50vh")
            [
                Markdown.render text

                match youtubeImgList with
                | [] -> nothing
                | youtubeImgList ->
                    UI.flex
                        (fun x ->
                            x.marginTop <- "10px"
                            x.overflow <- "auto")
                        [
                            yield!
                                youtubeImgList
                                |> List.map
                                    (fun url ->
                                        FileThumbnail.ImageThumbnail [
                                            ImageModal.ImageModal
                                                State.UIFlagType.RawImage
                                                (State.UIFlag.RawImage url)
                                                (youtubeMetadataMap
                                                 |> Map.tryFind url
                                                 |> Option.map (fun metadata -> metadata.title)
                                                 |> Option.defaultValue "")
                                                url
                                        ])
                        ]
            ]
