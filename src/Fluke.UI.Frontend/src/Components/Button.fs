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
        (input: {| Icon: ((unit -> ReactElement) * IconPosition) option
                   Hint: ReactElement option
                   Props: Chakra.IChakraProps |})
        =

        let icon, iconPosition =
            match input.Icon with
            | Some (icon, iconPosition) -> Some icon, Some iconPosition
            | _ -> None, None

        Tooltip.wrap
            (input.Hint |> Option.defaultValue null)
            [
                match input.Props.children |> Seq.toList with
                | [] ->
                    Chakra.iconButton
                        (fun x ->
                            x <+ input.Props
                            x.icon <- icon)
                        []
                | children ->
                    let icon =
                        Chakra.box
                            (fun x ->
                                x.``as`` <- icon
                                x.fontSize <- "21px")
                            []

                    Chakra.button
                        (fun x ->
                            x <+ input.Props
                            x.height <- "auto"
                            x.paddingTop <- "2px"
                            x.paddingBottom <- "2px")
                        [
                            Chakra.stack
                                (fun x ->
                                    x.direction <- "row"
                                    x.spacing <- "7px")
                                [
                                    if iconPosition = Some IconPosition.Left then icon

                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            yield! children
                                        ]

                                    if iconPosition = Some IconPosition.Right then icon
                                ]
                        ]
            ]
