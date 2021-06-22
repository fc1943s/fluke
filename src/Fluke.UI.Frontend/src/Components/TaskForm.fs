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
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId
                   OnSave: Task -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()
        let debug = Store.useValue Atoms.debug
        let sessions, setSessions = Store.useState (Atoms.Task.sessions (input.Username, input.TaskId))

        let deleteSession =
            Store.useCallback (
                (fun _get _set start ->
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

        let taskUIFlag, setTaskUIFlag = Store.useState (Atoms.User.uiFlag (input.Username, Atoms.User.UIFlagType.Task))

        let taskDatabaseId =
            match taskUIFlag with
            | Atoms.User.UIFlag.Task (databaseId, taskId) when taskId = input.TaskId -> databaseId
            | _ -> Database.Default.Id

        let onSave =
            Store.useCallback (
                (fun get set _ ->
                    promise {
                        let taskName =
                            Store.getReadWrite get input.Username (Atoms.Task.name (input.Username, input.TaskId))

                        let taskInformation =
                            Store.getReadWrite
                                get
                                input.Username
                                (Atoms.Task.information (input.Username, input.TaskId))

                        let taskScheduling =
                            Store.getReadWrite get input.Username (Atoms.Task.scheduling (input.Username, input.TaskId))

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
                                        let task =
                                            Atoms.getAtomValue get (Selectors.Task.task (input.Username, input.TaskId))

                                        return
                                            { task with
                                                Name = taskName
                                                Information = taskInformation
                                                Scheduling = taskScheduling
                                            }
                                    }

                            Store.readWriteReset set input.Username (Atoms.Task.name (input.Username, input.TaskId))

                            Store.readWriteReset
                                set
                                input.Username
                                (Atoms.Task.information (input.Username, input.TaskId))

                            Store.readWriteReset
                                set
                                input.Username
                                (Atoms.Task.scheduling (input.Username, input.TaskId))

                            Atoms.setAtomValue
                                set
                                (Atoms.User.uiFlag (input.Username, Atoms.User.UIFlagType.Task))
                                (fun _ -> Atoms.User.UIFlag.None)

                            do! input.OnSave task
                    }),
                [|
                    box input
                    box toast
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
                                Username = input.Username
                                TaskId = input.TaskId
                                DatabaseId = taskDatabaseId
                                OnChange =
                                    fun databaseId -> setTaskUIFlag (Atoms.User.UIFlag.Task (databaseId, input.TaskId))
                            |}

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
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        JotaiTypes.InputAtom (
                                                            input.Username,
                                                            JotaiTypes.AtomPath.Atom (
                                                                Atoms.Task.name (input.Username, input.TaskId)
                                                            )
                                                        )
                                                    )

                                                x.inputScope <-
                                                    Some (JotaiTypes.InputScope.ReadWrite Gun.defaultSerializer)

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
                                                        ]

                                                    Menu.Menu
                                                        {|
                                                            Tooltip = ""
                                                            Trigger =
                                                                InputLabelIconButton.InputLabelIconButton
                                                                    {|
                                                                        Props =
                                                                            fun x ->
                                                                                x.``as`` <- Chakra.react.MenuButton

                                                                                x.icon <-
                                                                                    Icons.bs.BsThreeDots |> Icons.render

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
                                                                                    (fun x -> x.fontSize <- "13px")

                                                                            x.onClick <-
                                                                                fun _ ->
                                                                                    promise { do! deleteSession start })
                                                                        [
                                                                            str "Delete Session"
                                                                        ]
                                                                ]
                                                            MenuListProps = fun _ -> ()
                                                        |}
                                                ])
                        ]
            ]
