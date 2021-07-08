namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open System
open Fluke.Shared.Domain
open Fluke.Shared


module InformationForm =
    [<ReactComponent>]
    let rec InformationForm information =
        let attachmentIdMap =
            Store.useValue (
                Selectors.Information.attachmentIdMap (
                    information
                    |> Option.defaultValue (Area Area.Default)
                )
            )

        let setInformationUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Information)

        let databaseId, setDatabaseId = Store.useState Atoms.User.lastInformationDatabase

        let onAttachmentAdd =
            Store.useCallback (
                (fun _ setter attachmentId ->
                    promise {
                        match databaseId, information with
                        | Some databaseId, Some information ->
                            Store.change
                                setter
                                (Atoms.Database.informationAttachmentMap databaseId)
                                (fun informationAttachmentMap ->
                                    informationAttachmentMap
                                    |> Map.add
                                        information
                                        (informationAttachmentMap
                                         |> Map.tryFind information
                                         |> Option.defaultValue Set.empty
                                         |> Set.add attachmentId))
                        | _ -> ()
                    }),
                [|
                    box databaseId
                    box information
                |]
            )

        let onAttachmentDelete =
            Store.useCallback (
                (fun getter setter attachmentId ->
                    promise {
                        let databaseIdSearch =
                            attachmentIdMap
                            |> Map.tryFindKey (fun _ attachmentIdSet -> attachmentIdSet.Contains attachmentId)

                        match databaseIdSearch, information with
                        | Some databaseIdSearch, Some information ->
                            Store.change
                                setter
                                (Atoms.Database.informationAttachmentMap databaseIdSearch)
                                (fun informationAttachmentMap ->
                                    informationAttachmentMap
                                    |> Map.add
                                        information
                                        (informationAttachmentMap
                                         |> Map.tryFind information
                                         |> Option.defaultValue Set.empty
                                         |> Set.remove attachmentId))

                            do! Store.deleteRoot getter (Atoms.Attachment.attachment attachmentId)
                        | _ -> ()
                    }),
                [|
                    box attachmentIdMap
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
                        (UI.stack
                            (fun x -> x.spacing <- "15px")
                            [
                                DatabaseSelector.DatabaseSelector
                                    (databaseId
                                     |> Option.defaultValue Database.Default.Id)
                                    (Some >> setDatabaseId)

                                InformationSelector.InformationSelector
                                    {|
                                        DisableResource = false
                                        SelectionType = InformationSelector.InformationSelectionType.Information
                                        Information = information
                                        OnSelect = UIFlag.Information >> setInformationUIFlag
                                    |}
                            ])

                        match information with
                        | Some information when
                            information
                            |> Information.Name
                            |> InformationName.Value
                            |> String.IsNullOrWhiteSpace
                            |> not ->
                            "Attachments",
                            (UI.stack
                                (fun x ->
                                    x.spacing <- "10px"
                                    x.flex <- "1")
                                [
                                    AttachmentPanel.AttachmentPanel
                                        (if databaseId.IsSome then Some onAttachmentAdd else None)
                                        onAttachmentDelete
                                        (attachmentIdMap
                                         |> Map.values
                                         |> Seq.fold Set.union Set.empty
                                         |> Seq.toList)
                                ])
                        | _ -> ()
                    ]
            |}

    [<ReactComponent>]
    let InformationFormWrapper () =
        let informationUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Information)

        let information =
            match informationUIFlag with
            | UIFlag.Information information -> Some information
            | _ -> None

        InformationForm information
