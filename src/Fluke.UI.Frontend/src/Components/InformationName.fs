namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module InformationName =

    [<ReactComponent>]
    let InformationName (input: {| InformationId: Recoil.Atoms.Information.InformationId |}) =
        let information = Recoil.useValue (Recoil.Atoms.Information.wrappedInformation input.InformationId)
        let attachments = Recoil.useValue (Recoil.Atoms.Information.attachments input.InformationId)
        let (Model.InformationName informationName) = information.Name

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
                        str informationName
                    ]


                TooltipPopup.TooltipPopup attachments
            ]
