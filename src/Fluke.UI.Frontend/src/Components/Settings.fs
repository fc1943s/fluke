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
                   Props: Chakra.IChakraProps |})
        =
        Accordion.Accordion
            {|
                Props = JS.newObj (fun x -> x <+ input.Props)
                Atom = Atoms.User.accordionFlag (input.Username, TextKey (nameof Settings))
                Items =
                    [
                        "View",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                Input.Input (
                                    JS.newObj
                                        (fun x ->
                                            x.label <- str "Days Before"
                                            x.atom <- Some (Recoil.Atom (Atoms.User.daysBefore input.Username))
                                            x.atomScope <- Some Recoil.AtomScope.ReadOnly
                                            x.inputFormat <- Some Input.InputFormat.Number)
                                )

                                Input.Input (
                                    JS.newObj
                                        (fun x ->
                                            x.label <- str "Days After"
                                            x.atom <- Some (Recoil.Atom (Atoms.User.daysAfter input.Username))
                                            x.atomScope <- Some Recoil.AtomScope.ReadOnly
                                            x.inputFormat <- Some Input.InputFormat.Number)
                                )
                            ])

                        "Connection",
                        (Chakra.stack
                            (fun x -> x.spacing <- "10px")
                            [
                                InputList.InputList (
                                    JS.newObj
                                        (fun x ->
                                            x.label <- str "Gun peers"
                                            x.atom <- Some (Recoil.Atom Atoms.gunPeers)
                                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                                )

                                Input.Input (
                                    JS.newObj
                                        (fun x ->
                                            x.label <- str "Legacy API URL"
                                            x.atom <- Some (Recoil.Atom Atoms.apiBaseUrl)
                                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                                )
                            ])
                    ]
            |}
