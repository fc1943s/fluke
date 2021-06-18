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

                        yield!
                            match activeSessions.valueMaybe () with
                            | Some activeSessions ->
                                activeSessions
                                |> List.map
                                    (fun (TempUI.ActiveSession (taskName,
                                                                Minute duration,
                                                                Minute totalDuration,
                                                                Minute totalBreakDuration)) ->
                                        let sessionType, color, duration, left =
                                            let left = totalDuration - duration

                                            match duration < totalDuration with
                                            | true -> "Session", "#7cca7c", duration, left
                                            | false -> "Break", "#ca7c7c", -left, totalBreakDuration + left

                                        Chakra.box
                                            (fun x -> x.color <- color)
                                            [
                                                str
                                                    $"{sessionType}: Task[ {taskName} ]; Duration[ {duration} ]; Left[ {
                                                                                                                            left
                                                    } ]"
                                            ])
                                |> List.intersperse (br [])
                                |> function
                                | [] ->
                                    [
                                        str "Sessions: No active sessions"
                                    ]
                                | list -> list
                            | _ ->
                                [
                                    str "Sessions: Loading sessions"
                                ]

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
