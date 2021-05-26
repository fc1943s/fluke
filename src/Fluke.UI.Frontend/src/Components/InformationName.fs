namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
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
        let attachments = Recoil.useValue (Atoms.Information.attachments input.Information)
        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)

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
                        )
                    ]


                TooltipPopup.TooltipPopup
                    {|
                        Username = input.Username
                        Attachments = attachments
                    |}
            ]
