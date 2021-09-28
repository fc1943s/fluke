namespace Fluke.UI.Frontend.Components

open FsStore.Bindings.Gun
open FsStore.State
open FsJs
open Browser.Types
open FsStore
open FsStore.Hooks
open FsCore
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open FsStore.Model
open FsStore.Utils
open FsUi.Bindings
open System
open Fable.Core
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared
open Fluke.UI.Frontend.TempUI
open Fluke.UI.Frontend.State.State
open FsUi.Components


module TaskForm =
    let inline useStartSession () =
        Store.useCallbackRef
            (fun getter setter taskId ->
                promise {
                    let sessions = Atom.get getter (Atoms.Task.sessions taskId)

                    Atom.set
                        setter
                        (Atoms.Task.sessions taskId)
                        (Session (
                            (let now = DateTime.Now in if now.Second < 30 then now else now.AddMinutes 1.)
                            |> FlukeDateTime.FromDateTime
                         )
                         :: sessions)
                })

    [<ReactComponent>]
    let MissedAfterInput (missedAfter: FlukeTime option) setMissedAfter =
        let dayStart = Store.useValue Atoms.User.dayStart

        Ui.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Missed After"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Ui.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            (if missedAfter.IsNone then Some "Enable" else None)
                            (fun x ->
                                x.isChecked <- missedAfter.IsSome
                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            setMissedAfter (if missedAfter.IsSome then None else (Some dayStart)) })

                        match missedAfter with
                        | Some missedAfter ->
                            Input.Input
                                {|
                                    CustomProps =
                                        fun x ->
                                            x.fixedValue <- Some missedAfter
                                            x.onFormat <- Some FlukeTime.Stringify

                                            x.onValidate <-
                                                Some (
                                                    fst
                                                    >> DateTime.TryParse
                                                    >> function
                                                        | true, value -> value
                                                        | _ -> DateTime.Parse "00:00"
                                                    >> FlukeTime.FromDateTime
                                                    >> Some
                                                )

                                            x.inputFormat <- Some Input.InputFormat.Time
                                    Props =
                                        fun x ->
                                            x.placeholder <- "00:00"

                                            x.onChange <-
                                                (fun (e: KeyboardEvent) ->
                                                    promise {
                                                        e.Value
                                                        |> DateTime.Parse
                                                        |> FlukeTime.FromDateTime
                                                        |> Some
                                                        |> setMissedAfter
                                                    })
                                |}
                        | None -> nothing
                    ]
            ]

    [<ReactComponent>]
    let PendingAfterInput (pendingAfter: FlukeTime option) setPendingAfter =
        let dayStart = Store.useValue Atoms.User.dayStart

        Ui.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Pending After"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Ui.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            (if pendingAfter.IsNone then Some "Enable" else None)
                            (fun x ->
                                x.isChecked <- pendingAfter.IsSome
                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            setPendingAfter (if pendingAfter.IsSome then None else (Some dayStart)) })

                        match pendingAfter with
                        | Some pendingAfter ->
                            Input.Input
                                {|
                                    CustomProps =
                                        fun x ->
                                            x.fixedValue <- Some pendingAfter
                                            x.onFormat <- Some FlukeTime.Stringify

                                            x.onValidate <-
                                                Some (
                                                    fst
                                                    >> DateTime.TryParse
                                                    >> function
                                                        | true, value -> value
                                                        | _ -> DateTime.Parse "00:00"
                                                    >> FlukeTime.FromDateTime
                                                    >> Some
                                                )

                                            x.inputFormat <- Some Input.InputFormat.Time
                                    Props =
                                        fun x ->
                                            x.placeholder <- "00:00"

                                            x.onChange <-
                                                (fun (e: KeyboardEvent) ->
                                                    promise {
                                                        e.Value
                                                        |> DateTime.Parse
                                                        |> FlukeTime.FromDateTime
                                                        |> Some
                                                        |> setPendingAfter
                                                    })
                                |}
                        | None -> nothing
                    ]
            ]

    [<ReactComponent>]
    let DurationInput (duration: Minute option) setDuration =
        let sessionDuration = Store.useValue Atoms.User.sessionDuration

        Ui.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Duration (minutes)"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Ui.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            (if duration.IsNone then Some "Enable" else None)
                            (fun x ->
                                x.isChecked <- duration.IsSome
                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            setDuration (if duration.IsSome then None else (Some sessionDuration)) })

                        match duration with
                        | Some duration ->
                            Input.Input
                                {|
                                    CustomProps =
                                        fun x ->
                                            x.fixedValue <- Some duration
                                            x.onFormat <- Some (Minute.Value >> string)

                                            x.onValidate <-
                                                Some (
                                                    fst
                                                    >> String.parseIntMin 1
                                                    >> Option.defaultValue 1
                                                    >> Minute
                                                    >> Some
                                                )

                                            x.inputFormat <- Some Input.InputFormat.Number
                                    Props =
                                        fun x ->
                                            x.onChange <-
                                                (fun (e: KeyboardEvent) ->
                                                    promise { e.Value |> int |> Minute |> Some |> setDuration })
                                |}
                        | None -> nothing
                    ]
            ]

    [<ReactComponent>]
    let PriorityInput priority setPriority =
        let priorityNumber =
            React.useMemo (
                (fun () ->
                    match priority with
                    | Some priority ->
                        let priorityNumber = (priority |> Priority.toTag) + 1
                        Some priorityNumber
                    | None -> None),
                [|
                    box priority
                |]
            )

        Ui.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Priority"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Ui.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            (if priorityNumber.IsNone then Some "Enable" else None)
                            (fun x ->
                                x.isChecked <- priorityNumber.IsSome

                                x.onChange <-
                                    fun _ ->
                                        promise { setPriority (if priorityNumber.IsSome then None else (Some Medium5)) })

                        match priorityNumber with
                        | Some priorityNumber ->
                            Ui.slider
                                (fun x ->
                                    x.min <- 1
                                    x.max <- 10
                                    x.value <- priorityNumber

                                    x.onChange <-
                                        fun x ->
                                            promise {
                                                setPriority (
                                                    match x with
                                                    | 1 -> Some Low1
                                                    | 2 -> Some Low2
                                                    | 3 -> Some Low3
                                                    | 4 -> Some Medium4
                                                    | 5 -> Some Medium5
                                                    | 6 -> Some Medium6
                                                    | 7 -> Some High7
                                                    | 8 -> Some High8
                                                    | 9 -> Some High9
                                                    | 10 -> Some Critical10
                                                    | _ -> None
                                                )
                                            })
                                [
                                    let bgColor =
                                        if priorityNumber <= 3 then "#68d638"
                                        elif priorityNumber <= 6 then "#f5ec13"
                                        elif priorityNumber <= 9 then "#e44c07"
                                        else "#a13c0e"

                                    Ui.sliderTrack
                                        (fun x -> x.backgroundColor <- $"{bgColor}55")
                                        [
                                            Ui.sliderFilledTrack (fun x -> x.backgroundColor <- bgColor) []
                                        ]

                                    Ui.sliderThumb (fun _ -> ()) []
                                ]

                            Ui.str (string priorityNumber)
                        | None -> nothing
                    ]
            ]

    let inline useDeleteTask () =
        Store.useCallbackRef
            (fun getter _ taskId ->
                promise {
                    do! Hydrate.deleteRecord getter Atoms.Task.collection (taskId |> TaskId.Value)
                    return true
                })

    [<ReactComponent>]
    let AddTaskButton information =
        let navigate = Store.useSetState Navigate.Actions.navigate
        let taskUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Task)
        let setInformationUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Information)

        let databaseId =
            React.useMemo (
                (fun () ->
                    match taskUIFlag with
                    | UIFlag.Task (databaseId, _) -> databaseId
                    | _ -> Database.Default.Id),
                [|
                    box taskUIFlag
                |]
            )

        Tooltip.wrap
            (str "Add Task")
            [
                TransparentIconButton.TransparentIconButton
                    {|
                        Props =
                            fun x ->
                                Ui.setTestId x "Add Task"
                                x.icon <- Icons.fi.FiPlus |> Icons.render
                                x.fontSize <- "17px"

                                x.onClick <-
                                    fun _ ->
                                        promise {
                                            navigate (
                                                Navigate.DockPosition.Right,
                                                Some DockType.Task,
                                                UIFlagType.Task,
                                                UIFlag.Task (databaseId, Task.Default.Id)
                                            )

                                            match information with
                                            | Some information -> setInformationUIFlag (UIFlag.Information information)
                                            | None -> ()
                                        }
                    |}
            ]

    [<ReactComponent>]
    let rec TaskForm (taskId: TaskId) (onSave: Task -> JS.Promise<unit>) =
        let toast = Ui.useToast ()
        let logLevel = Store.useValue Atoms.logLevel
        let startSession = useStartSession ()
        let deleteTask = useDeleteTask ()
        let sessions, setSessions = Store.useState (Atoms.Task.sessions taskId)
        let taskUIFlag, setTaskUIFlag = Store.useState (Atoms.User.uiFlag UIFlagType.Task)
        let attachmentIdSet = Store.useValue (Selectors.Task.attachmentIdSet taskId)
        let cellAttachmentIdMap = Store.useValue (Selectors.Task.cellAttachmentIdMap taskId)
        let statusMap = Store.useValue (Atoms.Task.statusMap taskId)

        let taskDatabaseId, attachmentIdList =
            React.useMemo (
                (fun () ->
                    let taskDatabaseId =
                        match taskUIFlag with
                        | UIFlag.Task (databaseId, taskId') when taskId' = taskId -> databaseId
                        | _ -> Database.Default.Id

                    taskDatabaseId, (attachmentIdSet |> Set.toList)),
                [|
                    box taskUIFlag
                    box taskId
                    box attachmentIdSet
                |]
            )

        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite taskDatabaseId)

        let onAttachmentAdd =
            Store.useCallbackRef
                (fun _ setter attachmentId ->
                    promise {
                        Atom.set setter (Atoms.Attachment.parent attachmentId) (Some (AttachmentParent.Task taskId)) })


        let onAttachmentDelete =
            Store.useCallbackRef
                (fun getter _setter attachmentId ->
                    promise {
                        do! Hydrate.deleteRecord getter Atoms.Attachment.collection (attachmentId |> AttachmentId.Value)
                        return true
                    })

        let tempInformation = Store.useAtomTempState (Atoms.Task.information taskId)
        let tempPriority = Store.useAtomTempState (Atoms.Task.priority taskId)
        let tempDuration = Store.useAtomTempState (Atoms.Task.duration taskId)
        let tempPendingAfter = Store.useAtomTempState (Atoms.Task.pendingAfter taskId)
        let tempMissedAfter = Store.useAtomTempState (Atoms.Task.missedAfter taskId)
        let tempScheduling = Store.useAtomTempState (Atoms.Task.scheduling taskId)

        let onSave =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        let taskName = TempValue.get getter (Atoms.Task.name taskId)

                        if taskDatabaseId = Database.Default.Id then
                            toast (fun x -> x.description <- "Invalid database")
                        elif (match taskName |> TaskName.Value with
                              | String.Invalid -> true
                              | _ -> false) then
                            toast (fun x -> x.description <- "Invalid name")
                        elif (match tempInformation.Value
                                    |> Information.Name
                                    |> InformationName.Value with
                              | String.Invalid -> true
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
                                        Information = tempInformation.Value
                                        Scheduling = tempScheduling.Value
                                        Priority = tempPriority.Value
                                        Duration = tempDuration.Value
                                        MissedAfter = tempMissedAfter.Value
                                        PendingAfter = tempPendingAfter.Value
                                    }
                                    |> Promise.lift
                                else
                                    promise {
                                        let task = Atom.get getter (Selectors.Task.task taskId)

                                        return
                                            { task with
                                                Name = taskName
                                                Information = tempInformation.Value
                                                Scheduling = tempScheduling.Value
                                                Priority = tempPriority.Value
                                                Duration = tempDuration.Value
                                                MissedAfter = tempMissedAfter.Value
                                                PendingAfter = tempPendingAfter.Value
                                            }
                                    }

                            TempValue.reset setter (Atoms.Task.name taskId)
                            TempValue.reset setter (Atoms.Task.information taskId)
                            TempValue.reset setter (Atoms.Task.scheduling taskId)
                            TempValue.reset setter (Atoms.Task.priority taskId)
                            TempValue.reset setter (Atoms.Task.duration taskId)
                            TempValue.reset setter (Atoms.Task.missedAfter taskId)
                            TempValue.reset setter (Atoms.Task.pendingAfter taskId)

                            do! onSave task
                    })

        let deleteSession =
            Store.useCallbackRef
                (fun _ _ start ->
                    promise {
                        let index =
                            sessions
                            |> List.findIndex (fun (Session start') -> start' = start)

                        setSessions (sessions |> List.removeAt index)

                        return true
                    })

        Accordion.AccordionAtom
            {|
                Props = fun x -> x.flex <- "1"
                Atom = Atoms.User.accordionHiddenFlag AccordionType.TaskForm
                Items =
                    [
                        if taskId <> Task.Default.Id then
                            str "Info",
                            (Ui.stack
                                (fun x -> x.spacing <- "15px")
                                [
                                    Ui.str $"Cell Status Count: {statusMap |> Map.count}"
                                    Ui.str
                                        $"Cell Attachment Count: {cellAttachmentIdMap
                                                                  |> Map.values
                                                                  |> Seq.map Set.count
                                                                  |> Seq.sum}"
                                ])

                        (Ui.box
                            (fun _ -> ())
                            [
                                str $"""{if taskId = Task.Default.Id then "Add" else "Edit"} Task"""

                                if taskId <> Task.Default.Id then
                                    Menu.Menu
                                        {|
                                            Tooltip = ""
                                            Trigger =
                                                Menu.FakeMenuButton
                                                    InputLabelIconButton.InputLabelIconButton
                                                    (fun x ->
                                                        x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                                        x.fontSize <- "11px"
                                                        x.height <- "15px"
                                                        x.color <- "whiteAlpha.700"
                                                        x.display <- if isReadWrite then null else "none"
                                                        x.marginTop <- "-3px"
                                                        x.marginLeft <- "6px")
                                            Body =
                                                [
                                                    Popover.MenuItemConfirmPopover
                                                        Icons.bi.BiTrash
                                                        "Delete Task"
                                                        (fun () -> deleteTask taskId)
                                                ]
                                            MenuListProps = fun _ -> ()
                                        |}

                            ]),
                        (Ui.stack
                            (fun x -> x.spacing <- "15px")
                            [
                                if logLevel <= Logger.LogLevel.Debug then
                                    Ui.str $"{taskId}"
                                else
                                    nothing

                                DatabaseSelector.DatabaseSelector
                                    taskDatabaseId
                                    (fun databaseId -> setTaskUIFlag (UIFlag.Task (databaseId, taskId)))

                                InformationSelector.InformationSelector
                                    {|
                                        DisableResource = true
                                        SelectionType = InformationSelector.InformationSelectionType.Information
                                        Information = Some tempInformation.Value
                                        OnSelect = tempInformation.SetValue
                                    |}

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <- Some (InputAtom (AtomReference.Atom (Atoms.Task.name taskId)))

                                                x.inputScope <- Some (InputScope.Temp defaultSerializer)

                                                x.onEnterPress <- Some onSave
                                                x.onFormat <- Some (fun (TaskName name) -> name)
                                                x.onValidate <- Some (fst >> TaskName >> Some)
                                        Props =
                                            fun x ->
                                                x.autoFocus <- true
                                                x.label <- str "Name"

                                                x.placeholder <-
                                                    $"""new-task-{DateTime.Now |> DateTime.format "yyyy-MM-dd"}"""
                                    |}


                                Ui.box
                                    (fun x -> x.display <- "inline")
                                    [
                                        InputLabel.InputLabel
                                            {|
                                                Hint = None
                                                HintTitle = None
                                                Label = str "Scheduling"
                                                Props = fun x -> x.marginBottom <- "5px"
                                            |}
                                        SchedulingSelector.SchedulingDropdown
                                            tempScheduling.Value
                                            tempScheduling.SetValue
                                    ]

                                PriorityInput tempPriority.Value tempPriority.SetValue

                                DurationInput tempDuration.Value tempDuration.SetValue

                                PendingAfterInput tempPendingAfter.Value tempPendingAfter.SetValue

                                MissedAfterInput tempMissedAfter.Value tempMissedAfter.SetValue

                                Button.Button
                                    {|
                                        Tooltip = None
                                        Icon = Some (Icons.fi.FiSave |> Icons.render, Button.IconPosition.Left)
                                        Props = fun x -> x.onClick <- onSave
                                        Children =
                                            [
                                                str "Save"
                                            ]
                                    |}
                            ])

                        if taskId <> Task.Default.Id then
                            (Ui.box
                                (fun _ -> ())
                                [
                                    str "Sessions"

                                    Menu.Menu
                                        {|
                                            Tooltip = ""
                                            Trigger =
                                                Menu.FakeMenuButton
                                                    InputLabelIconButton.InputLabelIconButton
                                                    (fun x ->
                                                        x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                                        x.fontSize <- "11px"
                                                        x.height <- "15px"
                                                        x.color <- "whiteAlpha.700"
                                                        x.display <- if isReadWrite then null else "none"
                                                        x.marginTop <- "-3px"
                                                        x.marginLeft <- "6px")
                                            Body =
                                                [
                                                    MenuItem.MenuItem
                                                        Icons.gi.GiHourglass
                                                        "Start Session"
                                                        (Some (fun () -> startSession taskId))
                                                        (fun _ -> ())
                                                ]
                                            MenuListProps = fun _ -> ()
                                        |}

                                ]),
                            (match sessions with
                             | [] -> Ui.str "No sessions found"
                             | sessions ->
                                 Ui.stack
                                     (fun _ -> ())
                                     [
                                         yield!
                                             sessions
                                             |> List.map
                                                 (fun (Session start) ->
                                                     Ui.flex
                                                         (fun x ->
                                                             x.key <- $"session-{start |> FlukeDateTime.Stringify}")
                                                         [
                                                             Ui.box
                                                                 (fun _ -> ())
                                                                 [
                                                                     str (start |> FlukeDateTime.Stringify)

                                                                     Menu.Menu
                                                                         {|
                                                                             Tooltip = ""
                                                                             Trigger =
                                                                                 InputLabelIconButton.InputLabelIconButton
                                                                                     (fun x ->
                                                                                         x.``as`` <- Ui.react.MenuButton

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
                                                                                     Popover.MenuItemConfirmPopover
                                                                                         Icons.bi.BiTrash
                                                                                         "Delete Session"
                                                                                         (fun () -> deleteSession start)
                                                                                 ]
                                                                             MenuListProps = fun _ -> ()
                                                                         |}
                                                                 ]
                                                         ])
                                     ])

                            str "Attachments",
                            (Ui.stack
                                (fun x ->
                                    x.spacing <- "10px"
                                    x.flex <- "1")
                                [
                                    AttachmentPanel.AttachmentPanel
                                        (AttachmentParent.Task taskId)
                                        (Some onAttachmentAdd)
                                        onAttachmentDelete
                                        attachmentIdList
                                ])
                    ]
            |}

    [<ReactComponent>]
    let TaskFormWrapper () =
        let hydrateTaskState = Store.useCallbackRef Hydrate.hydrateTaskState
        let hydrateTask = Store.useCallbackRef Hydrate.hydrateTask
        let archive = Store.useValue Atoms.User.archive
        let selectedTaskIdListByArchive = Store.useValue Selectors.Session.selectedTaskIdListByArchive
        let taskUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Task)

        let taskDatabaseId =
            match taskUIFlag with
            | UIFlag.Task (databaseId, _) -> databaseId
            | _ -> Database.Default.Id

        let taskId =
            React.useMemo (
                (fun () ->
                    match taskUIFlag with
                    | UIFlag.Task (_, taskId) when
                        selectedTaskIdListByArchive
                        |> List.contains taskId
                        ->
                        taskId
                    | _ -> Task.Default.Id),
                [|
                    box taskUIFlag
                    box selectedTaskIdListByArchive
                |]
            )

        TaskForm
            taskId
            (fun task ->
                promise {
                    if task.Id <> taskId then
                        let taskState =
                            { TaskState.Default with
                                Task = task
                                Archived = archive |> Option.defaultValue false
                            }

                        do! hydrateTaskState (AtomScope.Current, taskDatabaseId, taskState)
                    else
                        do! hydrateTask (AtomScope.Current, taskDatabaseId, task)
                })
