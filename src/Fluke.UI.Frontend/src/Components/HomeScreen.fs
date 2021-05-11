namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module HomeScreen =

    open Domain.UserInteraction

    [<ReactComponent>]
    let HomeScreen
        (input: {| Username: Username
                   Props: Chakra.IChakraProps -> unit |})
        =
        let view, setView = Recoil.useState (Atoms.User.view input.Username)

        let tabs =
            [
                {|
                    View = View.View.HabitTracker
                    Name = "Habit Tracker View"
                    Icon = Icons.bs.BsGrid
                    Content = fun () -> HabitTrackerView.HabitTrackerView {| Username = input.Username |}
                |}
                {|
                    View = View.View.Priority
                    Name = "Priority View"
                    Icon = Icons.fa.FaSortNumericDownAlt
                    Content = fun () -> PriorityView.PriorityView {| Username = input.Username |}
                |}
                {|
                    View = View.View.BulletJournal
                    Name = "Bullet Journal View"
                    Icon = Icons.bs.BsListCheck
                    Content = fun () -> BulletJournalView.BulletJournalView {| Username = input.Username |}
                |}
                {|
                    View = View.View.Information
                    Name = "Information View"
                    Icon = Icons.ti.TiFlowChildren
                    Content = fun () -> InformationView.InformationView {| Username = input.Username |}
                |}
            ]

        let tabIndex =
            tabs
            |> List.findIndex (fun tab -> tab.View = view)

        printfn $"HomeScreen.render. current view: {view}. tabIndex: {tabIndex}"

        let handleTabsChange (e: Browser.Types.KeyboardEvent) =
            promise {
                let index = e |> box |> unbox
                setView tabs.[index].View
            }

        Chakra.flex
            input.Props
            [
                LeftDock.LeftDock {| Username = input.Username |}

                Chakra.stack
                    (fun x ->
                        x.spacing <- "0"
                        x.flex <- 1
                        x.marginLeft <- "10px"
                        x.marginRight <- "10px")
                    [
                        Chakra.tabs
                            (fun x ->
                                x.isLazy <- true
                                x.index <- tabIndex
                                x.onChange <- handleTabsChange
                                x.flexDirection <- "column"
                                x.display <- "flex"
                                x.flex <- 1)
                            [
                                Chakra.tabList
                                    (fun x -> x.borderColor <- "transparent")
                                    [
                                        yield!
                                            tabs
                                            |> List.map
                                                (fun tab ->
                                                    Chakra.tab
                                                        (fun x ->
                                                            x.padding <- "12px"
                                                            x.color <- "gray.45"

                                                            x._hover <-
                                                                JS.newObj
                                                                    (fun x ->
                                                                        x.borderBottomWidth <- "2px"
                                                                        x.borderBottomColor <- "gray.45")

                                                            x._selected <-
                                                                JS.newObj
                                                                    (fun x ->
                                                                        x.color <- "gray.77"
                                                                        x.borderColor <- "gray.77"))
                                                        [
                                                            Chakra.icon
                                                                (fun x ->
                                                                    x.``as`` <- tab.Icon
                                                                    x.marginRight <- "6px")
                                                                []
                                                            str tab.Name
                                                        ])
                                    ]
                                Chakra.tabPanels
                                    (fun x ->
                                        x.flex <- 1
                                        x.overflowY <- "auto"
                                        x.flexBasis <- 0)
                                    [
                                        yield!
                                            tabs
                                            |> List.map
                                                (fun tab ->
                                                    Chakra.tabPanel
                                                        (fun x ->
                                                            x.padding <- "0"
                                                            x.boxShadow <- "none !important")
                                                        [
                                                            tab.Content ()
                                                        ])
                                    ]
                            ]
                    ]
            ]
