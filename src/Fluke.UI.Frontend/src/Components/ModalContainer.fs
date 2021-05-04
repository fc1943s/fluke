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
                        fun (formIdFlag, onHide) ->
                            DatabaseForm.DatabaseForm
                                {|
                                    Username = input.Username
                                    DatabaseId = formIdFlag |> Option.map DatabaseId
                                    OnSave =
                                        fun database ->
                                            promise {
                                                hydrateDatabase Recoil.AtomScope.ReadOnly database
                                                onHide ()
                                            }
                                |}
                    TextKey = TextKey (nameof DatabaseForm)
                |}

            ModalForm.ModalForm
                {|
                    Username = input.Username
                    Content =
                        fun (formIdFlag, onHide) ->
                            TaskForm.TaskForm
                                {|
                                    Username = input.Username
                                    TaskId = formIdFlag |> Option.map TaskId
                                    OnSave = fun task -> promise {
                                        hydrateTask Recoil.AtomScope.ReadOnly task
                                        onHide ()
                                    }
                                |}
                    TextKey = TextKey (nameof TaskForm)
                |}
        ]
