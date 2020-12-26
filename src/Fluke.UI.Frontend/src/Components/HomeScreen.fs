namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open FSharpPlus


module HomeScreen =

    open Domain.UserInteraction

    [<ReactComponent>]
    let HomeScreen (username: Username) (props: {| flex: int |}) =
        let view, setView = Recoil.useState Recoil.Atoms.view

        let tabs =
            [
                {|
                    View = View.View.HabitTracker
                    Name = "Habit Tracker View"
                    Icon = Icons.bs.BsGrid
                    Content = fun () -> HabitTrackerView.HabitTrackerView username
                |}
                {|
                    View = View.View.Priority
                    Name = "Priority View"
                    Icon = Icons.fa.FaSortNumericDownAlt
                    Content = fun () -> PriorityView.PriorityView username
                |}
                {|
                    View = View.View.BulletJournal
                    Name = "Bullet Journal View"
                    Icon = Icons.bs.BsListCheck
                    Content = fun () -> BulletJournalView.BulletJournalView username
                |}
                {|
                    View = View.View.Information
                    Name = "Information View"
                    Icon = Icons.ti.TiFlowChildren
                    Content = fun () -> InformationView.InformationView username
                |}
            ]

        let tabIndex =
            tabs
            |> List.findIndex (fun tab -> tab.View = view)

        printfn $"HomeScreen.render. current view: {view}. tabIndex: {tabIndex}"

        let handleTabsChange index = setView (tabs.[index].View)

        Chakra.flex
            props
            [
                LeftDock.LeftDock username

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
                                        yield!
                                            tabs
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
                                                        _selected = {| color = "gray.77%"; borderColor = "gray.77%" |}
                                                    |}
                                                    [
                                                        Chakra.box {| ``as`` = tab.Icon; marginRight = "6px" |} []
                                                        str tab.Name
                                                    ])
                                    ]
                                Chakra.tabPanels
                                    {| flex = 1; overflowY = "auto"; flexBasis = 0 |}
                                    [
                                        yield!
                                            tabs
                                            |> List.map (fun tab ->
                                                Chakra.tabPanel
                                                    {| padding = 0 |}
                                                    [
                                                        tab.Content ()
                                                    ])
                                    ]
                            ]
                    ]
            ]
