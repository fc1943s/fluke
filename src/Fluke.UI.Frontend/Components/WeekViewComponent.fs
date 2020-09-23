namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Suigetsu.Core
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fable.DateFunctions
open Fluke.UI.Frontend
open Fluke.Shared


module WeekViewComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let weekCellsMap = Recoil.useValue Recoil.Selectors.weekCellsMap

            Html.div [
                prop.className Css.lanesPanel
                prop.style [
                    style.custom ("width", "300%")
                    style.display.block
                ]
                prop.children
                    [
                        yield! weekCellsMap
                               |> List.map (fun week ->
                                   Html.div [
                                       prop.style [
                                           style.display.flex
                                           style.marginTop 15
                                           style.marginBottom 15
                                           style.custom ("width", "100%")
                                       ]
                                       prop.children
                                           [
                                               yield! week
                                                      |> Map.keys
                                                      |> Seq.map (fun ((DateId referenceDay) as dateId) ->
                                                          let cells = week.[dateId]

                                                          Html.div [
                                                              prop.style [
                                                                  style.paddingLeft 10
                                                                  style.paddingRight 10
                                                                  style.custom ("width", "100%")
                                                              ]
                                                              prop.children [
                                                                  Html.div [
                                                                      prop.classes
                                                                          [
                                                                              if cells
                                                                                 |> List.forall (fun x -> x.IsToday) then
                                                                                  Css.todayHeader
                                                                          ]
                                                                      prop.style [
                                                                          style.marginBottom 3
                                                                          style.borderBottom
                                                                              (length.px 1, borderStyle.solid, "#333")
                                                                          style.fontSize 14
                                                                      ]
                                                                      prop.children
                                                                          [
                                                                              referenceDay.DateTime.Format
                                                                                  "EEEE, dd MMM yyyy"
                                                                              |> String.toLower
                                                                              |> str
                                                                          ]
                                                                  ]

                                                                  yield! cells
                                                                         |> List.map (fun cell ->
                                                                             Html.div [
                                                                                 prop.style
                                                                                     [
                                                                                         style.display.flex
                                                                                     ]
                                                                                 prop.children [
                                                                                     CellComponent.render
                                                                                         {|
                                                                                             Username = input.Username
                                                                                             Date = referenceDay
                                                                                             TaskId = cell.Task.Id
                                                                                         |}
                                                                                     Html.div [
                                                                                         prop.style
                                                                                             [
                                                                                                 style.paddingLeft 4
                                                                                             ]
                                                                                         prop.children
                                                                                             [
                                                                                                 let (TaskName taskName) =
                                                                                                     cell.Task.Name

                                                                                                 str taskName
                                                                                             ]
                                                                                     ]
                                                                                 ]
                                                                             ])
                                                              ]
                                                          ])
                                           ]
                                   ])
                    ]
            ])
