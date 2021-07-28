namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open FsStore
open FsCore
open FsUi.Bindings
open Fable.Core
open Fluke.Shared
open FsUi.Components


module AreaForm =

    [<ReactComponent>]
    let AreaForm (area: Area) (onSave: Area -> JS.Promise<unit>) =
        let toast = UI.useToast ()
        let areaName, setAreaName = React.useState area.Name

        let onSave =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        match areaName |> AreaName.Value with
                        | String.InvalidString -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let area: Area = { Name = areaName }
                            do! onSave area
                    }),
                [|
                    box areaName
                    box onSave
                    box toast
                |]
            )

        UI.stack
            (fun x -> x.spacing <- "18px")
            [
                UI.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Input.Input
                            {|
                                CustomProps =
                                    fun x ->
                                        x.fixedValue <- areaName |> AreaName.Value |> Some
                                        x.onEnterPress <- Some onSave
                                Props =
                                    fun x ->
                                        x.autoFocus <- true
                                        x.label <- str "Name"
                                        x.placeholder <- "e.g. chores"

                                        x.onChange <-
                                            fun (e: KeyboardEvent) -> promise { setAreaName (AreaName e.Value) }
                            |}
                    ]


                Button.Button
                    {|
                        Hint = None
                        Icon = Some (Icons.fi.FiSave |> Icons.render, Button.IconPosition.Left)
                        Props = fun x -> x.onClick <- onSave
                        Children =
                            [
                                str "Save"
                            ]
                    |}
            ]
