namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.UseListener
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module TopBar =
    let render =
        React.memo (fun () ->
            let logout = Auth.useLogout ()

            Chakra.flex
                {|
                    height = "31px"
                    backgroundColor = "gray.10%"
                    align = "center"
                    padding = "7px"
                |}
                [
                    Logo.render ()

                    Chakra.box
                        {| marginLeft = "4px" |}
                        [
                            str "Fluke"
                        ]

                    Button.render
                        {|
                            icon = Icons.fiLogOut
                            rightIcon = false
                            props = {| marginLeft = "37px" |}
                            children =
                                [
                                    str "Add Task"
                                ]
                        |}

                    Chakra.spacer () []

                    Chakra.iconButton
                        {|
                            icon = Icons.fiLogOut ()
                            backgroundColor = "transparent"
                            variant = "outline"
                            border = 0
                            width = "30px"
                            height = "30px"
                            borderRadius = 0
                            onClick = logout
                        |}
                        []
                ])
