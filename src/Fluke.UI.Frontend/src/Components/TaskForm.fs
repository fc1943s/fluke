namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings

module TaskForm =
    let render =
        React.memo (fun () ->
            Chakra.box
                ()
                [
                    str "Task name: "
                ])

