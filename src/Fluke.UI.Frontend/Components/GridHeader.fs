namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.DateFunctions
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared

module GridHeader =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let cellSize = Recoil.useValue Recoil.Atoms.cellSize
            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence

            let datesByMonth =
                dateSequence
                |> List.groupBy (fun date -> date.Month)
                |> List.map snd

            Chakra.box
                ()
                [
                    Chakra.flex
                        ()
                        [
                            yield! datesByMonth
                                   |> List.map (fun dates ->
                                       let firstDate =
                                           dates
                                           |> List.tryHead
                                           |> Option.defaultValue FlukeDate.MinValue

                                       let cellWidth = cellSize * dates.Length


                                       MonthResponsiveCell.render
                                           {|
                                               Username = input.Username
                                               Date = firstDate
                                               Props = {| width = cellWidth |}
                                           |})
                        ]


                    // Day of Week row
                    Chakra.flex
                        ()
                        [
                            yield! dateSequence
                                   |> List.map (fun date ->
                                       Day.render
                                           {|
                                               Date = date
                                               Label = date.DateTime.Format "EEEEEE"
                                               Username = input.Username
                                           |})
                        ]


                    // Day row
                    Chakra.flex
                        ()
                        [
                            yield! dateSequence
                                   |> List.map (fun ({ Day = Day day } as date) ->
                                       Day.render
                                           {|
                                               Date = date
                                               Label = day.ToString "D2"
                                               Username = input.Username
                                           |})
                        ]
                ])
