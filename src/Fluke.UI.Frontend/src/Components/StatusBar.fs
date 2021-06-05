namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
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
        let position = Recoil.useValue Atoms.position

        let selectedTaskIdSet =
            Recoil.useValueLoadableDefault (Selectors.Session.selectedTaskIdSet input.Username) Set.empty

        let filteredTaskIdList = Recoil.useValueLoadableDefault (Selectors.Session.filteredTaskIdList input.Username) []
        let activeSessions = Recoil.useValueLoadableDefault (Selectors.Session.activeSessions input.Username) []

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
                                                $"{sessionType}: Task[ {taskName} ]; Duration[ %.1f{duration} ]; Left[ %.1f{
                                                                                                                                left
                                                } ]"
                                        ])
                            |> List.intersperse (br [])
                            |> function
                            | [] ->
                                [
                                    str "No active session"
                                ]
                            | list -> list

                    ]

                Chakra.flex
                    (fun _ -> ())
                    [
                        Chakra.icon
                            (fun x ->
                                x.``as`` <- Icons.bi.BiTask
                                x.marginRight <- "4px")
                            []

                        str $"{filteredTaskIdList.Length} of {selectedTaskIdSet.Count} tasks visible"
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
                | None -> nothing
            ]
