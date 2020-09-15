namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.DateFunctions
open Suigetsu.Core
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared.Model



module GridHeaderComponent =
    let render =
        React.memo (fun () ->
            let cellSize = Recoil.useValue Recoil.Atoms.cellSize
            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence

            let datesByMonth =
                dateSequence
                |> List.groupBy (fun date -> date.Month)
                |> List.map snd

            Html.div [
                Html.div [
                    prop.style
                        [
                            style.display.flex
                        ]
                    prop.children
                        [
                            yield! datesByMonth
                                   |> List.map (fun dates ->
                                       let firstDate =
                                           dates
                                           |> List.tryHead
                                           |> Option.defaultValue FlukeDate.MinValue

                                       let cellWidth = cellSize * dates.Length


                                       MonthResponsiveCellComponent.render
                                           {|
                                               Date = firstDate
                                               Css =
                                                   [
                                                       style.width cellWidth
                                                   ]
                                           |})
                        ]
                ]

                // Day of Week row
                Html.div [
                    prop.style
                        [
                            style.display.flex
                        ]
                    prop.children
                        [
                            yield! dateSequence
                                   |> List.map (fun date ->
                                       DayComponent.render
                                           {|
                                               Date = date
                                               Label = date.DateTime.Format "EEEEEE"
                                           |})
                        ]
                ]

                // Day row
                Html.div [
                    prop.style
                        [
                            style.display.flex
                        ]
                    prop.children
                        [
                            yield! dateSequence
                                   |> List.map (fun ({ Day = Day day } as date) ->
                                       DayComponent.render {| Date = date; Label = day.ToString "D2" |})
                        ]
                ]
            ])
