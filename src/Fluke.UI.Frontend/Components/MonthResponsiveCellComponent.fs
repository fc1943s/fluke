namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fable.DateFunctions
open Fluke.Shared.Model


module MonthResponsiveCellComponent =
    let render =
        React.memo (fun (input: {| Date: FlukeDate; Css: IStyleAttribute list |}) ->
            let month = input.Date.DateTime.Format "MMM"

            Html.span [
                prop.style [
                    style.textAlign.center
                    yield! input.Css
                ]
                prop.children
                    [
                        str month
                    ]
            ])
