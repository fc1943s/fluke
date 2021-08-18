namespace Fluke.UI.Frontend.Components

open FsCore
open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open FsStore
open FsStore.Hooks
open FsUi.Bindings


module InformationView =
    [<ReactComponent>]
    let InformationTree information taskIdAtoms =
        let cellSize = Store.useValue Atoms.User.cellSize

        Ui.box
            (fun x -> x.paddingLeft <- $"{cellSize}px")
            [
                InformationName.InformationName information

                // Task Name
                Ui.box
                    (fun x -> x.flex <- "1")
                    [
                        yield!
                            taskIdAtoms
                            |> Array.map
                                (fun taskIdAtom ->
                                    Ui.stack
                                        (fun x ->
                                            x.position <- "relative"
                                            x.direction <- "row"
                                            x.spacing <- "10px"
                                            x.paddingLeft <- $"{cellSize}px")
                                        [
                                            Ui.box
                                                (fun x ->
                                                    x.position <- "absolute"
                                                    x.left <- "10px"
                                                    x.top <- "0")
                                                [
                                                    TaskPriority.TaskPriority taskIdAtom
                                                ]
                                            TaskName.TaskName taskIdAtom
                                        ])
                    ]
            ]

    [<ReactComponent>]
    let InformationTreeWrapper informationTaskIdAtom =
        let information, taskIdAtoms = Store.useValue informationTaskIdAtom
        InformationTree information taskIdAtoms

    [<ReactComponent>]
    let KindInformationTreeWrapper kindInformationTaskIdAtom =
        let informationKindName, groups = Store.useValue kindInformationTaskIdAtom

        let cellSize = Store.useValue Atoms.User.cellSize

        Ui.flex
            (fun x ->
                x.direction <- "column"
                x.flex <- "1")
            [
                Ui.box
                    (fun x ->
                        x.height <- $"{cellSize}px"
                        x.lineHeight <- $"{cellSize}px"
                        x.color <- "#444")
                    [
                        str informationKindName
                    ]

                Ui.box
                    (fun _ -> ())
                    [
                        yield! groups |> Array.map InformationTreeWrapper
                    ]
            ]

    [<ReactComponent>]
    let CellsWrapper informationTaskIdAtom =
        let informationTaskId = Store.useValue informationTaskIdAtom

        Cells.Cells (
            informationTaskId
            |> Option.ofObjUnbox
            |> Option.map snd
            |> Option.defaultValue [||]
        )

    [<ReactComponent>]
    let KindCellsWrapper kindInformationTaskIdAtom =
        let _informationKindName, groups = Store.useValue kindInformationTaskIdAtom
        let cellSize = Store.useValue Atoms.User.cellSize

        Ui.box
            (fun _ -> ())
            [
                Ui.box
                    (fun x ->
                        x.position <- "relative"
                        x.height <- $"{cellSize}px"
                        x.lineHeight <- $"{cellSize}px")
                    []
                yield!
                    groups
                    |> Array.map
                        (fun informationTaskIdAtom ->
                            Ui.box
                                (fun _ -> ())
                                [
                                    Ui.box
                                        (fun x ->
                                            x.position <- "relative"
                                            x.height <- $"{cellSize}px"
                                            x.lineHeight <- $"{cellSize}px")
                                        []
                                    CellsWrapper informationTaskIdAtom
                                ])
            ]


    [<ReactComponent>]
    let InformationView () =
        let _groupIndentationLength = 20

        let informationTaskIdAtomsByKind = Store.useValue Selectors.Session.informationTaskIdAtomsByKind
        let cellSize = Store.useValue Atoms.User.cellSize

        Ui.flex
            (fun x -> x.flex <- "1")
            [
                Ui.flex
                    (fun x ->
                        x.direction <- "column"
                        x.flex <- "1"
                        x.paddingRight <- "10px"
                        x.paddingLeft <- "4px"
                        x.maxWidth <- "400px")
                    [
                        yield!
                            Ui.box (fun x -> x.minHeight <- $"{cellSize}px") []
                            |> List.replicate 3

                        Ui.flex
                            (fun x -> x.direction <- "column")
                            [
                                yield!
                                    informationTaskIdAtomsByKind
                                    |> Array.map KindInformationTreeWrapper
                            ]
                    ]
                // Column: Grid
                Ui.box
                    (fun _ -> ())
                    [
                        GridHeader.GridHeader ()

                        Ui.box
                            (fun _ -> ())
                            [
                                yield!
                                    informationTaskIdAtomsByKind
                                    |> Array.map KindCellsWrapper
                            ]
                    ]
            ]
