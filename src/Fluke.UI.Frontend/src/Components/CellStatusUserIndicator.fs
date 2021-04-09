namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.Shared


module CellStatusUserIndicator =
    open Domain.UserInteraction

    [<ReactComponent>]
    let CellStatusUserIndicator (input: {| Username: Username |}) =
        let color = Recoil.useValue (Recoil.Atoms.User.color input.Username)

        Chakra.box
            {|
                height = "17px"
                lineHeight = "17px"
                position = "absolute"
                top = 0
                width = "100%"
                _after =
                    {|
                        borderBottomColor =
                            match color with
                            | UserColor.Blue -> Some "#000000"
                            | UserColor.Pink -> Some "#a91c77"
                            | _ -> None
                        borderBottomWidth = "8px"
                        borderLeftColor = "transparent"
                        borderLeftWidth = "8px"
                        position = "absolute"
                        content = "\"\""
                        bottom = 0
                        right = 0
                    |}
            |}
            []
