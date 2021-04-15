namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module TopBar =

    [<ReactComponent>]
    let TopBar () =
        let logout = Auth.useLogout ()

        Chakra.flex
            {|
                height = "31px"
                backgroundColor = "gray.10"
                align = "center"
                padding = "7px"
            |}
            [
                Logo.Logo ()

                Chakra.box
                    {| marginLeft = "4px" |}
                    [
                        str "Fluke"
                    ]

                AddDatabaseButton.AddDatabaseButton {| marginLeft = "37px" |}
                AddTaskButton.AddTaskButton {| marginLeft = "10px" |}

                Chakra.spacer {|  |} []

                Chakra.stack
                    {|
                        spacing = "10px"
                        direction = "row"
                    |}
                    [
                        Chakra.link
                            {|
                                href = "https://github.com/fc1943s/fluke"
                                isExternal = true
                            |}
                            [
                                Tooltip.Tooltip
                                    (Dom.newObj (fun x -> x.label <- str "GitHub repository"))
                                    [
                                        Chakra.iconButton
                                            {|
                                                icon = Icons.aiOutlineGithub ()
                                                backgroundColor = "transparent"
                                                variant = "outline"
                                                border = 0
                                                fontSize = "18px"
                                                width = "30px"
                                                height = "30px"
                                                borderRadius = 0
                                                onClick = logout
                                            |}
                                            []
                                    ]
                            ]

                        Tooltip.Tooltip
                            (Dom.newObj (fun x -> x.label <- str "Logout"))
                            [
                                Chakra.iconButton
                                    {|
                                        icon = Icons.fiLogOut ()
                                        backgroundColor = "transparent"
                                        variant = "outline"
                                        border = 0
                                        fontSize = "18px"
                                        width = "30px"
                                        height = "30px"
                                        borderRadius = 0
                                        onClick = logout
                                    |}
                                    []
                            ]
                    ]
            ]
