namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.Shared


module ResourceForm =
    [<ReactComponent>]
    let ResourceForm (resource: Resource) (onSave: Resource -> JS.Promise<unit>) =
        let toast = UI.useToast ()
        let resourceName, setResourceName = React.useState resource.Name

        let onSave =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        match resourceName with
                        | ResourceName String.InvalidString -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let resource: Resource = { Name = resourceName }
                            do! onSave resource
                    }),
                [|
                    box onSave
                    box resourceName
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
                                        x.fixedValue <- resourceName |> ResourceName.Value |> Some
                                        x.onEnterPress <- Some onSave
                                Props =
                                    fun x ->
                                        x.autoFocus <- true
                                        x.label <- str "Name"
                                        x.placeholder <- "e.g. linux"

                                        x.onChange <-
                                            fun (e: KeyboardEvent) -> promise { setResourceName (ResourceName e.Value) }
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
