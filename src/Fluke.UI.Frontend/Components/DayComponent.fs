namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.Shared.Model
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend


module DayComponent =
    let render =
        React.memo (fun (input: {| Date: FlukeDate
                                   Label: string |}) ->
            let user = Recoil.useValue Recoil.Selectors.user

            let isToday =
                Recoil.useValue (Recoil.Selectors.RecoilFlukeDate.isTodayFamily input.Date)

            let hasSelection =
                Recoil.useValue (Recoil.Selectors.RecoilFlukeDate.hasSelectionFamily input.Date)

            match user with
            | None -> str "No user"
            | Some user ->
                Html.span [
                    prop.classes [
                        Css.cellSquare

                        if isToday then
                            Css.todayHeader

                        if hasSelection then
                            Css.selectionHighlight

                        match (user.WeekStart, input.Date) with
                        | StartOfMonth -> Css.cellStartMonth
                        | StartOfWeek -> Css.cellStartWeek
                        | _ -> ()
                    ]
                    prop.children
                        [
                            str <| String.toLower input.Label
                        ]
                ])
