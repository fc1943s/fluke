namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module TaskForm =

    [<ReactComponent>]
    let taskForm () =
        Chakra.box
            ()
            [
                str "Task name: "
            ]
