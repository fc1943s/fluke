namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module ModalContainer =
    [<ReactComponent>]
    let ModalContainer (input: {| Username: Username |}) =
        let hydrateDatabase = Hydrate.useHydrateDatabase ()
        let hydrateTask = Hydrate.useHydrateTask ()

        React.fragment [
            ModalForm.ModalForm
                {|
                    Username = input.Username
                    Content =
                        fun (formIdFlag, onHide, _) ->
                            let databaseId =
                                formIdFlag
                                |> Option.map DatabaseId
                                |> Option.defaultValue Database.Default.Id

                            DatabaseForm.DatabaseForm
                                {|
                                    Username = input.Username
                                    DatabaseId = databaseId
                                    OnSave =
                                        fun database ->
                                            promise {
                                                hydrateDatabase input.Username Recoil.AtomScope.ReadOnly database
                                                onHide ()
                                            }
                                |}
                    TextKey = TextKey (nameof DatabaseForm)
                |}

            ModalForm.ModalForm
                {|
                    Username = input.Username
                    Content =
                        fun (formIdFlag, onHide, setter) ->
                            let taskId =
                                formIdFlag
                                |> Option.map TaskId
                                |> Option.defaultValue Task.Default.Id

                            TaskForm.TaskForm
                                {|
                                    Username = input.Username
                                    TaskId = taskId
                                    OnSave =
                                        fun task ->
                                            promise {
                                                let! databaseId =
                                                    setter()
                                                        .snapshot
                                                        .getPromise (
                                                            Atoms.Task.databaseId (input.Username, taskId)
                                                        )

                                                hydrateTask input.Username Recoil.AtomScope.ReadOnly databaseId task
                                                onHide ()
                                            }
                                |}
                    TextKey = TextKey (nameof TaskForm)
                |}
        ]
