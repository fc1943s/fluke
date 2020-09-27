namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Suigetsu.Core
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Model
open Fluke.Shared


module CalendarViewComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let taskIdList = Recoil.useValue (Recoil.Atoms.Session.taskIdList input.Username)

            Html.div [
                prop.className Css.lanesPanel
                prop.children [
                    Html.div [
                        yield! Html.div
                                   [
                                       prop.className Css.cellRectangle
                                   ]
                               |> List.replicate 3

                        Html.div [
                            prop.style
                                [
                                    style.display.flex
                                ]
                            prop.children [
                                Html.div [
                                    prop.style
                                        [
                                            style.paddingRight 10
                                        ]
                                    prop.children
                                        [
                                            yield! taskIdList
                                                   |> List.map (fun taskId ->
                                                       TaskInformationNameComponent.render {| TaskId = taskId |})
                                        ]
                                ]
                                // Column: Priority
                                Html.div [
                                    prop.style [
                                        style.paddingRight 10
                                        style.textAlign.center
                                    ]
                                    prop.children
                                        [
                                            yield! taskIdList
                                                   |> List.map (fun taskId ->
                                                       TaskPriorityComponent.render {| TaskId = taskId |})
                                        ]
                                ]
                                // Column: Task Name
                                Html.div [
                                    prop.style
                                        [
                                            style.width 200
                                        ]
                                    prop.children
                                        [
                                            yield! taskIdList
                                                   |> List.map (fun taskId ->
                                                       TaskNameComponent.render {| Css = []; TaskId = taskId |})
                                        ]
                                ]
                            ]
                        ]
                    ]
                    Html.div [
                        GridHeaderComponent.render {| Username = input.Username |}
                        CellsComponent.render
                            {|
                                Username = input.Username
                                TaskIdList = taskIdList
                            |}
                    ]
                ]
            ])
