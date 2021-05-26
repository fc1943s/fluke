namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings

module Hint =
    type IProps =
        abstract hint : ReactElement with get, set
        abstract hintTitle : ReactElement option with get, set


    [<ReactComponent>]
    let Hint (props: IProps -> unit) =
        let props = JS.newObj props

        Popover.Popover
            {|
                Trigger =
                    InputLabelIconButton.InputLabelIconButton
                        {|
                            Props =
                                (fun x ->
                                    x.icon <- Icons.bs.BsQuestionCircle |> Icons.render
                                    x.marginLeft <- "4px"
                                    x.marginTop <- "-5px")
                        |}
                Body =
                    fun (_disclosure, _initialFocusRef) ->
                        [
                            match props.hintTitle with
                            | Some hintTitle ->
                                Chakra.box
                                    (fun x ->
                                        x.paddingBottom <- "12px"
                                        x.fontSize <- "15px")
                                    [
                                        Chakra.icon
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
