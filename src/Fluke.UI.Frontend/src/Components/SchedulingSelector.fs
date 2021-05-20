namespace rec Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared.Domain
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module SchedulingSelector =
    [<ReactComponent>]
    let SchedulingSelector
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId |})
        =

        let schedulingFieldOptions =
            Recoil.useAtomFieldOptions
                (Some (Recoil.AtomFamily (Atoms.Task.scheduling, input.TaskId)))
                (Some (Recoil.InputScope.ReadWrite Gun.defaultSerializer))

        Chakra.box
            (fun x -> x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Scheduling"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}
                Menu.Menu
                    {|
                        Tooltip = ""
                        Trigger =
                            Chakra.menuButton
                                (fun x ->
                                    x.``as`` <- Chakra.react.Button
                                    x.rightIcon <- Chakra.Icons.chevronDownIcon (fun _ -> ()) [])
                                [
                                    schedulingFieldOptions.AtomValue
                                    |> Scheduling.Label
                                    |> str
                                ]
                        Menu =
                            [
                                Chakra.radioGroup
                                    (fun x ->
                                        x.onChange <- fun (radioValueSelected: string) -> promise { () }

                                        x.value <- null)
                                    [
                                        Chakra.stack
                                            (fun x ->
                                                x.spacing <- "15px"
                                                x.direction <- "row")
                                            [
                                                let label text =
                                                    Chakra.box
                                                        (fun x -> x.marginBottom <- "-2px")
                                                        [
                                                            str text
                                                        ]

                                                Chakra.radio
                                                    (fun x ->
                                                        x.colorScheme <- "purple"
                                                        x.value <- nameof Project)
                                                    [
                                                        label "Project"
                                                    ]

                                                Chakra.radio
                                                    (fun x ->
                                                        x.colorScheme <- "purple"
                                                        x.value <- nameof Area)
                                                    [
                                                        label "Area"
                                                    ]
                                            ]
                                    ]
                            ]
                        MenuListProps = fun x -> x.padding <- "10px"
                    |}
            ]
