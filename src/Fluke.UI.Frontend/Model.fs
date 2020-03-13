namespace Fluke.UI.Frontend

open Fable.React.Props
open Fluke.Shared
open Fable.Core
open Fable.React
open System


module Temp =
    // TODO: Move to Suigetsu
    module Core =
        let rec recFn fn x = fn (recFn fn) x
        
    module CustomHooks =
        // TODO: move to Suigetsu
        let useInterval fn interval =
            let savedCallback = Hooks.useRef fn 
            
            Hooks.useEffect (fun () ->
                savedCallback.current <- fn
            , [| fn |])
            
            Hooks.useEffectDisposable (fun () ->
                let id =
                    JS.setInterval (fun () ->
                        savedCallback.current ()
                    ) interval
                
                { new IDisposable with
                    member _.Dispose () =
                        JS.clearInterval id }
            , [| interval |])

module SharedState =
    type SharedClientMessage = unit
    type SharedServerMessage = unit
        
module UIState =
    type State =
        { x: unit }
        static member inline Default = { x = () }

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

          

