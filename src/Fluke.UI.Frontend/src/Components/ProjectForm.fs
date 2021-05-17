namespace rec Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.UI.Frontend.State
open Fluke.Shared


module ProjectForm =
    [<ReactComponent>]
    let ProjectForm
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId option
                   OnSave: Project -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()

        let informationFieldOptions =
            Recoil.useAtomFieldOptions
                (Some (Recoil.AtomFamily (Atoms.Task.information, input.TaskId)))
                (Some (Recoil.InputScope.ReadWrite Gun.defaultSerializer))

        let project =
            match informationFieldOptions.AtomValue with
            | Project project -> project
            | _ -> Project.Default

        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        match project.Name, project.Area.Name with
                        | ProjectName (String.NullString
                          | String.WhitespaceStr),
                          _ -> toast (fun x -> x.description <- "Invalid name")
                        | _,
                          AreaName (String.NullString
                          | String.WhitespaceStr) -> toast (fun x -> x.description <- "Invalid area")
                        | _ ->
                            do! setter.readWriteReset Atoms.Task.information input.TaskId
                            do! input.OnSave project
                    })

        Chakra.stack
            (fun x -> x.spacing <- "25px")
            [
                Chakra.box
                    (fun x -> x.fontSize <- "15px")
                    [
                        str "Add Project"
                    ]

                AreaSelector.AreaSelector
                    {|
                        Username = input.Username
                        Area = project.Area
                        OnSelect =
                            fun area -> informationFieldOptions.SetAtomValue (Project { project with Area = area })
                    |}

                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Input.Input
                            (fun x ->
                                x.autoFocus <- true
                                x.label <- str "Name"
                                x.placeholder <- "e.g. home-renovation"

                                x.value <-
                                    informationFieldOptions.AtomValue
                                    |> Information.Name
                                    |> InformationName.Value
                                    |> Some

                                x.onChange <-
                                    fun (e: KeyboardEvent) ->
                                        promise {
                                            informationFieldOptions.SetAtomValue (
                                                Project
                                                    { project with
                                                        Name = ProjectName e.Value
                                                    }
                                            )
                                        }

                                x.onEnterPress <- Some onSave)
                    ]

                Chakra.button
                    (fun x -> x.onClick <- onSave)
                    [
                        str "Save"
                    ]
            ]
