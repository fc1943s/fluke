namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fable.DateFunctions
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module WeekViewComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let weekCellsMap = Recoil.useValue (Recoil.Selectors.Session.weekCellsMap input.Username)

            Chakra.flex
                {|
                    className = "lanes-panel"
                    width = "300%"
                    display = "block"
                |}
                [
                    yield! weekCellsMap
                           |> List.map (fun week ->
                               Chakra.flex
                                   {|
                                       marginTop = "15px"
                                       marginBottom = "15px"
                                       width = "100%"
                                   |}
                                   [
                                       yield! week
                                              |> Map.keys
                                              |> Seq.map (fun ((DateId referenceDay) as dateId) ->
                                                  let cells = week.[dateId]

                                                  Chakra.box
                                                      {|
                                                          paddingLeft = "10px"
                                                          paddingRight = "10px"
                                                          width = "100%"
                                                      |}
                                                      [
                                                          Chakra.box
                                                              {|
                                                                  marginBottom = "3px"
                                                                  borderBottom = "1px solid #333"
                                                                  fontSize = "14px"
                                                                  color =
                                                                      if cells |> List.forall (fun x -> x.IsToday) then
                                                                          "#777"
                                                                      else
                                                                          ""
                                                              |}
                                                              [
                                                                  referenceDay.DateTime.Format "EEEE, dd MMM yyyy"
                                                                  |> String.toLower
                                                                  |> str
                                                              ]


                                                          yield! cells
                                                                 |> List.map (fun cell ->
                                                                     Chakra.flex
                                                                         ()
                                                                         [
                                                                             CellComponent.render
                                                                                 {|
                                                                                     Username = input.Username
                                                                                     DateId = dateId
                                                                                     TaskId = cell.TaskId
                                                                                     SemiTransparent = false
                                                                                 |}
                                                                             Chakra.box
                                                                                 {| paddingLeft = "4px" |}
                                                                                 [
                                                                                     TaskNameComponent.render
                                                                                         {|
                                                                                             TaskId = cell.TaskId
                                                                                             Props =
                                                                                                 {| paddingLeft = "0" |}
                                                                                         |}
                                                                                 ]
                                                                         ])
                                                      ])
                                   ])
                ])
