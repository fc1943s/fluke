namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop
open FsJs
open FsUi.Bindings


module DragDrop =
    let reactDnd: {| DragDropContext: obj -> obj
                     Droppable: obj -> obj
                     Draggable: obj -> obj |} =
        importAll "react-beautiful-dnd"

    let inline dragDropContext onDragEnd children =
        ReactBindings.React.createElement (reactDnd.DragDropContext, {| onDragEnd = onDragEnd |}, children)

    let inline droppable props containerProps children =
        let content =
            [
                Js.invoke
                    (fun provided snapshot ->
                        Browser.Dom.window?provided <- provided
                        Browser.Dom.window?snapshot <- snapshot

                        printfn $"provided={provided} snapshot={snapshot}"

                        Ui.box
                            (fun x ->
                                x.ref <- provided?innerRef
                                x <+ provided?droppableProps
                                containerProps x)
                            [
                                yield! children
                                yield provided?placeholder
                            ])
            ]
            |> box
            |> unbox

        ReactBindings.React.createElement (reactDnd.Droppable, props, content)

    let inline draggable props containerProps children =
        let content =
            [
                Js.invoke
                    (fun provided snapshot ->
                        Browser.Dom.window?provided2 <- provided
                        Browser.Dom.window?snapshot2 <- snapshot

                        printfn $"@ provided={provided} snapshot={snapshot}"

                        Ui.box
                            (fun x ->
                                x.ref <- provided?innerRef
                                x <+ provided?draggableProps
                                x <+ provided?dragHandleProps
                                containerProps x)
                            [
                                yield! children
                            ])
            ]
            |> box
            |> unbox

        ReactBindings.React.createElement (reactDnd.Draggable, props, content)
