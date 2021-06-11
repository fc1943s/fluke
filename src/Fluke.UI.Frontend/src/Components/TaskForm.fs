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
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared


module TaskForm =
    [<ReactComponent>]
    let TaskForm
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId
                   OnSave: Task -> JS.Promise<unit> |})
        =
        let databaseId, setDatabaseId = Recoil.useState (Selectors.Task.databaseId (input.Username, input.TaskId))

        let (DatabaseName databaseName) = Recoil.useValue (Atoms.Database.name (input.Username, databaseId))

        let databaseIdSet = Recoil.useValue (Atoms.User.databaseIdSet input.Username)

        JS.log (fun () -> $"TaskForm.render. databaseIdSet={databaseIdSet}")

        let setDatabaseIdSet = Recoil.useSetStatePrev (Atoms.User.databaseIdSet input.Username)

        let hydrateDatabase = Hydrate.useHydrateDatabase ()

        let databaseIdList = databaseIdSet |> Set.toList

        let filteredDatabaseIdList =
            databaseIdList
            |> List.map Selectors.Database.isReadWrite
            |> Recoil.waitForNone
            |> Recoil.useValue
            |> List.mapi
                (fun i isReadWrite ->
                    match isReadWrite.state () with
                    | HasValue true -> Some databaseIdList.[i]
                    | _ -> None)
            |> List.choose id

        let databaseNameList =
            filteredDatabaseIdList
            |> List.map (fun databaseId -> Atoms.Database.name (input.Username, databaseId))
            |> Recoil.waitForNone
            |> Recoil.useValue
            |> List.map
                (fun name ->
                    name.valueMaybe ()
                    |> Option.map DatabaseName.Value
                    |> Option.defaultValue "")

        let index =
            React.useMemo (
                (fun () ->
                    filteredDatabaseIdList
                    |> List.sort
                    |> List.tryFindIndex ((=) databaseId)
                    |> Option.defaultValue -1),
                [|
                    box filteredDatabaseIdList
                    box databaseId
                |]
            )

        let toast = Chakra.useToast ()

        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        let! taskName =
                            setter.snapshot.getReadWritePromise
                                input.Username
                                Atoms.Task.name
                                (input.Username, input.TaskId)

                        let! taskInformation =
                            setter.snapshot.getReadWritePromise
                                input.Username
                                Atoms.Task.information
                                (input.Username, input.TaskId)

                        let! taskScheduling =
                            setter.snapshot.getReadWritePromise
                                input.Username
                                Atoms.Task.scheduling
                                (input.Username, input.TaskId)

                        if databaseId = Database.Default.Id then
                            toast (fun x -> x.description <- "Invalid database")
                        elif (match taskName |> TaskName.Value with
                              | String.InvalidString -> true
                              | _ -> false) then
                            toast (fun x -> x.description <- "Invalid name")
                        elif (match taskInformation
                                    |> Information.Name
                                    |> InformationName.Value with
                              | String.InvalidString -> true
                              | _ -> false) then
                            toast (fun x -> x.description <- "Invalid information")
                        else
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
                                        let! task =
                                            setter.snapshot.getPromise (
                                                Selectors.Task.task (input.Username, input.TaskId)
                                            )

                                        return
                                            { task with
                                                Name = taskName
                                                Information = taskInformation
                                                Scheduling = taskScheduling
                                            }
                                    }

                            do! setter.readWriteReset input.Username Atoms.Task.name (input.Username, input.TaskId)

                            do!
                                setter.readWriteReset
                                    input.Username
                                    Atoms.Task.information
                                    (input.Username, input.TaskId)

                            do!
                                setter.readWriteReset
                                    input.Username
                                    Atoms.Task.scheduling
                                    (input.Username, input.TaskId)

                            do! input.OnSave task
                    })

        Chakra.stack
            (fun x -> x.spacing <- "25px")
            [
                Chakra.box
                    (fun x -> x.fontSize <- "15px")
                    [

                        str $"""{if input.TaskId = Task.Default.Id then "Add" else "Edit"} Task"""
                    ]

                Chakra.box
                    (fun _ -> ())
                    [
                        InputLabel.InputLabel
                            {|
                                Hint = None
                                HintTitle = None
                                Label = str "Database"
                                Props = fun x -> x.marginBottom <- "5px"
                            |}
                        Menu.Drawer
                            {|
                                Tooltip = ""
                                Trigger =
                                    fun visible setVisible ->
                                        Button.Button
                                            {|
                                                Hint = None
                                                Icon =
                                                    Some (
                                                        Icons.fi.FiChevronDown |> Icons.wrap,
                                                        Button.IconPosition.Right
                                                    )
                                                Props =
                                                    fun x ->
                                                        x.onClick <- fun _ -> promise { setVisible (not visible) }
                                                        if input.TaskId <> Task.Default.Id then x.isDisabled <- true
                                                Children =
                                                    [
                                                        match databaseName with
                                                        | String.ValidString name -> str name
                                                        | _ -> str "Select..."
                                                    ]
                                            |}
                                Body =
                                    fun onHide ->
                                        [
                                            Chakra.stack
                                                (fun x ->
                                                    x.flex <- "1"
                                                    x.spacing <- "1px"
                                                    x.padding <- "1px"
                                                    x.marginBottom <- "6px"
                                                    x.maxHeight <- "217px"
                                                    x.overflowY <- "auto"
                                                    x.flexBasis <- 0)
                                                [
                                                    yield!
                                                        filteredDatabaseIdList
                                                        |> List.mapi
                                                            (fun i databaseId ->
                                                                let label = databaseNameList.[i]

                                                                let cmp =
                                                                    Button.Button
                                                                        {|
                                                                            Hint = None
                                                                            Icon =
                                                                                Some (
                                                                                    (if index = i then
                                                                                         Icons.fi.FiCheck |> Icons.wrap
                                                                                     else
                                                                                         fun () ->
                                                                                             (Chakra.box
                                                                                                 (fun x ->
                                                                                                     x.width <- "11px")
                                                                                                 [])),
                                                                                    Button.IconPosition.Left
                                                                                )
                                                                            Props =
                                                                                fun x ->
                                                                                    x.onClick <-
                                                                                        fun _ ->
                                                                                            promise {
                                                                                                setDatabaseId databaseId

                                                                                                onHide ()
                                                                                            }

                                                                                    x.alignSelf <- "stretch"

                                                                                    x.backgroundColor <-
                                                                                        "whiteAlpha.100"

                                                                                    x.borderRadius <- "2px"
                                                                            Children =
                                                                                [
                                                                                    str label
                                                                                ]
                                                                        |}

                                                                Some (label, cmp))
                                                        |> List.sortBy (Option.map fst)
                                                        |> List.map (Option.map snd)
                                                        |> List.map (Option.defaultValue nothing)
                                                ]

                                            Menu.Drawer
                                                {|
                                                    Tooltip = ""
                                                    Trigger =
                                                        fun visible setVisible ->
                                                            Button.Button
                                                                {|
                                                                    Hint = None
                                                                    Icon =
                                                                        Some (
                                                                            (if visible then
                                                                                 Icons.fi.FiChevronUp
                                                                             else
                                                                                 Icons.fi.FiChevronDown)
                                                                            |> Icons.wrap,
                                                                            Button.IconPosition.Right
                                                                        )
                                                                    Props =
                                                                        fun x ->
                                                                            x.onClick <-
                                                                                fun _ ->
                                                                                    promise { setVisible (not visible) }
                                                                    Children =
                                                                        [
                                                                            str "Add Database"
                                                                        ]
                                                                |}
                                                    Body =
                                                        fun onHide ->
                                                            [
                                                                DatabaseForm.DatabaseForm
                                                                    {|
                                                                        Username = input.Username
                                                                        DatabaseId = Database.Default.Id
                                                                        OnSave =
                                                                            fun database ->
                                                                                promise {
                                                                                    hydrateDatabase
                                                                                        input.Username
                                                                                        Recoil.AtomScope.ReadOnly
                                                                                        database

                                                                                    JS.setTimeout
                                                                                        (fun () ->
                                                                                            setDatabaseIdSet (
                                                                                                Set.add database.Id
                                                                                            ))
                                                                                        0
                                                                                    |> ignore

                                                                                    onHide ()
                                                                                }
                                                                    |}
                                                            ]
                                                |}
                                        ]
                            |}
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

                                x.atom <-
                                    Some (
                                        Recoil.AtomFamily (
                                            input.Username,
                                            Atoms.Task.name,
                                            (input.Username, input.TaskId)
                                        )
                                    )

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
