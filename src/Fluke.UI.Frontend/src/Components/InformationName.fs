namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.Shared


module InformationName =
    [<ReactComponent>]
    let InformationName information =
        let attachmentIdMap = Store.useValue (Selectors.Information.attachmentIdMap information)
        let cellSize = Store.useValue Atoms.User.cellSize

        let selectedDatabaseIdSet = Store.useValue Atoms.User.selectedDatabaseIdSet

        let detailsClick =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let deviceInfo = Store.value getter Selectors.deviceInfo
                        if deviceInfo.IsMobile then Store.set setter Atoms.User.leftDock None
                        Store.set setter Atoms.User.rightDock (Some TempUI.DockType.Information)
                        Store.set setter (Atoms.User.uiFlag UIFlagType.Information) (information |> UIFlag.Information)

                        let databaseIdSearch =
                            attachmentIdMap
                            |> Map.map (fun _ attachmentIdSet -> not attachmentIdSet.IsEmpty)
                            |> Map.toList
                            |> List.filter snd
                            |> List.map fst

                        Store.set
                            setter
                            Atoms.User.lastInformationDatabase
                            (match databaseIdSearch with
                             | [ databaseId ] -> Some databaseId
                             | _ ->
                                 if selectedDatabaseIdSet.Count = 1 then
                                     (selectedDatabaseIdSet |> Seq.head |> Some)
                                 else
                                     None)
                    }),
                [|
                    box selectedDatabaseIdSet
                    box attachmentIdMap
                    box information
                |]
            )

        UI.box
            (fun x ->
                x.position <- "relative"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px")
            [
                UI.box
                    (fun x ->
                        x.whiteSpace <- "nowrap"
                        x.color <- TempUI.informationColor information)
                    [
                        str (
                            information
                            |> Information.Name
                            |> InformationName.Value
                            |> function
                            | "" -> "???"
                            | x -> x
                        )

                        InputLabelIconButton.InputLabelIconButton
                            (fun x ->
                                x.icon <- Icons.fi.FiArrowRight |> Icons.render
                                x.fontSize <- "11px"
                                x.height <- "15px"
                                x.color <- "whiteAlpha.700"
                                x.marginTop <- "-1px"
                                x.marginLeft <- "6px"
                                x.onClick <- detailsClick)
                    ]

                if attachmentIdMap
                   |> Map.values
                   |> Seq.map Set.count
                   |> Seq.sum > 0 then
                    AttachmentIndicator.AttachmentIndicator ()
                else
                    nothing
            ]
