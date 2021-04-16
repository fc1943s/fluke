namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Settings =
    open Fluke.UI.Frontend.Recoil

    [<ReactComponent>]
    let rec Settings
        (input: {| Props: {| flex: int
                             overflowY: string
                             flexBasis: int |} |})
        =
        Chakra.stack
            {| input.Props with spacing = "10px" |}
            [
                Input.Input (
                    Dom.newObj
                        (fun x ->
                            x.label <- "Days Before"
                            x.atom <- Some (Recoil.Atom Atoms.daysBefore)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly
                            x.inputFormat <- Some Input.InputFormat.Number)
                )

                Input.Input (
                    Dom.newObj
                        (fun x ->
                            x.label <- "Days After"
                            x.atom <- Some (Recoil.Atom Atoms.daysAfter)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly
                            x.inputFormat <- Some Input.InputFormat.Number)
                )

                Input.Input (
                    Dom.newObj
                        (fun x ->
                            x.label <- "Old API URL"
                            x.atom <- Some (Recoil.Atom Atoms.apiBaseUrl)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )

                Input.Input (
                    Dom.newObj
                        (fun x ->
                            x.label <- "Gun peer 1"
                            x.atom <- Some (Recoil.Atom Atoms.gunPeer1)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )

                Input.Input (
                    Dom.newObj
                        (fun x ->
                            x.label <- "Gun peer 2"
                            x.atom <- Some (Recoil.Atom Atoms.gunPeer2)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )

                Input.Input (
                    Dom.newObj
                        (fun x ->
                            x.label <- "Gun peer 3"
                            x.atom <- Some (Recoil.Atom Atoms.gunPeer3)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )
            ]
