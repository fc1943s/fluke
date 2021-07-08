namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module ViewTabs =

    [<ReactComponent>]
    let ViewTabs () =
        let view, setView = Store.useState Atoms.User.view
        let filteredTaskIdSet = Store.useValue Selectors.Session.filteredTaskIdSet
        let sortedTaskIdList = Store.useValue Selectors.Session.sortedTaskIdList

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
                                                MenuItemToggle.MenuItemToggle
                                                    Atoms.User.hideSchedulingOverlay
                                                    "Hide Scheduling Overlay"
                                            ]
                                        MenuListProps = fun _ -> ()
                                    |}
                            ]
                    ]

                UI.stack
                    (fun x ->
                        x.paddingTop <- "4px"
                        x.paddingRight <- "10px"
                        x.paddingBottom <- "4px"
                        x.paddingLeft <- "10px")
                    [
                        Input.LeftIconInput
                            {|
                                Icon = Icons.bs.BsSearch |> Icons.render
                                CustomProps =
                                    fun x ->
                                        x.atom <-
                                            Some (Store.InputAtom (Store.AtomReference.Atom Atoms.User.filterTasksText))
                                Props =
                                    fun x ->
                                        x.autoFocus <- true
                                        x.placeholder <- "Filter Tasks"
                            |}
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
                                            match sortedTaskIdList with
                                            | [] ->
                                                if filteredTaskIdSet.IsEmpty then
                                                    UI.box
                                                        (fun x ->
                                                            x.padding <- "7px"
                                                            x.whiteSpace <- "nowrap")
                                                        [
                                                            str "No tasks found. Add tasks in the Databases panel."
                                                        ]
                                                else
                                                    UI.box
                                                        (fun x -> x.padding <- "15px")
                                                        [
                                                            LoadingSpinner.InlineLoadingSpinner ()
                                                        ]
                                            | _ -> tab.Content
                                        ])
                    ]
            ]
