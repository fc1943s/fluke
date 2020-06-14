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

module NavBar =
    open Model
    let render = React.memo (fun () ->
        let view, setView = Recoil.useState Recoil.Atoms.view
        let activeSessions = Recoil.useValue Recoil.Atoms.activeSessions

        Ext.useEventListener "keydown" (fun (e: KeyboardEvent) ->
            match e.ctrlKey, e.shiftKey, e.key with
            | _, true, "C" -> setView View.Calendar
            | _, true, "G" -> setView View.Groups
            | _, true, "T" -> setView View.Tasks
            | _, true, "W" -> setView View.Week
            | _            -> ()
        )

        Navbar.navbar [ Navbar.Color IsBlack
                        Navbar.Props [ Style [ Height 36
                                               MinHeight 36
                                               Display DisplayOptions.Flex
                                               JustifyContent "space-around" ]]][

            let checkbox newView text =
                Navbar.Item.div [ Navbar.Item.Props [ Class "field"
                                                      OnClick (fun _ -> setView newView)
                                                      Style [ MarginBottom 0
                                                              AlignSelf AlignSelfOptions.Center ] ] ][

                    Checkbox.input [ CustomClass "switch is-small is-dark"
                                     Props [ Checked (view = newView)
                                             OnChange (fun _ -> ()) ]]

                    Checkbox.checkbox [][
                        str text
                    ]
                ]

            checkbox View.Calendar "calendar view"
            checkbox View.Groups "groups view"
            checkbox View.Tasks "tasks view"
            checkbox View.Week "week view"

            Navbar.Item.div [][
                activeSessions
                |> List.map (fun (ActiveSession (task, duration)) ->
                    let sessionType, color, duration, left =
                        let left = TempData.sessionLength - duration
                        match duration < TempData.sessionLength with
                        | true  -> "Session", "#7cca7c", duration, left
                        | false -> "Break",   "#ca7c7c", -left,    TempData.sessionBreakLength + left

                    span [ Style [ Color color ] ][
                        sprintf "%s: Task[ %s ]; Duration[ %.1f ]; Left[ %.1f ]" sessionType task.Name duration left
                        |> str
                    ]
                )
                |> List.intersperse (br [])
                |> function
                    | [] -> str "No active session"
                    | list -> ofList list
            ]

        ]
    )

module MainComponent =
    let render = React.memo (fun () ->
        let setNow = Recoil.useSetState Recoil.Atoms.now
        let selection, setSelection = Recoil.useState Recoil.Atoms.selection
        let ctrlPressed, setCtrlPressed = Recoil.useState Recoil.Atoms.ctrlPressed

        do
            let updateNow () =
                Recoil.Temp.tempState.GetNow
                |> fun x -> x ()
                |> setNow

            updateNow ()
//            CustomHooks.useInterval updateNow (60 * 1000)

        do
            let keyEvent (e: KeyboardEvent) =
                if e.ctrlKey <> ctrlPressed then
                    setCtrlPressed e.ctrlKey

                if e.key = "Escape" && not selection.IsEmpty then
                    setSelection Map.empty

            Ext.useEventListener "keydown" keyEvent
            Ext.useEventListener "keyup" keyEvent

        React.suspense ([
            NavBar.render ()

            HomePageComponent.render ()
        ], PageLoader.render ())
    )

