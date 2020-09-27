namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Suigetsu.Core
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module GroupsViewComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let groupIndentationLength = 20

            let tasksByInformationKind =
                Recoil.useValue (Recoil.Selectors.Session.tasksByInformationKind input.Username)

            Html.div [
                prop.className Css.lanesPanel
                prop.children [
                    Html.div [
                        yield! Html.div
                                   [
                                       prop.className Css.cellRectangle
                                   ]
                               |> List.replicate 3

                        Html.div
                            [
                                yield! tasksByInformationKind
                                       |> List.map (fun (informationKindName, groups) ->
                                           Html.div [
                                               Html.div [
                                                   prop.style
                                                       [
                                                           style.color "#444"
                                                       ]
                                                   prop.children
                                                       [
                                                           str informationKindName
                                                       ]
                                               ]
                                               Html.div
                                                   [
                                                       yield! groups
                                                              |> List.map (fun (informationId, taskIdList) ->



                                                                  Html.div [
                                                                      InformationNameComponent.render
                                                                          {| InformationId = informationId |}

                                                                      // Task Name
                                                                      Html.div [
                                                                          prop.style
                                                                              [
                                                                                  style.width 400
                                                                              ]
                                                                          prop.children
                                                                              [
                                                                                  yield! taskIdList
                                                                                         |> List.map (fun taskId ->
                                                                                             Html.div [
                                                                                                 prop.style
                                                                                                     [
                                                                                                         style.display.flex
                                                                                                     ]
                                                                                                 prop.children [
                                                                                                     TaskPriorityComponent.render
                                                                                                         {|
                                                                                                             TaskId =
                                                                                                                 taskId
                                                                                                         |}
                                                                                                     TaskNameComponent.render
                                                                                                         {|
                                                                                                             Css =
                                                                                                                 [
                                                                                                                     style.paddingLeft
                                                                                                                         (groupIndentationLength
                                                                                                                          * 2)
                                                                                                                 ]
                                                                                                             TaskId =
                                                                                                                 taskId
                                                                                                         |}
                                                                                                 ]
                                                                                             ])
                                                                              ]
                                                                      ]
                                                                  ])
                                                   ]
                                           ])
                            ]
                    ]
                    // Column: Grid
                    Html.div [
                        GridHeaderComponent.render {| Username = input.Username |}
                        Html.div
                            [
                                yield! tasksByInformationKind
                                       |> List.map (fun (_, groups) ->
                                           Html.div [
                                               Html.div
                                                   [
                                                       prop.className Css.cellRectangle
                                                   ]
                                               Html.div
                                                   [
                                                       yield! groups
                                                              |> List.map (fun (_, taskIdList) ->
                                                                  Html.div [
                                                                      Html.div
                                                                          [
                                                                              prop.className Css.cellRectangle
                                                                          ]
                                                                      CellsComponent.render
                                                                          {|
                                                                              Username = input.Username
                                                                              TaskIdList = taskIdList
                                                                          |}
                                                                  ])
                                                   ]
                                           ])
                            ]
                    ]
                ]
            ])
