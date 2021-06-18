namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain


module StatusBar =
    open Model
    open UserInteraction

    [<ReactComponent>]
    let StatusBar (input: {| Username: Username |}) =
        let position = Store.useValue Atoms.position

        let selectedTaskIdSet =
            Store.useValueLoadableDefault (Selectors.Session.selectedTaskIdSet input.Username) Set.empty

        let sortedTaskIdList = Store.useValueLoadable (Selectors.Session.sortedTaskIdList input.Username)
        let activeSessions = Store.useValueLoadable (Selectors.Session.activeSessions input.Username)

        let (Minute sessionDuration) = Store.useValue (Atoms.User.sessionDuration input.Username)
        let (Minute sessionBreakDuration) = Store.useValue (Atoms.User.sessionBreakDuration input.Username)

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

                Chakra.flex
                    (fun _ -> ())
                    [
                        Chakra.icon
                            (fun x ->
                                x.``as`` <- Icons.gi.GiHourglass
                                x.marginRight <- "4px")
                            []

                        match activeSessions.valueMaybe () with
                        | Some [] -> str "Sessions: No active sessions"
                        | Some activeSessions ->
                            let getSessionInfo (TempUI.ActiveSession (taskName, Minute duration)) =
                                let left = sessionDuration - duration

                                match duration < sessionDuration with
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
                                                                    $"). Started {sessionInfo.Duration}m ago ({
                                                                                                                   sessionInfo.Left
                                                                    }m left)"
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
                                                                                $"{sessionInfo.SessionType}: Started {
                                                                                                                          sessionInfo.Duration
                                                                                }m ago ({sessionInfo.Left}m left). {
                                                                                                                        sessionInfo.TaskName
                                                                                }"
                                                                        ])
                                                    ]
                                            ]
                                |}

                        | _ -> str "Sessions: Loading sessions"

                    ]

                Chakra.flex
                    (fun _ -> ())
                    [
                        Chakra.icon
                            (fun x ->
                                x.``as`` <- Icons.bi.BiTask
                                x.marginRight <- "4px")
                            []

                        match sortedTaskIdList.valueMaybe () with
                        | Some sortedTaskIdList ->
                            str $"Tasks: {sortedTaskIdList.Length} of {selectedTaskIdSet.Count} visible"
                        | _ -> str "Tasks: Loading tasks"
                    ]

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

                                str $"Position: {position |> FlukeDateTime.Stringify}"
                            ]
                    ]
                | None -> str $"Position: No databases selected"
            ]
