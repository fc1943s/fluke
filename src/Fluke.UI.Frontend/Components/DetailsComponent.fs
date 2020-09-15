namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend


module DetailsComponent =
    let render =
        React.memo (fun () ->
            //            let selectedCells = Recoil.useValue Recoil.Selectors.selectedCells


            Html.div [
                prop.className Css.detailsPanel
                prop.children
                    [
                        str "Details"
                    ]
            ])
