namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Suigetsu.Core
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared
open Fluke.UI.Frontend.Model


module TasksViewComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let currentTaskList = Recoil.useValue Recoil.Selectors.currentTaskList

            let taskIdList = currentTaskList |> List.map (fun x -> x.Id)

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
                                            yield! currentTaskList
                                                   |> List.map (fun task ->
                                                       Html.div [
                                                           prop.className Css.cellRectangle
                                                           prop.children [
                                                               Html.div [
                                                                   prop.style [
                                                                       style.color task.Information.Color
                                                                       style.whitespace.nowrap
                                                                   ]
                                                                   prop.children
                                                                       [
                                                                           let (InformationName informationName) =
                                                                               task.Information.Name

                                                                           str informationName
                                                                       ]
                                                               ]

                                                               TooltipPopupComponent.render
                                                                   {| Attachments = task.InformationAttachments |}
                                                           ]
                                                       ])
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
                                            yield! currentTaskList
                                                   |> List.map (fun task ->
                                                       Html.div [
                                                           prop.className Css.cellRectangle
                                                           prop.children
                                                               [
                                                                   task.Priority
                                                                   |> Option.map (fun x -> x.Value)
                                                                   |> Option.defaultValue 0
                                                                   |> string
                                                                   |> str
                                                               ]
                                                       ])
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
                                            yield! currentTaskList
                                                   |> List.map (fun task ->
                                                       TaskNameComponent.render {| Css = []; TaskId = task.Id |})
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
