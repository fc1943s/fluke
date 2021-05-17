namespace Fluke.UI.Frontend.Components

open Fable.DateFunctions
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module GridHeader =

    open Domain.Model
    open Domain.UserInteraction

    [<ReactComponent>]
    let GridHeader (input: {| Username: Username |}) =
        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)
        let dateSequence = Recoil.useValue Selectors.dateSequence

        let datesByMonth =
            dateSequence
            |> List.groupBy (fun date -> date.Month)
            |> List.map snd

        Chakra.box
            (fun _ -> ())
            [
                Chakra.flex
                    (fun _ -> ())
                    [
                        yield!
                            datesByMonth
                            |> List.map
                                (fun dates ->
                                    let firstDate =
                                        dates
                                        |> List.tryHead
                                        |> Option.defaultValue FlukeDate.MinValue

                                    let cellWidth = cellSize * dates.Length


                                    MonthResponsiveCell.MonthResponsiveCell
                                        {|
                                            Username = input.Username
                                            Date = firstDate
                                            Props = (fun x -> x.width <- $"{cellWidth}px")
                                        |})
                    ]


                // Day of Week row
                Chakra.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateSequence
                            |> List.map
                                (fun date ->
                                    Day.Day
                                        {|
                                            Date = date
                                            Label = (date |> FlukeDate.DateTime).Format "EEEEEE"
                                            Username = input.Username
                                        |})
                    ]


                // Day row
                Chakra.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateSequence
                            |> List.map
                                (fun date ->
                                    match date with
                                    | { Day = Day day } as date ->
                                        Day.Day
                                            {|
                                                Date = date
                                                Label = day.ToString "D2"
                                                Username = input.Username
                                            |})
                    ]
            ]
