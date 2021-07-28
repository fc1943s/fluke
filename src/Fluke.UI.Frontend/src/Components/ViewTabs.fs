namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Components
open FsJs
open FsStore
open FsUi.Bindings
open Fluke.Shared
open FsUi.Components


module ViewTabs =

    [<ReactComponent>]
    let ViewTabs () =
        let view, setView = Store.useState Atoms.User.view
        let filteredTaskIdCount = Store.useValue Selectors.Session.filteredTaskIdCount
        let sortedTaskIdCount = Store.useValue Selectors.Session.sortedTaskIdCount
        let informationSet = Store.useValue Selectors.Session.informationSet

        let tabs, tabIndex =
            React.useMemo (
                (fun () ->

                    let tabs =
                        [
                            {|
                                View = View.View.Information
                                Name = "Information View"
                                Icon = Icons.ti.TiFlowChildren
                                Content = InformationView.InformationView ()
                            |}
                            {|
                                View = View.View.HabitTracker
                                Name = "Habit Tracker View"
                                Icon = Icons.bs.BsGrid
                                Content = HabitTrackerView.HabitTrackerView ()
                            |}
                            {|
                                View = View.View.Priority
                                Name = "Priority View"
                                Icon = Icons.fa.FaSortNumericDownAlt
                                Content = PriorityView.PriorityView ()
                            |}
                            {|
                                View = View.View.BulletJournal
                                Name = "Bullet Journal View"
                                Icon = Icons.bs.BsListCheck
                                Content = BulletJournalView.BulletJournalView ()
                            |}
                        ]

                    let tabIndex =
                        tabs
                        |> List.tryFindIndex (fun tab -> tab.View = view)

                    tabs, tabIndex),
                [|
                    box view
                |]
            )

        let lastTabIndex = React.useRef (tabIndex |> Option.defaultValue 0)


        React.useEffect (
            (fun () ->
                match tabIndex with
                | Some tabIndex -> lastTabIndex.current <- tabIndex
                | None -> ()),
            [|
                box tabIndex
                box lastTabIndex
            |]
        )

        printfn $"ViewTabs.render. current view={view}. tabIndex={tabIndex} lastTabIndex={lastTabIndex.current}"

        UI.tabs
            (fun x ->
                x.isLazy <- true

                x.index <-
                    tabIndex
                    |> Option.defaultValue lastTabIndex.current

                x.onChange <- fun e -> promise { setView tabs.[e].View }
                x.flexDirection <- "column"
                x.display <- "flex"
                x.flex <- "1"
                x.overflow <- "auto")
            [
                UI.flex
                    (fun x ->
                        x.margin <- "1px"
                        x.overflowX <- "auto")
                    [
                        UI.tabList
                            (fun x ->
                                x.flex <- "1 1 auto"
                                x.display <- "flex"
                                x.borderColor <- "transparent"
                                x.marginBottom <- "5px"
                                x.padding <- "1px"
                                x.alignItems <- "center"
                                x.borderBottomWidth <- "1px"
                                x.borderBottomColor <- "gray.16")
                            [
                                yield!
                                    tabs
                                    |> List.map
                                        (fun tab ->
                                            UI.tab
                                                (fun x ->
                                                    x.alignSelf <- "stretch"
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
                                                    UI.icon
                                                        (fun x ->
                                                            x.``as`` <- tab.Icon
                                                            x.marginRight <- "6px")
                                                        []
                                                    str tab.Name
                                                ])

                                UI.spacer (fun _ -> ()) []

                                Menu.Menu
                                    {|
                                        Tooltip = ""
                                        Trigger =
                                            TransparentIconButton.TransparentIconButton
                                                {|
                                                    Props =
                                                        fun x ->
                                                            x.``as`` <- UI.react.MenuButton
                                                            x.fontSize <- "14px"
                                                            x.icon <- Icons.bs.BsThreeDotsVertical |> Icons.render
                                                            x.alignSelf <- "center"
                                                |}
                                        Body =
                                            [
                                                MenuItemToggle.MenuItemToggleAtom
                                                    Atoms.User.hideSchedulingOverlay
                                                    "Hide Scheduling Overlay"
                                            ]
                                        MenuListProps = fun _ -> ()
                                    |}
                            ]
                    ]

                UI.tabPanels
                    (fun x ->
                        x.flex <- "1"
                        x.display <- "flex"
                        x.overflowX <- "hidden"
                        x.flexBasis <- 0)
                    [
                        yield!
                            tabs
                            |> List.map
                                (fun tab ->
                                    UI.tabPanel
                                        (fun x ->
                                            x.display <- "flex"
                                            x.padding <- "0"
                                            x.boxShadow <- "none !important"
                                            x.flex <- "1"
                                            x.overflow <- "auto")
                                        [
                                            if sortedTaskIdCount = 0
                                               && (view <> View.View.Information
                                                   || informationSet.IsEmpty) then
                                                if filteredTaskIdCount = 0 then
                                                    UI.box
                                                        (fun x -> x.padding <- "7px")
                                                        [
                                                            str "No tasks found. Add tasks in the Databases panel."
                                                        ]
                                                else
                                                    UI.flex
                                                        (fun x -> x.flex <- "1")
                                                        [
                                                            LoadingSpinner.LoadingSpinner ()
                                                        ]
                                            else
                                                tab.Content
                                        ])
                    ]
            ]
