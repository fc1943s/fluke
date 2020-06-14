namespace Fluke.UI.Frontend.Components

open System
open Browser
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Browser.Types
open Browser.Types
open FSharpPlus
open Fable.Core
open Feliz
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.React
open Fable.React.Props
open Fable.DateFunctions
open Fulma
open System
open Fulma.Extensions.Wikiki
open Suigetsu.UI.Frontend.ElmishBridge
open Suigetsu.UI.Frontend.React
open Suigetsu.Core


module Temp =
    module CustomHooks =
        let useWindowSize () =
            let getWindowSize () =
                {| Width = window.innerWidth
                   Height = window.innerHeight |}
            let size, setSize = React.useState (getWindowSize ())

            React.useLayoutEffect (fun () ->
                let updateSize (_event: Event) =
                    setSize (getWindowSize ())

                window.addEventListener ("resize", updateSize)

                { new IDisposable with
                    member _.Dispose () =
                        window.removeEventListener ("resize", updateSize)
                }
            )
            size

module PageLoader =
    let render = React.memo (fun () ->
        PageLoader.pageLoader [ PageLoader.Color IsDark
                                PageLoader.IsActive true ][]
    )

module MainComponent =
    let render = React.memo (fun () ->
        let setNow = Recoil.useSetState Recoil.Atoms.now
//        let windowSize = CustomHooks.useWindowSize ()

        let updateNow () =
            Recoil.Temp.tempState.GetNow
            |> fun x -> x ()
            |> setNow

        updateNow ()
        CustomHooks.useInterval updateNow (60 * 1000)

        React.suspense ([
            HomePageComponent.``default`` ()
        ], PageLoader.render ())
    )

