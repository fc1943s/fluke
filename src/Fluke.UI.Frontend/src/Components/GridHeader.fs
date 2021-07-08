namespace Fluke.UI.Frontend.Components

open Fable.DateFunctions
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module GridHeader =

    open Domain.Model
    open Domain.UserInteraction

    [<ReactComponent>]
    let GridHeader () =
        let cellSize = Store.useValue Atoms.User.cellSize
        let dateSequence = Store.useValue Selectors.dateSequence

        let datesByMonth =
            dateSequence
            |> List.groupBy (fun date -> date.Month)
            |> List.map snd

        UI.box
            (fun _ -> ())
            [
                UI.flex
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
                                        firstDate
                                        (fun x -> x.width <- $"{cellWidth}px"))
                    ]

                // Day of Week row
                UI.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateSequence
                            |> List.map (fun date -> Day.Day date ((date |> FlukeDate.DateTime).Format "EEEEEE"))
                    ]

                // Day row
                UI.flex
                    (fun _ -> ())
                    [
                        yield!
                            dateSequence
                            |> List.map
                                (fun date ->
                                    match date with
                                    | { Day = Day day } as date -> Day.Day date (day.ToString "D2"))
                    ]
            ]
