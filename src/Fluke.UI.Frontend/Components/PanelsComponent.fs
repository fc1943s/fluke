namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Model


module PanelsComponent =
    let render =
        React.memo (fun () ->

            let view = Recoil.useValue Recoil.Selectors.view

            Html.div [
                prop.className Css.panels
                prop.children [
                    match view with
                    | View.Calendar -> CalendarViewComponent.render ()
                    | View.Groups -> GroupsViewComponent.render ()
                    | View.Tasks -> TasksViewComponent.render ()
                    | View.Week -> WeekViewComponent.render ()

                    DetailsComponent.render ()
                ]
            ])
