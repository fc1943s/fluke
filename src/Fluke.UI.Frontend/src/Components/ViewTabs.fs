namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module ViewTabs =
    [<ReactComponent>]
    let MenuOptionGroup (atom: Store.Atom<_>) label =
        let value, setValue = Store.useState atom

        let key = atom.toString ()

        Chakra.menuOptionGroup
            (fun x ->
                x.``type`` <- "checkbox"

                x.value <-
                    [|
                        if value then yield key
                    |]

                x.onChange <- fun (checks: string []) -> promise { setValue (checks |> Array.contains key) })
            [
                Chakra.menuItemOption
                    (fun x ->
                        x.value <- key
                        x.marginTop <- "2px"
                        x.marginBottom <- "2px")
                    [
                        str label
                    ]
            ]

    [<ReactComponent>]
    let ViewTabs () =
        let showViewOptions = Store.useValue Atoms.showViewOptions
        let view, setView = Store.useState Atoms.view
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

        Chakra.tabs
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
                Chakra.flex
                    (fun x ->
                        x.margin <- "1px"
                        x.overflowX <- "auto")
                    [
                        Chakra.tabList
                            (fun x ->
                                x.flex <- "1 1 auto"
                                x.display <- "flex"
                                x.borderColor <- "transparent"
                                x.marginBottom <- "5px"
                                x.padding <- "1px"
                                x.borderBottomWidth <- "1px"
                                x.borderBottomColor <- "gray.16")
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

                                Chakra.spacer (fun _ -> ()) []

                                Menu.Menu
                                    {|
                                        Tooltip = ""
                                        Trigger =
                                            TransparentIconButton.TransparentIconButton
                                                {|
                                                    Props =
                                                        fun x ->
                                                            x.``as`` <- Chakra.react.MenuButton
                                                            x.fontSize <- "14px"
                                                            x.icon <- Icons.bs.BsThreeDotsVertical |> Icons.render
                                                            x.alignSelf <- "center"
                                                |}
                                        Body =
                                            [
                                                MenuOptionGroup Atoms.hideSchedulingOverlay "Hide Scheduling Overlay"
                                                MenuOptionGroup Atoms.showViewOptions "Show View Options"
                                            ]
                                        MenuListProps = fun _ -> ()
                                    |}
                            ]
                    ]

                if showViewOptions then
                    Chakra.stack
                        (fun x -> x.padding <- "4px")
                        [
                            Chakra.box
                                (fun x -> x.marginLeft <- "2px")
                                [
                                    CheckboxInput.CheckboxInput
                                        {|
                                            Atom = Atoms.filterTasksByView
                                            Label = Some "Filter tasks by view"
                                            Props = (fun _ -> ())
                                        |}
                                ]

                            Input.LeftIconInput
                                {|
                                    Icon = Icons.bs.BsSearch |> Icons.render
                                    CustomProps =
                                        fun x ->
                                            x.atom <- Some (Store.InputAtom (Store.AtomReference.Atom Atoms.searchText))
                                    Props =
                                        fun x ->
                                            x.placeholder <- "Search task or information"
                                            x.isReadOnly <- true
                                |}
                        ]
                else
                    nothing

                Chakra.tabPanels
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
                                    Chakra.tabPanel
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
                                                    Chakra.box
                                                        (fun x ->
                                                            x.padding <- "7px"
                                                            x.whiteSpace <- "nowrap")
                                                        [
                                                            str "No tasks found. Add tasks in the Databases panel."
                                                        ]
                                                else
                                                    LoadingSpinner.InlineLoadingSpinner ()
                                            | _ -> tab.Content
                                        ])
                    ]
            ]
