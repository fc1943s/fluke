namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain


module StatusBar =
    open Model
    open UserInteraction

    [<ReactComponent>]
    let NowIndicator () =
        let now, setNow = React.useState DateTime.Now
        Scheduling.useScheduling Scheduling.Interval 1000 (fun _ _ -> promise { setNow DateTime.Now })
        str $"Now: {now.ToString ()}"

    [<ReactComponent>]
    let UserIndicator () =
        let username = Store.useValue Store.Atoms.username

        Chakra.flex
            (fun _ -> ())
            [
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fa.FaRegUser
                        x.marginRight <- "4px")
                    []

                match username with
                | Some (Username username) -> str $"User: {username}"
                | _ -> nothing
            ]


    [<ReactComponent>]
    let SessionIndicator () =
        let activeSessions = Store.useValue Selectors.Session.activeSessions

        let (Minute sessionDuration) = Store.useValue Atoms.User.sessionDuration
        let (Minute sessionBreakDuration) = Store.useValue Atoms.User.sessionBreakDuration

        Chakra.flex
            (fun _ -> ())
            [
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.gi.GiHourglass
                        x.marginRight <- "4px")
                    []

                match activeSessions with
                | [] -> str "Sessions: No active sessions"
                | activeSessions ->
                    let getSessionInfo (TempUI.ActiveSession (taskName, Minute duration)) =
                        let left = sessionDuration - duration

                        match duration < sessionDuration with
                        | true when duration < 0 ->
                            {|
                                TaskName = taskName
                                SessionType = "Session"
                                Color = "#d8b324"
                                Duration = -duration
                                Left = duration
                            |}
                        | true ->
                            {|
                                TaskName = taskName
                                SessionType = "Session"
                                Color = "#7cca7c"
                                Duration = duration
                                Left = left
                            |}
                        | false ->
                            {|
                                TaskName = taskName
                                SessionType = "Break"
                                Color = "#ca7c7c"
                                Duration = -left
                                Left = sessionBreakDuration + left
                            |}


                    Popover.Popover
                        {|
                            Trigger =
                                Tooltip.wrap
                                    (str "Session Details")
                                    [
                                        Chakra.box
                                            (fun x -> x.cursor <- "pointer")
                                            [
                                                let sessionInfo = getSessionInfo activeSessions.Head

                                                Chakra.flex
                                                    (fun x -> x.color <- sessionInfo.Color)
                                                    [
                                                        str
                                                            $"{sessionInfo.SessionType}: {activeSessions.Length} active ("

                                                        Chakra.box
                                                            (fun x ->
                                                                x.display <- "inline"
                                                                x.textOverflow <- "ellipsis"
                                                                x.whiteSpace <- "nowrap"
                                                                x.overflow <- "hidden"
                                                                x.maxWidth <- "100px")
                                                            [
                                                                str sessionInfo.TaskName
                                                            ]
                                                        str
                                                            $"""). {
                                                                        if sessionInfo.Left < 0 then
                                                                            $"Starts in {sessionInfo.Duration}m."
                                                                        else
                                                                            $"Started {sessionInfo.Duration}m ago ({
                                                                                                                        sessionInfo.Left
                                                                            }m left)"
                                                            }"""
                                                    ]
                                            ]
                                    ]
                            Body =
                                fun (_disclosure, _initialFocusRef) ->
                                    [
                                        Chakra.stack
                                            (fun x -> x.spacing <- "10px")
                                            [
                                                Chakra.box
                                                    (fun x -> x.fontSize <- "15px")
                                                    [
                                                        str "Session Details"
                                                    ]

                                                yield!
                                                    activeSessions
                                                    |> List.map
                                                        (fun session ->
                                                            let sessionInfo = getSessionInfo session

                                                            Chakra.flex
                                                                (fun x -> x.color <- sessionInfo.Color)
                                                                [
                                                                    str
                                                                        $"""{sessionInfo.SessionType}: {
                                                                                                            if sessionInfo.Left < 0 then
                                                                                                                $"Starts in {
                                                                                                                                 sessionInfo.Duration
                                                                                                                }m"
                                                                                                            else
                                                                                                                $" {
                                                                                                                        sessionInfo.Duration
                                                                                                                }m ago ({
                                                                                                                             sessionInfo.Left
                                                                                                                }m left)"
                                                                        }. Task: {sessionInfo.TaskName}"""
                                                                ])
                                            ]
                                    ]
                            Props = fun _ -> ()
                        |}

            //                        | _ -> str "Sessions: Loading sessions"

            ]


    [<ReactComponent>]
    let TasksIndicator () =
        let informationSet = Store.useValue Selectors.Session.informationSet
        let informationStateList = Store.useValue Selectors.Session.informationStateList
        let taskStateList = Store.useValue Selectors.Session.taskStateList

        let cellAttachmentMapArray =
            taskStateList
            |> List.map (fun taskState -> taskState.Task.Id)
            |> List.map Atoms.Task.cellAttachmentMap
            |> List.toArray
            |> Store.waitForAll
            |> Store.useValue

        let selectedTaskIdAtoms = Store.useValue Selectors.Session.selectedTaskIdAtoms
        let sortedTaskIdList = Store.useValue Selectors.Session.sortedTaskIdList
        let databaseIdAtoms = Store.useValue Selectors.asyncDatabaseIdAtoms

        let detailsText, total =
            React.useMemo (
                (fun () ->
                    let database = databaseIdAtoms.Length
                    let information = informationSet.Count

                    let informationAttachment =
                        informationStateList
                        |> List.map (fun informationState -> informationState.Attachments.Length)
                        |> List.sum

                    let tasks = taskStateList.Length

                    let taskAttachment =
                        taskStateList
                        |> List.map (fun taskState -> taskState.Attachments.Length)
                        |> List.sum

                    let taskSession =
                        taskStateList
                        |> List.map (fun taskState -> taskState.Sessions.Length)
                        |> List.sum

                    let cellStatus =
                        taskStateList
                        |> List.map
                            (fun taskState ->
                                taskState.CellStateMap
                                |> Map.values
                                |> Seq.filter
                                    (function
                                    | { Status = UserStatus _ } -> true
                                    | _ -> false)
                                |> Seq.length)
                        |> List.sum

                    let cellAttachment =
                        cellAttachmentMapArray
                        |> Array.map (Map.values >> Seq.map Set.count >> Seq.sum)
                        |> Array.sum

                    let total =
                        database
                        + information
                        + informationAttachment
                        + tasks
                        + taskAttachment
                        + taskSession
                        + cellStatus
                        + cellAttachment

                    let detailsText =
                        [
                            $"Database: {database}"
                            $"Information: {information}"
                            $"Information Attachment: {informationAttachment}"
                            $"Task: {tasks}"
                            $"Task Attachment: {taskAttachment}"
                            $"Task Session: {taskSession}"
                            $"Cell Status: {cellStatus}"
                            $"Cell Attachment: {cellAttachment}"
                            $"Total: {total}"
                        ]
                        |> List.map str
                        |> List.intersperse (br [])
                        |> React.fragment

                    detailsText, total),
                [|
                    box cellAttachmentMapArray
                    box informationSet
                    box informationStateList
                    box taskStateList
                    box databaseIdAtoms
                |]
            )

        Chakra.flex
            (fun _ -> ())
            [
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.bi.BiTask
                        x.marginRight <- "4px")
                    []

                //                        match sortedTaskIdList with
