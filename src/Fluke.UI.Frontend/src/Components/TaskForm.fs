namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions


module TaskForm =

    [<ReactComponent>]
    let TaskForm
        (input: {| Username: UserInteraction.Username
                   TaskId: Recoil.Atoms.Task.TaskId
                   OnSave: Async<unit> |})
        =
        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) ->
                    async {
                        let eventId =
                            Recoil.Atoms.Events.EventId (Fable.Core.JS.Constructors.Date.now (), Guid.NewGuid ())

                        let! name = setter.snapshot.getAsync (Recoil.Atoms.Task.name input.TaskId)

                        let! selectedDatabaseIds = setter.snapshot.getAsync Recoil.Atoms.selectedDatabaseIds

                        let databaseId = selectedDatabaseIds |> Array.last

                        let event = Recoil.Atoms.Events.Event.AddTask (eventId, name)

                        setter.set (Recoil.Atoms.Events.events eventId, event)

                        let! databaseStateMapCache =
                            setter.snapshot.getAsync (Recoil.Atoms.Session.databaseStateMapCache input.Username)

                        let databaseState = databaseStateMapCache |> Map.tryFind databaseId

                        let newDatabaseStateMapCache =
                            match databaseState with
                            | Some databaseState ->
                                let information = Area ({ Name = AreaName "workflow" }, [])

                                let task =
                                    {
                                        Name = name
                                        Information = information
                                        Duration = None
                                        PendingAfter = None
                                        MissedAfter = None
                                        Scheduling = Scheduling.Manual ManualScheduling.WithoutSuggestion
                                        Priority = None
                                    }

                                let taskState =
                                    {
                                        Task = task
                                        Sessions = []
                                        Attachments = []
                                        SortList = []
                                        CellStateMap = Map.empty
                                        InformationMap =
                                            [
                                                information, ()
                                            ]
                                            |> Map.ofList
                                    }

                                databaseStateMapCache
                                |> Map.add
                                    databaseId
                                    { databaseState with
                                        InformationStateMap = databaseState.InformationStateMap
                                        TaskStateMap =
                                            databaseState.TaskStateMap
                                            |> Map.add task taskState
                                    }
                            | None -> databaseStateMapCache

                        setter.set (Recoil.Atoms.Session.databaseStateMapCache input.Username, newDatabaseStateMapCache)

                        printfn $"event {event}"
                        do! input.OnSave
                    }
                    |> Async.StartImmediate)

        Chakra.stack
            {| spacing = "25px" |}
            [
                Chakra.box
                    {| fontSize = "15px" |}
                    [
                        str "Add Task"
                    ]

                Chakra.stack
                    {| spacing = "15px" |}
                    [
                        Input.Input
                            {|
                                Label = Some "Name"
                                Placeholder = sprintf "new-task-%s" (DateTime.Now.Format "yyyy-MM-dd")
                                Atom = Recoil.Atoms.Task.name input.TaskId
                                InputFormat = Input.InputFormat.Text
                                OnFormat = fun (TaskName name) -> name
                                OnValidate = TaskName >> Some
                            |}
                    ]

                Chakra.button
                    {| onClick = onSave |}
                    [
                        str "Save"
                    ]
            ]
