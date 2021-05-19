namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module CellBorder =
    open Domain.UserInteraction

    [<ReactComponent>]
    let CellBorder
        (input: {| Username: Username
                   Date: FlukeDate |})
        =
        let weekStart = Recoil.useValue (Atoms.User.weekStart input.Username)

        match (weekStart, input.Date) with
        | StartOfMonth -> Some ("1px", "#ffffff3d")
        | StartOfWeek -> Some ("1px", "#222")
        | _ -> None
        |> Option.map
            (fun (borderLeftWidth, borderLeftColor) ->
                Chakra.box
                    (fun x ->
                        x.position <- "absolute"
                        x.top <- "-1px"
                        x.left <- "-1px"
                        x.bottom <- "-1px"
                        x.borderLeftWidth <- borderLeftWidth
                        x.borderLeftColor <- borderLeftColor)
                    [])
        |> Option.defaultValue nothing
