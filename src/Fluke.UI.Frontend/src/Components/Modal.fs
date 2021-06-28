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
    let Modal (props: IProps) =
        let localState, setLocalState = React.useState (if props.isOpen then LocalState.Rendered else LocalState.Closed)

        React.useEffect (
            (fun () ->
                match localState with
                | LocalState.Closed when props.isOpen -> setLocalState LocalState.Rendered
                | LocalState.Rendered when not props.isOpen -> setLocalState LocalState.Closing
                | LocalState.Closing -> setLocalState LocalState.Closed
                | _ -> ()),
            [|
                box props
                box localState
                box setLocalState
            |]
        )

        printfn $"input.input.Props.isOpen={props.isOpen} localState={localState}"

        if not props.isOpen && localState = LocalState.Closed then
            nothing
        else
            Chakra.modal
                (fun x ->
                    //                x.isCentered <- true
                    x.isLazy <- true
                    x.isOpen <- props.isOpen
                    x.onClose <- props.onClose)
                [
                    Chakra.modalOverlay (fun _ -> ()) []
                    Chakra.modalContent
                        (fun x -> x.backgroundColor <- "gray.13")
                        [
                            Chakra.modalBody
                                (fun x -> x.padding <- "40px")
                                [
                                    yield! props.children
                                ]
                            Chakra.modalCloseButton (fun _ -> ()) []
                        ]
                ]
