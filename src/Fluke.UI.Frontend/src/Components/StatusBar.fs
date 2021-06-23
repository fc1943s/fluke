namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
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
    let SessionIndicator (input: {| Username: Username |}) =
        let activeSessions = Store.useValue (Selectors.Session.activeSessions input.Username)

        let (Minute sessionDuration) = Store.useValue (Atoms.User.sessionDuration input.Username)
        let (Minute sessionBreakDuration) = Store.useValue (Atoms.User.sessionBreakDuration input.Username)

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
                        |}

            //                        | _ -> str "Sessions: Loading sessions"

            ]


    [<ReactComponent>]
    let TasksIndicator (input: {| Username: Username |}) =
        let selectedTaskIdSet = Store.useValue (Selectors.Session.selectedTaskIdSet input.Username)
        let sortedTaskIdList = Store.useValue (Selectors.Session.sortedTaskIdList input.Username)

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
                str $"Tasks: {sortedTaskIdList.Length} of {selectedTaskIdSet.Count} visible"
            //                        | _ -> str "Tasks: Loading tasks"
            ]

    [<ReactComponent>]
    let StatusBar (input: {| Username: Username |}) =
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
                Chakra.flex
                    (fun _ -> ())
                    [
                        Chakra.icon
                            (fun x ->
                                x.``as`` <- Icons.fa.FaRegUser
                                x.marginRight <- "4px")
                            []

                        let (Username username) = input.Username
                        str $"User: {username}"
                    ]

                React.suspense (
                    [
                        SessionIndicator {| Username = input.Username |}
                    ],
                    LoadingSpinner.InlineLoadingSpinner ()
                )

                React.suspense (
                    [
                        TasksIndicator {| Username = input.Username |}
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
