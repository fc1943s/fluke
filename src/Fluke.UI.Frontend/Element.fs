namespace Fluke.UI.Frontend

module Test =
    open Feliz
    
    // open Feliz.Html
    
    let test () =
        Html.div [
        ]
        

open Fable.React
open Fulma
open Fable.FontAwesome

module Element =
    let icon (icon: Fa.IconOption) (label: string) =
        span [][
            Icon.icon [][
                Fa.i [ icon ] []
            ]
            if label <> "" then
                span [][
                    str label
                ]
        ]
