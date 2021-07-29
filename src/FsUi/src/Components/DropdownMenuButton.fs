namespace FsUi.Components

open Fable.Core
open Fable.React
open FsUi.Bindings


module DropdownMenuButton =
    let rec inline DropdownMenuButton
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
                             Icons.fi.FiCheck |> Icons.render
                         else
                             UI.box (fun x -> x.width <- "11px") []),
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
