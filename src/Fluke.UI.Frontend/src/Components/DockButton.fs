namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module DockButton =

    [<ReactComponent>]
    let DockButton
        (input: {| Name: string
                   Icon: obj
                   Atom: RecoilValue<TempUI.DockType option, ReadWrite>
                   DockType: TempUI.DockType |})
        =
        let atom, setAtom = Recoil.useState input.Atom

        Chakra.button
            {|
                height = "100%"
                borderRadius = 0
                backgroundColor =
                    if atom = Some input.DockType then
                        "gray.10"
                    else
                        "transparent"
                fontWeight = "inherit"
                fontSize = "14px"
                fontFamily = "inherit"
                onClick =
                    fun () ->
                        setAtom (
                            if atom = Some input.DockType then
                                None
                            else
                                Some input.DockType
                        )
            |}
            [
                Chakra.box
                    {|
                        ``as`` = input.Icon
                        marginRight = "6px"
                    |}
                    []
                str input.Name
            ]
