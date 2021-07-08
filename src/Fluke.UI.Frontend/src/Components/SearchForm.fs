namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module SearchForm =
    [<RequireQualifiedAccess>]
    type SearchResultType =
        | Information of information: string
        | InformationAttachment of information: string
        | Task of task: string
        | TaskAttachment of task: string
        | CellAttachment of task: string * date: string

    [<RequireQualifiedAccess>]
    type SearchResult =
        | Information of information: Information
        | InformationAttachment of information: Information
        | Task of databaseId: DatabaseId * taskId: TaskId
        | TaskAttachment of databaseId: DatabaseId * taskId: TaskId
        | CellAttachment of taskId: TaskId * dateId: DateId


    [<ReactComponent>]
    let SearchResultItem searchResult searchResultText =
        let setTaskUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Task)
        let setInformationUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Information)
        let setCellUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Cell)
        let setRightDock = Store.useSetState Atoms.User.rightDock

        Chakra.box
            (fun x ->
                x.overflow <- "auto"
                x.maxHeight <- "50px"
                x.padding <- "1px")
            [
                str searchResultText

                InputLabelIconButton.InputLabelIconButton
                    (fun x ->
                        x.icon <- Icons.fi.FiArrowRight |> Icons.render

                        x.fontSize <- "11px"
                        x.height <- "15px"
                        x.color <- "whiteAlpha.700"
                        x.marginTop <- "-1px"
                        x.marginLeft <- "6px"

                        x.onClick <-
                            fun _ ->
                                promise {
                                    match searchResult with
                                    | SearchResult.Task (databaseId, taskId) ->
                                        setTaskUIFlag (UIFlag.Task (databaseId, taskId))
                                        setRightDock (Some TempUI.DockType.Task)
                                    | SearchResult.Information information ->
                                        setInformationUIFlag (UIFlag.Information information)
                                        setRightDock (Some TempUI.DockType.Information)
                                    | SearchResult.InformationAttachment information ->
                                        setInformationUIFlag (UIFlag.Information information)
                                        setRightDock (Some TempUI.DockType.Information)
                                    | SearchResult.TaskAttachment (databaseId, taskId) ->
                                        setTaskUIFlag (UIFlag.Task (databaseId, taskId))
                                        setRightDock (Some TempUI.DockType.Task)
                                    | SearchResult.CellAttachment (taskId, dateId) ->
                                        setCellUIFlag (UIFlag.Cell (taskId, dateId))
                                        setRightDock (Some TempUI.DockType.Cell)
                                })
            ]

    [<ReactComponent>]
    let SearchForm () =
        let searchResults, setSearchResults = React.useState []
        let searchText, setSearchText = Store.useState Atoms.User.searchText
        let loading, setLoading = React.useState (searchText.Length > 0)
        let toast = Chakra.useToast ()

        let search =
            Store.useCallback (
                (fun getter _ searchText ->
                    promise {
                        if searchText = "" then
                            setSearchResults []
                        else
                            let searchAttachments attachments =
                                attachments
                                |> List.map snd
                                |> List.choose
                                    (fun attachment ->
                                        match attachment with
                                        | Attachment.Comment (Comment.Comment comment) -> Some comment
                                        | _ -> None)
                                |> List.choose
                                    (fun comment -> if comment.Contains searchText then Some comment else None)

                            let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                            let databaseStateArray =
                                selectedDatabaseIdSet
                                |> Set.toArray
                                |> Array.map Selectors.Database.databaseState
                                |> Store.waitForAll
                                |> Store.value getter

                            let results =
                                databaseStateArray
                                |> Array.toList
                                |> List.collect
                                    (fun databaseState ->
                                        match databaseState with
                                        | Ok databaseState ->
                                            let informationResults =
                                                databaseState.InformationStateMap
                                                |> Map.values
                                                |> Seq.toList
                                                |> List.collect
                                                    (fun informationState ->
                                                        let informationName =
                                                            informationState.Information
                                                            |> Information.Name
                                                            |> InformationName.Value

                                                        [
                                                            if informationName.Contains searchText then
                                                                yield
                                                                    SearchResultType.Information informationName,
                                                                    (SearchResult.Information
                                                                        informationState.Information,
                                                                     informationName)

                                                            yield!
                                                                searchAttachments informationState.Attachments
                                                                |> List.map
                                                                    (fun attachmentText ->
                                                                        SearchResultType.InformationAttachment
                                                                            informationName,
                                                                        (SearchResult.InformationAttachment
                                                                            informationState.Information,
                                                                         attachmentText))
                                                        ])

                                            let taskResults =
                                                databaseState.TaskStateMap
                                                |> Map.toList
                                                |> List.collect
                                                    (fun (taskId, taskState) ->
                                                        let taskName = taskState.Task.Name |> TaskName.Value

                                                        [
                                                            if taskName.Contains searchText then
                                                                yield
                                                                    SearchResultType.Task taskName,
                                                                    (SearchResult.Task (
                                                                        databaseState.Database.Id,
                                                                        taskId
                                                                     ),
                                                                     taskName)

                                                            yield!
                                                                searchAttachments taskState.Attachments
                                                                |> List.map
                                                                    (fun attachmentText ->
                                                                        SearchResultType.TaskAttachment taskName,
                                                                        (SearchResult.TaskAttachment (
                                                                            databaseState.Database.Id,
                                                                            taskId
                                                                         ),
                                                                         attachmentText))

                                                            yield!
                                                                taskState.CellStateMap
                                                                |> Map.toList
                                                                |> List.collect
                                                                    (fun (dateId, cellState) ->
                                                                        [
                                                                            yield!
                                                                                searchAttachments cellState.Attachments
                                                                                |> List.map
                                                                                    (fun attachmentText ->
                                                                                        SearchResultType.CellAttachment (
                                                                                            taskName,
                                                                                            dateId
                                                                                            |> DateId.Value
                                                                                            |> FlukeDate.Stringify
                                                                                        ),
                                                                                        (SearchResult.CellAttachment (
                                                                                            taskId,
                                                                                            dateId
                                                                                         ),
                                                                                         attachmentText))
                                                                        ])

                                                        ])

                                            informationResults @ taskResults
                                        | Error error ->
                                            toast (fun x -> x.description <- error)
                                            [])

                            setSearchResults results

                        setLoading false
                    }),
                [|
                    box setLoading
                    box toast
                    box setSearchResults
                |]
            )

        let debouncedSearch =
            React.useMemo (
                (fun () -> JS.debounce (search >> Promise.start) 1000),
                [|
                    box search
                |]
            )

        React.useEffect (
            (fun () -> debouncedSearch searchText),
            [|
                box searchText
                box debouncedSearch
            |]
        )

        Chakra.stack
            (fun x ->
                x.spacing <- "10px"
                x.padding <- "15px")
            [
                Chakra.box
                    (fun x -> x.position <- "relative")
                    [
                        if loading then
                            Chakra.flex
                                (fun x ->
                                    x.position <- "absolute"
                                    x.top <- "0"
                                    x.bottom <- "0"
                                    x.right <- "16px"
                                    x.zIndex <- 1)
                                [
                                    LoadingSpinner.InlineLoadingSpinner ()
                                ]

                        Input.LeftIconInput
                            {|
                                Icon = Icons.bs.BsSearch |> Icons.render
                                CustomProps = fun x -> x.fixedValue <- Some searchText
                                Props =
                                    fun x ->
                                        x.onChange <-
                                            fun (e: KeyboardEvent) ->
                                                promise {
                                                    setLoading true
                                                    setSearchText e.Value
                                                }

                                        x.autoFocus <- true
                                        x.placeholder <- "Search"
                            |}
                    ]

                yield!
                    searchResults
                    |> List.groupBy fst
                    |> List.map (fun (searchResultType, results) -> searchResultType, results |> List.map snd)
                    |> List.sortBy fst
                    |> List.map
                        (fun (searchResultType, results) ->
                            Chakra.box
                                (fun x -> x.marginBottom <- "10px")
                                [
                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            str (
                                                match searchResultType with
                                                | SearchResultType.Information _ -> "Information"
                                                | SearchResultType.InformationAttachment information ->
                                                    $"Information Attachment ({information})"
                                                | SearchResultType.Task _ -> "Task"
                                                | SearchResultType.TaskAttachment task -> $"Task Attachment ({task})"
                                                | SearchResultType.CellAttachment (task, date) ->
                                                    $"Cell Attachment (Task: {task} / Date: {date})"
                                            )
                                        ]
                                    Chakra.box
                                        (fun x -> x.marginLeft <- "25px")
                                        [
                                            yield!
                                                results
                                                |> List.map
                                                    (fun (searchResult, resultText) ->
                                                        SearchResultItem searchResult resultText)
                                        ]
                                ])
            ]
