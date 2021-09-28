namespace Fluke.UI.Frontend.Components

open FsCore
open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open FsJs
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.Shared
open Fluke.UI.Frontend.State.State
open FsUi.Components


module InformationName =
    module Actions =
        let details =
            Atom.Primitives.setSelector
                (fun getter setter information ->
                    let attachmentIdMap = Atom.get getter (Selectors.Information.attachmentIdMap information)
                    let selectedDatabaseIdSet = Atom.get getter Atoms.User.selectedDatabaseIdSet
                    Profiling.addTimestamp (fun () -> $"{nameof Fluke} | InformationName.Actions.details") getLocals

                    Atom.set
                        setter
                        Navigate.Actions.navigate
                        (Navigate.DockPosition.Right,
                         Some TempUI.DockType.Information,
                         UIFlagType.Information,
                         UIFlag.Information information)

                    let databaseIdSearch =
                        attachmentIdMap
                        |> Map.map (fun _ attachmentIdSet -> not attachmentIdSet.IsEmpty)
                        |> Map.toList
                        |> List.filter snd
                        |> List.map fst

                    Atom.set
                        setter
                        Atoms.User.lastDatabaseSelected
                        (match databaseIdSearch with
                         | [ databaseId ] -> Some databaseId
                         | _ ->
                             if selectedDatabaseIdSet.Count = 1 then
                                 (selectedDatabaseIdSet |> Seq.head |> Some)
                             else
                                 None))

    [<ReactComponent>]
    let InformationName information =
        let attachmentIdMap = Store.useValue (Selectors.Information.attachmentIdMap information)
        let cellSize = Store.useValue Atoms.User.cellSize
        let details = Store.useSetState Actions.details

        Ui.flex
            (fun x ->
                x.position <- "relative"
                x.height <- $"{cellSize}px"
                x.alignItems <- "center"
                x.lineHeight <- $"{cellSize}px")
            [
                Ui.box
                    (fun x ->
                        x.whiteSpace <- "nowrap"
                        x.color <- TempUI.informationColor information)
                    [
                        match information
                              |> Information.Name
                              |> InformationName.Value with
                        | String.Valid name ->
                            React.fragment [
                                str name

                                InputLabelIconButton.InputLabelIconButton
                                    (fun x ->
                                        x.icon <- Icons.fi.FiArrowRight |> Icons.render
                                        x.fontSize <- "11px"
                                        x.height <- "15px"
                                        x.color <- "whiteAlpha.700"
                                        x.marginTop <- "-1px"
                                        x.marginLeft <- "6px"
                                        x.onClick <- fun _ -> promise { details information })
                            ]
                        | _ -> LoadingSpinner.InlineLoadingSpinner ()
                    ]

                if attachmentIdMap
                   |> Map.values
                   |> Seq.map Set.count
                   |> Seq.sum > 0 then
                    AttachmentIndicator.AttachmentIndicator ()
                else
                    nothing
            ]
