namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Settings =
    [<ReactComponent>]
    let rec Settings
        (input: {| Username: UserInteraction.Username
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
                            (fun x ->
                                x.spacing <- "10px"
                                x.alignItems <- "flex-start")
                            [
                                ChangeUserPasswordButton.ChangeUserPasswordButton ()
//                                DeleteUserButton.DeleteUserButton ()
                            ])

                        "View",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Input.Input
                                    (fun x ->
                                        x.label <- str "Days Before"
                                        x.atom <- Some (Recoil.Atom (Atoms.User.daysBefore input.Username))
                                        x.inputFormat <- Some Input.InputFormat.Number)

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Days After"
                                        x.atom <- Some (Recoil.Atom (Atoms.User.daysAfter input.Username))
                                        x.inputFormat <- Some Input.InputFormat.Number)

                                Input.Input
                                    (fun x ->
                                        x.label <- str "Cell Size"
                                        x.atom <- Some (Recoil.Atom (Atoms.User.cellSize input.Username))
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
                                        x.atom <- Some (Recoil.Atom Atoms.gunPeers))
                            ])
                    ]
            |}
