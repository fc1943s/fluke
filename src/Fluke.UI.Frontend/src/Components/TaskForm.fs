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


module TaskForm =

    [<ReactComponent>]
    let TaskForm
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId option
                   OnSave: unit -> JS.Promise<unit> |})
        =
        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        let eventId = Atoms.Events.newEventId ()

                        let! name = setter.snapshot.getReadWritePromise Atoms.Task.name input.TaskId

                        let! selectedDatabaseIds = setter.snapshot.getPromise Atoms.selectedDatabaseIds

                        let databaseId = selectedDatabaseIds |> Array.last

                        let event = Atoms.Events.Event.AddTask (eventId, name)

                        setter.set (Atoms.Events.events eventId, event)

                        let! databaseStateMapCache =
                            setter.snapshot.getPromise (Atoms.Session.databaseStateMapCache input.Username)

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
                                        TaskId = TaskId.NewId ()
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

                        setter.set (Atoms.Session.databaseStateMapCache input.Username, newDatabaseStateMapCache)

                        do! setter.readWriteReset Atoms.Task.name input.TaskId

                        printfn $"event {event}"
                        do! input.OnSave ()
                    })

        let selectedDatabaseIds = Recoil.useValue Atoms.selectedDatabaseIds

        let (DatabaseName databaseName) = Recoil.useValue (Atoms.Database.name (selectedDatabaseIds |> Array.tryHead))

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
                        str $"Selected Database: {databaseName}"
                    ]

                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Input.Input (
                            JS.newObj
                                (fun x ->
                                    x.autoFocus <- true
                                    x.label <- str "Name"
                                    x.placeholder <- $"""new-task-{DateTime.Now.Format "yyyy-MM-dd"}"""
                                    x.atom <- Some (Recoil.AtomFamily (Atoms.Task.name, input.TaskId))
                                    x.onFormat <- Some (fun (TaskName name) -> name)
                                    x.onEnterPress <- Some onSave
                                    x.onValidate <- Some (TaskName >> Some))
                        )
                    ]

                Chakra.button
                    (fun x -> x.onClick <- onSave)
                    [
                        str "Save"
                    ]
            ]
