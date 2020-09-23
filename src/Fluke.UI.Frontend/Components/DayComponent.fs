namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
open Fluke.Shared

module DayComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username
                                   Date: FlukeDate
                                   Label: string |}) ->
            let isToday = Recoil.useValue (Recoil.Selectors.RecoilFlukeDate.isTodayFamily input.Date)
            let hasSelection = Recoil.useValue (Recoil.Selectors.RecoilFlukeDate.hasSelectionFamily input.Date)
            let weekStart = Recoil.useValue (Recoil.Atoms.RecoilUser.weekStartFamily input.Username)

            Html.span [
                prop.classes [
                    Css.cellSquare

                    if isToday then
                        Css.todayHeader

                    if hasSelection then
                        Css.selectionHighlight

                    match (weekStart, input.Date) with
                    | StartOfMonth -> Css.cellStartMonth
                    | StartOfWeek -> Css.cellStartWeek
                    | _ -> ()
                ]
                prop.children
                    [
                        str <| String.toLower input.Label
                    ]
            ])
