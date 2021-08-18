namespace Fluke.UI.Frontend.Components

open FsCore.BaseModel
open FsJs
open FsCore
open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open FsStore
open FsStore.Bindings
open FsStore.Hooks
open FsUi.Bindings
open FsUi.Hooks
open Fluke.Shared
open Fluke.Shared.Domain
open FsUi.Components


module StatusBar =
    open Model
    open UserInteraction

    [<ReactComponent>]
    let NowIndicator () =
        let now, setNow = React.useState DateTime.Now

        Scheduling.useScheduling Scheduling.Interval 1000 (fun _ _ -> promise { setNow DateTime.Now })

        let devicePingList = Store.useValue Selectors.Session.devicePingList

        Ui.box
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
        let alias = Store.useValue Selectors.Gun.alias

        Ui.stack
            (fun x ->
                x.direction <- "row"
                x.spacing <- "4px")
            [
                Ui.icon (fun x -> x.``as`` <- Icons.fa.FaRegUser) []

                match alias with
                | Some (Gun.Alias alias) ->
                    Ui.str "User:"

                    Ui.box
                        (fun x -> x.userSelect <- "text")
                        [
                            str alias
                        ]
                | _ -> nothing
            ]


    [<ReactComponent>]
    let SessionIndicator () =
        let activeSessions = Store.useValue Selectors.Session.activeSessions

        let (Minute sessionDuration) = Store.useValue Atoms.User.sessionDuration
        let (Minute sessionBreakDuration) = Store.useValue Atoms.User.sessionBreakDuration

        Ui.stack
            (fun x ->
                x.direction <- "row"
                x.spacing <- "4px")
            [
                Ui.icon (fun x -> x.``as`` <- Icons.gi.GiHourglass) []
                Ui.str "Sessions:"

                Ui.box
                    (fun x -> x.userSelect <- "text")
                    [
                        match activeSessions with
                        | [] -> Ui.str "No active sessions"
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
                                                Ui.box
                                                    (fun x -> x.cursor <- "pointer")
                                                    [
                                                        let sessionInfo = getSessionInfo activeSessions.Head

                                                        Ui.stack
                                                            (fun x ->
                                                                x.color <- sessionInfo.Color
                                                                x.direction <- "row"
                                                                x.spacing <- "0"
                                                                x.display <- "inline")
                                                            [
                                                                Ui.box
                                                                    (fun x -> x.display <- "inline")
                                                                    [
                                                                        str
                                                                            $"{sessionInfo.SessionType}: {activeSessions.Length} active ("
                                                                    ]

                                                                Ui.box
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

                                                                Ui.box
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
                                                Ui.stack
                                                    (fun x -> x.spacing <- "10px")
                                                    [
                                                        Ui.box
                                                            (fun x -> x.fontSize <- "1.3rem")
                                                            [
                                                                str "Session Details"
                                                            ]

                                                        yield!
                                                            activeSessions
                                                            |> List.map
                                                                (fun session ->
                                                                    let sessionInfo = getSessionInfo session

                                                                    Ui.flex
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
                    ]
            ]


    [<ReactComponent>]
    let TasksIndicator () =
        let selectedDatabaseIdSet = Store.useValue Atoms.User.selectedDatabaseIdSet

        let informationAttachmentIdSet =
            selectedDatabaseIdSet
            |> Set.toArray
            |> Array.map Selectors.Database.informationAttachmentIdMap
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
            |> Array.map Selectors.Task.attachmentIdSet
            |> Store.waitForAll
            |> Store.useValue
            |> Array.collect Set.toArray

        let selectedTaskIdListByArchive = Store.useValue Selectors.Session.selectedTaskIdListByArchive

        let sortedTaskIdAtoms = Store.useValue Selectors.Session.sortedTaskIdAtoms

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

        Ui.stack
            (fun x ->
                x.direction <- "row"
                x.spacing <- "4px")
            [
                Ui.icon (fun x -> x.``as`` <- Icons.bi.BiTask) []

                Ui.str "Tasks:"

                Ui.box
                    (fun x -> x.userSelect <- "text")
                    [

                        Tooltip.wrap
                            detailsText
                            [
                                str
                                    $"{sortedTaskIdAtoms.Length} of {selectedTaskIdListByArchive.Length} visible (Total: {total})"
                            ]

                    ]
            ]

    [<ReactComponent>]
    let PositionIndicator () =
        let position = Store.useValue Atoms.Session.position

        match position with
        | Some position ->
            Ui.stack
                (fun x ->
                    x.direction <- "row"
                    x.spacing <- "4px")
                [
                    Ui.icon (fun x -> x.``as`` <- Icons.fa.FaRegClock) []

                    Ui.str "Position:"

                    Ui.box
                        (fun x -> x.userSelect <- "text")
                        [
                            Tooltip.wrap
                                (NowIndicator ())
                                [
                                    str (position |> FlukeDateTime.Stringify).[0..-4]
                                ]
                        ]
                ]
        | None -> str "Position: No databases selected"

    [<ReactComponent>]
    let StatusBar () =
        Ui.simpleGrid
            (fun x ->
                x.display <-
                    unbox (
                        Js.newObj
                            (fun (x: Ui.IBreakpoints<string>) ->
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

                PositionIndicator ()
            ]
