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
                backgroundColor = "gray.10%"
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

                AddDatabaseButton.AddDatabaseButton {| props = {| marginLeft = "37px" |} |}
                AddTaskButton.AddTaskButton {| props = {| marginLeft = "10px" |} |}

                Chakra.spacer {|  |} []

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
            ]
