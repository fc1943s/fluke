namespace Fluke.UI.Frontend

open System
open Fluke.Shared

module Functions =
    let getDateRange (cellDates: DateTime list) =
        let minDate =
            cellDates
            |> List.min
            |> fun x -> x.AddDays -10.
            
        let maxDate =
            cellDates
            |> List.max
            |> fun x -> x.AddDays 10.
            
        let rec loop date = seq {
            if date < maxDate then
                yield date
                yield! loop (date.AddDays 1.)
        }
        
        minDate
        |> loop
        |> Seq.toList
        
    let isToday (date: DateTime) =
        date.Date = DateTime.Now.Date.AddDays (if DateTime.Now.Hour < Model.hourOffset then -1. else 0.)
