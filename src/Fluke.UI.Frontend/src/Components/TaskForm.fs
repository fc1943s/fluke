namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open System
open Fable.DateFunctions
open Fable.Core
open Fluke.UI.Frontend.State
open Fluke.Shared


module TaskForm =
    [<ReactComponent>]
    let rec TaskForm
        (input: {| TaskId: TaskId
                   OnSave: Task -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()
        let debug = Store.useValue Atoms.debug
        let sessions, setSessions = Store.useState (Atoms.Task.sessions input.TaskId)

        let deleteSession =
            Store.useCallback (
                (fun _ _ start ->
                    promise {
                        let index =
                            sessions
                            |> List.findIndex (fun (Session start') -> start' = start)

                        setSessions (sessions |> List.removeAt index)
                    }),
                [|
                    box sessions
                    box setSessions
                |]
            )

        let taskUIFlag, setTaskUIFlag = Store.useState (Atoms.uiFlag Atoms.UIFlagType.Task)

        let selectedTaskIdSet = Store.useValue Selectors.Session.selectedTaskIdSet

        let taskDatabaseId =
            React.useMemo (
                (fun () ->
                    match taskUIFlag with
                    | Atoms.UIFlag.Task (databaseId, taskId) when
                        taskId = input.TaskId
                        && selectedTaskIdSet.Contains taskId -> databaseId
                    | _ -> Database.Default.Id),
                [|
                    box selectedTaskIdSet
                    box taskUIFlag
                    box input.TaskId
                |]
            )

        let onSave =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let taskName = Store.getReadWrite getter (Atoms.Task.name input.TaskId)
                        let taskInformation = Store.getReadWrite getter (Atoms.Task.information input.TaskId)
                        let taskScheduling = Store.getReadWrite getter (Atoms.Task.scheduling input.TaskId)

                        if taskDatabaseId = Database.Default.Id then
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
                                        let task = Store.value getter (Selectors.Task.task input.TaskId)

                                        return
                                            { task with
                                                Name = taskName
                                                Information = taskInformation
                                                Scheduling = taskScheduling
                                            }
                                    }

                            Store.readWriteReset setter (Atoms.Task.name input.TaskId)
                            Store.readWriteReset setter (Atoms.Task.information input.TaskId)
                            Store.readWriteReset setter (Atoms.Task.scheduling input.TaskId)
                            Store.set setter (Atoms.uiFlag Atoms.UIFlagType.Task) Atoms.UIFlag.None

                            do! input.OnSave task
                    }),
                [|
                    box input
                    box toast
                    box taskDatabaseId
                |]
            )

        Chakra.stack
            (fun x -> x.spacing <- "30px")
            [
                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Chakra.box
                            (fun x -> x.fontSize <- "15px")
                            [
                                str $"""{if input.TaskId = Task.Default.Id then "Add" else "Edit"} Task"""
                            ]

                        if not debug then
                            nothing
                        else
                            Chakra.box
                                (fun _ -> ())
                                [
                                    str $"{input.TaskId}"
                                ]

                        DatabaseSelector.DatabaseSelector
                            {|
                                TaskId = input.TaskId
                                DatabaseId = taskDatabaseId
                                OnChange =
                                    fun databaseId -> setTaskUIFlag (Atoms.UIFlag.Task (databaseId, input.TaskId))
                            |}

                        InformationSelector.InformationSelector
                            {|
                                DisableResource = true
                                SelectionType = InformationSelector.InformationSelectionType.Information
                                TaskId = input.TaskId
                            |}

                        SchedulingSelector.SchedulingSelector {| TaskId = input.TaskId |}

                        Chakra.stack
                            (fun x -> x.spacing <- "15px")
                            [
                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (
                                                            Store.AtomReference.Atom (Atoms.Task.name input.TaskId)
                                                        )
                                                    )

                                                x.inputScope <- Some (Store.InputScope.ReadWrite Gun.defaultSerializer)

                                                x.onFormat <- Some (fun (TaskName name) -> name)
                                                x.onEnterPress <- Some onSave
                                                x.onValidate <- Some (fst >> TaskName >> Some)
                                        Props =
                                            fun x ->
                                                x.autoFocus <- true
                                                x.label <- str "Name"
                                                x.placeholder <- $"""new-task-{DateTime.Now.Format "yyyy-MM-dd"}"""
                                    |}
                            ]

                        Button.Button
                            {|
                                Hint = None
                                Icon = Some (Icons.fi.FiSave |> Icons.wrap, Button.IconPosition.Left)
                                Props = fun x -> x.onClick <- onSave
                                Children =
                                    [
                                        str "Save"
                                    ]
                            |}
                    ]


                if input.TaskId = Task.Default.Id then
                    nothing
                else
                    Html.hr []

                    Chakra.stack
                        (fun x -> x.spacing <- "15px")
                        [
                            Chakra.box
                                (fun x -> x.fontSize <- "15px")
                                [
                                    str "Sessions"
                                ]

                            match sessions with
                            | [] ->
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str "No sessions found"
                                    ]
                            | sessions ->
                                yield!
                                    sessions
                                    |> List.map
                                        (fun (Session start) ->
                                            Chakra.flex
                                                (fun _ -> ())
                                                [
                                                    Chakra.box
                                                        (fun _ -> ())
                                                        [
                                                            str (start |> FlukeDateTime.Stringify)

                                                            Menu.Menu
                                                                {|
                                                                    Tooltip = ""
                                                                    Trigger =
                                                                        InputLabelIconButton.InputLabelIconButton
                                                                            {|
                                                                                Props =
                                                                                    fun x ->
                                                                                        x.``as`` <-
                                                                                            Chakra.react.MenuButton

                                                                                        x.icon <-
                                                                                            Icons.bs.BsThreeDots
                                                                                            |> Icons.render

                                                                                        x.fontSize <- "11px"
                                                                                        x.height <- "15px"
                                                                                        x.color <- "whiteAlpha.700"
                                                                                        x.marginTop <- "-1px"
                                                                                        x.marginLeft <- "6px"
                                                                            |}
                                                                    Body =
                                                                        [
                                                                            Chakra.menuItem
                                                                                (fun x ->
                                                                                    x.closeOnSelect <- true

                                                                                    x.icon <-
                                                                                        Icons.bs.BsTrash
                                                                                        |> Icons.renderChakra
                                                                                            (fun x ->
                                                                                                x.fontSize <- "13px")

                                                                                    x.onClick <-
                                                                                        fun _ ->
                                                                                            promise {
                                                                                                do! deleteSession start
                                                                                            })
                                                                                [
                                                                                    str "Delete Session"
                                                                                ]
                                                                        ]
                                                                    MenuListProps = fun _ -> ()
                                                                |}
                                                        ]
                                                ])
                        ]
            ]
