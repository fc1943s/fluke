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
        let taskIdList = Recoil.useValue (Selectors.Session.taskIdList input.Username)
        let visibleTaskIdList = Recoil.useValue (Selectors.Session.visibleTaskIdList input.Username)
        //        let sessionData = Recoil.useValue (Selectors.Session.sessionData input.Username)
        let activeSessions = Recoil.useValue (Selectors.Session.activeSessions input.Username)

        Chakra.flex
            (fun x ->
                x.height <- "30px"
                x.padding <- "7px"
                x.align <- "center")
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

                Chakra.spacer (fun _ -> ()) []

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

                Chakra.spacer (fun _ -> ()) []

                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.bi.BiTask
                        x.marginRight <- "4px")
                    []

                str $"{visibleTaskIdList.Length} of {taskIdList.Length} tasks visible"

                Chakra.spacer (fun _ -> ()) []

                match position with
                | Some position ->
                    Chakra.flex
                        (fun _ -> ())
                        [
                            Chakra.icon
                                (fun x ->
                                    x.``as`` <- Icons.fa.FaRegClock
                                    x.marginRight <- "4px")
                                []

                            str $"Position: {position.Stringify ()}"
                        ]
                | None -> ()
            ]
