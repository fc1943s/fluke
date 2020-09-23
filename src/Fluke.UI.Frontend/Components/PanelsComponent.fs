namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Model


module PanelsComponent =
    let render =
        React.memo (fun () ->

            let view = Recoil.useValue Recoil.Selectors.view
            let username = Recoil.useValue Recoil.Atoms.username

            Html.div [
                prop.className Css.panels
                prop.children [
                    match username with
                    | None -> str "no user"
                    | Some username ->
                        match view with
                        | View.Calendar -> CalendarViewComponent.render {| Username = username |}
                        | View.Groups -> GroupsViewComponent.render {| Username = username |}
                        | View.Tasks -> TasksViewComponent.render {| Username = username |}
                        | View.Week -> WeekViewComponent.render {| Username = username |}

                    DetailsComponent.render ()
                ]
            ])
