namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model


module InformationName =
    [<ReactComponent>]
    let InformationName (input: {| Information: Information |}) =
        let attachments = Store.useValue (Selectors.Information.attachments input.Information)
        let cellSize = Store.useValue Atoms.cellSize

        let detailsClick =
            Store.useCallback (
                (fun get set _ ->
                    promise {
                        let deviceInfo = Atoms.getAtomValue get Selectors.deviceInfo

                        if deviceInfo.IsMobile then Atoms.setAtomValue set Atoms.leftDock None

                        Atoms.setAtomValue set Atoms.rightDock (Some TempUI.DockType.Information)

                        Atoms.setAtomValue
                            set
                            (Atoms.uiFlag Atoms.UIFlagType.Information)
                            (input.Information |> Atoms.UIFlag.Information)
                    }),
                [|
                    box input
                |]
            )

        Chakra.box
            (fun x ->
                x.position <- "relative"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px")
            [
                Chakra.box
                    (fun x ->
                        x.whiteSpace <- "nowrap"
                        x.color <- TempUI.informationColor input.Information)
                    [
                        str (
                            input.Information
                            |> Information.Name
                            |> InformationName.Value
                            |> function
                            | "" -> "???"
                            | x -> x
                        )

                        InputLabelIconButton.InputLabelIconButton
                            {|
                                Props =
                                    fun x ->
                                        x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                        x.fontSize <- "11px"
                                        x.height <- "15px"
                                        x.color <- "whiteAlpha.700"
                                        x.marginTop <- "-1px"
                                        x.marginLeft <- "6px"
                                        x.onClick <- detailsClick
                            |}
                    ]

                AttachmentIndicator.AttachmentIndicator {| Attachments = attachments |}
            ]
