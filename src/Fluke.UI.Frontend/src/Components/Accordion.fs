namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fable.Core
open Fable.Core.JsInterop
open Fable.React


module Accordion =
    let accordionItem title children =
        UI.accordionItem
            (fun x ->

                if children
                   |> Seq.exists
                       (fun cmp ->
                           let props : {| props: UI.IChakraProps |} = unbox cmp

                           match props.props.flex with
                           | String.ValidString _ -> true
                           | _ -> false) then
                    x.flex <- "1"

                x.borderColor <- "gray.45"
                x.borderBottomWidth <- "0 !important"
                x.flexDirection <- "column"
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
                                str title
                            ]
                        UI.spacer (fun _ -> ()) []
                        UI.accordionIcon (fun _ -> ()) []
                    ]

                UI.accordionPanel
                    (fun x ->
                        x.flex <- "1"
                        x.flexDirection <- "column"
                        x.display <- "flex"
                        x.paddingTop <- "10px")
                    children
            ]

    [<ReactComponent>]
    let Accordion
        (input: {| Items: (string * ReactElement) list
                   Atom: Store.Atom<string []>
                   Props: UI.IChakraProps -> unit |})
        =
        let atomValue, setAtomValue = Store.useState input.Atom

        UI.accordion
            (fun x ->
                x.allowMultiple <- true
                x.reduceMotion <- true

                x.display <- "flex"
                x.flexDirection <- "column"
                x.flex <- "1"

                x.defaultIndex <-
                    input.Items
                    |> Seq.indexed
                    |> Seq.map fst
                    |> Seq.toArray

                x.index <-
                    atomValue
                    |> Array.map
                        (fun title ->
                            input.Items
                            |> List.map fst
                            |> List.findIndex (fun x -> x = title))
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
                                |> Array.map (fun index -> input.Items.[index] |> fst)

                            if newIndexes.Length > 0 then setAtomValue newIndexes
                        }

                input.Props x)
            [
                yield!
                    input.Items
                    |> List.map
                        (fun (title, cmp) ->
                            accordionItem
                                title
                                [
                                    cmp
                                ])
            ]
