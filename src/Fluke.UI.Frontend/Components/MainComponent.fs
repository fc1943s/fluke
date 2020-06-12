namespace Fluke.UI.Frontend.Components

open System
open Browser
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Browser.Types


module MainComponent =
    module CustomHooks =
        let useWindowSize () =
            let getWindowSize () =
                {| Width = window.innerWidth
                   Height = window.innerHeight |}
            let size, setSize = React.useState (getWindowSize ())

            React.useLayoutEffect (fun () ->
                let updateSize (_event: Event) =
                    setSize (getWindowSize ())

                window.addEventListener ("resize", updateSize)

                { new IDisposable with
                    member _.Dispose () =
                        window.removeEventListener ("resize", updateSize)
                }
            )
            size

    let render = React.functionComponent (fun () ->
//        let windowSize = CustomHooks.useWindowSize ()

        Recoil.root [
            HomePageComponent.``default`` ()
        ]
    )

