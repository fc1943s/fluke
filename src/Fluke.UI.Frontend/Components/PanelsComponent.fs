namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module PanelsComponent =

    let render =
        React.memo (fun () ->

            let view = Recoil.useValue Recoil.Selectors.view
            let username = Recoil.useValue Recoil.Atoms.username


            Chakra.flex
                {| className = "panels" |}
                [
                    match username with
                    | None -> str "no user"
                    | Some username ->
                        match view with
                        | View.View.Calendar -> CalendarViewComponent.render {| Username = username |}
                        | View.View.Groups -> GroupsViewComponent.render {| Username = username |}
                        | View.View.Tasks -> TasksViewComponent.render {| Username = username |}
                        | View.View.Week -> WeekViewComponent.render {| Username = username |}
                ])
