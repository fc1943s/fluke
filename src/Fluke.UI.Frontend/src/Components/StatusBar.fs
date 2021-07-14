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

        let devicePingList = Store.useValue Selectors.Session.devicePingList

        UI.box
            (fun _ -> ())
            [
                str $"Now: {now.ToString ()}"
                br []
                yield!
                    devicePingList
                    |> List.map
                        (fun (deviceId, ping) ->
                            let diff = ping |> Ping.Value |> DateTime.ticksDiff |> int

                            let diffSeconds = diff / 1000
                            let diffMinutes = diffSeconds / 60

                            //                            printfn
//                                $"
//                            i={i}
//                            deviceId={deviceId}
//                            diff={diff}
//                            diffSeconds={diffSeconds}
//                            diffMinutes={diffMinutes} "
//
                            if diffMinutes >= 0 && diffMinutes <= 60 then
                                React.fragment [
                                    br []

                                    str
                                        $"""Device {deviceId
                                                    |> DeviceId.Value
                                                    |> string
                                                    |> Seq.takeWhile ((<>) '-')
                                                    |> Seq.map string
                                                    |> String.concat ""}: {if diffMinutes >= 2 then
                                                                               $"{diffMinutes} minutes"
                                                                           else
                                                                               $"{diffSeconds} seconds"}"""
                                ]
                            else
                                nothing)
            ]

    [<ReactComponent>]
    let UserIndicator () =
        let username = Store.useValue Store.Atoms.username

        UI.flex
            (fun _ -> ())
            [
                UI.icon
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

        UI.flex
            (fun _ -> ())
            [
                UI.icon
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
                                        UI.box
                                            (fun x -> x.cursor <- "pointer")
                                            [
                                                let sessionInfo = getSessionInfo activeSessions.Head

                                                UI.stack
                                                    (fun x ->
                                                        x.color <- sessionInfo.Color
                                                        x.direction <- "row"
                                                        x.spacing <- "0"
                                                        x.display <- "inline")
                                                    [
                                                        UI.box
                                                            (fun x -> x.display <- "inline")
                                                            [
                                                                str
                                                                    $"{sessionInfo.SessionType}: {activeSessions.Length} active ("
                                                            ]

                                                        UI.box
                                                            (fun x ->
                                                                x.display <- "inline-block"
                                                                x.textOverflow <- "ellipsis"
                                                                x.whiteSpace <- "nowrap"
                                                                x.overflow <- "hidden"
                                                                x.verticalAlign <- "bottom"
                                                                x.maxWidth <- "100px")
                                                            [
                                                                str sessionInfo.TaskName
                                                            ]

                                                        UI.box
                                                            (fun x -> x.display <- "inline")
                                                            [
                                                                str
                                                                    $"""). {if sessionInfo.Left < 0 then
                                                                                $"Starts in {sessionInfo.Duration}m."
                                                                            else
                                                                                $"Started {sessionInfo.Duration}m ago ({sessionInfo.Left}m left)"}"""
                                                            ]
                                                    ]
                                            ]
                                    ]
                            Body =
                                fun (_disclosure, _fetchInitialFocusRef) ->
                                    [
                                        UI.stack
                                            (fun x -> x.spacing <- "10px")
                                            [
                                                UI.box
                                                    (fun x -> x.fontSize <- "1.3rem")
                                                    [
                                                        str "Session Details"
                                                    ]

                                                yield!
                                                    activeSessions
                                                    |> List.map
                                                        (fun session ->
                                                            let sessionInfo = getSessionInfo session

                                                            UI.flex
                                                                (fun x -> x.color <- sessionInfo.Color)
                                                                [
                                                                    str
                                                                        $"""{sessionInfo.SessionType}: {if sessionInfo.Left < 0 then
                                                                                                            $"Starts in {sessionInfo.Duration}m"
                                                                                                        else
                                                                                                            $" {sessionInfo.Duration}m ago ({sessionInfo.Left}m left)"}. Task: {sessionInfo.TaskName}"""
                                                                ])
                                            ]
                                    ]
                        |}

            //                        | _ -> str "Sessions: Loading sessions"

            ]


    [<ReactComponent>]
    let TasksIndicator () =
        let selectedDatabaseIdSet = Store.useValue Atoms.User.selectedDatabaseIdSet

        let informationAttachmentIdSet =
            selectedDatabaseIdSet
            |> Set.toArray
            |> Array.map Atoms.Database.informationAttachmentIdMap
            |> Store.waitForAll
            |> Store.useValue
            |> Array.collect (Map.values >> Seq.toArray)
            |> Array.fold Set.union Set.empty

        let informationSet = Store.useValue Selectors.Session.informationSet

        let selectedTaskIdAtoms = Store.useValue Selectors.Session.selectedTaskIdAtoms

        let selectedTaskIdArray =
            selectedTaskIdAtoms
            |> Store.waitForAll
            |> Store.useValue

        let taskAttachmentIdArray =
            selectedTaskIdArray
            |> Array.map Atoms.Task.attachmentIdSet
            |> Store.waitForAll
            |> Store.useValue
            |> Array.collect Set.toArray

        let selectedTaskIdListByArchive = Store.useValue Selectors.Session.selectedTaskIdListByArchive

        let sortedTaskIdList = Store.useValue Selectors.Session.sortedTaskIdList

        let cellStateMapArray =
            selectedTaskIdArray
            |> Array.map Selectors.Task.cellStateMap
            |> Store.waitForAll
            |> Store.useValue

        let detailsText, total =
            React.useMemo (
                (fun () ->
                    let information = informationSet.Count
                    let informationAttachment = informationAttachmentIdSet.Count
                    let taskAttachment = taskAttachmentIdArray.Length

                    let cellStatus =
                        cellStateMapArray
                        |> Array.map (
                            Map.values
                            >> Seq.filter
                                (function
                                | { Status = UserStatus _ } -> true
                                | _ -> false)
                            >> Seq.length
                        )
                        |> Array.sum

                    let session =
                        cellStateMapArray
                        |> Array.map (
                            Map.values
                            >> Seq.map (fun cellState -> cellState.SessionList.Length)
                            >> Seq.sum
                        )
                        |> Array.sum

                    let cellAttachment =
                        cellStateMapArray
                        |> Array.map (
                            Map.values
                            >> Seq.map
                                (fun cellState ->
                                    cellState.AttachmentStateList
                                    |> List.map (fun attachmentState -> attachmentState.Attachment)
                                    |> List.length)
                            >> Seq.sum
                        )
                        |> Array.sum

                    let total =
                        information
                        + informationAttachment
                        + selectedTaskIdAtoms.Length
                        + taskAttachment
                        + cellStatus
                        + session
                        + cellAttachment

                    let detailsText =
                        [
                            $"Information: {information}"
                            $"Information Attachment: {informationAttachment}"
                            $"Task: {selectedTaskIdAtoms.Length}"
                            $"Task Attachment: {taskAttachment}"
                            $"Task Session: {session}"
                            $"Cell Status: {cellStatus}"
                            $"Cell Attachment: {cellAttachment}"
                            $"Total: {total}"
                        ]
                        |> List.map str
                        |> List.intersperse (br [])
                        |> React.fragment

                    detailsText, total),
                [|
                    box selectedTaskIdAtoms
                    box taskAttachmentIdArray
                    box cellStateMapArray
                    box informationAttachmentIdSet
                    box informationSet
                |]
            )

        UI.flex
            (fun _ -> ())
            [
                UI.icon
                    (fun x ->
                        x.``as`` <- Icons.bi.BiTask
                        x.marginRight <- "4px")
                    []

                //                        match sortedTaskIdList with
//                        | sortedTaskIdList ->

                Tooltip.wrap
                    detailsText
                    [
                        str
                            $"Tasks: {sortedTaskIdList.Length} of {selectedTaskIdListByArchive.Length} visible (Total: {total})"
                    ]
            //                        | _ -> str "Tasks: Loading tasks"
            ]

    [<ReactComponent>]
    let StatusBar () =
        let position = Store.useValue Atoms.position

        Scheduling.useScheduling
            Scheduling.Interval
            2000
            (fun _ setter ->
                promise { Store.set setter (Atoms.Device.devicePing deviceId) (Ping (string DateTime.Now.Ticks)) })

        UI.simpleGrid
            (fun x ->
                x.display <-
                    unbox (
                        JS.newObj
                            (fun (x: UI.IBreakpoints<string>) ->
                                x.``base`` <- "grid"
                                x.md <- "flex")
                    )

                x.borderTopWidth <- "1px"
                x.borderTopColor <- "gray.16"
                x.minChildWidth <- "150px"
                x.justifyContent <- "space-between"
                x.justifyItems <- "center"
                x.padding <- "7px"
                x.alignItems <- "center"
                x.textAlign <- "center"
                x.spacing <- "12px"
                x.flexFlow <- "wrap")
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
                        UI.flex
                            (fun _ -> ())
                            [
                                UI.icon
                                    (fun x ->
                                        x.``as`` <- Icons.fa.FaRegClock
                                        x.marginRight <- "4px")
                                    []

                                Tooltip.wrap
                                    (NowIndicator ())
                                    [
                                        str $"Position: {(position |> FlukeDateTime.Stringify).[0..-4]}"
                                    ]
                            ]
                    ]
                | None -> str "Position: No databases selected"
            ]