//                        | sortedTaskIdList ->

                Tooltip.wrap
                    detailsText
                    [
                        str $"Tasks: {sortedTaskIdList.Length} of {selectedTaskIdAtoms.Length} visible (Total: {total})"
                    ]
            //                        | _ -> str "Tasks: Loading tasks"
            ]

    [<ReactComponent>]
    let StatusBar () =
        let position = Store.useValue Atoms.position

        Chakra.simpleGrid
            (fun x ->
                x.display <-
                    unbox (
                        JS.newObj
                            (fun (x: Chakra.IBreakpoints<string>) ->
                                x.``base`` <- "grid"
                                x.md <- "flex")
                    )

                x.borderTopWidth <- "1px"
                x.borderTopColor <- "gray.16"
                x.minChildWidth <- "150px"
                x.justifyContent <- "space-between"
                x.justifyItems <- "center"
                x.padding <- "7px"
                x.spacing <- "6px")
            [
                React.suspense (
                    [
                        UserIndicator ()
                    ],
                    LoadingSpinner.InlineLoadingSpinner ()
                )

                React.suspense (
                    [
                        SessionIndicator ()
                    ],
                    LoadingSpinner.InlineLoadingSpinner ()
                )

                React.suspense (
                    [
                        TasksIndicator ()
                    ],
                    LoadingSpinner.InlineLoadingSpinner ()
                )

                match position with
                | Some position ->
                    React.fragment [
                        Chakra.flex
                            (fun _ -> ())
                            [
                                Chakra.icon
                                    (fun x ->
                                        x.``as`` <- Icons.fa.FaRegClock
                                        x.marginRight <- "4px")
                                    []

                                Tooltip.wrap
                                    (NowIndicator ())
                                    [
                                        str $"Position: {position |> FlukeDateTime.Stringify}"
                                    ]
                            ]
                    ]
                | None -> str "Position: No databases selected"
            ]
