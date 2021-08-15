namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open FsUi.State
open FsStore
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State


module rec User =
    let rec userState =
        Store.readSelector
            Fluke.root
            (nameof userState)
            (fun getter ->
                {
                    Archive = Store.value getter Atoms.User.archive
                    AccordionHiddenFlagMap =
                        Reflection.unionCases<AccordionType>
                        |> List.map
                            (fun accordionType ->
                                accordionType, Store.value getter (Atoms.User.accordionHiddenFlag accordionType))
                        |> Map.ofList
                    CellColorDisabled = Store.value getter Atoms.User.cellColorDisabled
                    CellColorSuggested = Store.value getter Atoms.User.cellColorSuggested
                    CellColorPending = Store.value getter Atoms.User.cellColorPending
                    CellColorMissed = Store.value getter Atoms.User.cellColorMissed
                    CellColorMissedToday = Store.value getter Atoms.User.cellColorMissedToday
                    CellColorPostponedUntil = Store.value getter Atoms.User.cellColorPostponedUntil
                    CellColorPostponed = Store.value getter Atoms.User.cellColorPostponed
                    CellColorCompleted = Store.value getter Atoms.User.cellColorCompleted
                    CellColorDismissed = Store.value getter Atoms.User.cellColorDismissed
                    CellColorScheduled = Store.value getter Atoms.User.cellColorScheduled
                    CellSize = Store.value getter Atoms.User.cellSize
                    ClipboardAttachmentIdMap = Store.value getter Atoms.User.clipboardAttachmentIdMap
                    ClipboardVisible = Store.value getter Atoms.User.clipboardVisible
                    DaysAfter = Store.value getter Atoms.User.daysAfter
                    DaysBefore = Store.value getter Atoms.User.daysBefore
                    DayStart = Store.value getter Atoms.User.dayStart
                    EnableCellPopover = Store.value getter Atoms.User.enableCellPopover
                    ExpandedDatabaseIdSet = Store.value getter Atoms.User.expandedDatabaseIdSet
                    Filter = Store.value getter Atoms.User.filter
                    HideSchedulingOverlay = Store.value getter Atoms.User.hideSchedulingOverlay
                    HideTemplates = Store.value getter Atoms.User.hideTemplates
                    Language = Store.value getter Atoms.User.language
                    LastDatabaseSelected = Store.value getter Atoms.User.lastDatabaseSelected
                    LeftDock = Store.value getter Atoms.User.leftDock
                    LeftDockSize = Store.value getter Atoms.User.leftDockSize
                    RandomizeProject = Store.value getter Atoms.User.randomizeProject
                    RandomizeProjectAttachment = Store.value getter Atoms.User.randomizeProjectAttachment
                    RandomizeArea = Store.value getter Atoms.User.randomizeArea
                    RandomizeAreaAttachment = Store.value getter Atoms.User.randomizeAreaAttachment
                    RandomizeResource = Store.value getter Atoms.User.randomizeResource
                    RandomizeResourceAttachment = Store.value getter Atoms.User.randomizeResourceAttachment
                    RandomizeProjectTask = Store.value getter Atoms.User.randomizeProjectTask
                    RandomizeAreaTask = Store.value getter Atoms.User.randomizeAreaTask
                    RandomizeProjectTaskAttachment = Store.value getter Atoms.User.randomizeProjectTaskAttachment
                    RandomizeAreaTaskAttachment = Store.value getter Atoms.User.randomizeAreaTaskAttachment
                    RandomizeCellAttachment = Store.value getter Atoms.User.randomizeCellAttachment
                    RightDock = Store.value getter Atoms.User.rightDock
                    RightDockSize = Store.value getter Atoms.User.rightDockSize
                    SearchText = Store.value getter Atoms.User.searchText
                    SelectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet
                    SessionBreakDuration = Store.value getter Atoms.User.sessionBreakDuration
                    SessionDuration = Store.value getter Atoms.User.sessionDuration
                    UIFlagMap =
                        Reflection.unionCases<UIFlagType>
                        |> List.map (fun uiFlagType -> uiFlagType, Store.value getter (Atoms.User.uiFlag uiFlagType))
                        |> Map.ofList
                    UIVisibleFlagMap =
                        Reflection.unionCases<UIFlagType>
                        |> List.map
                            (fun uiFlagType -> uiFlagType, Store.value getter (Atoms.User.uiVisibleFlag uiFlagType))
                        |> Map.ofList
                    UserColor = Store.value getter Atoms.User.userColor
                    View = Store.value getter Atoms.User.view
                    WeekStart = Store.value getter Atoms.User.weekStart
                })

    let rec theme =
        Store.readSelector
            Fluke.root
            (nameof theme)
            (fun getter ->
                let darkMode = Store.value getter Atoms.Ui.darkMode
                let fontSize = Store.value getter Atoms.Ui.fontSize
                let systemUiFont = Store.value getter Atoms.Ui.systemUiFont

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
                                                borderLeft = "3px solid transparent"
                                            |}
                                        ``*::-webkit-scrollbar-thumb:hover`` =
                                            {|
                                                background = "gray.77"
                                                backgroundClip = "content-box"
                                                borderLeft = "3px solid transparent"
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
