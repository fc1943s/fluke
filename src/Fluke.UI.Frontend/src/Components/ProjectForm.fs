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
        let information, setInformation = Recoil.useState (Atoms.Task.information input.TaskId)

        let project =
            match information with
            | Project project -> Some project
            | _ -> None

        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        let projectName =
                            information
                            |> Information.Name
                            |> InformationName.Value

                        match projectName with
                        | String.NullString
                        | String.WhitespaceStr -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let project : Project =
                                {
                                    Name = ProjectName projectName
                                    Area = { Name = AreaName "" }
                                }

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
                        Area =
                            (project |> Option.defaultValue Project.Default)
                                .Area
                        OnSelect =
                            fun area ->
                                setInformation (
                                    Project
                                        { (project |> Option.defaultValue Project.Default) with
                                            Area = area
                                        }
                                )
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
                                    information
                                    |> Information.Name
                                    |> InformationName.Value
                                    |> Some

                                x.onChange <-
                                    fun (e: KeyboardEvent) ->
                                        promise {
                                            setInformation (
                                                Project
                                                    { (project |> Option.defaultValue Project.Default) with
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
