namespace FsUi.Components

open Feliz
open Fable.React
open FsUi.Bindings


module Modal =
    type IProps =
        inherit UI.IChakraProps

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

        //        printfn $"input.input.Props.isOpen={props.isOpen} localState={localState}"

        if not props.isOpen && localState = LocalState.Closed then
            nothing
        else
            UI.modal
                (fun x ->
                    //                x.isCentered <- true
                    x.isLazy <- true
                    x.isOpen <- props.isOpen
                    x.onClose <- props.onClose)
                [
                    UI.modalOverlay (fun _ -> ()) []
                    UI.modalContent
                        (fun x ->
                            x.backgroundColor <- "gray.13"
                            x.maxWidth <- "95vw"
                            x.maxHeight <- "80vh"
                            x.overflow <- "auto")
                        [
                            UI.modalBody
                                (fun x -> x.padding <- "40px")
                                [
                                    yield! props.children
                                ]
                            UI.modalCloseButton (fun _ -> ()) []
                        ]
                ]
