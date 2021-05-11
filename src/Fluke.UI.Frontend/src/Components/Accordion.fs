namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Feliz.Recoil
open Fable.React
open Fluke.UI.Frontend.Bindings


module Accordion =
    let accordionItem title children =
        Chakra.accordionItem
            (fun _ -> ())
            [
                Chakra.accordionButton
                    (fun x -> x.backgroundColor <- "gray.16")
                    [
                        Chakra.box
                            (fun _ -> ())
                            [
                                str title
                            ]
                        Chakra.spacer (fun _ -> ()) []
                        Chakra.accordionIcon (fun _ -> ()) []
                    ]

                Chakra.accordionPanel (fun x -> x.paddingTop <- "10px") children
            ]

    [<ReactComponent>]
    let Accordion
        (input: {| Items: (string * ReactElement) list
                   Atom: Recoil.RecoilValue<string [], _>
                   Props: Chakra.IChakraProps -> unit |})
        =
        let atomValue, setAtomValue = Recoil.useState input.Atom

        Chakra.accordion
            (fun x ->
                x.allowMultiple <- true
                x.reduceMotion <- true

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
