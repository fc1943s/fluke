namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
open Fable.Core
open Fluke.UI.Frontend.State
open Fluke.Shared


module TaskForm =
    [<ReactComponent>]
    let TaskForm
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId
                   OnSave: Task -> JS.Promise<unit> |})
        =
        let databaseId = Recoil.useValue (Atoms.Task.databaseId input.TaskId)
        let (DatabaseName databaseName) = Recoil.useValue (Atoms.Database.name databaseId)

        let toast = Chakra.useToast ()


        //        printfn
//            $"TaskForm databaseId={databaseId} databaseName={databaseName}
//        input.TaskId={input.TaskId} informationFieldOptions={informationFieldOptions}"

        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        let! taskName = setter.snapshot.getReadWritePromise Atoms.Task.name input.TaskId
                        let! taskInformation = setter.snapshot.getReadWritePromise Atoms.Task.information input.TaskId
                        let! taskScheduling = setter.snapshot.getReadWritePromise Atoms.Task.scheduling input.TaskId


                        match taskName |> TaskName.Value,
                              taskInformation
                              |> Information.Name
                              |> InformationName.Value with
                        | String.InvalidString, _ -> toast (fun x -> x.description <- "Invalid name")
                        | _, String.InvalidString -> toast (fun x -> x.description <- "Invalid information")
                        | _ ->

                            //
//                            let eventId = Atoms.Events.newEventId ()
//                            let event = Atoms.Events.Event.AddTask (eventId, name)
//                            setter.set (Atoms.Events.events eventId, event)
//                            printfn $"event {event}"

                            let! task =
                                if input.TaskId = Task.Default.Id then
                                    { Task.Default with
                                        Id = TaskId.NewId ()
                                        Name = taskName
                                        Information = taskInformation
                                        Scheduling = taskScheduling
                                    }
                                    |> Promise.lift
                                else
                                    promise {
                                        let! task = setter.snapshot.getPromise (Selectors.Task.task input.TaskId)

                                        return
                                            { task with
                                                Name = taskName
                                                Information = taskInformation
                                                Scheduling = taskScheduling
                                            }
                                    }

                            setter.set (Atoms.Task.databaseId task.Id, databaseId)

                            do! setter.readWriteReset Atoms.Task.name input.TaskId
                            do! setter.readWriteReset Atoms.Task.information input.TaskId
                            do! setter.readWriteReset Atoms.Task.scheduling input.TaskId

                            do! input.OnSave task
                    })

        Chakra.stack
            (fun x -> x.spacing <- "25px")
            [
                Chakra.box
                    (fun x -> x.fontSize <- "15px")
                    [
                        str "Add Task"
                    ]

                Chakra.box
                    (fun _ -> ())
                    [
                        str $"Database: {databaseName}"
                    ]


                InformationSelector.InformationSelector
                    {|
                        Username = input.Username
                        DisableResource = true
                        SelectionType = InformationSelector.InformationSelectionType.Information
                        TaskId = input.TaskId
                    |}

                SchedulingSelector.SchedulingSelector
                    {|
                        Username = input.Username
                        TaskId = input.TaskId
                    |}

                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Input.Input
                            (fun x ->
                                x.autoFocus <- true
                                x.label <- str "Name"
                                x.placeholder <- $"""new-task-{DateTime.Now.Format "yyyy-MM-dd"}"""
                                x.atom <- Some (Recoil.AtomFamily (Atoms.Task.name, input.TaskId))
                                x.inputScope <- Some (Recoil.InputScope.ReadWrite Gun.defaultSerializer)
                                x.onFormat <- Some (fun (TaskName name) -> name)
                                x.onEnterPress <- Some onSave
                                x.onValidate <- Some (fst >> TaskName >> Some))
                    ]

                Chakra.button
                    (fun x -> x.onClick <- onSave)
                    [
                        str "Save"
                    ]
            ]
