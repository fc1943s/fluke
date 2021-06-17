namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module ViewTabs =
    let menuOptionGroup atom label =
        let value, setValue = Store.useState atom

        Chakra.menuOptionGroup
            (fun x ->
                x.``type`` <- "checkbox"

                x.value <-
                    [|
                        if value then yield atom.key
                    |]

                x.onChange <- fun (checks: string []) -> promise { setValue (checks |> Array.contains atom.key) })
            [
                Chakra.menuItemOption
                    (fun x ->
                        x.value <- atom.key
                        x.marginTop <- "2px"
                        x.marginBottom <- "2px")
                    [
                        str label
                    ]
            ]


    [<ReactComponent>]
    let ViewTabs (input: {| Username: Username |}) =
        let showViewOptions = Store.useValue (Atoms.User.showViewOptions input.Username)
        let view, setView = Store.useState (Atoms.User.view input.Username)
        let sortedTaskIdList = Store.useValue (Selectors.Session.sortedTaskIdList input.Username)

        let tabs, tabIndex =
            React.useMemo (
                (fun () ->

                    let tabs =
                        [
                            {|
                                View = View.View.Information
                                Name = "Information View"
                                Icon = Icons.ti.TiFlowChildren
                                Content = fun () -> InformationView.InformationView {| Username = input.Username |}
                            |}
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
                        ]

                    let tabIndex =
                        tabs
                        |> List.tryFindIndex (fun tab -> tab.View = view)

                    tabs, tabIndex),
                [|
                    box view
                    box input.Username
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
                                                menuOptionGroup
                                                    (Atoms.User.hideSchedulingOverlay input.Username)
                                                    "Hide Scheduling Overlay"
                                                menuOptionGroup
                                                    (Atoms.User.showViewOptions input.Username)
                                                    "Show View Options"
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
                                            Atom = Atoms.User.filterTasksByView input.Username
                                            Label = Some "Filter tasks by view"
                                            Props = (fun _ -> ())
                                        |}
                                ]
                            Input.LeftIconInput
                                (Icons.bs.BsSearch |> Icons.render)
                                "Search task or information"
                                (fun x ->
                                    x.atom <- Some (Recoil.Atom (input.Username, Atoms.User.searchText input.Username)))
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
                                                Chakra.box
                                                    (fun x ->
                                                        x.padding <- "7px"
                                                        x.whiteSpace <- "nowrap")
                                                    [
                                                        str "No tasks found. Add tasks in the Databases panel."
                                                    ]
                                            | _ -> tab.Content ()
                                        ])
                    ]
            ]
