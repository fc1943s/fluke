namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Settings =
    open Fluke.UI.Frontend.Recoil

    [<ReactComponent>]
    let rec Settings
        (input: {| Username: UserInteraction.Username
                   Props: Chakra.IChakraProps |})
        =
        Chakra.stack
            (fun x ->
                x <+ input.Props
                x.spacing <- "10px")
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

                Input.Input (
                    JS.newObj
                        (fun x ->
                            x.label <- str "Legacy API URL"
                            x.atom <- Some (Recoil.Atom Atoms.apiBaseUrl)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )

                InputList.InputList (
                    JS.newObj
                        (fun x ->
                            x.label <- str "Gun peers"
                            x.atom <- Some (Recoil.Atom Atoms.gunPeers)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )
            ]
