namespace Fluke.UI.Frontend.Bindings

open Feliz
open Fable.React
open Fable.Core.JsInterop


module DragDrop =
    let private reactDnd : {| DragDropContext: obj -> obj
                              Droppable: obj -> obj
                              Draggable: obj -> obj |} =
        importAll "react-beautiful-dnd"

    let dragDropContext onDragEnd children =
        ReactBindings.React.createElement (reactDnd.DragDropContext, {| onDragEnd = onDragEnd |}, children)

    let droppable props children =
        let content =
            fun provided snapshot ->
                printfn $"provided={provided} snapshot={snapshot}"
                React.fragment [ yield! children ]

        ReactBindings.React.createElement (reactDnd.Droppable, props, unbox content)

    let draggable () =
        ReactBindings.React.createElement (reactDnd.Draggable, {|  |}, [])
