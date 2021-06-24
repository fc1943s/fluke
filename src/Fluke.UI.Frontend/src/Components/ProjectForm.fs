namespace rec Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.Shared


module ProjectForm =
    [<ReactComponent>]
    let ProjectForm
        (input: {| Project: Project
                   OnSave: Project -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()
        let projectName, setProjectName = React.useState input.Project.Name
        let area, setArea = React.useState input.Project.Area

        let onSave =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        match projectName, area.Name with
                        | ProjectName String.InvalidString, _ -> toast (fun x -> x.description <- "Invalid name")
                        | _, AreaName String.InvalidString -> toast (fun x -> x.description <- "Invalid area")
                        | _ ->
                            let project : Project = { Name = projectName; Area = area }
                            do! input.OnSave project
                    }),
                [|
                    box input
                    box projectName
                    box area
                    box toast
                |]
            )

        Chakra.stack
            (fun x -> x.spacing <- "18px")
            [
                Chakra.box
                    (fun x ->
                        x.fontSize <- "15px"
                        x.textAlign <- "left")
                    [
                        str "Add Project"
                    ]

                AreaSelector.AreaSelector {| Area = area; OnSelect = setArea |}

                Chakra.stack
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
                        Icon = Some (Icons.fi.FiSave |> Icons.wrap, Button.IconPosition.Left)
                        Props = fun x -> x.onClick <- onSave
                        Children =
                            [
                                str "Save"
                            ]
                    |}
            ]
