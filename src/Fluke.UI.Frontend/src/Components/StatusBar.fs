namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain


module StatusBar =
    open Model
    open UserInteraction

    [<ReactComponent>]
    let StatusBar (username: Username) =
        let position = Recoil.useValue Recoil.Selectors.position
        let taskIdList = Recoil.useValue (Recoil.Atoms.Session.taskIdList username)
        let sessionData = Recoil.useValue (Recoil.Selectors.Session.sessionData username)
        let activeSessions = Recoil.useValue (Recoil.Selectors.Session.activeSessions username)

        Chakra.flex
            {|
                height = "30px"
                padding = "7px"
                align = "center"
            |}
            [
                Chakra.flex
                    ()
                    [
                        Chakra.box
                            {|
                                ``as`` = Icons.fa.FaRegUser
                                marginRight = "4px"
                            |}
                            []

                        let (Username username) = username
                        str $"User: {username}"
                    ]

                Chakra.spacer () []

                Chakra.flex
                    ()
                    [
                        Chakra.box
                            {|
                                ``as`` = Icons.gi.GiHourglass
                                marginRight = "4px"
                            |}
                            []

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
                                    {| color = color |}
                                    [
                                        str
                                            $"{sessionType}: Task[ {taskName} ]; Duration[ %.1f{duration} ]; Left[ %.1f{
                                                                                                                            left
                                            } ]"
                                    ])
                        |> List.intersperse (br [])
                        |> function
                        | [] -> str "No active session"
                        | list -> ofList list

                    ]

                Chakra.spacer () []

                match sessionData with
                | Some sessionData -> str $"{taskIdList.Length} of {sessionData.UnfilteredTaskCount} tasks visible"
                | None -> ()

                Chakra.spacer () []

                match position with
                | Some position ->
                    Chakra.flex
                        ()
                        [
                            Chakra.box
                                {|
                                    ``as`` = Icons.fa.FaRegClock
                                    marginRight = "4px"
                                |}
                                []

                            str $"Position: {position.Stringify ()}"
                        ]
                | None -> ()

            ]
