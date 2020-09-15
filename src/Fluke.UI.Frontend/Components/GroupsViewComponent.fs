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
    let render =
        React.memo (fun () ->
            let groupIndentationLength = 20

            let currentTaskList = Recoil.useValue Recoil.Selectors.currentTaskList

            let groupMap =
                currentTaskList
                |> List.map (fun x -> x.Information, x)
                |> Map.ofList

            let groups =
                currentTaskList
                |> List.groupBy (fun group -> group.Information)
                |> List.sortBy (fun (information, _) -> information.Name)
                |> List.groupBy (fun (information, _) -> information.KindName)
                |> List.sortBy
                    (snd
                     >> List.head
                     >> fst
                     >> fun information -> information.Order)

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
                                yield! groups
                                       |> List.map (fun (informationKindName, taskGroups) ->
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
                                                       yield! taskGroups
                                                              |> List.map (fun (information, group) ->
                                                                  let informationAttachments =
                                                                      groupMap.[information].Attachments

                                                                  Html.div [
                                                                      Html.div [
                                                                          prop.className Css.cellRectangle
                                                                          prop.children [
                                                                              Html.div [
                                                                                  prop.style [
                                                                                      style.color "#444"
                                                                                      style.paddingLeft
                                                                                          groupIndentationLength
                                                                                  ]
                                                                                  prop.children
                                                                                      [
                                                                                          let (Model.InformationName informationName) =
                                                                                              information.Name

                                                                                          str informationName
                                                                                      ]
                                                                              ]
                                                                              TooltipPopupComponent.render
                                                                                  {|
                                                                                      Attachments =
                                                                                          informationAttachments
                                                                                  |}
                                                                          ]
                                                                      ]
                                                                      // Task Name
                                                                      Html.div [
                                                                          prop.style
                                                                              [
                                                                                  style.width 400
                                                                              ]
                                                                          prop.children
                                                                              [
                                                                                  yield! group
                                                                                         |> List.map (fun groupTask ->
                                                                                             TaskNameComponent.render
                                                                                                 {|
                                                                                                     Css =
                                                                                                         [
                                                                                                             style.paddingLeft
                                                                                                                 (groupIndentationLength
                                                                                                                  * 2)
                                                                                                         ]
                                                                                                     TaskId =
                                                                                                         groupTask.Id
                                                                                                 |})
                                                                              ]
                                                                      ]
                                                                  ])
                                                   ]
                                           ])
                            ]
                    ]
                    // Column: Grid
                    Html.div [
                        GridHeaderComponent.render ()
                        Html.div
                            [
                                yield! groups
                                       |> List.map (fun (_, taskGroups) ->
                                           Html.div [
                                               Html.div
                                                   [
                                                       prop.className Css.cellRectangle
                                                   ]
                                               Html.div
                                                   [
                                                       yield! taskGroups
                                                              |> List.map (fun (_, groupTask) ->
                                                                  Html.div [
                                                                      Html.div
                                                                          [
                                                                              prop.className Css.cellRectangle
                                                                          ]
                                                                      CellsComponent.render
                                                                          {|
                                                                              TaskIdList =
                                                                                  groupTask |> List.map (fun x -> x.Id)
                                                                          |}
                                                                  ])
                                                   ]
                                           ])
                            ]
                    ]
                ]
            ])
