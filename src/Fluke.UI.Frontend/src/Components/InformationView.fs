namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module InformationView =
    [<ReactComponent>]
    let InformationTree information taskIdAtoms =
        let cellSize = Store.useValue Atoms.User.cellSize

        UI.box
            (fun x -> x.paddingLeft <- $"{cellSize}px")
            [
                InformationName.InformationName information

                // Task Name
                UI.box
                    (fun x -> x.flex <- "1")
                    [
                        yield!
                            taskIdAtoms
                            |> Array.map
                                (fun taskIdAtom ->
                                    UI.stack
                                        (fun x ->
                                            x.position <- "relative"
                                            x.direction <- "row"
                                            x.spacing <- "10px"
                                            x.paddingLeft <- $"{cellSize}px")
                                        [
                                            UI.box
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

        UI.flex
            (fun x ->
                x.direction <- "column"
                x.flex <- "1")
            [
                UI.box
                    (fun x ->
                        x.height <- $"{cellSize}px"
                        x.lineHeight <- $"{cellSize}px"
                        x.color <- "#444")
                    [
                        str informationKindName
                    ]

                UI.box
                    (fun _ -> ())
                    [
                        yield! groups |> Array.map InformationTreeWrapper
                    ]
            ]

    [<ReactComponent>]
    let CellsWrapper informationTaskIdAtom =
        let _information, taskIdAtoms = Store.useValue informationTaskIdAtom
        Cells.Cells taskIdAtoms

    [<ReactComponent>]
    let KindCellsWrapper kindInformationTaskIdAtom =
        let _informationKindName, groups = Store.useValue kindInformationTaskIdAtom
        let cellSize = Store.useValue Atoms.User.cellSize

        UI.box
            (fun _ -> ())
            [
                UI.box
                    (fun x ->
                        x.position <- "relative"
                        x.height <- $"{cellSize}px"
                        x.lineHeight <- $"{cellSize}px")
                    []
                yield!
                    groups
                    |> Array.map
                        (fun informationTaskIdAtom ->
                            UI.box
                                (fun _ -> ())
                                [
                                    UI.box
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

        UI.flex
            (fun x -> x.flex <- "1")
            [
                UI.flex
                    (fun x ->
                        x.direction <- "column"
                        x.flex <- "1"
                        x.paddingRight <- "10px"
                        x.paddingLeft <- "4px"
                        x.maxWidth <- "400px")
                    [
                        yield!
                            UI.box (fun x -> x.minHeight <- $"{cellSize}px") []
                            |> List.replicate 3

                        UI.flex
                            (fun x -> x.direction <- "column")
                            [
                                yield!
                                    informationTaskIdAtomsByKind
                                    |> Array.map KindInformationTreeWrapper
                            ]
                    ]
                // Column: Grid
                UI.box
                    (fun _ -> ())
                    [
                        GridHeader.GridHeader ()

                        UI.box
                            (fun _ -> ())
                            [
                                yield!
                                    informationTaskIdAtomsByKind
                                    |> Array.map KindCellsWrapper
                            ]
                    ]
            ]
