namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Model
open Fluke.Shared


module InformationNameComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| InformationId: Recoil.Atoms.Information.InformationId |}) ->
            //            let informationId = Recoil.useValue (Recoil.Atoms.Task.informationId input.TaskId)
            let information = Recoil.useValue (Recoil.Atoms.Information.wrappedInformation input.InformationId)
            let attachments = Recoil.useValue (Recoil.Atoms.Information.attachments input.InformationId)
            let (InformationName informationName) = information.Name

            Chakra.box
                {| className = Css.cellRectangle |}
                [
                    Chakra.box
                        {|
                            whiteSpace = "nowrap"
                            color = information.Color
                        |}
                        [
                            str informationName
                        ]


                    TooltipPopupComponent.render {| Attachments = attachments |}
                ])
