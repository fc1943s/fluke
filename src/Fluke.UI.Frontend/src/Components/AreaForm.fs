namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.Shared


module AreaForm =

    [<ReactComponent>]
    let AreaForm
        (input: {| Username: UserInteraction.Username
                   Area: Area
                   OnSave: Area -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()
        let areaName, setAreaName = React.useState input.Area.Name

        let onSave =
            Recoil.useCallbackRef
                (fun _ _ ->
                    promise {
                        match areaName |> AreaName.Value with
                        | String.InvalidString -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let area = { Name = areaName }
                            do! input.OnSave area
                    })

        Chakra.stack
            (fun x -> x.spacing <- "25px")
            [
                Chakra.box
                    (fun x -> x.fontSize <- "15px")
                    [
                        str "Add Area"
                    ]

                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Input.Input
                            (fun x ->
                                x.autoFocus <- true
                                x.label <- str "Name"
                                x.placeholder <- "e.g. chores"
                                x.value <- areaName |> AreaName.Value |> Some
                                x.onChange <- fun (e: KeyboardEvent) -> promise { setAreaName (AreaName e.Value) }
                                x.onEnterPress <- Some onSave)
                    ]

                Chakra.button
                    (fun x -> x.onClick <- onSave)
                    [
                        str "Save"
                    ]
            ]
