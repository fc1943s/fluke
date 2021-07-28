namespace FsUi.Components

open Feliz
open FsJs
open FsUi.Bindings

module Hint =
    type IProps =
        abstract hint : ReactElement with get, set
        abstract hintTitle : ReactElement option with get, set


    [<ReactComponent>]
    let Hint (props: IProps -> unit) =
        let props =
            React.useMemo (
                (fun () -> JS.newObj props),
                [|
                    box props
                |]
            )

        Popover.Popover
            {|
                Trigger =
                    InputLabelIconButton.InputLabelIconButton
                        (fun x ->
                            x.icon <- Icons.bs.BsQuestionCircle |> Icons.render
                            x.marginLeft <- "4px"
                            x.marginTop <- "-5px")
                Body =
                    fun (_disclosure, _fetchInitialFocusRef) ->
                        [
                            match props.hintTitle with
                            | Some hintTitle ->
                                UI.box
                                    (fun x ->
                                        x.paddingBottom <- "12px"
                                        x.fontSize <- "1.3rem")
                                    [
                                        UI.icon
                                            (fun x ->
                                                x.``as`` <- Icons.bs.BsQuestionCircle
                                                x.marginTop <- "-3px"
                                                x.marginRight <- "5px"
                                                x.color <- "heliotrope")
                                            []

                                        hintTitle
                                    ]
                            | None -> ()
                            props.hint
                        ]
            |}
