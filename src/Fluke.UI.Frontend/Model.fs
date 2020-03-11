namespace Fluke.UI.Frontend

open Fable.React.Props
open Fluke.Shared


module Temp =
    // TODO: Move to Suigetsu
    module Core =
        let rec recFn fn x = fn (recFn fn) x

module Model =
    ()
    
module Functions =
    let getCellSeparatorBorderLeft (date: Model.FlukeDate) =
        if date.Day = 1
        then Some "#000"
        elif date.DateTime.DayOfWeek = System.DayOfWeek.Sunday
        then Some "#ffffff3d"
        else None
        |> Option.map (fun color -> "1px solid " + color)
        |> fun x -> CSSProp.BorderLeft x

          

