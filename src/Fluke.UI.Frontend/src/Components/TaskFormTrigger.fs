namespace Fluke.UI.Frontend.Components

open Fable.Core
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module TaskFormTrigger =
    [<ReactComponent>]
    let TaskFormTrigger
        (input: {| Username: Username
                   DatabaseId: DatabaseId
                   TaskId: TaskId option
                   Trigger: (unit -> unit) -> (unit -> CallbackMethods) -> ReactElement |})
        =
        let hydrateTask = Hydrate.useHydrateTask ()

        ModalFormTrigger.ModalFormTrigger
            {|
                Username = input.Username
                Trigger =
                    fun trigger setter ->
                        React.fragment [
                            input.Trigger trigger setter

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
                                                                hydrateTask
                                                                    input.Username
                                                                    Recoil.AtomScope.ReadOnly
                                                                    task

                                                                JS.setTimeout
                                                                    (fun () ->
                                                                        setter()
                                                                            .set (
                                                                                Atoms.Database.taskIdSet (
                                                                                    input.Username,
                                                                                    input.DatabaseId
                                                                                ),
                                                                                Set.add task.Id
                                                                            ))
                                                                    0
                                                                |> ignore

                                                                onHide ()
                                                            }
                                                |}
                                    TextKey = TextKey (nameof TaskForm)
                                |}
                        ]
                TextKey = TextKey (nameof TaskForm)
                TextKeyValue = input.TaskId |> Option.map TaskId.Value
            |}
