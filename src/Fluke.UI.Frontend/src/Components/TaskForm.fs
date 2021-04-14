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
open Fable.Core
open Fable.Core.JsInterop


module TaskForm =

    [<ReactComponent>]
    let TaskForm
        (input: {| Username: UserInteraction.Username
                   TaskId: Recoil.Atoms.Task.TaskId
                   OnSave: unit -> JS.Promise<unit> |})
        =
        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) ->
                    promise {
                        let eventId = Recoil.Atoms.Events.EventId (JS.Constructors.Date.now (), Guid.NewGuid ())

                        let! name = setter.snapshot.getReadWritePromise Recoil.Atoms.Task.name input.TaskId

                        let! selectedDatabaseIds = setter.snapshot.getPromise Recoil.Atoms.selectedDatabaseIds

                        let databaseId = selectedDatabaseIds |> Array.last

                        let event = Recoil.Atoms.Events.Event.AddTask (eventId, name)

                        setter.set (Recoil.Atoms.Events.events eventId, event)

                        let! databaseStateMapCache =
                            setter.snapshot.getPromise (Recoil.Atoms.Session.databaseStateMapCache input.Username)

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

                                let informationState =
                                    {
                                        Information = information
                                        Attachments = []
                                        SortList = []
                                    }

                                databaseStateMapCache
                                |> Map.add
                                    databaseId
                                    { databaseState with
                                        InformationStateMap =
                                            databaseState.InformationStateMap
                                            |> Map.add information informationState
                                        TaskStateMap =
                                            databaseState.TaskStateMap
                                            |> Map.add task taskState
                                    }
                            | None -> databaseStateMapCache

                        setter.set (Recoil.Atoms.Session.databaseStateMapCache input.Username, newDatabaseStateMapCache)

                        printfn $"event {event}"
                        do! input.OnSave ()
                    })


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
                        Input.Input (
                            jsOptions<_>
                                (fun x ->
                                    x.autoFocus <- true
                                    x.label <- "Name"
                                    x.placeholder <- $"""new-task-{DateTime.Now.Format "yyyy-MM-dd"}"""
                                    x.atom <- Some (Recoil.AtomFamily (Recoil.Atoms.Task.name, input.TaskId))
                                    x.onFormat <- Some (fun (TaskName name) -> name)
                                    x.onEnterPress <- Some onSave
                                    x.onValidate <- Some (TaskName >> Some))
                        )
                    ]

                Chakra.button
                    {| onClick = onSave |}
                    [
                        str "Save"
                    ]
            ]
