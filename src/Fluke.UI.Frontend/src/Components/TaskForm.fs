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
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared


module TaskForm =
    [<ReactComponent>]
    let rec TaskForm (taskId: TaskId) (onSave: Task -> JS.Promise<unit>) =
        let toast = Chakra.useToast ()
        let debug = Store.useValue Atoms.debug
        let sessions, setSessions = Store.useState (Atoms.Task.sessions taskId)

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

        let taskDatabaseId =
            React.useMemo (
                (fun () ->
                    match taskUIFlag with
                    | Atoms.UIFlag.Task (databaseId, taskId') when taskId' = taskId -> databaseId
                    | _ -> Database.Default.Id),
                [|
                    box taskUIFlag
                    box taskId
                |]
            )

        let taskState = Store.useValue (Selectors.Task.taskState taskId)

        let onAttachmentAdd =
            Store.useCallback (
                (fun _ setter attachmentId ->
                    promise { Store.change setter (Atoms.Task.attachmentIdSet taskId) (Set.add attachmentId) }),
                [|
                    box taskId
                |]
            )

        let onSave =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let taskName = Store.getReadWrite getter (Atoms.Task.name taskId)
                        let taskInformation = Store.getReadWrite getter (Atoms.Task.information taskId)
                        let taskScheduling = Store.getReadWrite getter (Atoms.Task.scheduling taskId)

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
                                if taskId = Task.Default.Id then
                                    { Task.Default with
                                        Id = TaskId.NewId ()
                                        Name = taskName
                                        Information = taskInformation
                                        Scheduling = taskScheduling
                                    }
                                    |> Promise.lift
                                else
                                    promise {
                                        let task = Store.value getter (Selectors.Task.task taskId)

                                        return
                                            { task with
                                                Name = taskName
                                                Information = taskInformation
                                                Scheduling = taskScheduling
                                            }
                                    }

                            Store.readWriteReset setter (Atoms.Task.name taskId)
                            Store.readWriteReset setter (Atoms.Task.information taskId)
                            Store.readWriteReset setter (Atoms.Task.scheduling taskId)
                            Store.set setter (Atoms.uiFlag Atoms.UIFlagType.Task) Atoms.UIFlag.None

                            do! onSave task
                    }),
                [|
                    box taskId
                    box onSave
                    box toast
                    box taskDatabaseId
                |]
            )

        Accordion.Accordion
            {|
                Props =
                    fun x ->
                        x.flex <- "1"
                        x.overflowY <- "auto"
                        x.flexBasis <- 0
                Atom = Atoms.accordionFlag (TextKey (nameof TaskForm))
                Items =
                    [
                        $"""{if taskId = Task.Default.Id then "Add" else "Edit"} Task""",
                        (Chakra.stack
                            (fun x -> x.spacing <- "15px")
                            [
                                if not debug then
                                    nothing
                                else
                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            str $"{taskId}"
                                        ]

                                DatabaseSelector.DatabaseSelector
                                    taskDatabaseId
                                    taskId
                                    (fun databaseId -> setTaskUIFlag (Atoms.UIFlag.Task (databaseId, taskId)))

                                InformationSelector.InformationSelector
                                    {|
                                        DisableResource = true
                                        SelectionType = InformationSelector.InformationSelectionType.Information
                                        TaskId = taskId
                                    |}

                                SchedulingSelector.SchedulingSelector taskId

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
                                                                    Store.AtomReference.Atom (Atoms.Task.name taskId)
                                                                )
                                                            )

                                                        x.inputScope <-
                                                            Some (Store.InputScope.ReadWrite Gun.defaultSerializer)

                                                        x.onFormat <- Some (fun (TaskName name) -> name)
                                                        x.onEnterPress <- Some onSave
                                                        x.onValidate <- Some (fst >> TaskName >> Some)
                                                Props =
                                                    fun x ->
                                                        x.autoFocus <- true
                                                        x.label <- str "Name"

                                                        x.placeholder <-
                                                            $"""new-task-{DateTime.Now.Format "yyyy-MM-dd"}"""
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
                            ])

                        if taskId <> Task.Default.Id then
                            "Sessions",
                            (match sessions with
                             | [] ->
                                 Chakra.box
                                     (fun _ -> ())
                                     [
                                         str "No sessions found"
                                     ]
                             | sessions ->
                                 React.fragment [
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
                                                                                 (fun x ->
                                                                                     x.``as`` <- Chakra.react.MenuButton

                                                                                     x.icon <-
                                                                                         Icons.bs.BsThreeDots
                                                                                         |> Icons.render

                                                                                     x.fontSize <- "11px"
                                                                                     x.height <- "15px"
                                                                                     x.color <- "whiteAlpha.700"
                                                                                     x.marginTop <- "-1px"
                                                                                     x.marginLeft <- "6px")
                                                                         Body =
                                                                             [
                                                                                 Chakra.menuItem
                                                                                     (fun x ->
                                                                                         x.closeOnSelect <- true

                                                                                         x.icon <-
                                                                                             Icons.bs.BsTrash
                                                                                             |> Icons.renderChakra
                                                                                                 (fun x ->
                                                                                                     x.fontSize <-
                                                                                                         "13px")

                                                                                         x.onClick <-
                                                                                             fun _ ->
                                                                                                 promise {
                                                                                                     do!
                                                                                                         deleteSession
                                                                                                             start
                                                                                                 })
                                                                                     [
                                                                                         str "Delete Session"
                                                                                     ]
                                                                             ]
                                                                         MenuListProps = fun _ -> ()
                                                                     |}
                                                             ]
                                                     ])
                                 ])

                            "Attachments",
                            (Chakra.stack
                                (fun x ->
                                    x.spacing <- "10px"
                                    x.flex <- "1")
                                [
                                    AttachmentPanel.AttachmentPanel taskState.Attachments onAttachmentAdd
                                ])
                    ]
            |}

    [<ReactComponent>]
    let TaskFormWrapper () =
        let hydrateTaskState = Hydrate.useHydrateTaskState ()
        let selectedTaskIdSet = Store.useValue Selectors.Session.selectedTaskIdSet
        let setRightDock = Store.useSetState Atoms.rightDock

        let taskUIFlag = Store.useValue (Atoms.uiFlag Atoms.UIFlagType.Task)

        let taskDatabaseId =
            match taskUIFlag with
            | Atoms.UIFlag.Task (databaseId, _) -> databaseId
            | _ -> Database.Default.Id

        let setTaskIdSet = Store.useSetStatePrev (Atoms.Database.taskIdSet taskDatabaseId)

        let taskId =
            React.useMemo (
                (fun () ->
                    match taskUIFlag with
                    | Atoms.UIFlag.Task (_, taskId) when selectedTaskIdSet.Contains taskId -> taskId
                    | _ -> Task.Default.Id),
                [|
                    box taskUIFlag
                    box selectedTaskIdSet
                |]
            )

        TaskForm
            taskId
            (fun task ->
                promise {
                    let taskState =
                        {
                            Task = task
                            SortList = []
                            Sessions = []
                            Attachments = []
                            CellStateMap = Map.empty
                        }

                    do! hydrateTaskState (Store.AtomScope.ReadOnly, taskDatabaseId, taskState)

                    if task.Id <> taskId then
                        JS.setTimeout (fun () -> setTaskIdSet (Set.add task.Id)) 0
                        |> ignore

                    setRightDock None
                })
