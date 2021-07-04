namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop
open Fable.Core


module DragDrop =
    let private reactDnd : {| DragDropContext: obj -> obj
                              Droppable: obj -> obj
                              Draggable: obj -> obj |} =
        importAll "react-beautiful-dnd"

    let dragDropContext onDragEnd children =
        ReactBindings.React.createElement (reactDnd.DragDropContext, {| onDragEnd = onDragEnd |}, children)

    let droppable props containerProps children =
        let content =
            [
                JS.invoke
                    (fun provided snapshot ->
                        Browser.Dom.window?provided <- provided
                        Browser.Dom.window?snapshot <- snapshot

                        printfn $"provided={provided} snapshot={snapshot}"

                        Chakra.box
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

    let draggable props containerProps children =
        let content =
            [
                JS.invoke
                    (fun provided snapshot ->
                        Browser.Dom.window?provided2 <- provided
                        Browser.Dom.window?snapshot2 <- snapshot

                        printfn $"@ provided={provided} snapshot={snapshot}"

                        Chakra.box
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
