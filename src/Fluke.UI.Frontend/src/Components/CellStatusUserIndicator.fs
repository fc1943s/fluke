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
            (fun x ->
                x.height <- "17px"
                x.lineHeight <- "17px"
                x.position <- "absolute"
                x.top <- "0"
                x.width <- "100%"

                x._after <-
                    (JS.newObj
                        (fun x ->
                            x.borderBottomWidth <- "8px"
                            x.borderBottomColor <-
                                match color with
                                | UserColor.Blue -> "#000000"
                                | UserColor.Pink -> "#a91c77"
                                | _ -> null

                            x.borderLeftWidth <- "8px"
                            x.borderLeftColor <- "transparent"
                            x.position <- "absolute"
                            x.content <- "\"\""
                            x.bottom <- "0"
                            x.right <- "0")))
            []
