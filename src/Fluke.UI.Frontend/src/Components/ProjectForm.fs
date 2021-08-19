namespace rec Fluke.UI.Frontend.Components

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


module ProjectForm =
    [<ReactComponent>]
    let ProjectForm (project: Project) (onSave: Project -> JS.Promise<unit>) =
        let toast = Ui.useToast ()
        let projectName, setProjectName = React.useState project.Name
        let area, setArea = React.useState project.Area

        let onSave =
            Store.useCallbackRef
                (fun _ _ _ ->
                    promise {
                        match projectName, area.Name with
                        | ProjectName String.Invalid, _ -> toast (fun x -> x.description <- "Invalid name")
                        | _, AreaName String.Invalid -> toast (fun x -> x.description <- "Invalid area")
                        | _ ->
                            let project: Project = { Name = projectName; Area = area }
                            do! onSave project
                    })

        Ui.stack
            (fun x -> x.spacing <- "18px")
            [
                AreaSelector.AreaSelector area setArea

                Ui.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Input.Input
                            {|
                                CustomProps =
                                    fun x ->
                                        x.fixedValue <- projectName |> ProjectName.Value |> Some
                                        x.onEnterPress <- Some onSave
                                Props =
                                    fun x ->
                                        x.autoFocus <- true
                                        x.label <- str "Name"
                                        x.placeholder <- "e.g. home renovation"

                                        x.onChange <-
                                            fun (e: KeyboardEvent) -> promise { setProjectName (ProjectName e.Value) }
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
