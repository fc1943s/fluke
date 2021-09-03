namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open FsCore
open Fluke.UI.Frontend.State
open System
open Fluke.Shared.Domain
open Fluke.Shared
open Fluke.UI.Frontend.State.State
open FsUi.Components


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

        let attachmentIdList =
            React.useMemo (
                (fun () ->
                    attachmentIdMap
                    |> Map.values
                    |> Seq.fold Set.union Set.empty
                    |> Seq.toList),
                [|
                    box attachmentIdMap
                |]
            )

        let setInformationUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Information)

        let lastDatabaseSelected, setLastDatabaseSelected = Store.useState Atoms.User.lastDatabaseSelected

        let isReadWrite =
            Store.useValue (
                Selectors.Database.isReadWrite (
                    lastDatabaseSelected
                    |> Option.defaultValue Database.Default.Id
                )
            )

        let onAttachmentAdd =
            Store.useCallbackRef
                (fun _ setter attachmentId ->
                    promise {
                        match lastDatabaseSelected, information with
                        | Some lastDatabaseSelected, Some information ->
                            Atom.set
                                setter
                                (Atoms.Attachment.parent attachmentId)
                                (Some (AttachmentParent.Information (lastDatabaseSelected, information)))
                        | _ -> ()
                    })

        let onAttachmentDelete =
            Store.useCallbackRef
                (fun getter _setter attachmentId ->
                    promise {
                        let databaseIdList =
                            attachmentIdMap
                            |> Map.toSeq
                            |> Seq.filter (fun (_, attachmentIdSet) -> attachmentIdSet.Contains attachmentId)
                            |> Seq.map fst
                            |> Seq.toList

                        match databaseIdList, information with
                        | _ :: _, Some _information ->
                            do! Engine.deleteParent getter (Atoms.Attachment.attachment attachmentId)
                            return true
                        | _ -> return false
                    })

        Accordion.AccordionAtom
            {|
                Props = fun x -> x.flex <- "1"
                Atom = Atoms.User.accordionHiddenFlag AccordionType.InformationForm
                Items =
                    [
                        str "Info",
                        (Ui.stack
                            (fun x -> x.spacing <- "15px")
                            [
                                DatabaseSelector.DatabaseSelector
                                    (lastDatabaseSelected
                                     |> Option.defaultValue Database.Default.Id)
                                    (Some >> setLastDatabaseSelected)

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
                            isReadWrite
                            && information
                               |> Information.Name
                               |> InformationName.Value
                               |> String.IsNullOrWhiteSpace
                               |> not
                            ->
                            str "Attachments",
                            (Ui.stack
                                (fun x ->
                                    x.spacing <- "10px"
                                    x.flex <- "1")
                                [
                                    AttachmentPanel.AttachmentPanel
                                        (AttachmentParent.Information (
                                            (lastDatabaseSelected
                                             |> Option.defaultValue Database.Default.Id),
                                            information
                                        ))
                                        (Some onAttachmentAdd)
                                        onAttachmentDelete
                                        attachmentIdList
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
