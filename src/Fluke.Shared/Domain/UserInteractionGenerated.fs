//------------------------------------------------------------------------------
//        This code was generated by myriad.
//        Changes to this file will be lost when the code is regenerated.
//------------------------------------------------------------------------------
namespace rec Fluke.Shared.Domain

module Attachment =
    open Fluke.Shared.Domain.UserInteraction
    let toString (x: Attachment) =
        match x with
        | Attachment.Comment _ -> "Comment"
        | Attachment.Link _ -> "Link"
        | Attachment.Video _ -> "Video"
        | Attachment.Image _ -> "Image"
        | Attachment.List _ -> "List"

    let fromString (x: string) =
        match x with
        | _ -> None

    let toTag (x: Attachment) =
        match x with
        | Attachment.Comment _ -> 0
        | Attachment.Link _ -> 1
        | Attachment.Video _ -> 2
        | Attachment.Image _ -> 3
        | Attachment.List _ -> 4

    let isComment (x: Attachment) =
        match x with
        | Attachment.Comment _ -> true
        | _ -> false

    let isLink (x: Attachment) =
        match x with
        | Attachment.Link _ -> true
        | _ -> false

    let isVideo (x: Attachment) =
        match x with
        | Attachment.Video _ -> true
        | _ -> false

    let isImage (x: Attachment) =
        match x with
        | Attachment.Image _ -> true
        | _ -> false

    let isList (x: Attachment) =
        match x with
        | Attachment.List _ -> true
        | _ -> false

