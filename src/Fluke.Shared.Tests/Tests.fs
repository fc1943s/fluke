namespace Fluke.Shared

open FsCore
open Expecto
open Fluke.Shared
open Expecto.Flip
open Fluke.Shared.TempData


module Tests =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State
    open Templates

    let databaseStateFromDslTemplate user dslTemplate =
        let dslDataList =
            dslTemplate.Tasks
            |> List.map
                (fun templateTask ->
                    Testing.createLaneRenderingDslData
                        {|
                            User = user
                            Position = dslTemplate.Position
                            Task = templateTask.Task
                            Events = templateTask.Events
                        |})

        let databaseState = DatabaseState.Create (name = DatabaseName "Test", owner = user.Username)

        let newDatabaseState =
            (databaseState, dslDataList)
            ||> List.fold
                    (fun databaseState dslData ->
                        databaseState
                        |> mergeDslDataIntoDatabaseState dslData)

        newDatabaseState


    let testWithTemplateData (dslTemplate: DslTemplate) =
        let databaseState = databaseStateFromDslTemplate templatesUser dslTemplate

        dslTemplate.Tasks
        |> List.iter
            (fun taskTemplate ->
                let taskState = databaseState.TaskStateMap.[taskTemplate.Task.Id]

                let expectedCellMetadataList =
                    taskTemplate.Expected
                    |> List.map
                        (fun (date, templateExpectList) ->
                            let defaultCellMetadata = {| CellStatus = None; Sessions = None |}

                            let cellMetadata =
                                (defaultCellMetadata, templateExpectList)
                                ||> List.fold
                                        (fun cellMetadata templateExpect ->
                                            match templateExpect with
                                            | TemplateExpect.Status cellStatus ->
                                                {| cellMetadata with
                                                    CellStatus = Some cellStatus
                                                |}
                                            | TemplateExpect.Session count ->
                                                {| cellMetadata with
                                                    Sessions = Some count
                                                |})

                            date, cellMetadata)

                let expectedStatus =
                    expectedCellMetadataList
                    |> List.choose
                        (fun (date, expectedCellMetadata) ->
                            match expectedCellMetadata.CellStatus with
                            | Some cellStatus -> Some (date, cellStatus)
                            | _ -> None)

                let expectedSessions =
                    expectedCellMetadataList
                    |> List.choose
                        (fun (date, expectedCellMetadata) ->
                            match expectedCellMetadata.Sessions with
                            | Some count -> Some (date, count)
                            | _ -> None)

                let statusAssertList =
                    match expectedStatus with
                    | [] -> []
                    | expectedStatus ->
                        let dateSequence = taskTemplate.Expected |> List.map fst

                        let laneStatusMap =
                            Rendering.renderTaskStatusMap
                                templatesUser.DayStart
                                dslTemplate.Position
                                dateSequence
                                taskState

                        expectedStatus
                        |> List.map
                            (fun expected ->
                                match expected with
                                | date, _ as expected ->
                                    let actual =
                                        laneStatusMap
                                        |> Map.tryFind date
                                        |> Option.defaultValue Disabled
                                        |> fun cellStatus -> date, cellStatus

                                    expected, actual)

                let sessionsAssertList =
                    match expectedSessions with
                    | [] -> []
                    | expectedSessions ->
                        expectedSessions
                        |> List.map
                            (fun (date, count) ->
                                let sessionCount =
                                    taskState.SessionList
                                    |> List.filter (fun (Session start) -> isToday templatesUser.DayStart start date)
                                    |> List.length

                                count, sessionCount)

                statusAssertList
                |> List.iter (fun (expected, actual) -> Expect.equal "" expected actual)

                sessionsAssertList
                |> List.iter (fun (expected, actual) -> Expect.equal "" expected actual))

    let createTests testDatabase =
        testDatabase
        |> List.map
            (fun (name1, list) ->
                testList
                    name1
                    [
                        yield!
                            list
                            |> List.map
                                (fun (name2, list) ->
                                    testList
                                        name2
                                        [
                                            yield!
                                                list
                                                |> List.map
                                                    (fun (name3, dslTemplate: DslTemplate) ->

                                                        let newDslTemplate =
                                                            { dslTemplate with
                                                                Tasks =
                                                                    dslTemplate.Tasks
                                                                    |> List.map
                                                                        (fun taskTemplate ->
                                                                            { taskTemplate with
                                                                                Task =
                                                                                    { taskTemplate.Task with
                                                                                        Id = TaskId.NewId ()
                                                                                    }
                                                                            })
                                                            }

                                                        test name3 { testWithTemplateData newDslTemplate })
                                        ])
                    ])

    let databaseTests =
        let database = getDatabase templatesUser
        createTests database

    let databaseMap = getDatabaseMap templatesUser

    let laneSortingDslTemplate = databaseMap.["Lane Sorting/Default/All task types mixed"]

    let dslTemplate =
        { laneSortingDslTemplate with
            Tasks =
                laneSortingDslTemplate.Tasks
                |> List.map
                    (fun templateTask ->
                        { templateTask with
                            Task =
                                { templateTask.Task with
                                    Id = TaskId.NewId ()
                                }
                        })
        }



    let (|NoSorting|IncomingRecurrency|TimeOfDay|All|) =
        function
        | Choice1Of4 _ -> NoSorting
        | Choice2Of4 _ -> IncomingRecurrency
        | Choice3Of4 _ -> TimeOfDay
        | Choice4Of4 _ -> All

    let noSorting = Choice1Of4 ()
    let sortByIncomingRecurrency = Choice2Of4 ()
    let sortByTimeOfDay = Choice3Of4 ()
    let sortByAll = Choice4Of4 ()

    let testWithLaneSortingData
        (props: {| Sort: Choice<_, _, _, _>
                   Data: (Task * DslTask list) list
                   Expected: string list
                   Position: FlukeDateTime |})
        =
        let databaseState =
            let dslData =
                Testing.createLaneSortingDslData
                    {|
                        User = templatesUser
                        Position = props.Position
                        Expected = props.Expected
                        Data = props.Data
                    |}

            DatabaseState.Create (name = DatabaseName "Test", owner = templatesUser.Username)
            |> mergeDslDataIntoDatabaseState dslData

        let newDateSequence padding =
            databaseState.TaskStateMap
            |> Map.values
            |> Seq.collect (fun taskState -> taskState.CellStateMap |> Map.keys)
            |> Seq.toList
            |> Rendering.getDateSequence padding

        let expect dateSequence =

            let lanes =
                databaseState.TaskStateMap
                |> Map.toList
                |> List.map
                    (fun (_, taskState) ->
                        let taskStatusMap =
                            Rendering.renderTaskStatusMap templatesUser.DayStart props.Position dateSequence taskState

                        taskState, taskStatusMap)
                |> List.sortBy (fun (taskState, _) -> taskState.Task.Name |> TaskName.Value)

            match props.Sort with
            | NoSorting -> lanes
            | IncomingRecurrency -> Sorting.sortLanesByIncomingRecurrency templatesUser.DayStart props.Position lanes
            | TimeOfDay -> Sorting.sortLanesByTimeOfDay templatesUser.DayStart props.Position lanes
            | All ->
                lanes
                |> Sorting.sortLanesByFrequency
                |> Sorting.sortLanesByIncomingRecurrency templatesUser.DayStart props.Position
                |> Sorting.sortLanesByTimeOfDay templatesUser.DayStart props.Position //input.TaskOrderList
            |> List.map (fun ({ Task = { Name = TaskName name } }, _) -> name)
            |> Expect.equal "" props.Expected

        let dateSequence1 = newDateSequence (35, 35)
        let dateSequence2 = newDateSequence (14, 14)

        expect dateSequence1
        expect dateSequence2



    [<Tests>]
    let tests =
        testList
            "Tests"
            [
                yield! databaseTests

                testList
                    "Lane Sorting"
                    [
                        test "All task types mixed: No Sorting" {
                            testWithLaneSortingData
                                {|
                                    Sort = noSorting
                                    Position =
                                        FlukeDateTime.Create (
                                            FlukeDate.Create 2020 Month.March 10,
                                            FlukeTime.Create 14 0,
                                            Second 0
                                        )
                                    Data =
                                        dslTemplate.Tasks
                                        |> List.map (fun templateTask -> templateTask.Task, templateTask.Events)
                                    Expected =
                                        [
                                            "01"
                                            "02"
                                            "03"
                                            "04"
                                            "05"
                                            "06"
                                            "07"
                                            "08"
                                            "09"
                                            "10"
                                            "11"
                                            "12"
                                            "13"
                                            "14"
                                            "15"
                                            "16"
                                            "17"
                                            "18"
                                        ]
                                |}
                        }

                        test "All task types mixed: Sort by Incoming Recurrency" {
                            testWithLaneSortingData
                                {|
                                    Sort = sortByIncomingRecurrency
                                    Position =
                                        FlukeDateTime.Create (
                                            FlukeDate.Create 2020 Month.March 10,
                                            FlukeTime.Create 14 0,
                                            Second 0
                                        )
                                    Data =
                                        dslTemplate.Tasks
                                        |> List.map (fun templateTask -> templateTask.Task, templateTask.Events)
                                    Expected =
                                        [
                                            "04"
                                            "14"
                                            "07"
                                            "15"
                                            "01"
                                            "02"
                                            "03"
                                            "05"
                                            "06"
                                            "08"
                                            "09"
                                            "10"
                                            "11"
                                            "12"
                                            "13"
                                            "16"
                                            "17"
                                            "18"
                                        ]
                                |}
                        }

                        test "All task types mixed: Sort by Time of Day" {
                            testWithLaneSortingData
                                {|
                                    Sort = sortByTimeOfDay
                                    Position =
                                        FlukeDateTime.Create (
                                            FlukeDate.Create 2020 Month.March 10,
                                            FlukeTime.Create 14 0,
                                            Second 0
                                        )
                                    Data =
                                        dslTemplate.Tasks
                                        |> List.map (fun templateTask -> templateTask.Task, templateTask.Events)
                                    Expected =
                                        [
                                            "16"
                                            "05"
                                            "03"
                                            "13"
                                            "17"
                                            "18"
                                            "11"
                                            "04"
                                            "01"
                                            "10"
                                            "09"
                                            "02"
                                            "08"
                                            "06"
                                            "07"
                                            "12"
                                            "14"
                                            "15"
                                        ]
                                |}
                        }

                        test "All task types mixed: Sort by All" {
                            testWithLaneSortingData
                                {|
                                    Sort = sortByAll
                                    Position =
                                        FlukeDateTime.Create (
                                            FlukeDate.Create 2020 Month.March 10,
                                            FlukeTime.Create 14 0,
                                            Second 0
                                        )
                                    Data =
                                        dslTemplate.Tasks
                                        |> List.map (fun templateTask -> templateTask.Task, templateTask.Events)
                                    Expected =
                                        [
                                            "16"
                                            "05"
                                            "17"
                                            "18"
                                            "13"
                                            "03"
                                            "11"
                                            "04"
                                            "01"
                                            "10"
                                            "09"
                                            "08"
                                            "02"
                                            "14"
                                            "07"
                                            "15"
                                            "06"
                                            "12"
                                        ]
                                |}
                        }
                    ]
            ]
