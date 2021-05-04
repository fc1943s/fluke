namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model


module InformationName =

    [<ReactComponent>]
    let InformationName (input: {| InformationId: InformationId |}) =
        let information = Recoil.useValue (Atoms.Information.wrappedInformation input.InformationId)
        let attachments = Recoil.useValue (Atoms.Information.attachments input.InformationId)

        Chakra.box
            (fun x ->
                x.position <- "relative"
                x.height <- "17px"
                x.lineHeight <- "17px")
            [
                Chakra.box
                    (fun x ->
                        x.whiteSpace <- "nowrap"
                        x.color <- TempUI.informationColor information)
                    [
                        str (
                            information
                            |> Information.Name
                            |> InformationName.Value
                        )
                    ]


                TooltipPopup.TooltipPopup attachments
            ]
