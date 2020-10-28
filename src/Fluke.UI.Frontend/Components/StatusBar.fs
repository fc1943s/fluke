namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module StatusBar =
    open Model
    open UserInteraction

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let (Username username) = input.Username
            let position = Recoil.useValue Recoil.Selectors.position
            let activeSessions = Recoil.useValue (Recoil.Selectors.Session.activeSessions input.Username)

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

                            str (sprintf "User: %s" username)
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
                            |> List.map (fun (TempUI.ActiveSession (taskName, (Minute duration), (Minute totalDuration), (Minute totalBreakDuration))) ->
                                let sessionType, color, duration, left =
                                    let left = totalDuration - duration
                                    match duration < totalDuration with
                                    | true -> "Session", "#7cca7c", duration, left
                                    | false -> "Break", "#ca7c7c", -left, totalBreakDuration + left

                                Chakra.box
                                    {| color = color |}
                                    [
                                        sprintf
                                            "%s: Task[ %s ]; Duration[ %.1f ]; Left[ %.1f ]"
                                            sessionType
                                            taskName
                                            duration
                                            left
                                        |> str
                                    ])
                            |> List.intersperse (br [])
                            |> function
                            | [] -> str "No active session"
                            | list -> ofList list

                        ]

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

                                str (sprintf "Position: %s" (position.Stringify ()))
                            ]
                    | None -> ()
                ])
