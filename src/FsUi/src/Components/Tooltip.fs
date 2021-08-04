namespace FsUi.Components

open Fable.React
open Feliz
open FsUi.Bindings
open FsUi.Hooks


module Tooltip =
    [<ReactComponent>]
    let Tooltip props wrapperProps children =
        let ref = React.useElementRef ()
        let hovered = Listener.useElementHover ref

        UI.box
            (fun x ->
                x.``as`` <- "span"
                x.ref <- ref
                wrapperProps x)
            [
                UI.tooltip
                    (fun x ->
                        x.isLazy <- true
                        x.isOpen <- hovered
                        x.paddingTop <- "3px"
                        x.paddingBottom <- "4px"
                        x.backgroundColor <- "gray.77"
                        x.zIndex <- 20000
                        x.color <- "gray.13"
                        x.closeOnMouseDown <- true
                        x.portalProps <- {| appendToParentPortal = true |}
                        //                        x.shouldWrapChildren <- true
                        props x)
                    [
                        yield! children
                    ]
            ]


    let inline wrap label children =
        if label = nothing then
            React.fragment children
        else
            Tooltip (fun x -> x.label <- label) (fun _ -> ()) children
