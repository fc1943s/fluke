namespace Fluke.UI.Frontend.Components


open Fable.React
open Feliz
open System
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.Model


module Settings =
    [<ReactComponent>]
    let rec Settings
        (input: {| Username: Username
                   Props: Chakra.IChakraProps -> unit |})
        =
        let debug, setDebug = Recoil.useState Atoms.debug

        Accordion.Accordion
            {|
                Props = input.Props
                Atom = Atoms.User.accordionFlag (input.Username, TextKey (nameof Settings))
                Items =
                    [
                        "User",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                ChangeUserPasswordButton.ChangeUserPasswordButton ()

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Day Start"
                                        x.alignSelf <- "flex-start"
                                        x.placeholder <- "00:00"

                                        x.atom <-
                                            Some (
                                                Recoil.AtomFamily (input.Username, Atoms.User.dayStart, input.Username)
                                            )

                                        x.inputFormat <- Some Input.InputFormat.Time
                                        x.onFormat <- Some FlukeTime.Stringify

                                        x.onValidate <-
                                            Some (
                                                fst
                                                >> DateTime.Parse
                                                >> FlukeTime.FromDateTime
                                                >> Some
                                            ))
                            ])

                        "View",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Input.Input
                                    (fun x ->
                                        x.label <- str "Days Before"

                                        x.atom <-
                                            Some (Recoil.Atom (input.Username, Atoms.User.daysBefore input.Username))

                                        x.inputFormat <- Some Input.InputFormat.Number)

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Days After"

                                        x.atom <-
                                            Some (Recoil.Atom (input.Username, Atoms.User.daysAfter input.Username))

                                        x.inputFormat <- Some Input.InputFormat.Number)

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Cell Size"

                                        x.atom <-
                                            Some (Recoil.Atom (input.Username, Atoms.User.cellSize input.Username))

                                        x.inputFormat <- Some Input.InputFormat.Number)

                                Checkbox.Checkbox
                                    {|
                                        Props =
                                            fun x ->
                                                x.isChecked <- debug
                                                x.onChange <- fun _ -> promise { setDebug (not debug) }

                                                x.children <-
                                                    [
                                                        str "Show Debug Information"
                                                    ]
                                    |}
                            ])

                        "Connection",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                InputList.InputList
                                    (fun x ->
                                        x.label <- str "Gun peers"
                                        x.atom <- Some (Recoil.Atom (input.Username, Atoms.gunPeers)))
                            ])
                    ]
            |}
