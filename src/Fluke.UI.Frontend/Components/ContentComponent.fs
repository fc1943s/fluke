namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Browser
open Feliz.Router
open Browser.Types
open FSharpPlus


module ContentComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username; Props: {| flex: int |} |}) ->
            let view = Recoil.useValue Recoil.Selectors.view

            let setView view =
                let path =
                    Router.formatPath [|
                        "view"
                        string view
                    |]

                Dom.window.location.href <- path

            React.useListener.onKeyDown (fun (e: KeyboardEvent) ->
                match e.ctrlKey, e.shiftKey, e.key with
                | _, true, "H" -> setView View.View.HabitTracker
                | _, true, "P" -> setView View.View.Priority
                | _, true, "B" -> setView View.View.BulletJournal
                | _, true, "I" -> setView View.View.Information
                | _ -> ())


            let tabs =
                [
                    View.View.HabitTracker,
                    "Habit Tracker View",
                    Icons.bs.BsGrid,
                    (fun () -> CalendarViewComponent.render {| Username = input.Username |})

                    View.View.Priority,
                    "Priority View",
                    Icons.fa.FaSortNumericDownAlt,
                    (fun () -> TasksViewComponent.render {| Username = input.Username |})

                    View.View.BulletJournal,
                    "Bullet Journal View",
                    Icons.bs.BsListCheck,
                    (fun () -> WeekViewComponent.render {| Username = input.Username |})

                    View.View.Information,
                    "Information View",
                    Icons.ti.TiFlowChildren,
                    (fun () -> GroupsViewComponent.render {| Username = input.Username |})
                ]

            let tabIndex =
                tabs
                |> List.findIndex (fun (view', _, _, _) -> view = view')

            let handleTabsChange index =
                let view, _, _, _ = tabs.[index]
                setView view

            Chakra.flex
                input.Props
                [
                    LeftDockComponent.render {| Username = input.Username |}

                    Chakra.stack
                        {|
                            spacing = 0
                            flex = 1
                            marginLeft = "10px"
                            marginRight = "10px"
                        |}
                        [
                            Chakra.tabs
                                {|
                                    isLazy = true
                                    index = tabIndex
                                    onChange = handleTabsChange
                                    flexDirection = "column"
                                    display = "flex"
                                    flex = 1
                                |}
                                [
                                    Chakra.tabList
                                        {| borderColor = "transparent" |}
                                        [
                                            yield! tabs
                                                   |> List.map (fun (_, name, icon, _) ->
                                                       Chakra.tab
                                                           {|
                                                               padding = "12px"
                                                               color = "gray.45%"
                                                               _hover =
                                                                   {|
                                                                       borderBottomColor = "gray.45%"
                                                                       borderBottom = "2px solid"
                                                                   |}
                                                               _selected =
                                                                   {| color = "gray.77%"; borderColor = "gray.77%" |}
                                                           |}
                                                           [
                                                               Chakra.box {| ``as`` = icon; marginRight = "6px" |} []
                                                               str name
                                                           ])
                                        ]
                                    Chakra.tabPanels
                                        {|
                                            className = "panels"
                                            flex = 1
                                            overflowY = "auto"
                                            flexBasis = 0
                                        |}
                                        [
                                            yield! tabs
                                                   |> List.map (fun (_, _, _, content) ->
                                                       Chakra.tabPanel
                                                           {| padding = 0 |}
                                                           [
                                                               content ()
                                                           ])
                                        ]
                                ]
                        ]
                ])
