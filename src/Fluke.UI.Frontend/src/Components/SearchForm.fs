namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module SearchForm =
    [<RequireQualifiedAccess>]
    type SearchResultType =
        | Information of information: string
        | InformationAttachment of information: string
        | Task of task: string
        | TaskAttachment of task: string
        | CellAttachment of task: string * date: string


    [<ReactComponent>]
    let SearchResultItem anchor searchResultText =
        let navigateAnchor = Navigate.useNavigateAnchor ()

        UI.box
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

                        x.onClick <- fun _ -> navigateAnchor anchor)
            ]

    [<ReactComponent>]
    let SearchForm () =
        let searchResults, setSearchResults = React.useState []
        let searchText, setSearchText = Store.useState Atoms.User.searchText
        let loading, setLoading = React.useState (searchText.Length > 0)
        let toast = UI.useToast ()

        let search =
            Store.useCallback (
                (fun getter _ searchText ->
                    promise {
                        if searchText = "" then
                            setSearchResults []
                        else
                            let searchAttachments attachmentStateList =
                                attachmentStateList
                                |> List.choose
                                    (fun attachmentState ->
                                        match attachmentState.Attachment with
                                        | Attachment.Comment (Comment.Comment comment) when comment.Contains searchText ->
                                            Some comment
                                        | _ -> None)

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
                                                                    Navigate.Anchor.Information
                                                                        informationState.Information,
                                                                    informationName

                                                            yield!
                                                                searchAttachments informationState.AttachmentStateList
                                                                |> List.map
                                                                    (fun attachmentText ->
                                                                        SearchResultType.InformationAttachment
                                                                            informationName,
                                                                        Navigate.Anchor.InformationAttachment (
                                                                            informationState.Information,
                                                                            AttachmentId System.Guid.Empty
                                                                        ),

                                                                        attachmentText)
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
                                                                    Navigate.Anchor.Task (
                                                                        databaseState.Database.Id,
                                                                        taskId
                                                                    ),
                                                                    taskName

                                                            yield!
                                                                searchAttachments taskState.AttachmentStateList
                                                                |> List.map
                                                                    (fun attachmentText ->
                                                                        SearchResultType.TaskAttachment taskName,
                                                                        Navigate.Anchor.TaskAttachment (
                                                                            databaseState.Database.Id,
                                                                            taskId,
                                                                            AttachmentId System.Guid.Empty
                                                                        ),
                                                                        attachmentText)

                                                            yield!
                                                                taskState.CellStateMap
                                                                |> Map.toList
                                                                |> List.collect
                                                                    (fun (dateId, cellState) ->
                                                                        [
                                                                            yield!
                                                                                searchAttachments
                                                                                    cellState.AttachmentStateList
                                                                                |> List.choose
                                                                                    (fun attachmentText ->
                                                                                        match dateId |> DateId.Value with
                                                                                        | Some date ->
                                                                                            Some (
                                                                                                SearchResultType.CellAttachment (
                                                                                                    taskName,
                                                                                                    date
                                                                                                    |> FlukeDate.Stringify
                                                                                                ),
                                                                                                Navigate.Anchor.CellAttachment (
                                                                                                    taskId,
                                                                                                    dateId,
                                                                                                    AttachmentId
                                                                                                        System.Guid.Empty
                                                                                                ),
                                                                                                attachmentText
                                                                                            )
                                                                                        | None -> None)
                                                                        ])
                                                        ])

                                            informationResults @ taskResults
                                        | Error error ->
                                            toast (fun x -> x.description <- error)
                                            setSearchText ""
                                            [])
                                |> List.sortBy (fun (_, _, resultText) -> resultText)
                                |> List.sortBy (fun (resultType, _, _) -> resultType)

                            setSearchResults results

                        setLoading false
                    }),
                [|
                    box setLoading
                    box toast
                    box setSearchResults
                    box setSearchText
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

        UI.stack
            (fun x ->
                x.spacing <- "10px"
                x.padding <- "15px")
            [
                UI.box
                    (fun x -> x.position <- "relative")
                    [
                        if loading then
                            UI.flex
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
                                                    match e.Value with
                                                    | "" ->
                                                        setSearchResults []
                                                        setSearchText ""
                                                    | value ->
                                                        setLoading true
                                                        setSearchText value
                                                }

                                        x.autoFocus <- true
                                        x.placeholder <- "Search"
                            |}
                    ]

                if searchResults.IsEmpty then
                    UI.str "No results"
                else
                    yield!
                        searchResults
                        |> List.map
                            (fun (resultType, result, resultText) ->
                                let header =
                                    match resultType with
                                    | SearchResultType.Information _ -> "Information"
                                    | SearchResultType.InformationAttachment information ->
                                        $"Information Attachment ({information})"
                                    | SearchResultType.Task _ -> "Task"
                                    | SearchResultType.TaskAttachment task -> $"Task Attachment ({task})"
                                    | SearchResultType.CellAttachment (task, date) ->
                                        $"Cell Attachment (Task: {task} / Date: {date})"

                                header, (result, resultText))
                        |> List.groupBy fst
                        |> List.map (fun (header, results) -> header, results |> List.map snd)
                        |> List.map
                            (fun (header, results) ->
                                UI.box
                                    (fun x -> x.marginBottom <- "10px")
                                    [
                                        UI.str header
                                        UI.box
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
