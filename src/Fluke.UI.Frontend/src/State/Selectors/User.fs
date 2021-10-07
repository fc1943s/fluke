namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open FsStore.Model
open FsUi.State
open FsStore

open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State


module User =
    let rec userState =
        Atom.readSelector
            (StoreAtomPath.ValueAtomPath (Fluke.root, Atoms.User.collection, [], AtomName (nameof userState)))
            (fun getter ->
                {
                    Archive = Atom.get getter Atoms.User.archive
                    AccordionHiddenFlagMap =
                        Reflection.unionCases<AccordionType>
                        |> List.map
                            (fun accordionType ->
                                accordionType,
                                Atom.get getter (Atoms.User.accordionHiddenFlag accordionType)
                                |> List.toArray)
                        |> Map.ofList
                    CellColorDisabled = Atom.get getter Atoms.User.cellColorDisabled
                    CellColorSuggested = Atom.get getter Atoms.User.cellColorSuggested
                    CellColorPending = Atom.get getter Atoms.User.cellColorPending
                    CellColorMissed = Atom.get getter Atoms.User.cellColorMissed
                    CellColorMissedToday = Atom.get getter Atoms.User.cellColorMissedToday
                    CellColorPostponedUntil = Atom.get getter Atoms.User.cellColorPostponedUntil
                    CellColorPostponed = Atom.get getter Atoms.User.cellColorPostponed
                    CellColorCompleted = Atom.get getter Atoms.User.cellColorCompleted
                    CellColorDismissed = Atom.get getter Atoms.User.cellColorDismissed
                    CellColorScheduled = Atom.get getter Atoms.User.cellColorScheduled
                    CellHeight = Atom.get getter Atoms.User.cellHeight
                    CellWidth = Atom.get getter Atoms.User.cellWidth
                    ClipboardAttachmentIdMap = Atom.get getter Atoms.User.clipboardAttachmentIdMap
                    ClipboardVisible = Atom.get getter Atoms.User.clipboardVisible
                    DaysAfter = Atom.get getter Atoms.User.daysAfter
                    DaysBefore = Atom.get getter Atoms.User.daysBefore
                    DayStart = Atom.get getter Atoms.User.dayStart
                    EnableCellPopover = Atom.get getter Atoms.User.enableCellPopover
                    ExpandedDatabaseIdSet = Atom.get getter Atoms.User.expandedDatabaseIdSet
                    Filter = Atom.get getter Atoms.User.filter
                    HideSchedulingOverlay = Atom.get getter Atoms.User.hideSchedulingOverlay
                    HideTemplates = Atom.get getter Atoms.User.hideTemplates
                    Language = Atom.get getter Atoms.User.language
                    LastDatabaseSelected = Atom.get getter Atoms.User.lastDatabaseSelected
                    LeftDock = Atom.get getter Atoms.User.leftDock
                    LeftDockSize = Atom.get getter Atoms.User.leftDockSize
                    RandomizeProject = Atom.get getter Atoms.User.randomizeProject
                    RandomizeProjectAttachment = Atom.get getter Atoms.User.randomizeProjectAttachment
                    RandomizeArea = Atom.get getter Atoms.User.randomizeArea
                    RandomizeAreaAttachment = Atom.get getter Atoms.User.randomizeAreaAttachment
                    RandomizeResource = Atom.get getter Atoms.User.randomizeResource
                    RandomizeResourceAttachment = Atom.get getter Atoms.User.randomizeResourceAttachment
                    RandomizeProjectTask = Atom.get getter Atoms.User.randomizeProjectTask
                    RandomizeAreaTask = Atom.get getter Atoms.User.randomizeAreaTask
                    RandomizeProjectTaskAttachment = Atom.get getter Atoms.User.randomizeProjectTaskAttachment
                    RandomizeAreaTaskAttachment = Atom.get getter Atoms.User.randomizeAreaTaskAttachment
                    RandomizeCellAttachment = Atom.get getter Atoms.User.randomizeCellAttachment
                    RightDock = Atom.get getter Atoms.User.rightDock
                    RightDockSize = Atom.get getter Atoms.User.rightDockSize
                    SearchText = Atom.get getter Atoms.User.searchText
                    SelectedDatabaseIdSet = Atom.get getter Atoms.User.selectedDatabaseIdSet
                    SessionBreakDuration = Atom.get getter Atoms.User.sessionBreakDuration
                    SessionDuration = Atom.get getter Atoms.User.sessionDuration
                    UIFlagMap =
                        Reflection.unionCases<UIFlagType>
                        |> List.map (fun uiFlagType -> uiFlagType, Atom.get getter (Atoms.User.uiFlag uiFlagType))
                        |> Map.ofList
                    UIVisibleFlagMap =
                        Reflection.unionCases<UIFlagType>
                        |> List.map
                            (fun uiFlagType -> uiFlagType, Atom.get getter (Atoms.User.uiVisibleFlag uiFlagType))
                        |> Map.ofList
                    UserColor = Atom.get getter Atoms.User.userColor
                    View = Atom.get getter Atoms.User.view
                    WeekStart = Atom.get getter Atoms.User.weekStart
                })

    let rec theme =
        Atom.readSelector
            (StoreAtomPath.ValueAtomPath (Fluke.root, Atoms.User.collection, [], AtomName (nameof theme)))
            (fun getter ->
                let darkMode = Atom.get getter Atoms.Ui.darkMode
                let fontSize = Atom.get getter Atoms.Ui.fontSize
                let systemUiFont = Atom.get getter Atoms.Ui.systemUiFont

                let alphaColors dark =
                    let n = if dark then "0" else "255"

                    {|
                        ``50`` = $"rgba({n}, {n}, {n}, 0.04)"
                        ``100`` = $"rgba({n}, {n}, {n}, 0.06)"
                        ``200`` = $"rgba({n}, {n}, {n}, 0.08)"
                        ``300`` = $"rgba({n}, {n}, {n}, 0.16)"
                        ``400`` = $"rgba({n}, {n}, {n}, 0.24)"
                        ``500`` = $"rgba({n}, {n}, {n}, 0.36)"
                        ``600`` = $"rgba({n}, {n}, {n}, 0.48)"
                        ``700`` = $"rgba({n}, {n}, {n}, 0.64)"
                        ``800`` = $"rgba({n}, {n}, {n}, 0.80)"
                        ``900`` = $"rgba({n}, {n}, {n}, 0.92)"
                    |}


                // https://htmlcolorcodes.com/
                let colors =
                    {|
                        heliotrope = "#b586ff"
                        gray =
                            {|
                                ``10`` = if darkMode then "#1a1a1a" else "#e5e5e5" // grayDark
                                ``13`` = if darkMode then "#212121" else "#dedede" // grayLight
                                ``16`` = if darkMode then "#292929" else "#d6d6d6" // grayLighter
                                ``30`` = if darkMode then "#4D4D4D" else "#B3B3B3" // ??
                                ``45`` = if darkMode then "#727272" else "#8d8d8d" // textDark
                                ``77`` = if darkMode then "#b0bec5" else "#4f413a" // textLight
                                ``87`` = if darkMode then "#dddddd" else "#222222" // text
                            |}
                        whiteAlpha = alphaColors (not darkMode)
                        blackAlpha = alphaColors darkMode
                        _orange = if darkMode then "#ffb836" else "#AF750B"
                        _green = if darkMode then "#a4ff8d" else "#269309" //https://paletton.com/#uid=12K0u0kt+lZlOstrKqzzSiaJidt
                    |}

                let focusShadow = $"0 0 0 1px {colors.heliotrope} !important"

                {|
                    config =
                        {|
                            initialColorMode = if darkMode then "dark" else "light"
                            useSystemColorMode = false
                        |}
                    breakpoints =
                        {|
                            sm = "350px"
                            md = "750px"
                            lg = "1000px"
                            xl = "1900px"
                        |}
                    colors = colors
                    fonts =
                        {|
                            main = if systemUiFont then "system-ui" else "'Roboto Condensed', sans-serif"
                        |}
                    fontWeights =
                        {|
                            hairline = 100
                            thin = 200
                            light = 300
                            normal = 400
                            medium = 500
                            semibold = 600
                            bold = 700
                            extrabold = 800
                            black = 900
                        |}
                    fontSizes = {| main = $"{fontSize}px" |}
                    lineHeights = {| main = $"{fontSize}px" |}
                    styles =
                        {|
                            ``global`` =
                                fun (_props: {| _colorMode: string |}) ->
                                    {|
                                        ``:root`` =
                                            {|
                                                ``--chakra-shadows-outline`` = focusShadow
                                            |}
                                        ``*, *::before, *::after`` = {| wordWrap = "break-word" |}
                                        html =
                                            {|
                                                fontSize = "main"
                                                overflow = "hidden"
                                            |}
                                        body =
                                            {|
                                                fontFamily = "main"
                                                backgroundColor = "gray.13"
                                                fontWeight = "light"
                                                letterSpacing = 0
                                                lineHeight = "main"
                                                fontFeatureSettings = "pnum"
                                                fontVariantNumeric = "proportional-nums"
                                                margin = 0
                                                padding = 0
                                                boxSizing = "border-box"
                                                fontSize = "main"
                                                color = "gray.87"
                                                userSelect = "none"
                                                touchAction = "pan-x pan-y"
                                                overflow = "hidden"
                                            |}
                                        ``input::-ms-reveal`` =
                                            {|
                                                filter = if darkMode then "invert(1)" else ""
                                            |}
                                        ``input::-ms-clear`` =
                                            {|
                                                filter = if darkMode then "invert(1)" else ""
                                            |}
                                        ``*::-webkit-calendar-picker-indicator`` =
                                            {|
                                                filter = if darkMode then "invert(1)" else ""
                                            |}
                                        ``*::-webkit-scrollbar`` = {| width = "9px" |}
                                        ``*::-webkit-scrollbar:horizontal`` = {| height = "6px" |}
                                        ``*::-webkit-scrollbar-track`` = {| display = "none" |}
                                        ``*::-webkit-scrollbar-corner`` = {| display = "none" |}
                                        ``*::-webkit-scrollbar-thumb`` =
                                            {|
                                                background = "gray.45"
                                                opacity = 0.8
                                                backgroundClip = "content-box"
                                                borderLeft = "2px solid transparent"
                                            |}
                                        ``*::-webkit-scrollbar-thumb:hover`` =
                                            {|
                                                background = "gray.77"
                                                backgroundClip = "content-box"
                                                borderLeft = "2px solid transparent"
                                            |}
                                        ``#root`` = {| display = "flex" |}
                                        ``[data-popper-placement][style*="visibility: visible"]`` = {| zIndex = 3 |}
                                        ``.rct-collapse-btn`` =
                                            {|
                                                padding = "0"
                                                marginLeft = "5px"
                                                marginRight = "15px"
                                            |}
                                        ``.rct-collapse-btn:focus`` = {| boxShadow = focusShadow |}
                                        ``.rct-disabled .rct-checkbox svg`` = {| visibility = "hidden" |}
                                        ``.rct-node label:hover, .rct-node label:active`` = {| background = "none" |}
                                        ``.rct-node-parent:not(:first-of-type)`` = {| marginTop = "7px" |}
                                        ``.rct-node:first-of-type`` = {| marginTop = "2px" |}
                                        ``.rct-node-leaf`` = {| marginBottom = "-11px" |}
                                        ``.rct-title`` = {| display = "contents" |}
                                        ``.sketch-picker`` =
                                            {|
                                                backgroundColor =
                                                    if darkMode then "#333 !important" else "#CCC !important"
                                            |}
                                        ``.sketch-picker span`` =
                                            {|
                                                color = if darkMode then "#DDD !important" else "#222 !important"
                                            |}
                                        ``.sketch-picker input`` =
                                            {|
                                                color = if darkMode then "#333 !important" else "#CCC !important"
                                            |}
                                        ``.markdown-container a`` = {| textDecoration = "underline" |}
                                        ``.markdown-container blockquote`` =
                                            {|
                                                borderLeft = "1px solid #888"
                                                paddingLeft = "6px"
                                                marginTop = "6px"
                                                marginBottom = "6px"
                                            |}
                                        ``.markdown-container h1, .markdown-container h2, .markdown-container h3, .markdown-container h4, .markdown-container h5, .markdown-container h6`` =
                                            {|
                                                display = "inline-flex"
                                                borderBottomColor = "#999"
                                                borderBottomWidth = "1px"
                                                marginTop = "3px"
                                                marginBottom = "7px"
                                                paddingBottom = "7px"
                                            |}
                                        ``.markdown-container h4`` = {| fontSize = "1.1rem" |}
                                        ``.markdown-container h3`` = {| fontSize = "1.2rem" |}
                                        ``.markdown-container h2`` = {| fontSize = "1.3rem" |}
                                        ``.markdown-container h1`` = {| fontSize = "1.4rem" |}
                                        ``.markdown-container hr`` =
                                            {|
                                                borderColor = "#888"
                                                marginTop = "6px"
                                                marginBottom = "6px"
                                            |}
                                        ``.markdown-container p`` =
                                            {|
                                                marginTop = "6px"
                                                marginBottom = "6px"
                                            |}
                                        ``.markdown-container pre`` =
                                            {|
                                                fontSize = "0.75em"
                                                lineHeight = "1.2em"
                                            |}
                                        ``.markdown-container li ul`` = {| marginLeft = "17px" |}
                                        ``.markdown-container li + li`` = {| marginTop = "8px" |}
                                        ``.markdown-container td, .markdown-container th`` =
                                            {|
                                                border = "1px solid #888"
                                                padding = "6px"
                                            |}
                                        ``.markdown-container ul + p`` = {| marginTop = "20px" |}
                                        ``.markdown-container ul`` = {| listStylePosition = "inside" |}
                                    |}
                        |}
                |})
