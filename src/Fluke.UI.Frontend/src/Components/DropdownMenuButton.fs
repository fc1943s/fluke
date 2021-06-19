namespace Fluke.UI.Frontend.Components

open Fable.Core
open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module DropdownMenuButton =
    [<ReactComponent>]
    let rec DropdownMenuButton
        (input: {| Label: string
                   OnClick: unit -> JS.Promise<unit>
                   Checked: bool |})
        =
        Button.Button
            {|
                Hint = None
                Icon =
                    Some (
                        (if input.Checked then
                             Icons.fi.FiCheck |> Icons.wrap
                         else
                             fun () -> (Chakra.box (fun x -> x.width <- "11px") [])),
                        Button.IconPosition.Left
                    )
                Props =
                    fun x ->
                        x.onClick <- fun _ -> promise { do! input.OnClick () }
                        x.alignSelf <- "stretch"
                        x.backgroundColor <- "whiteAlpha.100"
                        x.borderRadius <- "2px"
                Children =
                    [
                        str input.Label
                    ]
            |}
