namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Feliz.Router
open Browser.Types
open FSharpPlus


module ContentComponent =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username; Props: {| flex: int |} |}) ->
            let view, setView = Recoil.useState Recoil.Atoms.view

            let setView view =
                let path =
                    Router.formatPath [|
                        "view"
                        string view
                    |]

                //                Dom.window.location.href <- path
                Router.navigatePath path
                setView view

            React.useListener.onKeyDown (fun (e: KeyboardEvent) ->
                match e.ctrlKey, e.shiftKey, e.key with
                | _, true, "H" -> setView View.View.HabitTracker
                | _, true, "P" -> setView View.View.Priority
                | _, true, "B" -> setView View.View.BulletJournal
                | _, true, "I" -> setView View.View.Information
                | _ -> ())


            let tabs =
                [
                    {|
                        View = View.View.HabitTracker
                        Name = "Habit Tracker View"
                        Icon = Icons.bs.BsGrid
                        Content = fun () -> HabitTrackerViewComponent.render {| Username = input.Username |}
                    |}
                    {|
                        View = View.View.Priority
                        Name = "Priority View"
                        Icon = Icons.fa.FaSortNumericDownAlt
                        Content = fun () -> PriorityViewComponent.render {| Username = input.Username |}
                    |}
                    {|
                        View = View.View.BulletJournal
                        Name = "Bullet Journal View"
                        Icon = Icons.bs.BsListCheck
                        Content = fun () -> BulletJournalViewComponent.render {| Username = input.Username |}
                    |}
                    {|
                        View = View.View.Information
                        Name = "Information View"
                        Icon = Icons.ti.TiFlowChildren
                        Content = fun () -> InformationViewComponent.render {| Username = input.Username |}
                    |}
                ]

            let tabIndex =
                tabs
                |> List.findIndex (fun tab -> tab.View = view)

            let handleTabsChange index = setView (tabs.[index].View)

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
                                                   |> List.map (fun tab ->
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
                                                               Chakra.box
                                                                   {| ``as`` = tab.Icon; marginRight = "6px" |}
                                                                   []
                                                               str tab.Name
                                                           ])
                                        ]
                                    Chakra.tabPanels
                                        {| flex = 1; overflowY = "auto"; flexBasis = 0 |}
                                        [
                                            yield! tabs
                                                   |> List.map (fun tab ->
                                                       Chakra.tabPanel
                                                           {| padding = 0 |}
                                                           [
                                                               tab.Content ()
                                                           ])
                                        ]
                                ]
                        ]
                ])
