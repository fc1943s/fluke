namespace FsUi.Components

open FsCore
open FsCore.Model
open FsUi.State
open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open FsStore
open FsUi.Bindings
open FsUi.Hooks



module Dropdown =
    [<ReactComponent>]
    let Dropdown
        (input: {| Tooltip: string
                   Left: bool
                   Trigger: bool -> (bool -> unit) -> ReactElement
                   Body: (unit -> unit) -> ReactElement list |})
        =
        let visible, setVisible = React.useState false
        let darkMode = Store.useValue Atoms.Ui.darkMode

        UI.flex
            (fun x ->
                x.direction <- "column"
                x.overflow <- "auto"
                x.padding <- "1px")
            [
                Tooltip.wrap
                    (str input.Tooltip)
                    [
                        input.Trigger visible setVisible
                    ]
                if not visible then
                    nothing
                else
                    UI.box
                        (fun x ->
                            x.flex <- "1"
                            x.flexDirection <- "column"
                            x.marginTop <- "-1px"

                            if input.Left then
                                x.borderLeftWidth <- "1px"
                            else
                                x.borderRightWidth <- "1px"

                            x.borderBottomWidth <- "1px"
                            x.borderColor <- "whiteAlpha.200"

                            let n = if darkMode then "255" else "0"

                            x.background <-
                                $"""linear-gradient(
                                    180deg,
                                    rgba({n},{n},{n},0) 0%%,
                                    rgba({n},{n},{n},0.01) 20%%,
                                    rgba({n},{n},{n},0.02) 100%%);"""

                            x.padding <- "17px")
                        [
                            yield! input.Body (fun () -> setVisible false)
                        ]
            ]


    let inline InputDropdown props customProps bodyFn =
        Dropdown
            {|
                Tooltip = ""
                Left = false
                Trigger =
                    fun visible setVisible ->
                        UI.box
                            (fun x -> x.position <- "relative")
                            [
                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.rightButton <-
                                                    Some (
                                                        Button.Button
                                                            {|
                                                                Hint = None
                                                                Icon =
                                                                    Some (
                                                                        Icons.io5.IoCaretDown |> Icons.render,
                                                                        Button.IconPosition.Left
                                                                    )
                                                                Props =
                                                                    fun x ->
                                                                        x.borderRadius <- "0 5px 5px 0"
                                                                        x.minWidth <- "26px"

                                                                        x.onClick <-
                                                                            (fun _ ->
                                                                                promise { setVisible (not visible) })
                                                                Children = []
                                                            |}
                                                    )

                                                customProps x
                                        Props =
                                            fun x ->
                                                x.isReadOnly <- true
                                                props x
                                    |}
                            ]
                Body = bodyFn
            |}

    let inline EnumDropdown<'T when 'T: equality> (value: 'T) (setValue: 'T -> unit) props =
        InputDropdown
            props
            (fun x ->
                x.fixedValue <- Some value
                x.onFormat <- Some Enum.name)
            (fun onHide ->
                [
                    UI.stack
                        (fun x ->
                            x.flex <- "1"
                            x.spacing <- "1px"
                            x.padding <- "1px"
                            x.marginBottom <- "6px"
                            x.maxHeight <- "217px"
                            x.overflowY <- "auto"
                            x.flexBasis <- 0)
                        [
                            yield!
                                Enum.ToList<'T> ()
                                |> Seq.map
                                    (fun value' ->
                                        DropdownMenuButton.DropdownMenuButton
                                            {|
                                                Label = Enum.name value'
                                                OnClick =
                                                    fun () ->
                                                        promise {
                                                            setValue value'
                                                            onHide ()
                                                        }
                                                Checked = (value' = value)
                                            |})
                        ]
                ])

    let inline ColorDropdown color setColor props =
        InputDropdown
            (fun x ->
                x.color <- color |> Color.Value |> Option.defaultValue null
                x.fontWeight <- "bold"
                x.isReadOnly <- true
                props x)
            (fun x -> x.fixedValue <- color |> Color.Value |> Some)
            (fun _onHide ->
                [
                    ColorPicker.render
                        {|
                            color = color |> Color.Value |> Option.defaultValue null
                            onChange = fun color -> setColor (Color (color.hex.ToUpper ()))
                        |}
                ])

    [<ReactComponent>]
    let ColorDropdownAtom atom props =
        let value, setValue = Store.useState atom
        ColorDropdown value setValue props

    [<ReactComponent>]
    let CustomConfirmDropdown left onConfirm trigger children =
        let isMounted = React.useIsMounted ()

        Dropdown
            {|
                Tooltip = ""
                Left = left
                Trigger = trigger
                Body =
                    fun onHide ->
                        [
                            UI.stack
                                (fun x -> x.spacing <- "10px")
                                [
                                    yield! children onHide

                                    UI.box
                                        (fun _ -> ())
                                        [
                                            Button.Button
                                                {|
                                                    Hint = None
                                                    Icon =
                                                        Some (
                                                            Icons.fi.FiCheck |> Icons.render,
                                                            Button.IconPosition.Left
                                                        )
                                                    Props =
                                                        fun x ->
                                                            if isMounted.current then
                                                                x.onClick <-
                                                                    fun _ ->
                                                                        promise {
                                                                            if isMounted.current then
                                                                                let! result = onConfirm ()

                                                                                if result && isMounted.current then
                                                                                    onHide ()
                                                                        }
                                                    Children =
                                                        [
                                                            str "Confirm"
                                                        ]
                                                |}
                                        ]
                                ]
                        ]
            |}

    let inline ConfirmDropdown label onConfirm children =
        CustomConfirmDropdown
            true
            onConfirm
            (fun visible setVisible ->
                Button.Button
                    {|
                        Hint = None
                        Icon =
                            Some (
                                (if visible then Icons.fi.FiChevronUp else Icons.fi.FiChevronDown)
                                |> Icons.render,
                                Button.IconPosition.Right
                            )
                        Props = fun x -> x.onClick <- fun _ -> promise { setVisible (not visible) }
                        Children =
                            [
                                str label
                            ]
                    |})
            children
