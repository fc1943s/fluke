namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.DateFunctions
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open System
open Fable.Core
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared


module TaskForm =
    [<ReactComponent>]
    let DurationSelector taskId =
        let tempDuration =
            Store.Hooks.useTempAtom
                (Some (Store.InputAtom (Store.AtomReference.Atom (Atoms.Task.duration taskId))))
                (Some (Store.InputScope.Temp Gun.defaultSerializer))

        Chakra.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Duration (minutes)"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Chakra.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "15px")
                    [
                        Checkbox.Checkbox
                            (if tempDuration.Value.IsNone then Some "Enable" else None)
                            (fun x ->
                                x.isChecked <- tempDuration.Value.IsSome
                                x.alignSelf <- "center"

                                x.onChange <-
                                    fun _ ->
                                        promise {
                                            tempDuration.SetValue (
                                                if tempDuration.Value.IsSome then None else (Some (Minute 1))
                                            )
                                        })

                        match tempDuration.Value with
                        | Some duration ->
                            Input.Input
                                {|
                                    CustomProps =
                                        fun x ->
                                            x.fixedValue <- Some duration
                                            x.onFormat <- Some (Minute.Value >> string)

                                            //                                            x.atom <-
//                                                Some (
//                                                    Store.InputAtom (
//                                                        Store.AtomReference.Atom (Atoms.Task.duration taskId)
//                                                    )
//                                                )
//
//                                            x.inputScope <- Some (Store.InputScope.ReadWrite Gun.defaultSerializer)

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
                                                    promise {
                                                        e.Value
                                                        |> int
                                                        |> Minute
                                                        |> Some
                                                        |> tempDuration.SetValue
                                                    })
                                |}
                        | None -> nothing
                    ]
            ]

    [<ReactComponent>]
    let PrioritySelector taskId =
        let tempPriority =
            Store.Hooks.useTempAtom
                (Some (Store.InputAtom (Store.AtomReference.Atom (Atoms.Task.priority taskId))))
                (Some (Store.InputScope.Temp Gun.defaultSerializer))

        let priorityNumber =
            React.useMemo (
                (fun () ->
                    match tempPriority.Value with
                    | Some priority ->
                        let priorityNumber = (priority |> Priority.toTag) + 1
                        Some priorityNumber
                    | None -> None),
                [|
                    box tempPriority.Value
                |]
            )

        Chakra.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Priority"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Chakra.stack
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
                                        promise {
                                            tempPriority.SetValue (
                                                if priorityNumber.IsSome then None else (Some Medium5)
                                            )
                                        })

                        match priorityNumber with
                        | Some priorityNumber ->
                            Chakra.slider
                                (fun x ->
                                    x.min <- 1
                                    x.max <- 10
                                    x.value <- priorityNumber

                                    x.onChange <-
                                        fun x ->
                                            promise {
                                                tempPriority.SetValue (
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

                                    Chakra.sliderTrack
                                        (fun x -> x.backgroundColor <- $"{bgColor}55")
                                        [
                                            Chakra.sliderFilledTrack (fun x -> x.backgroundColor <- bgColor) []
                                        ]

                                    Chakra.sliderThumb (fun _ -> ()) []
                                ]

                            Chakra.box
                                (fun _ -> ())
                                [
                                    str (string priorityNumber)
                                ]
                        | None -> nothing
                    ]
            ]

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

        let taskUIFlag, setTaskUIFlag = Store.useState (Atoms.User.uiFlag UIFlagType.Task)

        let taskDatabaseId =
            React.useMemo (
                (fun () ->
                    match taskUIFlag with
                    | UIFlag.Task (databaseId, taskId') when taskId' = taskId -> databaseId
                    | _ -> Database.Default.Id),
                [|
                    box taskUIFlag
                    box taskId
                |]
            )

        let attachmentIdSet = Store.useValue (Atoms.Task.attachmentIdSet taskId)

        let cellAttachmentMap = Store.useValue (Atoms.Task.cellAttachmentMap taskId)
        let statusMap = Store.useValue (Atoms.Task.statusMap taskId)

        let onAttachmentAdd =
            Store.useCallback (
                (fun _ setter attachmentId ->
                    promise { Store.change setter (Atoms.Task.attachmentIdSet taskId) (Set.add attachmentId) }),
                [|
                    box taskId
                |]
            )


        let onAttachmentDelete =
            Store.useCallback (
                (fun getter setter attachmentId ->
                    promise {
                        Store.change setter (Atoms.Task.attachmentIdSet taskId) (Set.remove attachmentId)

                        do! Store.deleteRoot getter (Atoms.Attachment.attachment attachmentId)
                    }),
                [|
                    box taskId
                |]
            )

        let tempInformation =
            Store.Hooks.useTempAtom
                (Some (Store.InputAtom (Store.AtomReference.Atom (Atoms.Task.information taskId))))
                (Some (Store.InputScope.Temp Gun.defaultSerializer))

        let onSave =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let taskName = Store.getTemp getter (Atoms.Task.name taskId)
                        let taskInformation = Store.getTemp getter (Atoms.Task.information taskId)
                        let taskScheduling = Store.getTemp getter (Atoms.Task.scheduling taskId)
                        let taskPriority = Store.getTemp getter (Atoms.Task.priority taskId)
                        let taskDuration = Store.getTemp getter (Atoms.Task.duration taskId)

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
                                        Priority = taskPriority
                                        Duration = taskDuration
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
                                                Priority = taskPriority
                                                Duration = taskDuration
                                            }
                                    }

                            Store.resetTemp setter (Atoms.Task.name taskId)
                            Store.resetTemp setter (Atoms.Task.information taskId)
                            Store.resetTemp setter (Atoms.Task.scheduling taskId)
                            Store.resetTemp setter (Atoms.Task.priority taskId)
                            Store.resetTemp setter (Atoms.Task.duration taskId)
                            Store.set setter (Atoms.User.uiFlag UIFlagType.Task) UIFlag.None

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
                Atom = Atoms.User.accordionFlag (TextKey (nameof TaskForm))
                Items =
                    [
                        if taskId <> Task.Default.Id then
                            "Info",
                            (Chakra.stack
                                (fun x -> x.spacing <- "15px")
                                [
                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            str $"Cell Status Count: {statusMap |> Map.count}"
                                        ]
                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            str
                                                $"Cell Attachment Count: {
                                                                              cellAttachmentMap
                                                                              |> Map.values
                                                                              |> Seq.map Set.count
                                                                              |> Seq.sum
                                                }"
                                        ]
                                ])


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
                                    (fun databaseId -> setTaskUIFlag (UIFlag.Task (databaseId, taskId)))

                                InformationSelector.InformationSelector
                                    {|
                                        DisableResource = true
                                        SelectionType = InformationSelector.InformationSelectionType.Information
                                        Information = Some tempInformation.Value
                                        OnSelect = tempInformation.SetValue
                                    |}

                                SchedulingSelector.SchedulingSelector taskId

                                PrioritySelector taskId

                                DurationSelector taskId

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

                                                x.inputScope <- Some (Store.InputScope.Temp Gun.defaultSerializer)

                                                x.onFormat <- Some (fun (TaskName name) -> name)
                                                x.onEnterPress <- Some onSave
                                                x.onValidate <- Some (fst >> TaskName >> Some)
                                        Props =
                                            fun x ->
                                                x.autoFocus <- true
                                                x.label <- str "Name"

                                                x.placeholder <- $"""new-task-{DateTime.Now.Format "yyyy-MM-dd"}"""
                                    |}

                                Button.Button
                                    {|
                                        Hint = None
                                        Icon = Some (Icons.fi.FiSave |> Icons.render, Button.IconPosition.Left)
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
                                                                                 ConfirmPopover.ConfirmPopover
                                                                                     ConfirmPopover.ConfirmPopoverType.MenuItem
                                                                                     Icons.bi.BiTrash
                                                                                     "Delete Session"
                                                                                     (fun () -> deleteSession start)
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
                                    AttachmentPanel.AttachmentPanel
                                        (Some onAttachmentAdd)
                                        onAttachmentDelete
                                        (attachmentIdSet |> Set.toList)
                                ])
                    ]
            |}

    [<ReactComponent>]
    let TaskFormWrapper () =
        let hydrateTaskState = Hydrate.useHydrateTaskState ()
        let hydrateTask = Hydrate.useHydrateTask ()
        let selectedTaskIdList = Store.useValue Selectors.Session.selectedTaskIdList
        let setRightDock = Store.useSetState Atoms.User.rightDock
        let taskUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Task)

        let taskDatabaseId =
            match taskUIFlag with
            | UIFlag.Task (databaseId, _) -> databaseId
            | _ -> Database.Default.Id

        let taskId =
            React.useMemo (
                (fun () ->
                    match taskUIFlag with
                    | UIFlag.Task (_, taskId) when selectedTaskIdList |> List.contains taskId -> taskId
                    | _ -> Task.Default.Id),
                [|
                    box taskUIFlag
                    box selectedTaskIdList
                |]
            )

        TaskForm
            taskId
            (fun task ->
                promise {
                    if task.Id <> taskId then
                        let taskState =
                            {
                                Task = task
                                SortList = []
                                Sessions = []
                                Attachments = []
                                CellStateMap = Map.empty
                            }

                        do! hydrateTaskState (Store.AtomScope.Current, taskDatabaseId, taskState)
                    else
                        do! hydrateTask (Store.AtomScope.Current, taskDatabaseId, task)

                    setRightDock None
                })
