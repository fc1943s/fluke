namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
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

            Chakra.flex
                {| className = "lanes-panel" |}
                [
                    Chakra.box
                        ()
                        [
                            yield! Chakra.box {| className = Css.cellRectangle |} []
                                   |> List.replicate 3

                            Chakra.box
                                ()
                                [
                                    yield! tasksByInformationKind
                                           |> List.map (fun (informationKindName, groups) ->
                                               Chakra.box
                                                   ()
                                                   [
                                                       Chakra.box
                                                           {| color = "#444" |}
                                                           [
                                                               str informationKindName
                                                           ]
                                                       Chakra.box
                                                           ()
                                                           [
                                                               yield! groups
                                                                      |> List.map (fun (informationId, taskIdList) ->



                                                                          Chakra.box
                                                                              ()
                                                                              [
                                                                                  InformationNameComponent.render
                                                                                      {|
                                                                                          InformationId = informationId
                                                                                      |}

                                                                                  // Task Name
                                                                                  Chakra.box
                                                                                      {| width = "400px" |}
                                                                                      [
                                                                                          yield! taskIdList
                                                                                                 |> List.map (fun taskId ->
                                                                                                     Chakra.flex
                                                                                                         ()
                                                                                                         [
                                                                                                             TaskPriorityComponent.render
                                                                                                                 {|
                                                                                                                     TaskId =
                                                                                                                         taskId
                                                                                                                 |}
                                                                                                             TaskNameComponent.render
                                                                                                                 {|
                                                                                                                     TaskId =
                                                                                                                         taskId
                                                                                                                     Props =
                                                                                                                         {|
                                                                                                                             paddingLeft = sprintf "%dpx" (groupIndentationLength * 2)
                                                                                                                         |}
                                                                                                                 |}
                                                                                                         ])
                                                                                      ]
                                                                              ])
                                                           ]
                                                   ])
                                ]
                        ]
                    // Column: Grid
                    Chakra.box
                        ()
                        [
                            GridHeaderComponent.render {| Username = input.Username |}
                            Chakra.box
                                ()
                                [
                                    yield! tasksByInformationKind
                                           |> List.map (fun (_, groups) ->
                                               Chakra.box
                                                   ()
                                                   [
                                                       Chakra.box
                                                           {| className = Css.cellRectangle |}
                                                           [
                                                               yield! groups
                                                                      |> List.map (fun (_, taskIdList) ->
                                                                          Chakra.box
                                                                              ()
                                                                              [
                                                                                  Chakra.box
                                                                                      {|
                                                                                          className = Css.cellRectangle
                                                                                      |}
                                                                                      []
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
                ])
