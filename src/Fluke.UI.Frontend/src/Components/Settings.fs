namespace Fluke.UI.Frontend.Components


open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.Model
open System
open Fluke.Shared
open Browser.Types
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Settings =
    [<ReactComponent>]
    let rec Settings
        (input: {| Username: Username
                   Props: Chakra.IChakraProps -> unit |})
        =
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
                                                >> DateTime.TryParse
                                                >> function
                                                | true, value -> value
                                                | _ -> DateTime.Parse "00:00"
                                                >> FlukeTime.FromDateTime
                                                >> Some
                                            ))

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Session Duration"

                                        x.atom <-
                                            Some (
                                                Recoil.Atom (input.Username, Atoms.User.sessionDuration input.Username)
                                            )

                                        x.inputFormat <- Some Input.InputFormat.Number
                                        x.onFormat <- Some (Minute.Value >> string)

                                        x.onValidate <-
                                            Some (
                                                fst
                                                >> String.parseIntMin 1
                                                >> Option.defaultValue 1
                                                >> Minute
                                                >> Some
                                            ))

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Session Break Duration"

                                        x.atom <-
                                            Some (
                                                Recoil.Atom (
                                                    input.Username,
                                                    Atoms.User.sessionBreakDuration input.Username
                                                )
                                            )

                                        x.inputFormat <- Some Input.InputFormat.Number
                                        x.onFormat <- Some (Minute.Value >> string)

                                        x.onValidate <-
                                            Some (
                                                fst
                                                >> String.parseIntMin 1
                                                >> Option.defaultValue 1
                                                >> Minute
                                                >> Some
                                            ))

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Color"

                                        x.atom <- Some (Recoil.Atom (input.Username, Atoms.User.color input.Username)))

                                ChangeUserPasswordButton.ChangeUserPasswordButton ()
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

                                CheckboxInput.CheckboxInput
                                    {|
                                        Atom = Atoms.debug
                                        Label = Some "Show Debug Information"
                                        Props = fun _ -> ()
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
