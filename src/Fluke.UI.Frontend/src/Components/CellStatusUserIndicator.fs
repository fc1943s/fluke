namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module CellStatusUserIndicator =
    open Domain.UserInteraction

    [<ReactComponent>]
    let CellStatusUserIndicator (input: {| User: User |}) =
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
                            match input.User with
                            | { Color = UserColor.Blue } -> Some "#005688"
                            | { Color = UserColor.Pink } -> Some "#a91c77"
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
