namespace Fluke.UI.Frontend.Components

open FsCore
open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fable.Core
open Fluke.Shared
open FsUi.Components


module ResourceForm =
    [<ReactComponent>]
    let ResourceForm (resource: Resource) (onSave: Resource -> JS.Promise<unit>) =
        let toast = Ui.useToast ()
        let resourceName, setResourceName = React.useState resource.Name

        let onSave =
            Store.useCallbackRef
                (fun _ _ _ ->
                    promise {
                        match resourceName with
                        | ResourceName String.Invalid -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let resource: Resource = { Name = resourceName }
                            do! onSave resource
                    })

        Ui.stack
            (fun x -> x.spacing <- "18px")
            [
                Ui.stack
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
                        Tooltip = None
                        Icon = Some (Icons.fi.FiSave |> Icons.render, Button.IconPosition.Left)
                        Props = fun x -> x.onClick <- onSave
                        Children =
                            [
                                str "Save"
                            ]
                    |}
            ]
