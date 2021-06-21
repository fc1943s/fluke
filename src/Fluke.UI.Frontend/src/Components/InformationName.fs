namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model


module InformationName =

    [<ReactComponent>]
    let InformationName
        (input: {| Username: Username
                   Information: Information |})
        =
        let attachments = Store.useValue (Selectors.Information.attachments (input.Username, input.Information))
        let cellSize = Store.useValue (Atoms.User.cellSize input.Username)

        let detailsClick =
            Store.useCallbackRef
                (fun setter _ ->
                    promise {
                        let! deviceInfo = setter.snapshot.getPromise Selectors.deviceInfo

                        if deviceInfo.IsMobile then
                            setter.set (Atoms.User.leftDock input.Username, (fun _ -> None))

                        setter.set (Atoms.User.rightDock input.Username, (fun _ -> Some TempUI.DockType.Information))

                        setter.set (
                            Atoms.User.uiFlag (input.Username, Atoms.User.UIFlagType.Information),
                            fun _ ->
                                input.Information
                                |> Atoms.User.UIFlag.Information
                                |> Some
                        )
                    })

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

                AttachmentIndicator.AttachmentIndicator
                    {|
                        Username = input.Username
                        Attachments = attachments
                    |}
            ]
