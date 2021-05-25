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
    let HomeScreen (input: {| Username: Username |}) =
        let view, setView = Recoil.useState (Atoms.User.view input.Username)

        let hideSchedulingOverlay, setHideSchedulingOverlay =
            Recoil.useState (Atoms.User.hideSchedulingOverlay input.Username)

        let filteredTaskIdList = Recoil.useValue (Selectors.Session.filteredTaskIdList input.Username)

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
            |> List.findIndex (fun tab -> tab.View = view)

        printfn $"HomeScreen.render. current view: {view}. tabIndex: {tabIndex}"

        let handleTabsChange (e: Browser.Types.KeyboardEvent) =
            promise {
                let index = e |> box |> unbox
                setView tabs.[index].View
            }

        Chakra.flex
            (fun x -> x.flex <- "1")
            [
                LeftDock.LeftDock {| Username = input.Username |}

                Chakra.tabs
                    (fun x ->
                        x.isLazy <- true
                        x.index <- tabIndex
                        x.onChange <- handleTabsChange

                        x.marginLeft <- "4px"
                        x.marginRight <- "4px"
                        x.flexDirection <- "column"
                        x.display <- "flex"
                        x.flex <- "1"
                        x.overflow <- "auto")
                    [
                        Chakra.flex
                            (fun x -> x.margin <- "1px")
                            [
                                Chakra.tabList
                                    (fun x ->
                                        x.flex <- "1 1 auto"
                                        x.display <- "flex"
                                        x.borderColor <- "transparent"
                                        x.marginBottom <- "5px"
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

                                                                    x.icon <-
                                                                        Icons.bs.BsThreeDotsVertical |> Icons.render

                                                                    x.alignSelf <- "center"
                                                        |}
                                                Menu =
                                                    [
                                                        Chakra.menuOptionGroup
                                                            (fun x ->
                                                                x.``type`` <- "checkbox"

                                                                x.value <-
                                                                    [|
                                                                        if hideSchedulingOverlay then
                                                                            yield
                                                                                nameof Atoms.User.hideSchedulingOverlay
                                                                    |]

                                                                x.onChange <-
                                                                    fun (checks: string []) ->
                                                                        promise {
                                                                            setHideSchedulingOverlay (
                                                                                checks
                                                                                |> Array.contains (
                                                                                    nameof
                                                                                        Atoms.User.hideSchedulingOverlay
                                                                                )
                                                                            )
                                                                        })
                                                            [
                                                                Chakra.menuItemOption
                                                                    (fun x ->
                                                                        x.value <-
                                                                            nameof Atoms.User.hideSchedulingOverlay)
                                                                    [
                                                                        str "Hide Scheduling Overlay"
                                                                    ]
                                                            ]
                                                    ]
                                                MenuListProps = fun _ -> ()
                                            |}
                                    ]
                            ]

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
                                                    if filteredTaskIdList.IsEmpty then
                                                        Chakra.box
                                                            (fun x ->
                                                                x.padding <- "7px"
                                                                x.whiteSpace <- "nowrap")
                                                            [
                                                                str "No tasks found. Add tasks in the Databases panel."
                                                            ]
                                                    else
                                                        tab.Content ()
                                                ])
                            ]
                    ]
            ]
