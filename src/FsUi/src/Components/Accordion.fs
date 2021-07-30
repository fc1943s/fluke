namespace FsUi.Components

open FsJs
open FsStore
open FsStore.Model
open FsUi.Bindings
open FsCore
open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Fable.React


module Accordion =
    [<ReactComponent>]
    let AccordionItem title children =
        UI.accordionItem
            (fun x ->
                if children
                   |> Seq.exists
                       (fun cmp ->
                           let props: {| props: UI.IChakraProps |} = unbox cmp

                           match props.props.flex with
                           | String.ValidString _ -> true
                           | _ -> false) then
                    x.flex <- "1"

                x.borderColor <- "gray.45"
                x.borderBottomWidth <- "0 !important"
                x.flexDirection <- "column"
                //                x.flex <- "1"
//                x.overflow <- "auto"
//                x.flexBasis <- unbox "auto"
                x.display <- "flex")
            [
                UI.accordionButton
                    (fun x ->
                        x.backgroundColor <- "gray.16"
                        x.tabIndex <- -1)
                    [
                        UI.box
                            (fun _ -> ())
                            [
                                title
                            ]
                        UI.spacer (fun _ -> ()) []
                        UI.accordionIcon (fun _ -> ()) []
                    ]

                UI.accordionPanel
                    (fun x ->
                        x.flex <- "1"
                        x.flexDirection <- "column"
                        x.display <- "flex"
                        //                        x.overflow <- "auto"
//                        x.flexBasis <- 0
                        x.paddingTop <- "10px")
                    children
            ]

    [<ReactComponent>]
    let Accordion
        (input: {| Items: (ReactElement * ReactElement) list
                   Atom: Atom<string []>
                   Props: UI.IChakraProps -> unit |})
        =
        let atomValue, setAtomValue = Store.useState input.Atom

        let titleArray =
            React.useMemo (
                (fun () ->
                    input.Items
                    |> List.map fst
                    |> List.toArray
                    |> Array.map
                        (fun item ->
                            if jsTypeof item = "string" then
                                string item
                            else
                                item?props
                                |> Option.ofObjUnbox
                                |> Option.map
                                    (fun props ->
                                        props?children
                                        |> Option.ofObjUnbox
                                        |> Option.map
                                            (fun children ->
                                                if jsTypeof children = "string" then
                                                    string children
                                                else
                                                    children
                                                    |> Array.tryFind (fun x -> jsTypeof x = "string")
                                                    |> Option.defaultWith (fun () -> failwith $"{item}"))
                                        |> Option.defaultWith (fun () -> failwith $"{item}"))
                                |> Option.defaultWith (fun () -> failwith $"{item}"))),
                [|
                    box input.Items
                |]
            )

        UI.accordion
            (fun x ->
                x.allowMultiple <- true
                x.reduceMotion <- true
                x.display <- "flex"
                x.flexDirection <- "column"
                x.flex <- "1"
                x.overflow <- "auto"
                x.flexBasis <- 0
                x.defaultIndex <- [||]

                x.index <-
                    let hiddenTitleSet = atomValue |> Set.ofArray

                    titleArray
                    |> Array.indexed
                    |> Array.filter (fun (_, title) -> hiddenTitleSet.Contains title |> not)
                    |> Array.map fst
                    |> function
                        | [||] -> x.defaultIndex
                        | index -> index |> JS.toJsArray

                x.onChange <-
                    fun (indexes: obj) ->
                        promise {
                            let newIndexes =
                                match jsTypeof indexes with
                                | "number" ->
                                    match indexes |> unbox |> int with
                                    | -1 -> [||]
                                    | n ->
                                        [|
                                            n
                                        |]
                                | _ -> unbox indexes

                            let visibleTitles =
                                newIndexes
                                |> Array.map (fun index -> titleArray.[index])

                            let newHiddenTitles = titleArray |> Array.except visibleTitles

                            let newIndexes =
                                newIndexes
                                |> Array.map (fun index -> titleArray.[index])

                            if newIndexes.Length > 0 then setAtomValue newHiddenTitles
                        }

                input.Props x)
            [
                yield!
                    input.Items
                    |> List.map
                        (fun (title, cmp) ->
                            AccordionItem
                                title
                                [
                                    cmp
                                ])
            ]
