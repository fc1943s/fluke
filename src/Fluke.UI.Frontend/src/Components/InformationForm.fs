namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module InformationForm =
    [<ReactComponent>]
    let rec InformationForm information =
        let attachmentIdSet = Store.useValue (Selectors.Information.attachmentIdSet information)
        let setInformationUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Information)

        let onAttachmentAdd =
            Store.useCallback (
                (fun _ setter attachmentId ->
                    promise {
                        Store.change
                            setter
                            Atoms.User.informationAttachmentMap
                            (fun informationAttachmentMap ->
                                informationAttachmentMap
                                |> Map.add
                                    information
                                    (informationAttachmentMap
                                     |> Map.tryFind information
                                     |> Option.defaultValue Set.empty
                                     |> Set.add attachmentId))
                    }),
                [|
                    box information
                |]
            )

        let onAttachmentDelete =
            Store.useCallback (
                (fun getter setter attachmentId ->
                    promise {
                        Store.change
                            setter
                            Atoms.User.informationAttachmentMap
                            (fun informationAttachmentMap ->
                                informationAttachmentMap
                                |> Map.add
                                    information
                                    (informationAttachmentMap
                                     |> Map.tryFind information
                                     |> Option.defaultValue Set.empty
                                     |> Set.remove attachmentId))

                        do! Store.deleteRoot getter (Atoms.Attachment.attachment attachmentId)
                    }),
                [|
                    box information
                |]
            )

        Accordion.Accordion
            {|
                Props =
                    fun x ->
                        x.flex <- "1"
                        x.overflowY <- "auto"
                        x.flexBasis <- 0
                Atom = Atoms.User.accordionFlag (TextKey (nameof InformationForm))
                Items =
                    [
                        "Info",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        InformationSelector.InformationSelector
                                            {|
                                                DisableResource = false
                                                SelectionType = InformationSelector.InformationSelectionType.Information
                                                Information = information
                                                OnSelect = UIFlag.Information >> setInformationUIFlag
                                            |}
                                    ]
                            ])

                        "Attachments",
                        (Chakra.stack
                            (fun x ->
                                x.spacing <- "10px"
                                x.flex <- "1")
                            [
                                AttachmentPanel.AttachmentPanel
                                    onAttachmentAdd
                                    onAttachmentDelete
                                    (attachmentIdSet |> Set.toList)
                            ])
                    ]
            |}

    [<ReactComponent>]
    let InformationFormWrapper () =
        let informationUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Information)

        let information =
            match informationUIFlag with
            | UIFlag.Information information -> Some information
            | _ -> None

        match information with
        | Some information -> InformationForm information
        | _ ->
            Chakra.box
                (fun x -> x.padding <- "15px")
                [
                    str "No information selected"
                ]
