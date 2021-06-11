namespace Fluke.UI.Frontend.Components

open Fable.Core
open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module Modal =
    type IProps =
        inherit Chakra.IChakraProps

    [<RequireQualifiedAccess>]
    type LocalState =
        | Rendered
        | Closing
        | Closed

    [<ReactComponent>]
    let Modal (input: {| Props: IProps |}) =
        let localState, setLocalState =
            React.useState (if input.Props.isOpen then LocalState.Rendered else LocalState.Closed)

        React.useEffect (
            (fun () ->
                match localState with
                | LocalState.Closed when input.Props.isOpen -> setLocalState LocalState.Rendered
                | LocalState.Rendered when not input.Props.isOpen -> setLocalState LocalState.Closing
                | LocalState.Closing -> setLocalState LocalState.Closed
                | _ -> ()),
            [|
                box input.Props
                box localState
                box setLocalState
            |]
        )

        printfn $"input.input.Props.isOpen={input.Props.isOpen} localState={localState}"

        if not input.Props.isOpen
           && localState = LocalState.Closed then
            nothing
        else
            Chakra.modal
                (fun x ->
                    //                x.isCentered <- true
                    x.isLazy <- true
                    x.isOpen <- input.Props.isOpen
                    x.onClose <- input.Props.onClose)
                [
                    Chakra.modalOverlay (fun _ -> ()) []
                    Chakra.modalContent
                        (fun x -> x.backgroundColor <- "gray.13")
                        [
                            Chakra.modalBody
                                (fun x -> x.padding <- "40px")
                                [
                                    yield! input.Props.children
                                ]
                            Chakra.modalCloseButton (fun _ -> ()) []
                        ]
                ]
