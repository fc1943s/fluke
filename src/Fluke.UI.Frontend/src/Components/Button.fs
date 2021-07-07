namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module Button =
    [<RequireQualifiedAccess>]
    type IconPosition =
        | Left
        | Right

    [<ReactComponent>]
    let Button
        (input: {| Icon: (ReactElement * IconPosition) option
                   Hint: ReactElement option
                   Props: Chakra.IChakraProps -> unit
                   Children: seq<ReactElement> |})
        =

        let icon, iconPosition =
            match input.Icon with
            | Some (icon, iconPosition) -> Some icon, Some iconPosition
            | _ -> None, None

        Tooltip.wrap
            (input.Hint |> Option.defaultValue null)
            [
                match icon, input.Children |> Seq.toList with
                | Some icon, [] ->
                    Chakra.iconButton
                        (fun x ->
                            x.icon <- icon
                            input.Props x)
                        []
                | icon, children ->
                    let icon () =
                        match icon with
                        | Some icon ->
                            Chakra.box
                                (fun _ -> ())
                                [
                                    icon
                                ]
                        | None -> nothing

                    Chakra.button
                        (fun x ->
                            x.height <- "auto"
                            x.alignSelf <- "flex-start"
                            x.color <- "gray"
                            x.paddingTop <- "5px"
                            x.paddingBottom <- "5px"
                            x.borderRadius <- "3px"
                            input.Props x)
                        [
                            Chakra.stack
                                (fun x ->
                                    x.direction <- "row"
                                    x.spacing <- "7px"
                                    x.alignItems <- "center")
                                [
                                    match iconPosition with
                                    | Some IconPosition.Left -> icon ()
                                    | _ -> nothing

                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            yield! children
                                        ]

                                    match iconPosition with
                                    | Some IconPosition.Right -> icon ()
                                    | _ -> nothing
                                ]
                        ]
            ]
